﻿using IDS.Portable.Flic.Button.Platforms.Shared;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using IDS.Portable.Common;
using FlicLibraryIos;
using IDS.Core.IDS_CAN;
using IDS.Portable.LogicalDevice;

namespace IDS.Portable.Flic.Button.Platforms.iOS
{
    internal class NativeFlicButtonManager : IFlicButtonManager
    {
        private const string LogTag = "NativeFlicButtonManager";
        private const int ConfigureFlicManagerTimeoutMs = 10000;
        private const int FlicManagerInitDelayMs = 600;
        private const int UnpairFlicButtonTimeoutMs = 2000;

        private static bool _managerReady = false;
        private FLICButton? _flicButton;

        public NativeFlicButtonPlatform Platform => NativeFlicButtonPlatform.Ios;
        public bool IsConnected => _flicButton?.State is FLICButtonState.Connected;

        public async Task Init()
        {
            if (_managerReady)
                return;

            // The ConfigureWithDelegate call below will not function without this delay. We tried a handful of different approaches
            // to avoid this, but ultimately this is the only thing that would work.
            await TaskExtension.TryDelay(FlicManagerInitDelayMs, CancellationToken.None);

            var cts = new CancellationTokenSource(ConfigureFlicManagerTimeoutMs);
            var cancellationToken = cts.Token;

            var tcs = new TaskCompletionSource<bool>();

            TaggedLog.Debug(LogTag, $"FlicManager ConfigureWithDelegate called.");
            FLICManager.ConfigureWithDelegate(new FlicManagerCallback((managerReady) =>
            {
                TaggedLog.Debug(LogTag, $"FlicManager is ready.");
                _managerReady = managerReady;
                tcs.TrySetResult(managerReady);
            }), new FlicButtonCallback((data) =>
            {
                TaggedLog.Debug(LogTag, $"Received button data.");

            }), true);

            var success =  await tcs.TryWaitAsync(cancellationToken).ConfigureAwait(false);
            if (!success)
                throw new FlicButtonManagerNotReadyException();
        }

        // The iOS library has different behavior for scanning/pairing than the Android library. If a button has already been paired
        // with the flic manager, then the iOS scan will eventually just time out with an error and never find the button. Because
        // of that, it is important to make sure to unpair a button from the manager before removing it from the device source.
        public async Task<FlicButtonDeviceData?> ScanAndPairButton(CancellationToken cancellationToken)
        {
            if (!_managerReady)
                throw new FlicButtonManagerNotReadyException();

            var manager = FLICManager.SharedManager();

            if (manager is null)
                throw new FlicButtonManagerNullException();

            foreach (var button in manager.Buttons)
                manager.ForgetButton(button, (_, _) => { /* DO NOTHING */ });

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
                if (error is null)
                {
                    TaggedLog.Error(LogTag, $"Paired with flic button, SerialNumber: {button.SerialNumber} Mac: {button.BluetoothAddress} Uuid: {button.Uuid}.");
                    // We're complete and we don't have an error, our button is good to go.
                    tcs.TrySetResult(new FlicButtonDeviceData(button.Name ?? "", button.SerialNumber, button.BluetoothAddress, Convert.ToInt32(button.FirmwareRevision), button.Uuid));
                    return;
                }

                // Failure.
                TaggedLog.Error(LogTag, $"Failed to pair, error: {error}.");

                tcs.TrySetResult(null);
            });

            return await tcs.TryWaitAsync(cancellationToken);
        }

        public void SubscribeToButtonEvents(MAC mac, Action<FlicButtonEventData> flicEvent)
        {
            if (!_managerReady)
                throw new FlicButtonManagerNotReadyException();

            var manager = FLICManager.SharedManager();

            if (manager is null)
                throw new FlicButtonManagerNullException();

            var buttons = manager.Buttons;
            var button = buttons.FirstOrDefault(button => button.BluetoothAddress.ToMAC() == mac);

            _flicButton = button ?? throw new FlicButtonNullException($"No flic button found with the mac: {mac}");
            button.Delegate = null;
            button.Delegate = new FlicButtonCallback(flicEvent);
        }

        public void ConnectButton(MAC mac)
        {
            if (!_managerReady)
                throw new FlicButtonManagerNotReadyException();

            var manager = FLICManager.SharedManager();

            if (manager is null)
                throw new FlicButtonManagerNullException();

            var buttons = manager.Buttons;
            var button = buttons.FirstOrDefault(button => button.BluetoothAddress.ToMAC() == mac);
            if (button is null)
                throw new FlicButtonNullException($"No flic button found with the mac: {mac}");

            if (button.State == FLICButtonState.Connected)
                return;

            button.Connect();
        }

        public void DisconnectOrAbortPendingConnection(MAC mac)
        {
            if (!_managerReady)
                throw new FlicButtonManagerNotReadyException();

            var manager = FLICManager.SharedManager();

            if (manager is null)
                throw new FlicButtonManagerNullException();

            var buttons = manager.Buttons;
            var button = buttons.FirstOrDefault(button => button.BluetoothAddress.ToMAC() == mac);
            if (button is null)
                throw new FlicButtonNullException($"No flic button found with the mac: {mac}");

            if (button.State is FLICButtonState.Disconnecting or FLICButtonState.Disconnected)
                return;

            button.Disconnect();
        }

        public async Task<bool> UnpairButton(MAC mac)
        {
            if (!_managerReady)
                throw new FlicButtonManagerNotReadyException();

            var manager = FLICManager.SharedManager();

            if (manager is null)
                throw new FlicButtonManagerNullException();

            var buttons = manager.Buttons;
            var button = buttons.FirstOrDefault(button => button.BluetoothAddress.ToMAC() == mac);
            if (button is null)
                throw new FlicButtonNullException($"No flic button found with the mac: {mac}");

            var cts = new CancellationTokenSource(UnpairFlicButtonTimeoutMs);
            var cancellationToken = cts.Token;

            var tcs = new TaskCompletionSource<bool>();

            manager.ForgetButton(button, (_, error) =>
            {
                TaggedLog.Debug(LogTag, $"Error: {error}");
                if (error is null)
                {
                    TaggedLog.Debug(LogTag, $"No error ");
                    tcs.TrySetResult(true);
                }
            });

            return await tcs.TryWaitAsync(cancellationToken);
        }
    }
}
