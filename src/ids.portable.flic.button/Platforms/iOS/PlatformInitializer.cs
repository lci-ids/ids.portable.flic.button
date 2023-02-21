using IDS.Portable.Flic.Button.Platforms.Shared;

namespace IDS.Portable.Flic.Button.Platforms.iOS
{
    public static class PlatformInitializer
    {
        public static void Init()
        {
            ServiceCollection.RegisterSingleton<IFlicButtonManager, NativeFlicButtonManager>();
            ServiceCollection.Init();
        }
    }
}
