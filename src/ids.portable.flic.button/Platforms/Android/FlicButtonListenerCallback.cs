using IDS.Portable.Common;
using IO.Flic.Flic2libandroid;
using System;
using System.Collections.Generic;
using System.Text;
using IDS.Portable.Flic.Button.Platforms.Shared;

namespace IDS.Portable.Flic.Button.Platforms.Android
{
    public class FlicButtonListenerCallback : Flic2ButtonListener
    {
        private const string LogTag = "FlicButtonListenerCallback";
        private readonly Action<FlicButtonEventData> _flicEvent;
        private FlicButtonEventData _flicEventData = new ();

        public FlicButtonListenerCallback(Action<FlicButtonEventData> flicEvent)
        {
            _flicEvent += flicEvent;
        }

        public override void OnConnect(Flic2Button button)
        {
            base.OnConnect(button);
            // Connected but not necessarily ready yet.
            TaggedLog.Debug(LogTag, $"Button connected.");
        }

        public override void OnDisconnect(Flic2Button button)
        {
            base.OnDisconnect(button);
            TaggedLog.Debug(LogTag, $"Button disconnected.");
        }

        public override void OnReady(Flic2Button button, long timestamp)
        {
            base.OnReady(button, timestamp);

            // Connected and ready to go.
            TaggedLog.Debug(LogTag, $"Button connected and ready.");

            _flicEvent.Invoke(_flicEventData);
        }

        public override void OnButtonClickOrHold(Flic2Button button, bool wasQueued, bool lastQueued, long timestamp, bool isClick, bool isHold)
        {
            base.OnButtonClickOrHold(button, wasQueued, lastQueued, timestamp, isClick, isHold);
            TaggedLog.Debug(LogTag, $"Button click or hold event: isClick: {isClick} isHold: {isHold}.");

            _flicEventData.WasQueued = wasQueued;
            _flicEventData.LastQueued = lastQueued;
            _flicEventData.Timestamp = timestamp;
            _flicEventData.IsClick = isClick;
            _flicEventData.IsHold = isHold;

            _flicEvent.Invoke(_flicEventData);
        }

        public override void OnButtonSingleOrDoubleClick(Flic2Button button, bool wasQueued, bool lastQueued, long timestamp, bool isSingleClick, bool isDoubleClick)
        {
            base.OnButtonSingleOrDoubleClick(button, wasQueued, lastQueued, timestamp, isSingleClick, isDoubleClick);
            TaggedLog.Debug(LogTag, $"Button single or double click event: isSingleClick: {isSingleClick} isDoubleClick: {isDoubleClick}.");

            _flicEventData.WasQueued = wasQueued;
            _flicEventData.LastQueued = lastQueued;
            _flicEventData.Timestamp = timestamp;
            _flicEventData.IsSingleClick = isSingleClick;
            _flicEventData.IsDoubleClick = isDoubleClick;

            _flicEvent.Invoke(_flicEventData);
        }

        public override void OnButtonSingleOrDoubleClickOrHold(Flic2Button button, bool wasQueued, bool lastQueued, long timestamp, bool isSingleClick, bool isDoubleClick, bool isHold)
        {
            base.OnButtonSingleOrDoubleClickOrHold(button, wasQueued, lastQueued, timestamp, isSingleClick, isDoubleClick, isHold);
            TaggedLog.Debug(LogTag, $"Button single, double click, or hold event: isSingleClick: {isSingleClick} isDoubleClick: {isDoubleClick} isHold: {isHold}.");

            _flicEventData.WasQueued = wasQueued;
            _flicEventData.LastQueued = lastQueued;
            _flicEventData.Timestamp = timestamp;
            _flicEventData.IsSingleClick = isSingleClick;
            _flicEventData.IsDoubleClick = isDoubleClick;
            _flicEventData.IsHold = isHold;

            _flicEvent.Invoke(_flicEventData);
        }

        public override void OnButtonUpOrDown(Flic2Button button, bool wasQueued, bool lastQueued, long timestamp, bool isUp, bool isDown)
        {
            base.OnButtonUpOrDown(button, wasQueued, lastQueued, timestamp, isUp, isDown);
            TaggedLog.Debug(LogTag, $"Button up or down event: isUp: {isUp} isDown: {isDown}.");

            _flicEventData.WasQueued = wasQueued;
            _flicEventData.LastQueued = lastQueued;
            _flicEventData.Timestamp = timestamp;
            _flicEventData.IsUp = isUp;
            _flicEventData.IsDown = isDown;

            _flicEvent.Invoke(_flicEventData);
        }

        public override void OnBatteryLevelUpdated(Flic2Button button, BatteryLevel level)
        {
            base.OnBatteryLevelUpdated(button, level);
            TaggedLog.Debug(LogTag, $"Button battery level updated: level: {level.EstimatedPercentage}% .");

            _flicEventData.BatteryLevelPercent = level.EstimatedPercentage;
            _flicEventData.BatteryVoltage = level.Voltage;

            _flicEvent.Invoke(_flicEventData);
        }
    }
}
