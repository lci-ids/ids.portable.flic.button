using IDS.Portable.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IDS.Portable.Flic.Button.Platforms.Shared
{
    /// <summary>
    /// FlicButtonManager is a singleton, but the Android library does need to be initialized in OnCreate to pass in
    /// the application context, along with a handler. An example of this is:
    /// 
    /// Flic2Manager.Init(Android.App.Application.Context, new Handler());
    /// </summary>
    public class FlicButtonManager : Singleton<FlicButtonManager>, IFlicButtonManager
    {
        private readonly IFlicButtonManager _nativeFlicButtonManager;

        private FlicButtonManager()
        {
            _nativeFlicButtonManager = ServiceCollection.Resolve<IFlicButtonManager>();
        }

        public NativeFlicButtonPlatform Platform => _nativeFlicButtonManager.Platform;

        /// <summary>
        /// Scans for and pairs a flic button that is in pairing mode. The timeout on the scan is 30 seconds.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>A struct FlicButtonDeviceData that contains data about the device that was paired, or null if
        /// no device was paired.</returns>
        public async Task<FlicButtonDeviceData?> ScanAndPairButton(CancellationToken cancellationToken) =>
            await _nativeFlicButtonManager.ScanAndPairButton(cancellationToken);

        /// <summary>
        /// Subscribes to a flic button's events with an Action that returns FlicButtonEventData information about the events.
        /// </summary>
        /// <param name="serialNumber">The serial number of the device to subscribe to.</param>
        /// <param name="cancellationToken"></param>
        public void SubscribeToButtonEvents(string serialNumber, Action<FlicButtonEventData> flicEvent) =>
        _nativeFlicButtonManager.SubscribeToButtonEvents(serialNumber, flicEvent);

        /// <summary>
        /// Attempts to connect to a flic button with the given serial number. Returns connection status via SubscribeToButtonEvents.
        /// </summary>
        /// <param name="serialNumber">The serial number of the device to connect to.</param>
        public void ConnectButton(string serialNumber) =>
            _nativeFlicButtonManager.ConnectButton(serialNumber);

        /// <summary>
        /// Disconnects or aborts a pending connection to a flic button with the given serial number.
        /// </summary>
        /// <param name="serialNumber">The serial number of the device to disconnect from.</param>
        public void DisconnectOrAbortPendingConnection(string serialNumber) =>
            _nativeFlicButtonManager.DisconnectOrAbortPendingConnection(serialNumber);
    }
}
