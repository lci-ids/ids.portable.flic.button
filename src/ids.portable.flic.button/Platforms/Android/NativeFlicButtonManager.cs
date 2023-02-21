using IDS.Portable.Flic.Button.Platforms.Shared;
using System;
using System.Linq;
using IO.Flic.Flic2libandroid;
using IDS.Portable.Common;
using System.Threading.Tasks;
using System.Threading;

namespace IDS.Portable.Flic.Button.Platforms.Android
{
    internal class NativeFlicButtonManager : IFlicButtonManager
    {
        public NativeFlicButtonPlatform Platform => NativeFlicButtonPlatform.Android;
        public async Task<FlicButtonDeviceData?> ScanAndPairButton(CancellationToken cancellationToken)
        {
            var manager = Flic2Manager.Instance;

            if (manager is null)
                throw new FlicButtonManagerNullException();

            var tcs = new TaskCompletionSource<FlicButtonDeviceData?>();

            // One attempt is a 30 second scan.
            manager.StartScan(new FlicScanPairCallback(tcs));

            return await tcs.TryWaitAsync(cancellationToken);
        }

        public void SubscribeToButtonEvents(string serialNumber, Action<FlicButtonEventData> flicEvent)
        {
            var manager = Flic2Manager.Instance;

            if (manager is null)
                throw new FlicButtonManagerNullException();

            var buttons = manager.Buttons;
            var button = buttons.FirstOrDefault(button => button.SerialNumber == serialNumber);
            if (button is null)
                throw new FlicButtonNullException($"No flic button found with the serial number: {serialNumber}");

            button.AddListener(new FlicButtonListenerCallback(flicEvent));
        }

        public void ConnectButton(string serialNumber)
        {
            var manager = Flic2Manager.Instance;

            if (manager is null)
                throw new FlicButtonManagerNullException();

            var buttons = manager.Buttons;
            var button = buttons.FirstOrDefault(button => button.SerialNumber == serialNumber);
            if (button is null)
                throw new FlicButtonNullException($"No flic button found with the serial number: {serialNumber}");

            button.Connect();
        }

        public void DisconnectOrAbortPendingConnection(string serialNumber)
        {
            var manager = Flic2Manager.Instance;

            if (manager is null)
                throw new FlicButtonManagerNullException();

            var buttons = manager.Buttons;
            var button = buttons.FirstOrDefault(button => button.SerialNumber == serialNumber);
            if (button is null)
                throw new FlicButtonNullException($"No flic button found with the serial number: {serialNumber}");

            button.DisconnectOrAbortPendingConnection();
        }
    }
}
