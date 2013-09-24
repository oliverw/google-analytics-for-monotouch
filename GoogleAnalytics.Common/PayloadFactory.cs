using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Drawing;


#if NETFX_CORE
using Windows.Foundation;
#else
using System.Windows;
#endif

namespace GoogleAnalytics
{
    //https://developers.google.com/analytics/devguides/collection/protocol/v1/parameters
    internal sealed class PayloadFactory
    {
        const string HitType_Pageview = "appview";
        const string HitType_Event = "event";
        const string HitType_Exception = "exception";
        const string HitType_SocialNetworkInteraction = "social";
        const string HitType_UserTiming = "timing";
        const string HitType_Transaction = "transaction";
        const string HitType_TransactionItem = "item";

        public string PropertyId { get; set; }
        public string AppName { get; set; }
        public string AppVersion { get; set; }
        public bool AnonymizeIP { get; set; }
        public IDictionary<int, string> CustomDimensions { get; set; }
        public IDictionary<int, int> CustomMetrics { get; set; }
        public Size? ViewportSize { get; set; }
        // TODO: unused
        public string Referrer { get; set; }
        public string Campaign { get; set; }
        public string ScreenName { get; set; }

        public string AnonymousClientId { get; set; }
        public Size? ScreenResolution { get; set; }
        public string UserLanguage { get; set; }
        public int? ScreenColorDepthBits { get; set; }
        public string DocumentEncoding { get; set; }

        public PayloadFactory()
        {
            CustomDimensions = new Dictionary<int, string>();
            CustomMetrics = new Dictionary<int, int>();
        }

        public Payload TrackView(string screenName, SessionControl sessionControl = SessionControl.None, bool isNonInteractive = false)
        {
            ScreenName = screenName;
            return PostData(HitType_Pageview, null, isNonInteractive, sessionControl);
        }

        public Payload TrackEvent(string category, string action, string label, int value, SessionControl sessionControl = SessionControl.None, bool isNonInteractive = false)
        {
            var additionalData = new Dictionary<string, string>();
            additionalData.Add("ec", category);
            additionalData.Add("ea", action);
            if (label != null) additionalData.Add("el", label);
            if (value != 0) additionalData.Add("ev", value.ToString(CultureInfo.InvariantCulture));
            return PostData(HitType_Event, additionalData, isNonInteractive, sessionControl);
        }

        public Payload TrackException(string description, bool isFatal, SessionControl sessionControl = SessionControl.None, bool isNonInteractive = false)
        {
            var additionalData = new Dictionary<string, string>();
            if (description != null) additionalData.Add("exd", description);
            if (!isFatal) additionalData.Add("exf", "0");
            return PostData(HitType_Exception, additionalData, isNonInteractive, sessionControl);
        }

        public Payload TrackSocialInteraction(string network, string action, string target, SessionControl sessionControl = SessionControl.None, bool isNonInteractive = false)
        {
            var additionalData = new Dictionary<string, string>();
            additionalData.Add("sn", network);
            additionalData.Add("sa", action);
            additionalData.Add("st", target);
            return PostData(HitType_SocialNetworkInteraction, additionalData, isNonInteractive, sessionControl);
        }

        public Payload TrackUserTiming(string category, string variable, TimeSpan? time, string label, TimeSpan? loadTime, TimeSpan? dnsTime, TimeSpan? downloadTime, TimeSpan? redirectResponseTime, TimeSpan? tcpConnectTime, TimeSpan? serverResponseTime, SessionControl sessionControl = SessionControl.None, bool isNonInteractive = false)
        {
            var additionalData = new Dictionary<string, string>();
            if (category != null) additionalData.Add("utc", category);
            if (variable != null) additionalData.Add("utv", variable);
            if (time.HasValue) additionalData.Add("utt", Math.Round(time.Value.TotalMilliseconds).ToString(CultureInfo.InvariantCulture));
            if (label != null) additionalData.Add("utl", label);
            if (loadTime.HasValue) additionalData.Add("ptl", Math.Round(loadTime.Value.TotalMilliseconds).ToString(CultureInfo.InvariantCulture));
            if (dnsTime.HasValue) additionalData.Add("dns", Math.Round(dnsTime.Value.TotalMilliseconds).ToString(CultureInfo.InvariantCulture));
            if (downloadTime.HasValue) additionalData.Add("pdt", Math.Round(downloadTime.Value.TotalMilliseconds).ToString(CultureInfo.InvariantCulture));
            if (redirectResponseTime.HasValue) additionalData.Add("rrt", Math.Round(redirectResponseTime.Value.TotalMilliseconds).ToString(CultureInfo.InvariantCulture));
            if (tcpConnectTime.HasValue) additionalData.Add("tcp", Math.Round(tcpConnectTime.Value.TotalMilliseconds).ToString(CultureInfo.InvariantCulture));
            if (serverResponseTime.HasValue) additionalData.Add("srt", Math.Round(serverResponseTime.Value.TotalMilliseconds).ToString(CultureInfo.InvariantCulture));
            return PostData(HitType_UserTiming, additionalData, isNonInteractive, sessionControl);
        }

