using Android.Content;
using Android.OS;
using IDS.Portable.Flic.Button.Platforms.Shared;
using IO.Flic.Flic2libandroid;

namespace IDS.Portable.Flic.Button.Platforms.Android
{
    public static class PlatformInitializer
    {
        public static int NotificationIcon;
        public static void Init(Context context)
        {
            ServiceCollection.RegisterSingleton<IFlicButtonManager, NativeFlicButtonManager>();
            ServiceCollection.Init();
            
            Flic2Manager.Init(context, new Handler());
        }
    }
}
