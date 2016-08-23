using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Syndication;
using Windows.Foundation;
using Windows.UI.Notifications;

namespace EQAlert
{
    public sealed class Generator
    {

        public Generator()
        {
         //   set = new EQSetting();
            data = new ChiguiData();

            tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
            tileUpdater.EnableNotificationQueue(true);
            //  this.tileUpdater.Clear();

            toastNotifier = ToastNotificationManager.CreateToastNotifier();

            bdgupdater = BadgeUpdateManager.CreateBadgeUpdaterForApplication();
            bdgupdater.Clear();


            tags = new HashSet<string>();
        }

      

        public bool DoCheck()
        {

            return doCheck();
        }
        public bool DoTiles()
        {
            return doTiles();
        }



        #region Func<>
      
        private Func<Chigui, bool> createToaster()
        {

            Func<Chigui, bool> toaster = eq =>
            {


                try
                {
                    Toast(prefixUri, ref eq, true);

                }
                catch (Exception ex)
                {

                    try
                    {
                        Toast(prefixUri, ref eq, false);

                    }
                    catch (Exception x)
                    {

                        return false;
                    }
                }
                return true;
            };
            return toaster;
        }


        private Func<Chigui, bool> createTiler(double secDur)
        {


            Func<Chigui, bool> tiler = eq =>
            {
                try
                {

                    Tile(ref eq, true, secDur);


                }
                catch (Exception ex)
                {
                    try
                    {
                        Tile(ref eq, false, secDur);


                    }
                    catch (Exception e)
                    {

                        return false;
                    }

                }
                return true;

            };


            return tiler;
        }


        #endregion




        private bool doCheck()
        {

            added = false;

            string sect = "Alerter";
            setStatus(sect, true, false);
            try
            {
             
              
                totoast = totoast.Take(3).ToList(); //take 5

                if (totoast.Count() != 0)
                {
                    Func<Chigui, bool> toaster = createToaster();
                    totoast = totoast.Where(toaster).ToList(); //actual toasting
                    added = true;


                }
                else added = false;

                if (!added) status = "Nothing found!";
                else status = "Alerts added!";
                setStatus(sect, false, false);
            }
            catch (Exception ex)
            {
                setStatus(ex.Message, false, true);
                return false;
            }
            return added;
        }
        private bool doTiles()
        {

            string sect = "Tiler";
            setStatus(sect, true, false);


            try
            {

              

               
                eqdim = eqdim.Take(6).ToList();

                int count = eqdim.Count();

                if (count != 0)
                {

                
                    deliv = DateTimeOffset.UtcNow;
                    Func<Chigui, bool> tiler = createTiler(secDur);
                    eqdim = eqdim.Where(tiler).ToList();
                    cnter++;

                    if (DateTimeOffset.UtcNow.Subtract(nowcollect).Minutes > 5)
                    {
                        GC.Collect();
                        nowcollect = deliv;
                    }


                    bdgupdater.Clear();
                    BadgeNotification badge = Notif.MakeBadge(false, eqdim.Count().ToString());
                    bdgupdater.Update(badge);

                    setStatus(sect, false, false);

                }


            }
            catch (Exception ex)
            {
                setStatus(ex.Message, false, true);
                return false;
            }

            return (eqdim.Count() > 0);

        }

      



     
        private void setStatus(string header, bool starting, bool failed)
        {
            string ppp = "...";
            this.status = header + " starting" + ppp;
            if (failed) this.status = header + " failed!" + ppp;
            else if (!starting) this.status = header + " ok!";

        }


