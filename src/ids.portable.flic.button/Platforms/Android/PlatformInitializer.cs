using IDS.Portable.Flic.Button.Platforms.Shared;

namespace IDS.Portable.Flic.Button.Platforms.Android
{
    public static class PlatformInitializer
    {
        public static int NotificationIcon;
        public static void Init()
        {
            ServiceCollection.RegisterSingleton<IFlicButtonManager, NativeFlicButtonManager>();
            ServiceCollection.Init();
        }
    }
}
