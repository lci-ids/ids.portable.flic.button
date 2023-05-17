using IDS.Core.IDS_CAN;
using IDS.Portable.Common;
using IDS.Portable.Common.Utils;
using IDS.Portable.LogicalDevice;
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
        private const string LogTag = "FlicButtonManager";
        private const int MaxFlicLibraryInitTimeMs = 1000;

        private FlicButtonManager()
        {
            _nativeFlicButtonManager = ServiceCollection.Resolve<IFlicButtonManager>();
        }

        public NativeFlicButtonPlatform Platform => _nativeFlicButtonManager.Platform;

        /// <summary>
        /// Initializes the native flic library and throws if unsuccessful.
        /// </summary>
        public async Task Init()
        {
            try
            {
                await _nativeFlicButtonManager.Init();
            }
            catch (Exception e)
            {
                TaggedLog.Debug(LogTag, $"Unable to initialize Flic Library: {e}");
            }
        }


        /// <summary>
        /// Scans for and pairs a flic button that is in pairing mode. The timeout on the scan is 30 seconds.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>A struct FlicButtonDeviceData that contains data about the device that was paired, or null if
        /// no device was paired.</returns>
        public async Task<FlicButtonDeviceData?> ScanAndPairButton(CancellationToken cancellationToken) =>
            await _nativeFlicButtonManager.ScanAndPairButton(cancellationToken);
        
        
        public bool IsConnected => _nativeFlicButtonManager.IsConnected;


        /// <summary>
        /// Subscribes to a flic button's events with an Action that returns FlicButtonEventData information about the events.
        /// </summary>
        /// <param name="serialNumber">The serial number of the device to subscribe to.</param>
        /// <param name="cancellationToken"></param>
        public void SubscribeToButtonEvents(MAC mac, Action<FlicButtonEventData> flicEvent) =>
        _nativeFlicButtonManager.SubscribeToButtonEvents(mac, flicEvent);

        /// <summary>
        /// Attempts to connect to a flic button with the given serial number. Returns connection status via SubscribeToButtonEvents.
        /// </summary>
        /// <param name="serialNumber">The serial number of the device to connect to.</param>
        public void ConnectButton(MAC mac) =>
            _nativeFlicButtonManager.ConnectButton(mac);

        /// <summary>
        /// Disconnects or aborts a pending connection to a flic button with the given serial number.
        /// </summary>
        /// <param name="serialNumber">The serial number of the device to disconnect from.</param>
        public void DisconnectOrAbortPendingConnection(MAC mac) =>
            _nativeFlicButtonManager.DisconnectOrAbortPendingConnection(mac);

        /// <summary>
        /// Completely removes a button from the flic manager. It is important to call this before we finish removing the device
        /// from the device layer.
        /// </summary>
        /// <param name="serialNumber">The serial number of the device to unpair from.</param>
        /// <returns>True if removing the button was successful.</returns>
        public async Task<bool> UnpairButton(MAC mac) =>
            await _nativeFlicButtonManager.UnpairButton(mac);
    }
}
