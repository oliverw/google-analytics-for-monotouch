using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoogleAnalytics
{
    public sealed partial class AnalyticsEngine
    {
        static AnalyticsEngine current;

        public static AnalyticsEngine Current
        {
            get
            {
                if (current == null)
                {
                    current = new AnalyticsEngine(new PlatformInfoProvider());
                }
                return current;
            }
        }

        readonly PlatformInfoProvider platformTrackingInfo;
        readonly Dictionary<string, Tracker> trackers;

        private AnalyticsEngine(PlatformInfoProvider platformTrackingInfo)
        {
            trackers = new Dictionary<string, Tracker>();
            this.platformTrackingInfo = platformTrackingInfo;
        }

        public Tracker DefaultTracker { get; set; }

        public bool IsDebugEnabled { get; set; }

        public Tracker GetTracker(string propertyId)
        {
            propertyId = propertyId ?? string.Empty;
            if (!trackers.ContainsKey(propertyId))
            {
                var tracker = new Tracker(propertyId, platformTrackingInfo, this);
                trackers.Add(propertyId, tracker);
                if (DefaultTracker == null)
                {
                    DefaultTracker = tracker;
                }
                return tracker;
            }
            else
            {
                return trackers[propertyId];
            }
        }

        public void CloseTracker(Tracker tracker)
        {
            trackers.Remove(tracker.TrackingId);
            if (DefaultTracker == tracker)
            {
                DefaultTracker = null;
            }
        }

        internal void SendPayload(Payload payload)
        {
            if (!AppOptOut)
            {
                GAServiceManager.Current.SendPayload(payload);
            }
        }
    }
}
