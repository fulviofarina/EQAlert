using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace EQAlert
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        private static Chiguirator chrator;

        private static EQAlerter clase;

        public static EQAlerter Clase
        {
            get { return clase; }
            set { clase = value; }
        }

        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            // this.world.IsChecked = true;
            //  this.near.IsChecked = false;

            //put settings
            clase = new EQAlerter();

            chrator = new Chiguirator();

            timer = new DispatcherTimer();
            timer.Tick += timer_Tick;

            timerTile = new DispatcherTimer();
            timerTile.Tick += timerTile_Tick;
        }

        public static void RegisterIf(string TaskName, bool force, uint minTime)
        {
            IBackgroundTrigger trigger = new TimeTrigger(minTime, false);
            bool regis = BKG.FindIfRegistered(TaskName, true);

            if (!regis && force)
            {
                BKG.Build(EQBKG.FriendName, EQBKG.ClassName, trigger);
            }
        }

        public static void LaunchTimers()
        {
            timer.Stop();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();

            timerTile.Stop();
            timerTile.Interval = new TimeSpan(0, 0, 3);
            timerTile.Start();
        }

        private static DispatcherTimer timer = null;
        private static DispatcherTimer timerTile = null;

        private async void timerTile_Tick(object sender, object e)
        {
            timerTile.Stop();

            //     await chrator.Data.GetList();

            //  chrator.DoTiles();
            await clase.Data.GetList(clase.Setting);

            clase.DoTiles();

            timerTile.Interval = new TimeSpan(0, Convert.ToInt32(clase.Setting.ExpireMin), 0);

            timerTile.Start();
        }

        private async void timer_Tick(object sender, object e)
        {
            timer.Stop();

            await clase.Data.GetList(clase.Setting);

            clase.DoCheck();

            timer.Interval = new TimeSpan(0, 0, clase.Setting.CheckEq);
            timer.Start();
        }

        /*
        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(MainPage), args.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
        */

        private async void OnSuspending(object sender, SuspendingEventArgs args)
        {
            SuspendingDeferral deferral = args.SuspendingOperation.GetDeferral();
            await clase.WriteSettings();
            deferral.Complete();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        ///
        private LaunchActivatedEventArgs launchArgs = null;

        public LaunchActivatedEventArgs LaunchArgs
        {
            get { return launchArgs; }
            set { launchArgs = value; }
        }

        async protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            this.launchArgs = args;

            ApplicationExecutionState state = args.PreviousExecutionState;

            // if (state == ApplicationExecutionState.Terminated || state == ApplicationExecutionState.NotRunning || state == ApplicationExecutionState.)
            {
                // Do an asynchronous restore
            }

            if (Window.Current.Content == null)
            {
                var rootFrame = new Frame();
                //  rootFrame.Navigate(typeof(MainPage));
                Window.Current.Content = rootFrame;
                //   if (!rootFrame.Navigate(typeof(SA.ItemsPage), "AllGroups"))
                {
                    // throw new Exception("Failed to create initial page");
                }

                await clase.ReadSettings();

                clase.LoadSettings();

                await clase.Data.GetList(clase.Setting);

                LaunchTimers();

                CreateGroups();

                rootFrame.Navigate(typeof(ItemsP), "AllGroups");
            }
            Window.Current.Activate();
        }

        public static void CreateGroups()
        {
            string id = "PastHour";
            string title = "Past hour Eq's";
            string subtitle = "Subtitle";

            string description = "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante";

            string id2 = "Past6";
            string title2 = "Past 6 hours Eq's";
            string subtitle2 = "Subtitle";

            string description2 = "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante";

            IList<EQ> aux = App.clase.Data.EqLS.ToList();

            Data.SampleDataSource.Fill(aux, id, title, subtitle, description);

            aux = App.clase.Data.Past24.ToList();

            int sixhours = 6 * 60;
            IList<EQ> aux2 = aux.Where(o => o.SinceMin <= sixhours).ToList();
            Data.SampleDataSource.Fill(aux2, id2, title2, subtitle2, description2);

            int Twelvehours = 12 * 60;
            aux2 = aux.Except(aux2).Where(o => o.SinceMin <= Twelvehours).ToList();
            string id3 = "Past12";
            string title3 = "Past 12 hours Eq's";
            string subtitle3 = "Subtitle";
            string description3 = "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante";
            Data.SampleDataSource.Fill(aux2, id3, title3, subtitle3, description3);

            aux = null;
            aux2 = null;
        }
    }
}