using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Notifications;



namespace EQAlert

{

    public sealed class EQAlerter
    {

        public EQAlerter()
        {
            set = new EQSetting();
            data = new EQData();

            tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
            tileUpdater.EnableNotificationQueue(true);
            //  this.tileUpdater.Clear();

            toastNotifier = ToastNotificationManager.CreateToastNotifier();

            bdgupdater = BadgeUpdateManager.CreateBadgeUpdaterForApplication();
            bdgupdater.Clear();


            tags = new HashSet<string>();
        }

        public IAsyncAction GetPosition()
        {
            return getPosition().AsAsyncAction();
        }

        public bool DoCheck()
        {

            return doCheck();
        }
        public bool DoTiles()
        {
            return doTiles();
        }

        public void ClearTiles()
        {

            tileUpdater.Clear();
            this.bdgupdater.Clear();

        }

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



        #region GEO

        private Func<EQ, bool> createGeoFilter()
        {
            Func<EQ, bool> geoFilter = o =>
            {

                double distance = Geodesy.DistanceBetweenCoords(o.Latitude, o.Longitude, LLA[0], LLA[1]);
                return (distance >= set.Radio);
            };
            return geoFilter;
           
        }
   
        private void reportByLocation(ref IEnumerable<EQ> eq)
        {
            if (!set.World && LLA != null)
            {
                Func<EQ, bool> geofilter = createGeoFilter();
                eq = eq.Where(geofilter).ToList();
                geofilter = null;
            }
        }
        
        private async Task<bool> getPosition()
        {

            bool located = false;
            string section = "Location Services";

            latlonacu = null;
            Geoposition pos = null;
            Geolocator geo = null;

            try
            {
                setStatus(section, true, false);

                geo = new Geolocator();
                pos = await geo.GetGeopositionAsync();

                latlonacu = new double[3];
                latlonacu[0] = pos.Coordinate.Latitude;
                latlonacu[1] = pos.Coordinate.Longitude;
                latlonacu[2] = pos.Coordinate.Accuracy;

                if (!set.SI) latlonacu[2] = latlonacu[2] * 1.09361;

                setStatus(section, false, false);
                located = true;
            }
            catch (Exception ex)
            {
                setStatus(section, false, true);
            }

            pos = null;
            geo = null;

            return located;

        }

#endregion


