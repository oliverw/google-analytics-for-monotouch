using System;
using System.Collections.Generic;
using System.Drawing;


#if NETFX_CORE
using Windows.Foundation;
#else
using System.Windows;
#endif

namespace GoogleAnalytics
{
    public sealed class Tracker
    {
        readonly PayloadFactory engine;
        readonly PlatformInfoProvider platformInfoProvider;
        readonly TokenBucket hitTokenBucket;
        readonly AnalyticsEngine analyticsEngine;

        internal Tracker(string propertyId, PlatformInfoProvider platformInfoProvider, AnalyticsEngine analyticsEngine)
        {
            this.analyticsEngine = analyticsEngine;
            this.platformInfoProvider = platformInfoProvider;
            engine = new PayloadFactory()
            {
                PropertyId = propertyId,
                AnonymousClientId = platformInfoProvider.AnonymousClientId,
                DocumentEncoding = platformInfoProvider.DocumentEncoding,
                ScreenColorDepthBits = platformInfoProvider.ScreenColorDepthBits,
                ScreenResolution = platformInfoProvider.ScreenResolution,
                UserLanguage = platformInfoProvider.UserLanguage,
                ViewportSize = platformInfoProvider.ViewPortResolution
            };
            platformInfoProvider.ViewPortResolutionChanged += platformTrackingInfo_ViewPortResolutionChanged;
            platformInfoProvider.ScreenResolutionChanged += platformTrackingInfo_ScreenResolutionChanged;
            SampleRate = 100.0F;
            hitTokenBucket = new TokenBucket(60, .5);
        }

        public void SetCustomDimension(int index, string value)
        {
            engine.CustomDimensions[index] = value;
        }

        public void SetCustomMetric(int index, int value)
        {
            engine.CustomMetrics[index] = value;
        }

#if NETFX_CORE
        private void platformTrackingInfo_ViewPortResolutionChanged(object sender, object args)
#else
        private void platformTrackingInfo_ViewPortResolutionChanged(object sender, EventArgs args)
#endif
        {
            engine.ViewportSize = platformInfoProvider.ViewPortResolution;
        }

#if NETFX_CORE
        private void platformTrackingInfo_ScreenResolutionChanged(object sender, object args)
#else
        private void platformTrackingInfo_ScreenResolutionChanged(object sender, EventArgs args)
#endif
        {
            engine.ScreenResolution = platformInfoProvider.ScreenResolution;
        }

        public string TrackingId
        {
            get { return engine.PropertyId; }
        }

        public bool IsAnonymizeIpEnabled
        {
            get { return engine.AnonymizeIP; }
            set { engine.AnonymizeIP = value; }
        }

        public string AppName
        {
            get { return engine.AppName; }
            set { engine.AppName = value; }
        }

        public string AppVersion
        {
            get { return engine.AppVersion; }
            set { engine.AppVersion = value; }
        }

        public Size? AppScreen
        {
            get { return engine.ViewportSize; }
            set { engine.ViewportSize = value; }
        }

        public string Referrer
        {
            get { return engine.Referrer; }
            set { engine.Referrer = value; }
        }

        public string Campaign
        {
            get { return engine.Campaign; }
            set { engine.Campaign = value; }
        }

        public float SampleRate { get; set; }
        public bool IsUseSecure { get; set; }
        public bool ThrottlingEnabled { get; set; }

        public void SendView(string screenName)
        {
            platformInfoProvider.OnTracking(); // give platform info provider a chance to refresh.
            var payload = engine.TrackView(screenName, SessionControl);
            SendPayload(payload);
        }

        public void SendException(string description, bool isFatal)
        {
            platformInfoProvider.OnTracking(); // give platform info provider a chance to refresh.
            var payload = engine.TrackException(description, isFatal, SessionControl);
            SendPayload(payload);
        }

        public void SendSocial(string network, string action, string target)
        {
            platformInfoProvider.OnTracking(); // give platform info provider a chance to refresh.
            var payload = engine.TrackSocialInteraction(network, action, target, SessionControl);
            SendPayload(payload);
        }

        public void SendTiming(TimeSpan time, string category, string variable, string label)
        {
            platformInfoProvider.OnTracking(); // give platform info provider a chance to refresh.
            var payload = engine.TrackUserTiming(category, variable, time, label, null, null, null, null, null, null, SessionControl);
            SendPayload(payload);
        }

        public void SendEvent(string category, string action, string label, int value)
        {
            platformInfoProvider.OnTracking(); // give platform info provider a chance to refresh.
            var payload = engine.TrackEvent(category, action, label, value, SessionControl);
            SendPayload(payload);
        }

        public void SendTransaction(Transaction transaction)
        {
            platformInfoProvider.OnTracking(); // give platform info provider a chance to refresh.
            foreach (var payload in TrackTransaction(transaction, SessionControl))
            {
                SendPayload(payload);
            }
        }

        IEnumerable<Payload> TrackTransaction(Transaction transaction, SessionControl sessionControl = SessionControl.None, bool isNonInteractive = false)
        {
            yield return engine.TrackTransaction(transaction.TransactionId, transaction.Affiliation, (double)transaction.TotalCostInMicros / 1000000, (double)transaction.ShippingCostInMicros / 1000000, (double)transaction.TotalTaxInMicros / 1000000, transaction.CurrencyCode, sessionControl, isNonInteractive);

            foreach (var item in transaction.Items)
            {
                yield return engine.TrackTransactionItem(transaction.TransactionId, item.Name, (double)item.PriceInMicros / 1000000, item.Quantity, item.SKU, item.Category, transaction.CurrencyCode, sessionControl, isNonInteractive);
            }
        }

        SessionControl SessionControl
        {
            get
            {
                if (endSession)
                {
                    endSession = false;
                    return SessionControl.End;
                }
                else if (startSession)
                {
                    startSession = false;
                    return SessionControl.Start;
                }
                else
                {
                    return SessionControl.None;
                }
            }
        }

        bool startSession;
        public void SetStartSession(bool value)
        {
            startSession = value;
        }

        bool endSession;
        public void SetEndSession(bool value)
        {
            endSession = value;
        }

        void SendPayload(Payload payload)
        {
            if (!string.IsNullOrEmpty(TrackingId))
            {
                if (!IsSampledOut())
                {
                    if (!ThrottlingEnabled || hitTokenBucket.Consume())
                    {
                        payload.IsUseSecure = IsUseSecure;
                        analyticsEngine.SendPayload(payload);
                    }
                }
            }
        }

        bool IsSampledOut()
        {
            if (SampleRate <= 0.0F)
            {
                return true;
            }
            else if (SampleRate < 100.0F)
            {
                var clientId = platformInfoProvider.AnonymousClientId;
                return ((clientId != null) && (Math.Abs(clientId.GetHashCode()) % 10000 >= SampleRate * 100.0F));
            }
            else return false;
        }
    }
}
