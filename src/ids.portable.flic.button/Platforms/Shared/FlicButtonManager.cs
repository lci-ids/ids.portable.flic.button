using IDS.Portable.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IDS.Portable.Flic.Button.Platforms.Shared
{
    public class FlicButtonManager : Singleton<FlicButtonManager>, IFlicButtonManager
    {
        private readonly IFlicButtonManager _nativeFlicButtonManager;

        private FlicButtonManager()
        {
            _nativeFlicButtonManager = ServiceCollection.Resolve<IFlicButtonManager>();
        }

        public NativeFlicButtonPlatform Platform => _nativeFlicButtonManager.Platform;

        public async Task<FlicButtonDeviceData?> ScanAndPairButton(CancellationToken cancellationToken) =>
            await _nativeFlicButtonManager.ScanAndPairButton(cancellationToken);

        public void SubscribeToButtonEvents(string serialNumber, Action<FlicButtonEventData> flicEvent) =>
        _nativeFlicButtonManager.SubscribeToButtonEvents(serialNumber, flicEvent);

        public void ConnectButton(string serialNumber) =>
            _nativeFlicButtonManager.ConnectButton(serialNumber);

        public void DisconnectOrAbortPendingConnection(string serialNumber) =>
            _nativeFlicButtonManager.DisconnectOrAbortPendingConnection(serialNumber);
    }
}
