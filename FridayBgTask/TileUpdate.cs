using FridayBgTask.Class;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media.Imaging;
using static FridayBgTask.Class.Model;

namespace FridayBgTask
{
    public sealed class TileUpdate : IBackgroundTask
    {
        ApplicationDataContainer localSetting = ApplicationData.Current.LocalSettings;
        int currentWeek = -1;

        async void IBackgroundTask.Run(IBackgroundTaskInstance taskInstance)
        {
           
            var _taskDeferral = taskInstance.GetDeferral();
            await UpdateTile();
            _taskDeferral.Complete();
        }
        private async Task UpdateTile()
        {
            var jsonString = (string)localSetting.Values["userdata"];
            if (jsonString != null)
            {
                var userdata = Data.Json.DataContractJsonDeSerialize<User.Login_Result>(jsonString);
                currentWeek = (userdata.attachmentBO.nowWeekMsg.nowWeek);
            }

            var course=await Class.Model.CourseManager.GetCourse();
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.EnableNotificationQueueForWide310x150(true);
            updater.EnableNotificationQueueForSquare150x150(true);
            updater.EnableNotificationQueueForSquare310x310(true);
            updater.EnableNotificationQueue(true);
            updater.Clear();
            if (course != null && course.Count > 0)
            {
                //SetState(StateType.busy);
                var newcourselist = new List<Class.Model.CourseManager.CourseModel>();
                var week=(int)DateTime.Today.DayOfWeek;
                if (week == 0) week = 7;
                foreach (var item in course)
                {
                    if (week != -1)
                    {
                        string[] weeks;
                        if (item.smartPeriod != null) weeks = item.smartPeriod.Split(' '); else weeks = item.period.Split(' ', ',');
                        if (Array.IndexOf(weeks, currentWeek.ToString()) < 0) continue;
                    }

                    if (item.day == week) newcourselist.Add(item);
                }
                if (newcourselist.Count > 0)
                {
                    try
                    {
                        setBadgeNumber(newcourselist.Count);
                        foreach (var n in newcourselist)
                        {

                            var doc = new XmlDocument();
                            var xml = "";
                            if (n.sectionStart == n.sectionEnd)
                            {
                                xml = string.Format(TileTemplateXml, n.name, "节数:" + n.sectionStart, "地点:" + n.classroom);
                            }
                            else
                            {
                                xml= string.Format(TileTemplateXml, n.name, "节数:" + n.sectionStart + "-" + n.sectionEnd, "地点:" + n.classroom);
                            }
                            doc.LoadXml(System.Net.WebUtility.HtmlDecode(xml), new XmlLoadSettings
                            {
                                ProhibitDtd = false,
                                ValidateOnParse = false,
                                ElementContentWhiteSpace = false,
                                ResolveExternals = false
                            });
                            updater.Update(new TileNotification(doc));
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Error:" + e.Message);
                    }
                }
                else
                {
                    clearBadge();
                }
            }else
            {
                clearBadge();
            }
        }

        private const string TileTemplateXml = @"
<tile branding='name'> 
  <visual version='2' displayName='今日课程'>
    <binding template='TileMedium'>
      <text hint-style='subtitle' hint-wrap='true'>{0}</text>
      <text hint-style='captionsubtle' hint-wrap='true'>{1}</text>
      <text hint-style='captionsubtle' hint-wrap='true'>{2}</text>
    </binding>
    <binding template='TileWide'>
      <text hint-style='subtitle' id='1'>{0}</text>
      <text id='2' hint-style='captionsubtle' hint-wrap='true'>{1}</text>
      <text id='3' hint-style='captionsubtle' hint-wrap='true'>{2}</text>
    </binding>
  </visual>
</tile>";

        private enum StateType
        {
            available, away, busy
        }

        private void SetState(StateType state)
        {
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();
            string badgeGlyphValue = "";
            switch (state)
            {
                case StateType.available:
                    badgeGlyphValue = "available";
                    break;
                case StateType.away:
                    badgeGlyphValue = "away";
                    break;
                case StateType.busy:
                    badgeGlyphValue = "busy";
                    break;
                default:
                    break;
            }
            XmlDocument badgeXml =BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeGlyph);
            XmlElement badgeElement =badgeXml.SelectSingleNode("/badge") as XmlElement;
            badgeElement.SetAttribute("value", badgeGlyphValue);
            BadgeNotification badge = new BadgeNotification(badgeXml);
            BadgeUpdater badgeUpdater =BadgeUpdateManager.CreateBadgeUpdaterForApplication();
            badgeUpdater.Update(badge);
        }

        private void setBadgeNumber(int num)
        {
            XmlDocument badgeXml =BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);
            XmlElement badgeElement = badgeXml.SelectSingleNode("/badge") as XmlElement;
            badgeElement.SetAttribute("value", num.ToString());
            BadgeNotification badge = new BadgeNotification(badgeXml);
            BadgeUpdater badgeUpdater =BadgeUpdateManager.CreateBadgeUpdaterForApplication();
            badgeUpdater.Update(badge);
        }

        private void clearBadge()
        {
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();
        }
    }
}
