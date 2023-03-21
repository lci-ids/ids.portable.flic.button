using FlicLibraryIos;
using IDS.Portable.Common;
using System;

namespace IDS.Portable.Flic.Button.Platforms.iOS
{
    public class FlicManagerCallback : FLICManagerDelegate
    {
        private const string LogTag = "FlicManagerCallback";
        private readonly Action<bool> _flicManagerEvent;

        public FlicManagerCallback(Action<bool> flicManagerEvent)
        {
            _flicManagerEvent += flicManagerEvent;
        }

        public override void Manager(FLICManager manager, FLICManagerState state)
        {
            TaggedLog.Debug(LogTag, $"FLICManagerState change, current state: {state}.");
            // This is used to track changes to bluetooth being enabled/disabled on the phone. This should already be
            // handled in any app using this library as they're all communicating with BLE devices.
        }

        /// <summary>
        /// This is called once the manager has been restored. This means that all the FLICButton instances from your previous session
        /// are restored as well. After this method has been called you may start using the manager and communicate with the Flic buttons.
        /// This method will only be called once on each application launch.
        /// </summary>
        /// <param name="manager">The manager instance that the event originates from. Since this is intended to be used as a singleton,
        /// there should only ever be one manager instance.</param>
        public override void ManagerDidRestoreState(FLICManager manager)
        {
            TaggedLog.Debug(LogTag, $"Flic manager did restore state.");
            _flicManagerEvent.Invoke(true);
        }
    }
}