        #region Tiling
        private void Tile(ref Chigui eq, bool img, double secDur)
        {
            string tag = string.Empty;
            tag = eq.ID.Substring(eq.ID.LastIndexOf(':') + 1);

            if (null != this.tileUpdater.GetScheduledTileNotifications().FirstOrDefault(o => o.Id.Equals(tag))) return;
            status = "Tiling...";
            try
            {
                Type schnotfi = typeof(ScheduledTileNotification);

                if (cnter >= maxCounter) cnter = 0;


                eq.Nr = cnter;
             //   eq.SI = set.SI;
                deliv = deliv.AddSeconds(1);
                ScheduledTileNotification notif = (ScheduledTileNotification)Notif.MakeNotification(schnotfi, tag, eq.Summary, eq.ImgUri, string.Empty, img, eq.Nr, !set.NoSound, deliv);
                eq.Notification = null;
                eq.Notification = notif;
                deliv = deliv.AddSeconds(secDur);
                notif.ExpirationTime = deliv.AddSeconds(secDur);

                //  deliv = notif.ExpirationTime.Value.AddSeconds(2);

                this.tileUpdater.AddToSchedule(notif);

                cnter++;



            }
            catch (Exception ex)
            {
                status = "Error tiling...";
            }

        }
        private void Toast(string prefixUri, ref Chigui eq, bool img)
        {


            string tag = string.Empty;

            tag = eq.ID.Substring(eq.ID.LastIndexOf(':') + 1);

            if (tags.Contains(tag)) return;

            tags.Add(tag);

            if (lastLoad.Subtract(eq.EventDate.UtcDateTime).TotalSeconds >= 0) return;

       
            ToastNotification toastnotif = (ToastNotification)Notif.MakeNotification(typeof(ToastNotification), tag, eq.Summary, eq.ImgUri, prefixUri, img, 0, !set.NoSound, null);
            toastnotif.ExpirationTime = DateTime.Now.AddMinutes(1);
            // toastnotif.ExpirationTime = null;

            toastnotif.Activated += toastnotif_Activated;
            this.toastNotifier.Show(toastnotif);
            status = "Toasted!";

            // maxMag = Convert.ToInt32(eq.Magnitude);
            // string content = maxMag.ToString();

            lastLoad = eq.EventDate.UtcDateTime;
            //  maxMag++;

        }

        public void ClearTiles()
        {

            tileUpdater.Clear();
            this.bdgupdater.Clear();

        }

        async void toastnotif_Activated(ToastNotification sender, object args)
        {
            if (totoast == null) return;
            if (totoast.Count() == 0) return;

            Chigui e = totoast.FirstOrDefault();
            if (e == null) return;
            Windows.System.LauncherOptions o = new Windows.System.LauncherOptions();
            o.DisplayApplicationPicker = false;
            //   o.PreferredApplicationDisplayName = "iexplorer.exe";

            o.TreatAsUntrusted = false;
            // Display the contents of the favorites folder in the browser.
            await Windows.System.Launcher.LaunchUriAsync(e.Uri, o);



        }

        #endregion


        #region Readding/Writing settings



        private DateTime lastLoad = DateTime.UtcNow.AddDays(-1);
        public DateTimeOffset LastLoad
        {
            get { return lastLoad; }

        }



        #endregion


        #region Private Fields

        private DateTimeOffset nowcollect = DateTimeOffset.UtcNow;

        private IEnumerable<Chigui> eqdim = null;
        private IEnumerable<Chigui> totoast = null;
        private HashSet<string> tags = null;

        private DateTimeOffset deliv = DateTimeOffset.UtcNow.AddSeconds(2);
        private const int maxCounter = 2;
        private const string fileSettings = "EQsettings.txt";
        private const string prefixUri = "";

        private int cnter = 0;
        private bool saved = false;
        private bool added = false;

        // private  string prefixUri = "ms-appx:///";

        private bool settingsLoaded = false;
        private string status = string.Empty;
        private string configData = string.Empty;
        private double[] latlonacu = null;

        private BadgeUpdater bdgupdater = null;
        private ToastNotifier toastNotifier = null;
        private TileUpdater tileUpdater = null;

     
        private ChiguiData data = null;
        #endregion

        #region Public Fields

