using IDS.Portable.Flic.Button.Platforms.Shared;
using System;
using System.Linq;
using IO.Flic.Flic2libandroid;
using IDS.Portable.Common;
using System.Threading.Tasks;
using System.Threading;
using IDS.Core.IDS_CAN;
using IDS.Portable.LogicalDevice;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace IDS.Portable.Flic.Button.Platforms.Android
{
    internal class NativeFlicButtonManager : IFlicButtonManager
    {
        private readonly ConcurrentDictionary<MAC, List<FlicButtonListenerCallback>> _buttonListenerCallbacks = new();

        private Flic2Button? _flicButton;

        public NativeFlicButtonPlatform Platform => NativeFlicButtonPlatform.Android;
        public bool IsConnected => _flicButton?.ConnectionState is Flic2Button.ConnectionStateConnectedReady;

        public Task Init()
        {
            // We don't have to worry about any initialization here on the android side (it's taken care of in MainActivity),
            // so we can just return here.
            return Task.CompletedTask;
        }


        // The Android library has different behavior for scanning/pairing than the iOS library. If a button has already been paired
        // with the flic manager, then the Android library receives a callback that we found an already paired button and we just
        // return that button here.
        public async Task<FlicButtonDeviceData?> ScanAndPairButton(CancellationToken cancellationToken)
        {
            var manager = Flic2Manager.Instance;
            
            if (manager is null)
                throw new FlicButtonManagerNullException();

            var tcs = new TaskCompletionSource<FlicButtonDeviceData?>();

            // One attempt is a 30 second scan.
            foreach (var button in manager.Buttons)
                await UnpairButton(button.BdAddr.ToMAC());
            
            manager.StartScan(new FlicScanPairCallback(tcs));

            return await tcs.TryWaitAsync(cancellationToken);
        }

        public void SubscribeToButtonEvents(MAC mac, Action<FlicButtonEventData> flicEvent)
        {
            var manager = Flic2Manager.Instance;

            if (manager is null)
                throw new FlicButtonManagerNullException();

            var buttons = manager.Buttons;
            var button = buttons.FirstOrDefault(button => button.BdAddr.ToMAC() == mac);

            _flicButton = button ?? throw new FlicButtonNullException($"No flic button found with the mac: {mac}");
            AddButtonListenerCallback(mac, button, flicEvent);
        }

        public void ConnectButton(MAC mac)
        {
            var manager = Flic2Manager.Instance;

            if (manager is null)
                throw new FlicButtonManagerNullException();

            var buttons = manager.Buttons;
            var button = buttons.FirstOrDefault(button => button.BdAddr.ToMAC() == mac);
            if (button is null)
                throw new FlicButtonNullException($"No flic button found with the mac: {mac}");
            
            button.Connect();
        }

        public void DisconnectOrAbortPendingConnection(MAC mac)
        {
            var manager = Flic2Manager.Instance;

            if (manager is null)
                throw new FlicButtonManagerNullException();

            var buttons = manager.Buttons;
            var button = buttons.FirstOrDefault(button => button.BdAddr.ToMAC() == mac);
            if (button is null)
                throw new FlicButtonNullException($"No flic button found with the mac: {mac}");

            RemoveButtonListenerCallback(mac, button);

            button.DisconnectOrAbortPendingConnection();
        }

        public Task<bool> UnpairButton(MAC mac)
        {
            var manager = Flic2Manager.Instance;

            if (manager is null)
                throw new FlicButtonManagerNullException();

            var buttons = manager.Buttons;
            var button = buttons.FirstOrDefault(button => button.BdAddr.ToMAC() == mac);
            if (button is null)
                throw new FlicButtonNullException($"No flic button found with the mac: {mac}");

            RemoveButtonListenerCallback(mac, button);

            manager.ForgetButton(button);

            if (manager.Buttons.Contains(button))
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        private void AddButtonListenerCallback(MAC mac, Flic2Button button, Action<FlicButtonEventData> flicEvent)
        {
            var buttonListenerCallback = new FlicButtonListenerCallback(flicEvent);

            _buttonListenerCallbacks.TryAdd(mac, new List<FlicButtonListenerCallback>());
            _buttonListenerCallbacks[mac].Add(buttonListenerCallback);

            button.AddListener(buttonListenerCallback);
        }

        private void RemoveButtonListenerCallback(MAC mac, Flic2Button button)
        {
            if (_buttonListenerCallbacks.TryGetValue(mac, out var buttonListenerCallbacks) is false)
                return;

            foreach (var listenerCallback in buttonListenerCallbacks)
                button.RemoveListener(listenerCallback);

            _buttonListenerCallbacks.TryRemove(mac);
        }
    }
}
