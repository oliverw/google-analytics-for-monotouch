using System;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace GoogleAnalytics
{
    public sealed class PlatformInfoProvider
    {
        const string Key_AnonymousClientId = "GoogleAnaltyics.AnonymousClientId";

        public event EventHandler ViewPortResolutionChanged;

        public event EventHandler ScreenResolutionChanged;


        public string AnonymousClientId
        {
            get
            {
                if (NSUserDefaults.StandardUserDefaults.ValueForKey(new NSString(Key_AnonymousClientId)) == null)
                {
                    var result = Guid.NewGuid().ToString();
                    NSUserDefaults.StandardUserDefaults.SetString(result, Key_AnonymousClientId);
                    return result;
                }
                else
                {
                    return NSUserDefaults.StandardUserDefaults.StringForKey(Key_AnonymousClientId);
                }
            }
        }

        public Size? ScreenResolution
        {
            get { return new Size((int)UIScreen.MainScreen.Bounds.Width, (int) UIScreen.MainScreen.Bounds.Height); }
        }

        public Size? ViewPortResolution
        {
            get { return new Size((int)UIScreen.MainScreen.Bounds.Width, (int)UIScreen.MainScreen.Bounds.Height); }
        }

        public string UserLanguage
        {
            get { return NSLocale.PreferredLanguages[0]; }
        }

        public int? ScreenColorDepthBits
        {
            get { return null; }
        }

        public string DocumentEncoding
        {
            get { return null; }
        }

        public void OnTracking()
        {
        }
    }
}