        public bool SettingsLoaded
        {
            get { return settingsLoaded; }
            set { settingsLoaded = value; }
        }
        public string ConfigData
        {
            get
            {

                return configData;
            }

            set
            {
                configData = value;
                LoadSettings();
                WriteSettings();

            }

        }
        public string Status
        {

            get { return status; }
            set
            {

                status = value;
            }
        }

        public EQSetting Setting
        {
            get { return set; }
            set { set = value; }
        }
        public double[] LLA
        {

            get { return latlonacu; }
            set { latlonacu = value; }
        }


        public EQData Data
        {
            get
            {

                return data;
            }

        }
        public IEnumerable<string> Tags
        {

            get { return tags; }
            //  set { tags = new HashSet<string>(value); }
        }


        #endregion





    }

   
   

    public sealed class GenData
    {

        private Func<SyndicationItem, bool> createFeedAdder<T>(Int16 mode, double minTime,  Func<T,bool> finder)
        {


            HashSet<T> output = null;

            if (mode == 1) output = this.eqls;
            else if (mode == 2) output = this.past24;

            T eq;


            Func<SyndicationItem, bool> feedAdder = o =>
            {

             

               

                bool added = false;
                try
                {
                 
               //     if (eq.SinceMin <= minTime)
                    {
                        T find = output.FirstOrDefault(finder);
                        if (find == null)
                        {
                            output.Add(eq);
                            added = true;
                        }
                       
                    }
                    return added;
                }
                catch (Exception ex)
                {
                    return added;

                }
            };

            return feedAdder;
        }
        public IAsyncAction GetList()
        {

            return getList(set).AsAsyncAction();
        }

        public GenData(object O)
        {
            uri = new Uri[2];

            uri[0] = new Uri("http://feeds.feedburner.com/elchiguirebipolar?format=xml");
          

      


            feeder = new Feeder();


            eqls = new HashSet<object>();



            past24 = new HashSet<object>();



        }


        private Feeder feeder = null;


        private async Task<bool> getList<T>( T eq)
        {



            feeder.Uri = uri[0];
            feeder.AdderMethod = createFeedAdder<Chigui>(1,"1", new Chigui();
            await feeder.SetFunc();


            feeder.Uri = uri[1];
            feeder.AdderMethod = createFeedAdder(2);
            await feeder.SetFunc();


          //  eqls = new HashSet<Chigui>(eqls.OrderBy(o => o.SinceMin).ToList()); //filter base on minToast
            eqls.TrimExcess();


        //    IEnumerable<EQ> aux = past24.OrderBy(o => o.SinceMin).ToList().Except(eqls).ToList();
         //   past24 = new HashSet<Chigui>(aux.ToList()); //filter base on minToast

            past24.TrimExcess();





            return eqls.Count > 0;
        }


        public Uri[] UriPath
        {

            get { return uri; }
            set { uri = value; }
        }

        private Uri[] uri = null;
        private HashSet<object> eqls = null;

        private HashSet<object> past24 = null;


        public IEnumerable<object> EqLS
        {

            get { return eqls; }
            //  set { eqls = new HashSet<EQ>(value.ToList()); }
        }
        public IEnumerable<object> Past24
        {

            get { return past24; }
            //  set { eqls = new HashSet<EQ>(value.ToList()); }
        }

    }



    public sealed class Chigui
    {

        public Chigui(SyndicationItem item)
        {



            title = item.Title.NodeValue.ToString();
            author = item.ElementExtensions[2].NodeValue.ToString();
            uri = new Uri(item.ElementExtensions[6].NodeValue.ToString());
            html = item.Summary.NodeValue;


            string webimg = html.Trim();
            int ind = webimg.IndexOf('"');
            webimg = webimg.Substring(ind + 1);
            ind = webimg.IndexOf('"');
            webimg = webimg.Substring(0, ind);
            //      coord = item.ElementExtensions[1].NodeValue.ToString();
            //    string[] geo = coord.Split(' ');



            this.iD = item.Id.ToString().Trim();
            this.eventDate = item.LastUpdatedTime.ToUniversalTime();

            //   this.latitude = Convert.ToDouble(geo[0]);
            //   this.longitude = Convert.ToDouble(geo[1]);
            this.imgUri = webimg.Trim();
            //    this.elevation = Convert.ToDouble(item.ElementExtensions[2].NodeValue);
            //   this.magnitude = Convert.ToDouble(magLoc[0]);
            //   this.location = magLoc[1].Trim() + ", " + magLoc[2].Trim();

            //     string aux = item.Links.FirstOrDefault().Uri.ToString();
            //   int a = aux.LastIndexOf('/');
            //   this.uri = new Uri("http://earthquake.usgs.gov/earthquakes/eventpage" + aux.Substring(a).Replace(".php", "#summary"));

            this.nr = 0;

        }

