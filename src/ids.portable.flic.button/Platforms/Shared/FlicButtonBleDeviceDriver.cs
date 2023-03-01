using IDS.Core.IDS_CAN;
using IDS.Portable.Common;
using IDS.Portable.LogicalDevice;
using OneControl.Direct.IdsCanAccessoryBle.Connections;
using System;
using System.Diagnostics;
using OneControl.Devices.FlicButton;
using OneControl.Direct.IdsCanAccessoryBle.FlicButton;

namespace IDS.Portable.Flic.Button.Platforms.Shared;

public class FlicButtonBleDeviceDriver : CommonDisposable, IFlicButtonBleDeviceDriver
{
    private const string LogTag = nameof(FlicButtonBleDeviceDriver);

    private readonly object _lock = new();

    private bool _isStarted;

    public ILogicalDeviceFlicButton? LogicalDevice { get; private set; }

    private readonly IFlicButtonBleDeviceSource _sourceDirect;   // This needs to be a IFlicButtonBleDeviceSource so the primary source can be looked up!

    public bool IsConnected { get; private set; } = false;

    public SensorConnectionFlic SensorConnection { get; }

    internal Guid BleDeviceId => SensorConnection.ConnectionGuid;

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
        // TODO: Add correct Product ID once it's added in Core.
        var logicalDeviceId = new LogicalDeviceId(DEVICE_TYPE.BUTTON_FLIC, 0x00, FUNCTION_NAME.TRAILER_BRAKE_CONTROLLER, 0x00, PRODUCT_ID.LCI_ONECONTROL_ANDROID_MOBILE_APPLICATION, AccessoryMacAddress);
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

    public void Start()
    {
        lock (_lock)
        {
            if (_isStarted)
                return;

            _isStarted = true;

            try
            {
                // Close any open connection
                FlicButtonManager.Instance.DisconnectOrAbortPendingConnection(SensorConnection.SerialNumber);

                FlicButtonManager.Instance.SubscribeToButtonEvents(SensorConnection.SerialNumber, OnFlicButtonEventReceived);

                // Open Connection
                FlicButtonManager.Instance.ConnectButton(SensorConnection.SerialNumber);
            }
            catch (Exception ex)
            {
                TaggedLog.Error(LogTag, $"Failed to connect to Flic Button, message: {ex.Message}");
            }
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            try
            {
                FlicButtonManager.Instance.DisconnectOrAbortPendingConnection(SensorConnection.SerialNumber);
            }
            catch (Exception e)
            {
                TaggedLog.Debug(LogTag, $"Problem disconnecting Flic Button: {e}");
            }

            IsConnected = false;
            _isStarted = false;
        }
    }

    public override void Dispose(bool disposing)
    {
        Stop();
    }
}
