using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using MonoTouch.UIKit;
using System.IO;
using MonoTouch.Foundation;

namespace GoogleAnalytics
{
    public sealed partial class EasyTracker
    {
        private EasyTracker()
        {
            ConfigPath = "analytics.xml";
        }

        System.Threading.Timer networkStatusUpdateTimer;

        private void InitConfig(string configPath)
        {
            var absolutePath = Path.Combine(NSBundle.MainBundle.ResourcePath, configPath);

            using (var stream = File.OpenRead(absolutePath))
            {
                using (var reader = XmlReader.Create(stream))
                {
                    InitConfig(reader);
                }
            }
        }

        void Init()
        {
            if (Config == null) InitConfig(ConfigPath);
            PopulateMissingConfig();

            if (Config.AutoTrackNetworkConnectivity)
            {
                UpdateConnectionStatus();

                networkStatusUpdateTimer = new System.Threading.Timer(_OnTimer, this, 0, 30000);
            }

            InitTracker();
        }

        void PopulateMissingConfig()
        {
            if (string.IsNullOrEmpty(Config.AppName))
            {
                var infoDictionary = NSBundle.MainBundle.InfoDictionary;
                Config.AppName = infoDictionary.ObjectForKey(new NSString("CFBundleDisplayName")).ToString();
            }
            if (string.IsNullOrEmpty(Config.AppVersion))
            {
                var infoDictionary = NSBundle.MainBundle.InfoDictionary;
                Config.AppVersion = infoDictionary.ObjectForKey(new NSString("CFBundleShortVersionString")).ToString();
            }
        }

        public void OnApplicationActivated(object sender)
        {
            if (suspended.HasValue && Config.SessionTimeout.HasValue)
            {
                var suspendedAgo = DateTime.UtcNow.Subtract(suspended.Value);
                if (suspendedAgo > Config.SessionTimeout.Value)
                {
                    tracker.SetStartSession(true);
                }
            }
        }

        public async void OnApplicationDeactivated(object sender)
        {
            if (Config.AutoAppLifetimeTracking)
            {
                tracker.SendEvent("app", "suspend", null, 0);
            }

            suspended = DateTime.UtcNow;
            await Dispatch(); // there is no way to get a deferral in WP so this will not actually happen until after we return to the app
        }

        static void _OnTimer(object state)
        {
            NSThread.Current.InvokeOnMainThread(() => ((EasyTracker)state).OnTimer());
        }

        void OnTimer()
        {
            UpdateConnectionStatus();
        }

        private async static void UpdateConnectionStatus()
        {
            GAServiceManager.Current.IsConnected = await Reachability.IsHostReachableAsync("www.google.com");
        }

        bool reportingException = false;

        async public void OnApplicationUnhandledException(Exception ex)
        {
            if (!reportingException)
            {
                reportingException = true;
                try
                {
                    tracker.SendException(ex.ToString(), true);
                    await Dispatch();
                    // rethrow the exception now that we're done logging it. wrap in another exception in order to prevent stack trace from getting reset.
                    throw new Exception("Tracked exception rethrown", ex);
                }
                finally
                {
                    // we have to do some trickery in order to make sure the flag is reset only after the new exception has passed all the way through the UE pipeline. Otherwise we would have an infinite loop.
                    NSThread.Current.InvokeOnMainThread(async () =>
                    {
                        await Task.Yield();
                        reportingException = false;
                    });
                }
            }
        }

    }
}
