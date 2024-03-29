﻿using Foundation;
using System;
using IDS.Portable.Flic.Button.Platforms.Shared;
using IDS.Portable.Common;
using FlicLibraryIos;

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
            // Connected but not necessarily ready yet.
            TaggedLog.Debug(LogTag, $"Flic button connected but not yet ready.");
        }

        public override void ButtonIsReady(FLICButton button)
        {
            // Connected and ready to go.
            TaggedLog.Debug(LogTag, $"Flic button connected and ready.");

            _flicEventData.Connected = true;

            _flicEvent.Invoke(_flicEventData);
        }

        public override void ButtonDidDisconnectWithError(FLICButton button, NSError? error)
        {
            TaggedLog.Debug(LogTag, $"Button disconnected with error: {error}.");

            _flicEventData.Connected = false;

            _flicEvent.Invoke(_flicEventData);
        }

        public override void ButtonDidFailToConnectWithError(FLICButton button, NSError? error)
        {
            TaggedLog.Debug(LogTag, $"Button failed to connect: {error}.");

            _flicEventData.Connected = false;

            _flicEvent.Invoke(_flicEventData);
        }

        public override void ButtonDidReceiveButtonClick(FLICButton button, bool queued, nint age)
        {
            TaggedLog.Debug(LogTag, $"ButtonDidReceiveButtonClick.");

            UpdateButtonState(button);

            _flicEventData.WasQueued = queued;
            _flicEventData.Timestamp = age;
            _flicEventData.IsSingleClick = true;
            _flicEventData.IsDoubleClick = false;

            _flicEvent.Invoke(_flicEventData);
        }

        public override void ButtonDidReceiveButtonDoubleClick(FLICButton button, bool queued, nint age)
        {
            TaggedLog.Debug(LogTag, $"ButtonDidReceiveButtonDoubleClick.");

            UpdateButtonState(button);

            _flicEventData.WasQueued = queued;
            _flicEventData.Timestamp = age;
            _flicEventData.IsDoubleClick = true;
            _flicEventData.IsSingleClick = false;

            _flicEvent.Invoke(_flicEventData);
        }

        public override void ButtonDidReceiveButtonDown(FLICButton button, bool queued, nint age)
        {
            TaggedLog.Debug(LogTag, $"ButtonDidReceiveButtonDown.");

            UpdateButtonState(button);

            _flicEventData.WasQueued = queued;
            _flicEventData.Timestamp = age;
            _flicEventData.IsDown = true;
            _flicEventData.IsUp = false;

            _flicEvent.Invoke(_flicEventData);
        }

        public override void ButtonDidReceiveButtonHold(FLICButton button, bool queued, nint age)
        {
            TaggedLog.Debug(LogTag, $"ButtonDidReceiveButtonHold.");

            UpdateButtonState(button);

            _flicEventData.WasQueued = queued;
            _flicEventData.Timestamp = age;
            _flicEventData.IsHold = true;

            _flicEvent.Invoke(_flicEventData);
        }

        public override void ButtonDidReceiveButtonUp(FLICButton button, bool queued, nint age)
        {
            TaggedLog.Debug(LogTag, $"ButtonDidReceiveButtonUp.");

            UpdateButtonState(button);

            _flicEventData.WasQueued = queued;
            _flicEventData.Timestamp = age;
            _flicEventData.IsDown = false;
            _flicEventData.IsHold = false;
            _flicEventData.IsUp = true;

            _flicEvent.Invoke(_flicEventData);
        }

        public override void ButtonDidUnpairWithError(FLICButton button, NSError? error)
        {
            TaggedLog.Debug(LogTag, $"ButtonDidUnpairWithError: {error}.");

            _flicEventData.Connected = false;

            _flicEvent.Invoke(_flicEventData);
        }

        public override void ButtonDidUpdateBatteryVoltage(FLICButton button, float voltage)
        {
            TaggedLog.Debug(LogTag, $"ButtonDidUpdateBatteryVoltage: {voltage}.");

            UpdateButtonState(button);

            _flicEventData.BatteryVoltage = voltage;

            _flicEvent.Invoke(_flicEventData);
        }

        public override void ButtonDidUpdateNickname(FLICButton button, string nickname)
        {
            TaggedLog.Debug(LogTag, $"ButtonDidUpdateNickname: {nickname}.");

            UpdateButtonState(button);

            _flicEvent.Invoke(_flicEventData);
        }

        private void UpdateButtonState(FLICButton button)
        {
            // For the properties below, we can't rely on specific events because we can easily miss
            // them by not having the button listener setup, so we need to set them using the current button state.
            //
            _flicEventData.Connected = button.State is FLICButtonState.Connected;
            _flicEventData.BatteryVoltage = button.BatteryVoltage;
        }
    }
}
