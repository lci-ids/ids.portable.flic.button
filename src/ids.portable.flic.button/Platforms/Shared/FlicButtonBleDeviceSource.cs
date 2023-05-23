using OneControl.Direct.IdsCanAccessoryBle.Connections;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IDS.Core.IDS_CAN;
using IDS.Portable.Common;
using IDS.Portable.LogicalDevice;
using System.Collections.Concurrent;
using System.Linq;
using OneControl.Direct.IdsCanAccessoryBle.FlicButton;
using OneControl.Devices.EchoBrakeControl;


namespace IDS.Portable.Flic.Button.Platforms.Shared
{

    public class FlicButtonBleDeviceSource : CommonDisposable, IFlicButtonBleDeviceSource
    {
        private const string LogTag = nameof(FlicButtonBleDeviceSource);
        private const string DeviceSourceTokenDefault = "Ids.Accessory.FlicButton.Default";
        private const string FlicButtonSoftwarePartNumber = "Flic2";

        private readonly object _lock = new();
        private readonly ConcurrentDictionary<Guid, FlicButtonBleDeviceDriver> _registeredFlicButtons = new ConcurrentDictionary<Guid, FlicButtonBleDeviceDriver>();

        public string DeviceSourceToken { get; }
        public bool AllowAutoOfflineLogicalDeviceRemoval => false;
        public bool IsDeviceSourceActive => _registeredFlicButtons.Any();

        public IN_MOTION_LOCKOUT_LEVEL InTransitLockoutLevel => IN_MOTION_LOCKOUT_LEVEL.LEVEL_0_NO_LOCKOUT;
        public IN_MOTION_LOCKOUT_LEVEL GetLogicalDeviceInTransitLockoutLevel(ILogicalDevice? logicalDevice) => IN_MOTION_LOCKOUT_LEVEL.LEVEL_0_NO_LOCKOUT;
        public bool IsLogicalDeviceHazardous(ILogicalDevice? logicalDevice) => false;


        public ILogicalDeviceService DeviceService { get; }
        public IEnumerable<ISensorConnection> SensorConnectionsAll
        {
            get
            {
                foreach (var sensorConnection in _registeredFlicButtons.Values)
                    yield return sensorConnection.SensorConnection;
            }
        }
        public IEnumerable<IFlicButtonBleDeviceDriver> SensorDevices => _registeredFlicButtons.Values;

        /// <summary>
        /// Flic Button is an optional accessory, so it needs to be registered separately from other devices. The
        /// following calls need to be made in the app layer while registering other accessories:
        /// 
        /// Resolver<IFlicButtonBleDeviceSource>.LazyConstructAndRegister(() => new FlicButtonBleDeviceSource(Resolver<ILogicalDeviceService>.Resolve));
        /// AccessoryRegistration.FlicButtonBleDeviceSource = Resolver<IFlicButtonBleDeviceSource>.Resolve;
        /// </summary>
        public FlicButtonBleDeviceSource(ILogicalDeviceService deviceService, string deviceSourceToken = DeviceSourceTokenDefault)
        {
            DeviceService = deviceService;
            DeviceSourceToken = deviceSourceToken ?? DeviceSourceTokenDefault;
        }

        public IEnumerable<ILogicalDeviceTag> MakeDeviceSourceTags(ILogicalDevice? logicalDevice) => new ILogicalDeviceTag[] { };

        public bool IsLogicalDeviceSupported(ILogicalDevice? logicalDevice)
        {
            foreach (var flicButtonBle in _registeredFlicButtons.Values)
            {
                if (flicButtonBle.LogicalDevice == logicalDevice)
                    return true;
            }

            return false;
        }
        public bool IsLogicalDeviceOnline(ILogicalDevice? logicalDevice)
        {
            foreach (var flicButtonBle in _registeredFlicButtons.Values)
            {
                if (flicButtonBle.LogicalDevice == logicalDevice)
                    return flicButtonBle.IsConnected;
            }

            return false;
        }

        #region Rename
        public bool IsLogicalDeviceRenameSupported(ILogicalDevice? logicalDevice) => IsLogicalDeviceSupported(logicalDevice);

        public async Task RenameLogicalDevice(ILogicalDevice? logicalDevice, FUNCTION_NAME toName, byte toFunctionInstance, CancellationToken cancellationToken)
        {
            TaggedLog.Debug(LogTag, $"Flic button rename: LogicalDevice: {logicalDevice}  toName: {toName} toFunctionInstance: {toFunctionInstance}. This has not been implemented yet.");
            throw new NotSupportedException();
        }
        #endregion

        #region ILogicalDeviceSourceDirectMetadata
        public Task<string> GetSoftwarePartNumberAsync(ILogicalDevice logicalDevice, CancellationToken cancelToken) => Task.FromResult(FlicButtonSoftwarePartNumber);

        public Version? GetDeviceProtocolVersion(ILogicalDevice logicalDevice)
        {
            return null;  // No device protocol version info is available!
        }
        #endregion

