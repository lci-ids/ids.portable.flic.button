using System;
using System.Threading.Tasks;
using System.Threading;

namespace IDS.Portable.Flic.Button.Platforms.Shared
{
    public enum NativeFlicButtonPlatform
    {
        Ios,
        Android,
    }

    public struct FlicButtonDeviceData
    {
        public FlicButtonDeviceData(string name, string serialNumber, string macAddress, int firmwareVersion, string uuid)
        {
            Name = name;
            SerialNumber = serialNumber;
            MacAddress = macAddress;
            FirmwareVersion = firmwareVersion;
            Uuid = uuid;
        }

        public string Name;
        public string SerialNumber;
        public string MacAddress;
        public int FirmwareVersion;
        public string Uuid;
    }

    public struct FlicButtonEventData
    {
        public bool Connected;

        public long Timestamp;
        public bool WasQueued;
        public bool LastQueued;

        public bool IsClick;
        public bool IsHold;
        public bool IsSingleClick;
        public bool IsDoubleClick;

        public bool IsUp;
        public bool IsDown;

        public int BatteryLevelPercent;
        public float BatteryVoltage;
    }

    public interface IFlicButtonManager
    {
        NativeFlicButtonPlatform Platform { get; }

        Task Init();
        Task<FlicButtonDeviceData?> ScanAndPairButton(CancellationToken cancellationToken);
        void SubscribeToButtonEvents(string serialNumber, Action<FlicButtonEventData> flicEvent);
        void ConnectButton(string serialNumber);
        void DisconnectOrAbortPendingConnection(string serialNumber);
        Task<bool> UnpairButton(string serialNumber);
    }
}
