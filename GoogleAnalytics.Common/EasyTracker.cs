using System;
using System.Xml;
using UIKit;
using System.Windows;
using System.Threading.Tasks;

namespace GoogleAnalytics
{
    public sealed partial class EasyTracker
    {
        static EasyTracker current;
        static Tracker tracker;
        DateTime? suspended;

        public string ConfigPath { get; set; }
        public EasyTrackerConfig Config { get; set; }

        public static EasyTracker Current
        {
            get
            {
                if (current == null)
                {
                    current = new EasyTracker();
                }
                return current;
            }
        }

        public static Tracker GetTracker()
        {
            if (tracker == null)
            {
                Current.Init();
            }
            return tracker;
        }

        private void InitTracker()
        {
            var analyticsEngine = AnalyticsEngine.Current;
            analyticsEngine.IsDebugEnabled = Config.Debug;
            GAServiceManager.Current.DispatchPeriod = Config.DispatchPeriod;
            tracker = analyticsEngine.GetTracker(Config.TrackingId);
            tracker.SetStartSession(Config.SessionTimeout.HasValue);
            tracker.IsUseSecure = Config.UseSecure;
            tracker.AppName = Config.AppName;
            tracker.AppVersion = Config.AppVersion;
            tracker.IsAnonymizeIpEnabled = Config.AnonymizeIp;
            tracker.SampleRate = Config.SampleFrequency;
        }

        private void InitConfig(XmlReader reader)
        {
            Config = EasyTrackerConfig.Load(reader);
            Config.Validate();
        }

        public Task Dispatch()
        {
            return GAServiceManager.Current.Dispatch();
        }
    }
}
