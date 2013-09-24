using System.IO.IsolatedStorage;
using System.Windows;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Threading.Tasks;

namespace GoogleAnalytics
{
    public sealed partial class AnalyticsEngine
    {
        const string Key_AppOptOut = "GoogleAnaltyics.AppOptOut";

        bool? appOptOut;
        public bool AppOptOut
        {
            get
            {
                if (appOptOut.HasValue) return appOptOut.Value;
                return GetAppOptOut();
            }
            set
            {
                if (!appOptOut.HasValue) GetAppOptOut();
                if (appOptOut.Value != value)
                {
                    appOptOut = value;
                    NSUserDefaults.StandardUserDefaults.SetBool(value, Key_AppOptOut);
                    if (value) GAServiceManager.Current.Clear();
                }
            }
        }

        private bool GetAppOptOut()
        {
            if (NSUserDefaults.StandardUserDefaults.ValueForKey(new NSString(Key_AppOptOut)) != null)
            {
                appOptOut = NSUserDefaults.StandardUserDefaults.BoolForKey(Key_AppOptOut);
            }
            else
            {
                appOptOut = false;
            }
            return appOptOut.Value;
        }

        public Task<bool> RequestAppOptOutAsync()
        {
            var tcs = new TaskCompletionSource<bool>();

            var alertView = new UIAlertView("Help Improve User Experience", 
                "Allow anonomous information to be collected to help improve this application?", 
                null, "Cancel", "Ok");

            alertView.Clicked += (object _, UIButtonEventArgs eb) => 
            {
                tcs.SetResult(eb.ButtonIndex != alertView.CancelButtonIndex);
            };

            alertView.Show();

            return tcs.Task;
        }
    }
}
