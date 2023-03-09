using System;
using ObjCRuntime;
using Foundation;
using UIKit;
using ObjCRuntime;

namespace FLICButtoniOS
{
    [Static]
    interface Constants
    {
        // extern NSString *const FLICErrorDomain;
        [Field("FLICErrorDomain", "__Internal")]
        NSString FLICErrorDomain { get; }

        // extern NSString *const FLICButtonScannerErrorDomain;
        [Field("FLICButtonScannerErrorDomain", "__Internal")]
        NSString FLICButtonScannerErrorDomain { get; }

        //// extern double flic2libVersionNumber;
        //[Field("flic2libVersionNumber", "__Internal")]
        //double flic2libVersionNumber { get; }

        //// extern const unsigned char[] flic2libVersionString;
        //[Field("flic2libVersionString", "__Internal")]
        //byte[] flic2libVersionString { get; }
    }

    // @interface FLICButton : NSObject
    [BaseType(typeof(NSObject))]
    interface FLICButton
    {
        // @property (readonly, nonatomic, strong) NSUUID * _Nonnull identifier;
        [Export("identifier", ArgumentSemantic.Strong)]
        NSUuid Identifier { get; }

        [Wrap("WeakDelegate")]
        [NullAllowed]
        FLICButtonDelegate Delegate { get; set; }

        // @property (nonatomic, weak) id<FLICButtonDelegate> _Nullable delegate;
        [NullAllowed, Export("delegate", ArgumentSemantic.Weak)]
        NSObject WeakDelegate { get; set; }

        // @property (readonly, nonatomic, strong) NSString * _Nullable name;
        [NullAllowed, Export("name", ArgumentSemantic.Strong)]
        string Name { get; }

        // @property (readwrite, nonatomic, strong) NSString * _Nullable nickname;
        [NullAllowed, Export("nickname", ArgumentSemantic.Strong)]
        string Nickname { get; set; }

        // @property (readonly, nonatomic, strong) NSString * _Nonnull bluetoothAddress;
        [Export("bluetoothAddress", ArgumentSemantic.Strong)]
        string BluetoothAddress { get; }

        // @property (readonly, nonatomic, strong) NSString * _Nonnull uuid;
        [Export("uuid", ArgumentSemantic.Strong)]
        string Uuid { get; }

        // @property (readonly, nonatomic, strong) NSString * _Nonnull serialNumber;
        [Export("serialNumber", ArgumentSemantic.Strong)]
        string SerialNumber { get; }

        // @property (readwrite, nonatomic) FLICButtonTriggerMode triggerMode;
        [Export("triggerMode", ArgumentSemantic.Assign)]
        FLICButtonTriggerMode TriggerMode { get; set; }

        // @property (readonly, nonatomic) FLICButtonState state;
        [Export("state")]
        FLICButtonState State { get; }

        // @property (readonly, nonatomic) uint32_t pressCount;
        [Export("pressCount")]
        uint PressCount { get; }

        // @property (readonly, nonatomic) uint32_t firmwareRevision;
        [Export("firmwareRevision")]
        uint FirmwareRevision { get; }

        // @property (readonly, nonatomic) BOOL isReady;
        [Export("isReady")]
        bool IsReady { get; }

        // @property (readonly, nonatomic) float batteryVoltage;
        [Export("batteryVoltage")]
        float BatteryVoltage { get; }

        // @property (readonly, nonatomic) BOOL isUnpaired;
        [Export("isUnpaired")]
        bool IsUnpaired { get; }

        // @property (readwrite, nonatomic) FLICLatencyMode latencyMode;
        [Export("latencyMode", ArgumentSemantic.Assign)]
        FLICLatencyMode LatencyMode { get; set; }

        // -(void)connect;
        [Export("connect")]
        void Connect();

        // -(void)disconnect;
        [Export("disconnect")]
        void Disconnect();
    }

    // @protocol FLICButtonDelegate <NSObject>
    [BaseType(typeof(NSObject))]
    [Model]
    interface FLICButtonDelegate
    {
        // @required -(void)buttonDidConnect:(FLICButton * _Nonnull)button;
        [Abstract]
        [Export("buttonDidConnect:")]
        void ButtonDidConnect(FLICButton button);

        // @required -(void)buttonIsReady:(FLICButton * _Nonnull)button;
        [Abstract]
        [Export("buttonIsReady:")]
        void ButtonIsReady(FLICButton button);

        // @required -(void)button:(FLICButton * _Nonnull)button didDisconnectWithError:(NSError * _Nullable)error;
        [Abstract]
        [Export("button:didDisconnectWithError:")]
        void ButtonDidDisconnectWithError(FLICButton button, [NullAllowed] NSError error);

        // @required -(void)button:(FLICButton * _Nonnull)button didFailToConnectWithError:(NSError * _Nullable)error;
        [Abstract]
        [Export("button:didFailToConnectWithError:")]
        void ButtonDidFailToConnectWithError(FLICButton button, [NullAllowed] NSError error);

        // @optional -(void)button:(FLICButton * _Nonnull)button didReceiveButtonDown:(BOOL)queued age:(NSInteger)age;
        [Export("button:didReceiveButtonDown:age:")]
        void Button(FLICButton button, bool queued, nint age);