        public Payload TrackTransaction(string id, string affiliation, double revenue, double shipping, double tax, string currencyCode, SessionControl sessionControl = SessionControl.None, bool isNonInteractive = false)
        {
            var additionalData = new Dictionary<string, string>();
            additionalData.Add("ti", id);
            if (affiliation != null) additionalData.Add("ta", affiliation);
            if (revenue != 0) additionalData.Add("tr", revenue.ToString(CultureInfo.InvariantCulture));
            if (shipping != 0) additionalData.Add("ts", shipping.ToString(CultureInfo.InvariantCulture));
            if (tax != 0) additionalData.Add("tt", tax.ToString(CultureInfo.InvariantCulture));
            if (currencyCode != null) additionalData.Add("cu", currencyCode);
            return PostData(HitType_Transaction, additionalData, isNonInteractive, sessionControl);
        }

        public Payload TrackTransactionItem(string transactionId, string name, double price, long quantity, string code, string category, string currencyCode, SessionControl sessionControl = SessionControl.None, bool isNonInteractive = false)
        {
            var additionalData = new Dictionary<string, string>();
            additionalData.Add("ti", transactionId);
            if (name != null) additionalData.Add("in", name);
            if (price != 0) additionalData.Add("ip", price.ToString(CultureInfo.InvariantCulture));
            if (quantity != 0) additionalData.Add("iq", quantity.ToString(CultureInfo.InvariantCulture));
            if (code != null) additionalData.Add("ic", code);
            if (category != null) additionalData.Add("iv", category);
            if (currencyCode != null) additionalData.Add("cu", currencyCode);
            return PostData(HitType_TransactionItem, additionalData, isNonInteractive, sessionControl);
        }

        private Payload PostData(string hitType, IDictionary<string, string> additionalData, bool isNonInteractive, SessionControl sessionControl)
        {
            var payloadData = GetRequiredPayloadData(hitType, isNonInteractive, sessionControl);
            if (additionalData != null)
            {
                foreach (var item in additionalData)
                {
                    payloadData.Add(item);
                }
            }
            return new Payload(payloadData);
        }

        IDictionary<string, string> GetRequiredPayloadData(string hitType, bool isNonInteractive, SessionControl sessionControl)
        {
            var result = new Dictionary<string, string>();
            result.Add("v", "1");
            result.Add("tid", PropertyId);
            result.Add("cid", AnonymousClientId);
            result.Add("an", AppName);
            result.Add("av", AppVersion);
            result.Add("t", hitType);
            if (ScreenName != null) result.Add("cd", ScreenName);
            if (isNonInteractive) result.Add("ni", "1");
            if (AnonymizeIP) result.Add("aip", "1");
            if (sessionControl != SessionControl.None) result.Add("sc", sessionControl == SessionControl.Start ? "start" : "end");
            if (ScreenResolution.HasValue) result.Add("sr", string.Format("{0}x{1}", ScreenResolution.Value.Width, ScreenResolution.Value.Height));
            if (ViewportSize.HasValue) result.Add("vp", string.Format("{0}x{1}", ViewportSize.Value.Width, ViewportSize.Value.Height));
            if (UserLanguage != null) result.Add("ul", UserLanguage.ToLowerInvariant());
            if (ScreenColorDepthBits.HasValue) result.Add("sd", string.Format("{0}-bits", ScreenColorDepthBits.Value));
            if (DocumentEncoding != null) result.Add("de", DocumentEncoding);
            foreach (var dimension in CustomDimensions)
            {
                result.Add(string.Format("cd{0}", dimension.Key), dimension.Value);
            }
            foreach (var metric in CustomMetrics)
            {
                result.Add(string.Format("cm{0}", metric.Key), metric.Value.ToString(CultureInfo.InvariantCulture));
            }
            CustomDimensions.Clear();
            CustomMetrics.Clear();

            return result;
        }
    }

    internal enum SessionControl
    {
        None,
        Start,
        End
    }
}
