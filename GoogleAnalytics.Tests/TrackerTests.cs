using System;
using NUnit.Framework;
using System.Drawing;
using MonoTouch.UIKit;

namespace GoogleAnalytics.Tests
{
    [TestFixture]
    public class TrackerTests
    {
        [Test]
        public void get_tracker()
        {
            var tracker = GoogleAnalytics.EasyTracker.GetTracker();
            Assert.NotNull(tracker);
        }

        [Test]
        public void config_tracking_id()
        {
            var tracker = GoogleAnalytics.EasyTracker.GetTracker();
            Assert.AreEqual(tracker.TrackingId, "UA-XXXX-Y");
        }

        [Test]
        public void config_app_name()
        {
            var tracker = GoogleAnalytics.EasyTracker.GetTracker();
            Assert.AreEqual(tracker.AppName, "TestApp");
        }

        [Test]
        public void config_screen_information()
        {
            var tracker = GoogleAnalytics.EasyTracker.GetTracker();
            Assert.AreEqual(tracker.AppScreen.Value, new Size((int)UIScreen.MainScreen.Bounds.Width, (int) UIScreen.MainScreen.Bounds.Height));
        }
    }
}
