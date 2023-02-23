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
        public FlicButtonDeviceData(string serialNumber, string macAddress, int firmwareVersion, string uuid)
        {
            SerialNumber = serialNumber;
            MacAddress = macAddress;
            FirmwareVersion = firmwareVersion;
            Uuid = uuid;
        }

        private string SerialNumber;
        private string MacAddress;
        private int FirmwareVersion;
        private string Uuid;
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

        Task<FlicButtonDeviceData?> ScanAndPairButton(CancellationToken cancellationToken);
        void SubscribeToButtonEvents(string serialNumber, Action<FlicButtonEventData> flicEvent);
        void ConnectButton(string serialNumber);
        void DisconnectOrAbortPendingConnection(string serialNumber);
    }
}
