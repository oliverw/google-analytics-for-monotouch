using System;
using System.IO.IsolatedStorage;
using System.Windows;
using CoreGraphics;
using Foundation;
using UIKit;

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

        public CGSize? ScreenResolution
        {
            get { return new CGSize((int)UIScreen.MainScreen.Bounds.Width, (int) UIScreen.MainScreen.Bounds.Height); }
        }

        public CGSize? ViewPortResolution
        {
            get { return new CGSize((int)UIScreen.MainScreen.Bounds.Width, (int)UIScreen.MainScreen.Bounds.Height); }
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
