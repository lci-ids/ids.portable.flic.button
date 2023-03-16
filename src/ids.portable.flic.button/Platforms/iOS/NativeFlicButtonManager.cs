using IDS.Portable.Flic.Button.Platforms.Shared;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using IDS.Portable.Common;
using FlicLibraryIos;

namespace IDS.Portable.Flic.Button.Platforms.iOS
{
    internal class NativeFlicButtonManager : IFlicButtonManager
    {
        // TODO: Change this back.
        //private const string LogTag = "NativeFlicButtonManager";
        private const string LogTag = "FlicDebug";
        private static bool _managerReady = false;

        public NativeFlicButtonPlatform Platform => NativeFlicButtonPlatform.Ios;

        private async Task<bool> Init()
        {
            if (_managerReady)
                return true;

            var cts = new CancellationTokenSource(2000);
            var cancellationToken = cts.Token;

            var tcs = new TaskCompletionSource<bool>();

            TaggedLog.Debug(LogTag, $"FlicManager ConfigureWithDelegate called.");
            FLICManager.ConfigureWithDelegate(new FlicManagerCallback((managerReady) =>
            {
                TaggedLog.Debug(LogTag, $"FlicManager is ready.");
                _managerReady = managerReady;
                tcs.SetResult(managerReady);
            }), new FlicButtonCallback((data) =>
            {
                TaggedLog.Debug(LogTag, $"Received button data.");

            }), true);

            return await tcs.TryWaitAsync(cancellationToken);
        }

        public async Task<FlicButtonDeviceData?> ScanAndPairButton(CancellationToken cancellationToken)
        {
            var managerReady = await Init();
            if (!managerReady)
                throw new FlicButtonManagerNotReadyException();

            var manager = FLICManager.SharedManager();

            if (manager is null)
                throw new FlicButtonManagerNullException();

            // TODO: Decide how we really want to handle this.
            if (manager.Buttons.Length is 1)
            {
                TaggedLog.Debug("FlicDebug", $"Found already paired button.");

                var button = manager.Buttons[0];

                try
                {
                    SubscribeToButtonEvents(button.SerialNumber, (flicEvent) =>
                    {
                        TaggedLog.Debug("FlicDebug", $"Received button event.");
                    });

                    ConnectButton(button.SerialNumber);
                    await TaskExtension.TryDelay(5000, cancellationToken);
                    TaggedLog.Debug(LogTag, $"After delay State: {button.State} isReady: {button.IsReady} Delegate: {button.Delegate}.");

                }
                catch (Exception e)
                {
                    TaggedLog.Debug("FlicDebug", $"Exception subscribing/connecting button: {e}.");
                }

                return new FlicButtonDeviceData(button.Name ?? "", button.SerialNumber, button.BluetoothAddress, Convert.ToInt32(button.FirmwareRevision), button.Uuid);
            }

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
                // TODO: Verify this.
                if (error is null)
                {
                    TaggedLog.Error(LogTag, $"Paired with flic button, SerialNumber: {button.SerialNumber} Mac: {button.BluetoothAddress}.");
                    // We're complete and we don't have an error, our button is good to go.
                    tcs.TrySetResult(new FlicButtonDeviceData(button.Name ?? "", button.SerialNumber, button.BluetoothAddress, Convert.ToInt32(button.FirmwareRevision), button.Uuid));
                }

                // Failure.
                TaggedLog.Error(LogTag, $"Failed to pair, error: {error}.");

                tcs.TrySetResult(null);
            });

            return await tcs.TryWaitAsync(cancellationToken);
        }

        public void SubscribeToButtonEvents(string serialNumber, Action<FlicButtonEventData> flicEvent)
        {
            TaggedLog.Debug(LogTag, $"SubscribeToButtonEvents _managerReady: {_managerReady}.");
            if (!_managerReady)
                throw new FlicButtonManagerNotReadyException();

            var manager = FLICManager.SharedManager();

            if (manager is null)
                throw new FlicButtonManagerNullException();

            var buttons = manager.Buttons;
            var button = buttons.FirstOrDefault(button => button.SerialNumber == serialNumber);
            if (button is null)
                throw new FlicButtonNullException($"No flic button found with the serial number: {serialNumber}");

            TaggedLog.Debug(LogTag, $"State: {button.State} isReady: {button.IsReady} Delegate: {button.Delegate}.");
            button.Delegate = new FlicButtonCallback(flicEvent);
        }

        public void ConnectButton(string serialNumber)
        {
            TaggedLog.Debug(LogTag, $"ConnectButton _managerReady: {_managerReady}.");
            if (!_managerReady)
                throw new FlicButtonManagerNotReadyException();

            var manager = FLICManager.SharedManager();

            if (manager is null)
                throw new FlicButtonManagerNullException();

            var buttons = manager.Buttons;
            var button = buttons.FirstOrDefault(button => button.SerialNumber == serialNumber);
            if (button is null)
                throw new FlicButtonNullException($"No flic button found with the serial number: {serialNumber}");

            button.Connect();
            TaggedLog.Debug(LogTag, $"State: {button.State} isReady: {button.IsReady} Delegate: {button.Delegate}.");
        }

        public void DisconnectOrAbortPendingConnection(string serialNumber)
        {
            TaggedLog.Debug(LogTag, $"_managerReady: {_managerReady}.");
            if (!_managerReady)
                throw new FlicButtonManagerNotReadyException();

            var manager = FLICManager.SharedManager();

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