        private string title;
        private string html;
        private string author;


        private int nr = 0;
        public int Nr
        {
            get { return nr; }
            set { nr = value; }
        }

        private Uri uri = null;
        private string imgUri;
        //  private string location;
        // private double latitude;
        // private double longitude;
        // private double magnitude;
        private DateTimeOffset eventDate;

        //  private double elevation;
        private string iD;




        public string[] Summary
        {
            get
            {
                string[] lines = new string[5];
                string aux = string.Empty;
                for (int i = 0; i < lines.Count(); i++) lines[i] = aux;


                /*
                                if (nr == 2)
                                {

                                    lines[0] = this.Magnitude + " M <- " + since + tf;
                                    lines[1] = this.Location;

                                    lines[2] = NS + this.Latitude + EW + this.Longitude + " at " + elev + si;
                                    lines[3] = this.EventDate.UtcDateTime.ToString();
                                    //  lines[4] = this.SinceMin + tf + "ago"; 

                                }
                                else if (nr == 1)
                                {

                                    lines[4] = this.EventDate.UtcDateTime.ToString();
                                    lines[1] = Decimal.Floor(Convert.ToDecimal(this.Magnitude)).ToString() + "+";
                                    lines[0] = this.Location;
                                    lines[2] = since + tf + "ago";
                                    lines[3] = NS + this.Latitude + EW + this.Longitude + " at " + elev + si;
                                    //  lines[5] = since + tf + "ago"; 
                                }
                                else
                                {
                                    string title = this.Magnitude + " M" + " <- " + since + tf + "ago";
                                    string sub = this.Location;

                                    if (nr == 0)
                                    {
                                        //   lines[0] = sub;
                                        //   lines[1] = title;


                                        lines[1] = title;
                                        lines[0] = sub;

                                        //    lines[0] = this.Magnitude + " M" + " <- " + since + tf + "ago"; 

                                    }
                                    else
                                    {
                                        lines[0] = title;
                                        lines[1] = sub;

                                        //  lines[0] = this.Magnitude + " M";
                                        //    lines[1] = this.Location + " <- " + since + tf + "ago"; 
                                    }

                                    if (elev > 0) lines[3] = "Elevation: ";
                                    else lines[3] = "Depth: ";
                                    elev = Math.Abs(elev);

                                    lines[3] += elev + sis;
                                    lines[2] = NS + this.Latitude + EW + this.Longitude;
                                    lines[4] = this.EventDate.UtcDateTime.ToString();
                                }
 
                 */
                return lines;
            }

        }



        public DateTimeOffset EventDate
        {
            get { return eventDate; }
            set
            {
                eventDate = value;
            }

        }

        public Uri Uri
        {
            get { return uri; }
            set
            {
                uri = value;
            }

        }
        public string ImgUri
        {
            get { return imgUri; }
            set
            {
                imgUri = value;
            }

        }
        public string Author
        {
            get { return author; }


        }

        public string Title
        {
            get { return title; }


        }
        public string Html
        {
            get { return html; }


        }



        public string ID
        {
            get { return iD; }
            set
            {
                iD = value;
            }

        }


        private object notification;




        public object Notification
        {
            get { return notification; }
            set
            {
                notification = value;
            }

        }





    }

}