        /// <summary>
        /// Scans for and pairs with a Flic button. Timeout in the flic library is 30 seconds.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// SensorConnectionFlic object representing the connection to the paired Flic button if successful;
        /// otherwise, returns null if no Flic button is found or the pairing process fails.
        /// </returns>
        public async Task<SensorConnectionFlic?> ScanAndPairFlicButton(CancellationToken cancellationToken)
        {
            try
            {
                var data = await FlicButtonManager.Instance.ScanAndPairButton(cancellationToken);
                if (data is not null)
                {
                    TaggedLog.Debug(LogTag, $"Paired Flic Button. SerialNumber: {data.Value.SerialNumber} Firmware: {data.Value.FirmwareVersion} MAC: {data.Value.MacAddress} UUID: {data.Value.Uuid}");
                    return new SensorConnectionFlic(data.Value.Name, Guid.Parse(data.Value.Uuid), data.Value.MacAddress.ToMAC(), data.Value.SerialNumber);
                }
            }
            catch (Exception e)
            {
                TaggedLog.Debug(LogTag, $"Exception scanning and pairing flic button: {e}");
            }

            return null;
        }

        /// <summary>
        /// Unpairs the given flic button from the phone.
        /// </summary>
        /// <param name="mac">The MAC Address of the flic button to be unpaired.</param>
        /// <returns>
        /// True if it was successful unpairing the flic button, false otherwise.
        /// </returns>
        public async Task<bool> UnpairFlicButtonAsync(MAC mac)
        {
            return await FlicButtonManager.Instance.UnpairButton(mac);
        }

        public bool RegisterSensor(SensorConnectionFlic sensorConnection)
        {
            if (sensorConnection?.ConnectionGuid is not { } bleDeviceId)
                return false;

            try
            {
                if (IsSensorRegistered(bleDeviceId))
                    return false;

                var flicButton = new FlicButtonBleDeviceDriver(this, sensorConnection);
                flicButton.UpdateFlicButtonReachabilityEvent += DeviceReachabilityUpdated;
                flicButton.LogicalDevice?.AddDeviceSource(this);
                
                var newRegistration = _registeredFlicButtons.TryAdd(bleDeviceId, flicButton);
                if (newRegistration)
                    TaggedLog.Debug(LogTag, $"Register Flic Button {bleDeviceId}");

                if (IsStarted)
                    flicButton.Start();

                return newRegistration;

            }
            catch (Exception ex)
            {
                TaggedLog.Error(LogTag, $"Flic Button error registering {bleDeviceId}: {ex.Message}");
                return false;
            }
        }

        public void UnRegisterSensor(Guid bleDeviceId)
        {
            if (!_registeredFlicButtons.TryRemove(bleDeviceId, out var flicButton))
                return;

            flicButton.UpdateFlicButtonReachabilityEvent -= DeviceReachabilityUpdated;
            flicButton.TryDispose();  // This will also stop the brake control if it had been started
        }

        public bool IsSensorRegistered(Guid bleDeviceId) => _registeredFlicButtons.ContainsKey(bleDeviceId);

        public IFlicButtonBleDeviceDriver? GetSensorDevice(ILogicalDevice? logicalDevice) => _registeredFlicButtons.Values.FirstOrDefault((ts) => true);

        #region ICommonDisposable
        public override void Dispose(bool disposing)
        {
            foreach (var sensorDevice in _registeredFlicButtons)
            {
                UnRegisterSensor(sensorDevice.Key);
            }

            _registeredFlicButtons.Clear();
        }
        #endregion

        #region ILogicalDeviceSourceDirectConnection
        public IReadOnlyList<ILogicalDeviceTag> ConnectionTagList => throw new NotImplementedException();
        public bool IsConnected => IsStarted;
        public bool IsStarted = false;

        public void Start()
        {
            lock (_lock)
            {
                if (IsStarted)
                    return;

                IsStarted = true;

                //Start all child Direct Connections
                foreach (var flicButton in _registeredFlicButtons)
                {
                    flicButton.Value.Start();
                }
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                IsStarted = false;

                //Stop all child Direct Connections
                foreach (var flicButton in _registeredFlicButtons)
                {
                    flicButton.Value.Stop();
                }
            }
        }

        public event Action<ILogicalDeviceSourceDirectConnection>? DidConnectEvent;
        public event Action<ILogicalDeviceSourceDirectConnection>? DidDisconnectEvent;
        public LogicalDeviceReachability DeviceSourceReachability(ILogicalDevice logicalDevice)
        {
            var bleControl = FindAssociatedBleControlForLogicalDevice(logicalDevice);
            if (bleControl is null)
                return LogicalDeviceReachability.Unknown;

            return bleControl.Reachability(logicalDevice);
        }

        private FlicButtonBleDeviceDriver? FindAssociatedBleControlForLogicalDevice(ILogicalDevice logicalDevice)
        {
            foreach (var flicDevice in _registeredFlicButtons)
            {
                if (logicalDevice == flicDevice.Value.LogicalDevice)
                    return flicDevice.Value;
            }
            return null;
        }

        private void DeviceReachabilityUpdated(FlicButtonBleDeviceDriver flicButtonBle)
        {
            UpdateDeviceSourceReachabilityEvent?.Invoke(this);

            if (flicButtonBle.IsConnected)
                DidConnectEvent?.Invoke(this);
            else
                DidDisconnectEvent?.Invoke(this);

            // Update the device's online/offline status
            flicButtonBle.LogicalDevice?.UpdateDeviceOnline(flicButtonBle.IsConnected);
        }

        public event UpdateDeviceSourceReachabilityEventHandler? UpdateDeviceSourceReachabilityEvent;
        public ILogicalDeviceSessionManager? SessionManager => throw new NotImplementedException();
        #endregion
    }
}
