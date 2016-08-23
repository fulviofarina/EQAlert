using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace EQAlert
{
    public sealed partial class ucSettings : UserControl
    {
        private EQ lastone = null;
        private bool writeSetts = false;
        private DispatcherTimer savertimer = null;

        public ucSettings()
        {
            this.InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 2);
            timer.Tick += timer_Tick;

            savertimer = new DispatcherTimer();
            savertimer.Interval = new TimeSpan(0, 0, 5);
            savertimer.Tick += timer_Tick;

            loading = true;

            if (App.Clase != null)
            {
                LoadSettings(App.Clase.ConfigData);

                EQ primo = App.Clase.Data.EqLS.FirstOrDefault();
                if (primo != null)
                {
                    if (primo != lastone)
                    {
                        //  wview.Navigate(primo.Uri);
                        lastone = primo;
                    }
                }
            }
            loading = false;

            timer.Start();
        }

        public void WriteSettings()
        {
            if (loading) return;

            try
            {
                //    StorageFolder storageFolder =  DownloadsFolder;

                string TaskName = "EQAlertBackgroundClass";

                uint u = Convert.ToUInt32(this.runBkg.Text);
                if (u < 15)
                {
                    this.runBkg.Text = "15";
                    return;
                }
                int ce = Convert.ToInt32(this.checkEQ.Text);
                if (ce < 10)
                {
                    this.checkEQ.Text = "10";
                    return;
                }
                ce = Convert.ToInt32(this.minToast.Text);
                if (ce < 1)
                {
                    this.minToast.Text = "1";
                    return;
                }
                ce = Convert.ToInt32(this.magToast.Text);
                if (ce < 1)
                {
                    this.magToast.Text = "1";
                    return;
                }
                ce = Convert.ToInt32(this.mag.Text);
                if (ce < 1)
                {
                    this.mag.Text = "1";
                    return;
                }
                ce = Convert.ToInt32(this.radio.Text);
                if (ce < 1)
                {
                    this.radio.Text = "1";
                    return;
                }
                ce = Convert.ToInt32(this.minAllSince.Text);
                if (ce < 1)
                {
                    minAllSince.Text = "1";
                    return;
                }
                else if (ce > 1440)
                {
                    //     minAllSince.Text = "1440";
                    //  return;
                }

                int w = 0;
                int s = 0;
                int o = 0;
                int si = 0;
                int rb = 0;
                if ((bool)this.world.IsChecked) w = 1;
                if ((bool)this.noSound.IsChecked) s = 1;
                if ((bool)this.ordbyMag.IsChecked) o = 1;
                if ((bool)this.SIUnit.IsChecked) si = 1;
                if ((bool)this.runbackbox.IsChecked) rb = 1;

                string content = this.checkEQ.Text + "," + this.tilerefresh.Text + "," + this.minToast.Text + "," + this.mag.Text + ",";
                content += this.magToast.Text + "," + this.radio.Text + "," + w + "," + minAllSince.Text + "," + s + "," + o + ",";
                content += lat.Text + "," + lon.Text + "," + accu.Text + "," + App.Clase.LastLoad.ToUniversalTime().ToString() + ",";
                content += si + "," + rb + "," + this.runBkg.Text;

                App.Clase.ConfigData = content;

                App.RegisterIf(TaskName, Convert.ToBoolean(rb), u);

                App.Clase.ClearTiles();

                App.LaunchTimers();
            }
            catch (Exception ex)
            {
            }
        }

        private void timer_Tick(object sender, object e)
        {
            DispatcherTimer t = (DispatcherTimer)sender;
            t.Stop();

            if (t.Equals(savertimer))
            {
                if (writeSetts)
                {
                    WriteSettings();
                    writeSetts = false;
                }
            }

            this.status.Text = App.Clase.Status;

            t.Start();
        }

        private bool loading = true;

        public bool LoadSettings(string configData)
        {
            string settitle = "Settings";
            string loaded = "loaded correctly";
            bool ok = false;

            if (configData.Equals(string.Empty))
            {
                this.status.Text = settitle + " are empty";
                return ok;
            }

            this.near.Checked -= near_Checked;
            this.world.Checked -= near_Checked;

            try
            {
                Int16 aux = 0;
                string[] arr = configData.Split(',');

                checkEQ.Text = arr[0];
                tilerefresh.Text = arr[1];
                minToast.Text = arr[2];
                mag.Text = arr[3];
                magToast.Text = arr[4];
                radio.Text = arr[5];
                aux = Convert.ToInt16(arr[6]);
                world.IsChecked = Convert.ToBoolean(aux);
                this.minAllSince.Text = arr[7];
                aux = Convert.ToInt16(arr[8]);
                this.noSound.IsChecked = Convert.ToBoolean(aux);
                aux = Convert.ToInt16(arr[9]);
                this.ordbyMag.IsChecked = Convert.ToBoolean(aux);
                this.lat.Text = arr[10];
                this.lon.Text = arr[11];
                this.accu.Text = arr[12];
                ///datetime is 13
                aux = Convert.ToInt16(arr[14]);
                this.SIUnit.IsChecked = Convert.ToBoolean(aux);
                aux = Convert.ToInt16(arr[15]);
                this.runbackbox.IsChecked = Convert.ToBoolean(aux);
                this.runBkg.Text = arr[16];

                if ((bool)world.IsChecked) this.near.IsChecked = false;
                //    else this.near.IsChecked = true;
                this.status.Text = settitle + " " + loaded;
                ok = true;
            }
            catch (Exception ex)
            {
                this.status.Text = settitle + " not " + loaded;
                return ok;
            }

            this.near.Checked += near_Checked;
            this.world.Checked += near_Checked;
            return ok;
        }

        private void EQ_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (loading) return;

            writeSetts = true;
            savertimer.Stop();
            savertimer.Start();
        }

        private async void near_Checked(object sender, RoutedEventArgs e)
        {
            if (loading) return;

            if (sender.Equals(near))
            {
                if (world == null) return;

                world.IsChecked = false;

                await App.Clase.GetPosition();
                //  await EQAlerter.GetPosition();
                if (App.Clase.LLA != null)
                {
                    lat.Text = App.Clase.LLA[0].ToString();
                    lon.Text = App.Clase.LLA[1].ToString();
                    accu.Text = App.Clase.LLA[2].ToString();
                }
                EQ_TextChanged(sender, null);
            }
            else if (sender.Equals(world))
            {
                if (near == null) return;
                App.Clase.LLA = null;
                near.IsChecked = false;

                EQ_TextChanged(sender, null);
            }
        }
    }
}