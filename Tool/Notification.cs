using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Windows.ApplicationModel.Background;

namespace EQAlert
{

    public static class BKG
    {


        public static bool FindIfRegistered(string TaskName, bool UnReg)
        {
            IReadOnlyDictionary<Guid, IBackgroundTaskRegistration> back = BackgroundTaskRegistration.AllTasks;
            bool regis = false;
            IBackgroundTaskRegistration t = null;
            foreach (var task in back)
            {
                if (task.Value.Name == TaskName)
                {
                    regis = true;
                    t = (IBackgroundTaskRegistration)task.Value;
                    if (UnReg)
                    {
                        t.Unregister(true);
                        regis = false;
                    }
                    break;
                }
            }
            return regis;
        }


        public static void Build(string friendName, string className, IBackgroundTrigger trigger)
        {
            BackgroundTaskBuilder builder = new BackgroundTaskBuilder();
            // Friendly string name identifying the background task
            builder.Name = friendName;
            // Class name 
            builder.TaskEntryPoint = className;



            //   IBackgroundTrigger trigger = new SystemTrigger( SystemTriggerType. 
            //trigger = new SystemTrigger(SystemTriggerType.InternetAvailable,false);
            //    trigger = new TimeTrigger( 15, false);
            builder.SetTrigger(trigger);
            IBackgroundCondition condition = new SystemCondition(SystemConditionType.InternetAvailable);
            builder.AddCondition(condition);

            IBackgroundTaskRegistration td = builder.Register();

        }

        static void task_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            // throw new NotImplementedException();
        }

        static void task_Progress(BackgroundTaskRegistration sender, BackgroundTaskProgressEventArgs args)
        {
            //throw new NotImplementedException();
        }

    }



    public static class Notif
    {


        public static object MakeNotification(Type t, string tag, IList<string> lines, string webimg, string prefixUri, bool setimg, int nr, bool sound, DateTimeOffset? deliv)
        {

            object notification = null;





       


            if (!t.Equals(typeof(ToastNotification)))
            {

                TileTemplateType tiletype = TileTemplateType.TileWide310x150PeekImageAndText02;

                if (nr == 0)
                {
                    tiletype = TileTemplateType.TileWide310x150PeekImageAndText02;
                }
                else if (nr == 2) tiletype = TileTemplateType.TileWide310x150SmallImageAndText05;


                else if (nr == 1) tiletype = TileTemplateType.TileWide310x150BlockAndText02;
                else if (nr == 4) tiletype = TileTemplateType.TileWide310x150Text01;
                else if (nr == 3) tiletype = TileTemplateType.TileWide310x150Text09;
                XmlDocument tileXml = TileUpdateManager.GetTemplateContent(tiletype);
                SetTileText(lines, tileXml);


                if (setimg) SetTileImg(tileXml, webimg + "?scale=100&contrast=blk&lang=en-US", prefixUri);
                else SetTileImg(tileXml, "ms-appdata:///local/Assets/SmallLogo.png", prefixUri);




                string sqtxt = "Hello World! My very own tile notification";
                try
                {

                    XmlDocument squareTile = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Text04);
                    //  IXmlNode sqtxt = squareTile.CreateTextNode();
                    XmlNodeList sqnode = squareTile.GetElementsByTagName("text");
                    sqnode[0].InnerText = sqtxt;
                    XmlNodeList binnode = squareTile.GetElementsByTagName("binding");
                    XmlNodeList vnode = squareTile.GetElementsByTagName("visual");
                    IXmlNode bn = tileXml.ImportNode(binnode.Item(0), true);
                    vnode.Item(0).AppendChild(bn);
                }
                catch (Exception ex)
                {


                }



                if (t.Equals(typeof(ScheduledTileNotification)))
                {

                    ScheduledTileNotification tnotif = new ScheduledTileNotification(tileXml, (DateTimeOffset)deliv);

                    tnotif.Id = tag;
                    notification = tnotif;
                }
                else
                {
                    TileNotification tnotif = new TileNotification(tileXml);
                    tnotif.Tag = tag;
                    //   tnotif.Tag = tag.Substring(tag.LastIndexOf(':') + 1);
                    notification = tnotif;

                }




            }
            else
            {

                ToastTemplateType toasttype = ToastTemplateType.ToastImageAndText04;


                //toasttype = ToastTemplateType.ToastText04;

                XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toasttype);
                SetTileText(lines, toastXml);

                if (setimg) SetTileImg(toastXml, webimg + "?scale=100&contrast=blk&lang=en-US", prefixUri);
                // else SetTileImg(ref toastXml, "ms-appdata:///local/Assets/StoreLogo.png", prefixUri);
                else
                {

                    SetTileImg(toastXml, "ms-appdata:///local/Assets/SmallLogo.png", prefixUri);

                }



                IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
                XmlElement audio = toastXml.CreateElement("audio");
                if (sound)
                {
                    audio.SetAttribute("src", "ms-winsoundevent:Notification.IM");
                }
                else audio.SetAttribute("silent", "true");      //no sound ......    ///  


                ((XmlElement)toastNode).SetAttribute("duration", "long");

                // audio.SetAttribute("src", "ms-winsoundevent:Notification.Looping.Alarm");
                audio.SetAttribute("loop", "false");
                toastNode.AppendChild(audio);
                ((XmlElement)toastNode).SetAttribute("launch", "{\"type\":\"toast\",\"param1\":\"12345\",\"param2\":\"67890\"}");

                ToastNotification toastnotif = new ToastNotification(toastXml);

                notification = toastnotif;

            }



            return notification;



        }

        public static BadgeNotification MakeBadge(bool glyph, string content)
        {


            XmlDocument badgeXml = null;

            if (glyph)
            {
                badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeGlyph);

            }
            else
            {
                badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);

            }
            // XmlDocument 
            XmlElement badgeElement = (XmlElement)badgeXml.SelectSingleNode("/badge");
            badgeElement.SetAttribute("value", content);

            BadgeNotification badge = new BadgeNotification(badgeXml);
            return badge;

        }
        public static void SetTileImg(XmlDocument tileXml, string webimg, string prefixUri)
        {
            try
            {

                XmlNodeList imgnode = tileXml.GetElementsByTagName("image");
                XmlElement img = (XmlElement)imgnode[0];

                img.SetAttribute("src", prefixUri + webimg);
                img.SetAttribute("alt", "Earthquake");
                //      img.SetAttribute("id", webimg);

            }
            catch (Exception ex)
            {


            }
        }

        public static void SetTileText(IList<string> lines, XmlDocument tileXml)
        {
            try
            {

                XmlNodeList textnode = tileXml.GetElementsByTagName("text");

                for (int i = 0; i < textnode.Count; i++)
                {
                    textnode[i].InnerText = lines.ElementAt(i);
                }

            }
            catch (Exception ex)
            {


            }
        }


    }


}