        private void setStatus(string header, bool starting, bool failed)
        {
            string ppp = "...";
            this.status = header + " starting" + ppp;
            if (failed) this.status = header + " failed!" + ppp;
            else if (!starting) this.status = header + " ok!";

        }

    
        #region Tiling
        private void tile(ref EQ eq, bool img, double secDur)
        {
            string tag = string.Empty;
            tag = eq.ID.Split(':')[1];

            if (null != this.tileUpdater.GetScheduledTileNotifications().FirstOrDefault(o => o.Id.Equals(tag))) return;
            status = "Tiling...";
            try
            {
                Type schnotfi = typeof(ScheduledTileNotification);

                if (cnter >= maxCounter) cnter = 0;


                eq.Nr = cnter;
                eq.SI = set.SI;
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
        private void toast(string prefixUri, ref EQ eq, bool img)
        {


            string tag = string.Empty;

            tag = eq.ID.Substring(eq.ID.LastIndexOf(':') + 1);

            if (tags.Contains(tag)) return;

            tags.Add(tag);

            if (lastLoad.Subtract(eq.EventDate.UtcDateTime).TotalSeconds >= 0) return;

            eq.SI = set.SI;
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

        private bool doCheck()
        {

            added = false;

            string sect = "Alerter";
            setStatus(sect, true, false);
            try
            {
                totoast = data.EqLS.Where(o => o.SinceMin <= set.MinToast).Where(e => e.Magnitude >= set.FilterMagToast);

                reportByLocation(ref totoast);

                totoast = totoast.Take(3).ToList(); //take 5

                if (totoast.Count() != 0)
                {
                    Func<EQ, bool> toaster = createToaster();
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

                eqdim = data.EqLS.Where(e => e.Magnitude >= set.FilterMag);

                reportByLocation(ref eqdim);

                if (set.OrderbyMag) eqdim = eqdim.OrderBy(o => o.Magnitude).Reverse().ToList();

                eqdim = eqdim.Take(6).ToList();

                int count = eqdim.Count();

                if (count != 0)
                {

                    double secDur = (set.ExpireMin * 60) / (count);

                    deliv = DateTimeOffset.UtcNow;
                    Func<EQ, bool> tiler = createTiler(secDur);
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

        private Func<EQ, bool> createToaster()
        {

            Func<EQ, bool> toaster = eq =>
            {


                try
                {
                    toast(prefixUri, ref eq, true);

                }
                catch (Exception ex)
                {

                    try
                    {
                        toast(prefixUri, ref eq, false);

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


        private Func<EQ, bool> createTiler(double secDur)
        {


            Func<EQ, bool> tiler = eq =>
            {
                try
                {

                    tile(ref eq, true, secDur);


                }
                catch (Exception ex)
                {
                    try
                    {
                        tile(ref eq, false, secDur);


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

        async void toastnotif_Activated(ToastNotification sender, object args)
        {
            if (totoast == null) return;
            if (totoast.Count() == 0) return;

            EQ e = totoast.FirstOrDefault();
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


        public bool LoadSettings()
        {
            return loadSettings();

        }
        public IAsyncAction ReadSettings()
        {
            return readSettings().AsAsyncAction();
        }

        public IAsyncAction WriteSettings()
        {
            return writeSettings().AsAsyncAction();
        }
        public DateTimeOffset LastLoad
        {
            get { return lastLoad; }

        }



        private async Task<bool> readSettings()
        {
            string section = "Settings reader";

            bool read = false;
            try
            {
                setStatus(section, true, false);
                // string content = set.checkEq + "," + set.expireMin + "," + set.minToast + "," + set.filterMag + "," + set.filterMagToast + "," + set.radio + "," + set.world + "," + DateTime.Now.ToUniversalTime();
                StorageFile storageFile = await KnownFolders.PicturesLibrary.GetFileAsync(fileSettings);
                configData = await FileIO.ReadTextAsync(storageFile);
                // await Windows.Storage.FileIO.WriteTextAsync(sampleFile, content);
                //   saved = true;
                setStatus(section, false, false);
                read = true;

            }
            catch (Exception ex)
            {
                //saved = false;
                setStatus(section, false, true);
            }
            return read;
        }
        private async Task<bool> writeSettings()
        {
            saved = false;
            string section = "Settings saver";
            try
            {
                setStatus(section, true, false);
                //    StorageFolder storageFolder =  DownloadsFolder;
                StorageFile sampleFile = await KnownFolders.PicturesLibrary.CreateFileAsync(fileSettings, CreationCollisionOption.ReplaceExisting);
                await Windows.Storage.FileIO.WriteTextAsync(sampleFile, this.configData);
                saved = true;

                setStatus(section, false, false);
            }
            catch (Exception ex)
            {
                saved = false;
                setStatus(section, true, true);
            }

            return saved;

        }



        private DateTime lastLoad = DateTime.UtcNow.AddDays(-1);
     

        private bool loadSettings()
        {
            settingsLoaded = false;

            string section = "Settings loader";


            try
            {
                string[] arr = configData.Split(',');
                setStatus(section, true, false);

                if (arr == null || arr.Count() == 0)
                {
                    status = "Loading failed!";
                    return false;
                }

                set.CheckEq = Convert.ToInt32(arr[0]);
                set.ExpireMin = Convert.ToDouble(arr[1]);
                set.MinToast = Convert.ToDouble(arr[2]);
                set.FilterMag = Convert.ToDouble(arr[3]);
                set.FilterMagToast = Convert.ToDouble(arr[4]);
                set.Radio = Convert.ToDouble(arr[5]);
                int w = Convert.ToInt16(arr[6]);
                int s = Convert.ToInt16(arr[8]);
                int o = Convert.ToInt16(arr[9]);
                int si = Convert.ToInt16(arr[14]);
                int rb = Convert.ToInt16(arr[15]);

                set.World = Convert.ToBoolean(w);
                set.MinTime = Convert.ToDouble(arr[7]);
                set.NoSound = Convert.ToBoolean(s);
                set.OrderbyMag = Convert.ToBoolean(o);

                latlonacu = new double[3];
                latlonacu[0] = Convert.ToDouble(arr[10]);
                latlonacu[1] = Convert.ToDouble(arr[11]);
                latlonacu[2] = Convert.ToDouble(arr[12]);

                lastLoad = Convert.ToDateTime(arr[13]);
                set.SI = Convert.ToBoolean(si);

                set.RunBkg = Convert.ToBoolean(rb);
                set.BkgInterval = Convert.ToInt32(arr[16]);

                status = "Settings loaded!";

                settingsLoaded = true;
                setStatus(section, false, false);


            }
            catch (Exception ex)
            {
                setStatus(section, false, true);

            }

            return settingsLoaded;
        }
        #endregion


        #region Private Fields

        private DateTimeOffset nowcollect = DateTimeOffset.UtcNow;

        private IEnumerable<EQ> eqdim = null;
        private IEnumerable<EQ> totoast = null;
        private HashSet<string> tags = null;

        private DateTimeOffset deliv = DateTimeOffset.UtcNow.AddSeconds(2);
        private const int maxCounter = 2;
        private const string fileSettings = "EQsettings.txt";
        private const string prefixUri="";

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

        private EQSetting set = null;
        private EQData data = null;
            #endregion

     
     

    }

   
   
  

 
   


}
