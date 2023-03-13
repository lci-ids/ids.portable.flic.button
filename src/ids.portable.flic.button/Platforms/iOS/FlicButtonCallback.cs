using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using IDS.Portable.Flic.Button.Platforms.Shared;
using IDS.Portable.Common;
using System.Collections;

namespace IDS.Portable.Flic.Button.Platforms.iOS
{
    public class FlicButtonCallback : FLICButtonDelegate
    {
        private const string LogTag = "FlicButtonCallback";
        private readonly Action<FlicButtonEventData> _flicEvent;
        private FlicButtonEventData _flicEventData = new();

        public FlicButtonCallback(Action<FlicButtonEventData> flicEvent)
        {
            _flicEvent += flicEvent;
        }

        public override void ButtonDidConnect(FLICButton button)
        {
            base.ButtonDidConnect(button);
            // Connected but not necessarily ready yet.
            TaggedLog.Debug(LogTag, $"Button connected.");
        }

        public override void ButtonIsReady(FLICButton button)
        {
            base.ButtonIsReady(button);

            // Connected and ready to go.
            TaggedLog.Debug(LogTag, $"Button connected and ready.");

            _flicEventData.Connected = true;

            _flicEvent.Invoke(_flicEventData);
        }

        // TODO: What does this error map to?
        public override void ButtonDidDisconnectWithError(FLICButton button, NSError? error)
        {
            base.ButtonDidDisconnectWithError(button, error);
            TaggedLog.Debug(LogTag, $"Button disconnected.");

            _flicEventData.Connected = false;

            _flicEvent.Invoke(_flicEventData);
        }

        public override void ButtonDidFailToConnectWithError(FLICButton button, NSError? error)
        {
            base.ButtonDidFailToConnectWithError(button, error);
            TaggedLog.Debug(LogTag, $"Button failed to connect.");

            _flicEventData.Connected = false;

            _flicEvent.Invoke(_flicEventData);
        }

        // TODO: Finish everything below.
        public override void ButtonDidReceiveButtonDown(FLICButton button, bool queued, int age)
        {
            base.ButtonDidReceiveButtonDown(button, queued, age);
            TaggedLog.Debug(LogTag, $"ButtonDidReceiveButtonDown: .");

            _flicEventData.WasQueued = wasQueued;
            _flicEventData.LastQueued = lastQueued;
            _flicEventData.Timestamp = timestamp;
            _flicEventData.IsClick = isClick;
            _flicEventData.IsHold = isHold;

            _flicEvent.Invoke(_flicEventData);
        }


        public override void ButtonDidReceiveButtonUp(FLICButton button, bool queued, int age)
        {
            base.ButtonDidReceiveButtonUp(button, queued, age);
            TaggedLog.Debug(LogTag, $"ButtonDidReceiveButtonUp: .");

            _flicEventData.WasQueued = wasQueued;
            _flicEventData.LastQueued = lastQueued;
            _flicEventData.Timestamp = timestamp;
            _flicEventData.IsClick = isClick;
            _flicEventData.IsHold = isHold;

            _flicEvent.Invoke(_flicEventData);
        }

        public override void ButtonDidReceiveButtonClick(FLICButton button, bool queued, int age)
        {
            base.ButtonDidReceiveButtonClick(button, queued, age);
            TaggedLog.Debug(LogTag, $"ButtonDidReceiveButtonClick: .");

            _flicEventData.WasQueued = wasQueued;
            _flicEventData.LastQueued = lastQueued;
            _flicEventData.Timestamp = timestamp;
            _flicEventData.IsClick = isClick;
            _flicEventData.IsHold = isHold;

            _flicEvent.Invoke(_flicEventData);
        }

        public override void ButtonDidReceiveButtonDoubleClick(FLICButton button, bool queued, int age)
        {
            base.ButtonDidReceiveButtonDoubleClick(button, queued, age);
            TaggedLog.Debug(LogTag, $"ButtonDidReceiveButtonDoubleClick: .");

            _flicEventData.WasQueued = wasQueued;
            _flicEventData.LastQueued = lastQueued;
            _flicEventData.Timestamp = timestamp;
            _flicEventData.IsClick = isClick;
            _flicEventData.IsHold = isHold;

            _flicEvent.Invoke(_flicEventData);
        }

        public override void ButtonDidReceiveButtonHold(FLICButton button, bool queued, int age)
        {
            base.ButtonDidReceiveButtonHold(button, queued, age);
            TaggedLog.Debug(LogTag, $"ButtonDidReceiveButtonHold: .");

            _flicEventData.WasQueued = wasQueued;
            _flicEventData.LastQueued = lastQueued;
            _flicEventData.Timestamp = timestamp;
            _flicEventData.IsClick = isClick;
            _flicEventData.IsHold = isHold;

            _flicEvent.Invoke(_flicEventData);
        }

        public override void ButtonDidUnpairWithError(FLICButton button, NSError? error)
        {
            base.ButtonDidUnpairWithError(button, error);
            TaggedLog.Debug(LogTag, $"ButtonDidUnpairWithError: .");

            _flicEventData.WasQueued = wasQueued;
            _flicEventData.LastQueued = lastQueued;
            _flicEventData.Timestamp = timestamp;
            _flicEventData.IsClick = isClick;
            _flicEventData.IsHold = isHold;

            _flicEvent.Invoke(_flicEventData);
        }

        public override void ButtonDidUpdateBatteryVoltage(FLICButton button, float voltage)
        {
            base.ButtonDidUpdateBatteryVoltage(button, voltage);
            TaggedLog.Debug(LogTag, $"ButtonDidUpdateBatteryVoltage: .");

            _flicEventData.WasQueued = wasQueued;
            _flicEventData.LastQueued = lastQueued;
            _flicEventData.Timestamp = timestamp;
            _flicEventData.IsClick = isClick;
            _flicEventData.IsHold = isHold;

            _flicEvent.Invoke(_flicEventData);
        }

        public override void ButtonDidUpdateNickname(FLICButton button, string nickname)
        {
            base.ButtonDidReceiveButtonDown(button, nickname);
            TaggedLog.Debug(LogTag, $"ButtonDidReceiveButtonDown: .");

            _flicEventData.WasQueued = wasQueued;
            _flicEventData.LastQueued = lastQueued;
            _flicEventData.Timestamp = timestamp;
            _flicEventData.IsClick = isClick;
            _flicEventData.IsHold = isHold;

            _flicEvent.Invoke(_flicEventData);
        }
    }
}
