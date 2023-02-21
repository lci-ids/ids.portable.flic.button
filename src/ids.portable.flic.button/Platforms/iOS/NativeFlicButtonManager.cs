using IDS.Portable.Flic.Button.Platforms.Shared;
using System;
using System.Linq;
using IDS.Portable.Common;
using System.Threading.Tasks;
using System.Threading;

namespace IDS.Portable.Flic.Button.Platforms.iOS
{
    internal class NativeFlicButtonManager : IFlicButtonManager
    {
        public NativeFlicButtonPlatform Platform => NativeFlicButtonPlatform.Ios;
        public async Task<FlicButtonDeviceData?> ScanAndPairButton(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void SubscribeToButtonEvents(string serialNumber, Action<FlicButtonEventData> flicEvent)
        {
            throw new NotImplementedException();
        }

        public void ConnectButton(string serialNumber)
        {
            throw new NotImplementedException();
        }

        public void DisconnectOrAbortPendingConnection(string serialNumber)
        {
            throw new NotImplementedException();
        }
    }
}
