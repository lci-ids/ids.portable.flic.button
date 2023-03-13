using IDS.Portable.Flic.Button.Platforms.Shared;
using System;
using System.Threading.Tasks;
using System.Threading;
using FlicLibraryIos;
using IDS.Portable.Common;
using ObjCRuntime;
using CoreBluetooth;

namespace IDS.Portable.Flic.Button.Platforms.iOS
{
    internal class NativeFlicButtonManager : IFlicButtonManager
    {
        private const string LogTag = "NativeFlicButtonManager";

        private FLICManager _flicManager = new FLICManager();

        public NativeFlicButtonPlatform Platform => NativeFlicButtonPlatform.Ios;

        public void Init()
        {
            _flicManager.ConfigureWithDelegate(new FlicManagerCallback(), new FlicButtonCallback(), true);
        }

        // TODO: Look into manager didUpdateState, this lets us know our singleton manager is updated and ready.
        public async Task<FlicButtonDeviceData?> ScanAndPairButton(CancellationToken cancellationToken)
        {
            var manager = _flicManager.SharedManager();

            if (manager is null)
                throw new FlicButtonManagerNullException();

            var tcs = new TaskCompletionSource<FlicButtonDeviceData?>();

            manager.ScanForButtonsWithStateChangeHandler((scannerStatus) =>
            {
                switch (scannerStatus)
                {
                    case FLICButtonScannerStatusEvent.Discovered:
                        // Found the button during our scan.
                        TaggedLog.Debug(LogTag, $"Discovered a flic button.");
                        break;
                    case FLICButtonScannerStatusEvent.Connected:
                        // Connected, but not yet ready.
                        TaggedLog.Debug(LogTag, $"Flic button connected but not verified.");
                        break;
                    case FLICButtonScannerStatusEvent.Verified:
                        // Once we've hit this we know our button is ready.
                        TaggedLog.Debug(LogTag, $"Flic button is verified and ready to use.");
                        break;
                    case FLICButtonScannerStatusEvent.VerificationFailed:
                        // Could not successfully connect to the button.
                        TaggedLog.Debug(LogTag, $"Failed to verify flic button.");
                        break;
                }
            }, (button, error)=> 
            {
                if (!error)
                {
                    // We're complete and we don't have an error, our button is good to go.
                    tcs.TrySetResult(new FlicButtonDeviceData(button.Name, button.SerialNumber, button.Identifier, button.FirmwareVersion, button.Uuid));
                }

                // Failure.
                TaggedLog.Error(LogTag, $"Failed to pair, error: {error}.");

                tcs.TrySetResult(null);
            });

            return await tcs.TryWaitAsync(cancellationToken);
        }

        public void SubscribeToButtonEvents(string serialNumber, Action<FlicButtonEventData> flicEvent)
        {
            var manager = _flicManager.SharedManager();

            if (manager is null)
                throw new FlicButtonManagerNullException();

            var buttons = manager.Buttons;
            var button = buttons.FirstOrDefault(button => button.SerialNumber == serialNumber);
            if (button is null)
                throw new FlicButtonNullException($"No flic button found with the serial number: {serialNumber}");

            button.Delegate = new FlicButtonCallback(flicEvent);
        }

        public void ConnectButton(string serialNumber)
        {
            var manager = _flicManager.SharedManager();

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
            var manager = _flicManager.SharedManager();

            if (manager is null)
                throw new FlicButtonManagerNullException();

            var buttons = manager.Buttons;
            var button = buttons.FirstOrDefault(button => button.SerialNumber == serialNumber);
            if (button is null)
                throw new FlicButtonNullException($"No flic button found with the serial number: {serialNumber}");

            button.Disconnect();
        }
    }
}
