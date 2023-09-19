using IDS.Core.IDS_CAN;
using IDS.Portable.Common;
using IDS.Portable.LogicalDevice;
using OneControl.Direct.IdsCanAccessoryBle.Connections;
using System;
using System.Diagnostics;
using OneControl.Devices.FlicButton;
using OneControl.Direct.IdsCanAccessoryBle.FlicButton;
using System.Threading.Tasks;
using System.Threading;

namespace IDS.Portable.Flic.Button.Platforms.Shared
{
    public delegate void UpdateFlicButtonReachabilityEventHandler(FlicButtonBleDeviceDriver echoBrakeControlBle);

    public class FlicButtonBleDeviceDriver : BackgroundOperation, ICommonDisposable, IFlicButtonBleDeviceDriver
    {
        private const string LogTag = nameof(FlicButtonBleDeviceDriver);
        private readonly object _lock = new();
        private readonly IFlicButtonBleDeviceSource _sourceDirect;   // This needs to be a IFlicButtonBleDeviceSource so the primary source can be looked up!
        private const int SleepTimeMs = 2000;

        internal Guid BleDeviceId => SensorConnection.ConnectionGuid;

        public ILogicalDeviceFlicButton? LogicalDevice { get; private set; }
        public bool IsConnected { get; private set; } = false;
        public event UpdateFlicButtonReachabilityEventHandler? UpdateFlicButtonReachabilityEvent;
        public SensorConnectionFlic SensorConnection { get; }
        public MAC AccessoryMacAddress { get; }

        public FlicButtonBleDeviceDriver(IFlicButtonBleDeviceSource sourceDirect, SensorConnectionFlic sensorConnection)
        {
            _sourceDirect = sourceDirect;
            SensorConnection = sensorConnection ?? throw new ArgumentNullException(nameof(sensorConnection));
            AccessoryMacAddress = sensorConnection.AccessoryMac;
            LogicalDevice = CreateLogicalDevice();
        }

        /// <summary>
        /// Creates the logical device associated with this Flic Button.  Should only be called once.
        /// </summary>
        private ILogicalDeviceFlicButton? CreateLogicalDevice()
        {
            if (LogicalDevice is not null)
            {
                Debug.Assert(false, $"{LogTag} Logical Device Has Already Been Created");
                return null;
            }

            TaggedLog.Information(LogTag, $"Creating Logical Device for Flic Button");

            var logicalDeviceId = new LogicalDeviceId(DEVICE_TYPE.BUTTON_FLIC, 0x00, FUNCTION_NAME.TRAILER_BRAKE_CONTROLLER, 0x00, PRODUCT_ID.FLIC_BUTTON, AccessoryMacAddress);
            var logicalDevice = _sourceDirect.DeviceService.DeviceManager?.AddLogicalDevice(logicalDeviceId, 0, _sourceDirect, isAttemptAutoRenameEnabled: (ld) => true);
            if (logicalDevice is not ILogicalDeviceFlicButton logicalDeviceFlicButton || logicalDevice.IsDisposed)
            {
                TaggedLog.Warning(LogTag, $"Unable to create LogicalDeviceFlicButton");
                return null;
            }

            return logicalDeviceFlicButton;
        }

        private void OnFlicButtonEventReceived(FlicButtonEventData flicButtonEventData)
        {
            IsConnected = flicButtonEventData.Connected;
            UpdateFlicButtonReachabilityEvent?.Invoke(this);

            var status = new LogicalDeviceFlicButtonStatus();
            if (flicButtonEventData.IsSingleClick)
                status.SetAction(FlicButtonAction.SingleTap);
            else if (flicButtonEventData.IsDoubleClick)
                status.SetAction(FlicButtonAction.DoubleTap);
            else
                status.SetAction(FlicButtonAction.None);

            if (flicButtonEventData.IsDown)
                status.SetState(FlicButtonState.Down);
            else if (flicButtonEventData.IsUp)
                status.SetState(FlicButtonState.Up);
            else
                status.SetState(FlicButtonState.Unknown);

            status.SetIsHeldDown(flicButtonEventData.IsHold);

            status.SetFlicButtonStateChangedTimeSpan(TimeSpan.FromMilliseconds(flicButtonEventData.Timestamp));

            status.SetBatteryChargeLevel(flicButtonEventData.BatteryLevelPercent);

            LogicalDevice?.UpdateDeviceStatus(status.Data, status.Size);
        }

        public LogicalDeviceReachability Reachability(ILogicalDevice logicalDevice)
        {
            if (logicalDevice != LogicalDevice)
                return LogicalDeviceReachability.Unknown;

            return IsConnected ? LogicalDeviceReachability.Reachable : LogicalDeviceReachability.Unreachable;
        }

        /// <summary>
        /// Main background operation use Start() to start it and Stop() to stop it!
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task BackgroundOperationAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && !IsDisposed)
            {
                try
                {
                    // Close any open connection
                    FlicButtonManager.Instance.SubscribeToButtonEvents(SensorConnection.AccessoryMac, OnFlicButtonEventReceived);

                    // Open Connection
                    FlicButtonManager.Instance.ConnectButton(SensorConnection.AccessoryMac);
                }
                catch (Exception ex)
                {
                    TaggedLog.Error(LogTag, $"Failed to connect to Flic Button, message: {ex.Message}");
                }

                // Short delay to give the button time to connect.
                await TaskExtension.TryDelay(SleepTimeMs, cancellationToken);

                while (FlicButtonManager.Instance.IsConnected && !cancellationToken.IsCancellationRequested && !IsDisposed)
                {
                    await TaskExtension.TryDelay(SleepTimeMs, cancellationToken);
                }
            }

            UpdateFlicButtonReachabilityEvent?.Invoke(this);

            try
            {
                if(FlicButtonManager.Instance.IsConnected)
                    FlicButtonManager.Instance.DisconnectOrAbortPendingConnection(SensorConnection.AccessoryMac);
            }
            catch (Exception e)
            {
                TaggedLog.Debug(LogTag, $"Problem disconnecting Flic Button: {e}");
            }
        }

        #region ICommonDisposable
        private int _isDisposed;
        public bool IsDisposed => (uint)_isDisposed > 0U;

        public void TryDispose()
        {
            try
            {
                if (IsDisposed)
                    return;
                Dispose();
            }
            catch
            {
                /* ignored */
            }
        }

        public void Dispose()
        {
            if (this.IsDisposed || Interlocked.Exchange(ref this._isDisposed, 1) != 0)
                return;

            this.Dispose(true);
        }

        public virtual void Dispose(bool disposing)
        {
            Stop();
        }
        #endregion
    }
}