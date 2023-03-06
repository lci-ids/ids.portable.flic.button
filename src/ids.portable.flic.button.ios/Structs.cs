using System;
using ObjCRuntime;

namespace NativeLibrary
{
    [Native]
    public enum FLICManagerState : long
    {
        Unknown = 0,
        Resetting,
        Unsupported,
        Unauthorized,
        PoweredOff,
        PoweredOn
    }

    [Native]
    public enum FLICButtonScannerErrorCode : long
    {
        Unknown = 0,
        BluetoothNotActivated,
        NoPublicButtonDiscovered,
        BLEPairingFailedPreviousPairingAlreadyExisting,
        BLEPairingFailedUserCanceled,
        BLEPairingFailedUnknownReason,
        AppCredentialsDontMatch,
        UserCanceled,
        InvalidBluetoothAddress,
        GenuineCheckFailed,
        AlreadyConnectedToAnotherDevice,
        TooManyApps,
        CouldNotSetBluetoothNotify,
        CouldNotDiscoverBluetoothServices,
        ButtonDisconnectedDuringVerification,
        ConnectionTimeout,
        FailedToEstablish,
        ConnectionLimitReached,
        InvalidVerifier,
        NotInPublicMode
    }

    [Native]
    public enum FLICButtonScannerStatusEvent : long
    {
        Discovered = 0,
        Connected,
        Verified,
        VerificationFailed
    }

    [Native]
    public enum FLICError : long
    {
        Unknown = 0,
        NotConfigured,
        CouldNotDiscoverBluetoothServices,
        VerificationSignatureMismatch,
        InvalidUuid,
        GenuineCheckFailed,
        TooManyApps,
        Unpaired,
        UnsupportedOSVersion,
        AlreadyForgotten
    }

    [Native]
    public enum FLICButtonState : long
    {
        Disconnected = 0,
        Connecting,
        Connected,
        Disconnecting
    }

    [Native]
    public enum FLICButtonTriggerMode : long
    {
        AndHold = 0,
        AndDoubleClick,
        AndDoubleClickAndHold,
        FLICButtonTriggerModeClick
    }

    [Native]
    public enum FLICLatencyMode : long
    {
        Normal = 0,
        Low
    }

}