        // @optional -(void)button:(FLICButton * _Nonnull)button didReceiveButtonUp:(BOOL)queued age:(NSInteger)age;
        [Export("button:didReceiveButtonUp:age:")]
        void ButtonDidReceiveButtonUp(FLICButton button, bool queued, nint age);

        // @optional -(void)button:(FLICButton * _Nonnull)button didReceiveButtonClick:(BOOL)queued age:(NSInteger)age;
        [Export("button:didReceiveButtonClick:age:")]
        void ButtonDidReceiveButtonClick(FLICButton button, bool queued, nint age);

        // @optional -(void)button:(FLICButton * _Nonnull)button didReceiveButtonDoubleClick:(BOOL)queued age:(NSInteger)age;
        [Export("button:didReceiveButtonDoubleClick:age:")]
        void ButtonDidReceiveButtonDoubleClick(FLICButton button, bool queued, nint age);

        // @optional -(void)button:(FLICButton * _Nonnull)button didReceiveButtonHold:(BOOL)queued age:(NSInteger)age;
        [Export("button:didReceiveButtonHold:age:")]
        void ButtonDidReceiveButtonHold(FLICButton button, bool queued, nint age);

        // @optional -(void)button:(FLICButton * _Nonnull)button didUnpairWithError:(NSError * _Nullable)error;
        [Export("button:didUnpairWithError:")]
        void ButtonDidUnpairWithError(FLICButton button, [NullAllowed] NSError error);

        // @optional -(void)button:(FLICButton * _Nonnull)button didUpdateBatteryVoltage:(float)voltage;
        [Export("button:didUpdateBatteryVoltage:")]
        void ButtonDidUpdateBatteryVoltage(FLICButton button, float voltage);

        // @optional -(void)button:(FLICButton * _Nonnull)button didUpdateNickname:(NSString * _Nonnull)nickname;
        [Export("button:didUpdateNickname:")]
        void ButtonDidUpdateNickname(FLICButton button, string nickname);
    }

    // @interface FLICManager : NSObject
    [BaseType(typeof(NSObject))]
    interface FLICManager
    {
        [Wrap("WeakDelegate")]
        [NullAllowed]
        FLICManagerDelegate Delegate { get; set; }

        // @property (nonatomic, weak) id<FLICManagerDelegate> _Nullable delegate;
        [NullAllowed, Export("delegate", ArgumentSemantic.Weak)]
        NSObject WeakDelegate { get; set; }

        [Wrap("WeakButtonDelegate")]
        [NullAllowed]
        FLICButtonDelegate ButtonDelegate { get; set; }

        // @property (nonatomic, weak) id<FLICButtonDelegate> _Nullable buttonDelegate;
        [NullAllowed, Export("buttonDelegate", ArgumentSemantic.Weak)]
        NSObject WeakButtonDelegate { get; set; }

        // @property (readonly) FLICManagerState state;
        [Export("state")]
        FLICManagerState State { get; }

        // @property (readonly, nonatomic) BOOL isScanning;
        [Export("isScanning")]
        bool IsScanning { get; }

        // +(instancetype _Nullable)sharedManager;
        [Static]
        [Export("sharedManager")]
        [return: NullAllowed]
        FLICManager SharedManager();

        // +(instancetype _Nullable)configureWithDelegate:(id<FLICManagerDelegate> _Nullable)delegate buttonDelegate:(id<FLICButtonDelegate> _Nullable)buttonDelegate background:(BOOL)background;
        [Static]
        [Export("configureWithDelegate:buttonDelegate:background:")]
        [return: NullAllowed]
        FLICManager ConfigureWithDelegate([NullAllowed] FLICManagerDelegate @delegate, [NullAllowed] FLICButtonDelegate buttonDelegate, bool background);

        // -(NSArray<FLICButton *> * _Nonnull)buttons;
        [Export("buttons")]
        FLICButton[] Buttons { get; }

        // -(void)forgetButton:(FLICButton * _Nonnull)button completion:(void (^ _Nonnull)(NSUUID * _Nonnull, NSError * _Nullable))completion;
        [Export("forgetButton:completion:")]
        void ForgetButton(FLICButton button, Action<NSUuid, NSError> completion);

        // -(void)scanForButtonsWithStateChangeHandler:(void (^ _Nonnull)(FLICButtonScannerStatusEvent))stateHandler completion:(void (^ _Nonnull)(FLICButton * _Nullable, NSError * _Nullable))completion;
        [Export("scanForButtonsWithStateChangeHandler:completion:")]
        void ScanForButtonsWithStateChangeHandler(Action<FLICButtonScannerStatusEvent> stateHandler, Action<FLICButton, NSError> completion);

        // -(void)stopScan;
        [Export("stopScan")]
        void StopScan();
    }

    // @protocol FLICManagerDelegate <NSObject>
    [BaseType(typeof(NSObject))]
    [Model]
    interface FLICManagerDelegate
    {
        // @required -(void)managerDidRestoreState:(FLICManager * _Nonnull)manager;
        [Abstract]
        [Export("managerDidRestoreState:")]
        void ManagerDidRestoreState(FLICManager manager);

        // @required -(void)manager:(FLICManager * _Nonnull)manager didUpdateState:(FLICManagerState)state;
        [Abstract]
        [Export("manager:didUpdateState:")]
        void Manager(FLICManager manager, FLICManagerState state);
    }
}

