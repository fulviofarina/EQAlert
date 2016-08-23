using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.ApplicationModel.Background;
namespace EQAlert
{
    public sealed class EQBKG : IBackgroundTask
    {


        private static string friendName = "EQAlertBackgroundClass";
        public static string FriendName
        {
            get { return friendName; }
            set { friendName = value; }

        }


        private static string classdName = "EQAlert.EQBKG";

        public static string ClassName
        {
            get { return classdName; }
            set { classdName = value; }

        }


        public EQBKG()
        {
        }



        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            try
            {

                EQAlerter a = new EQAlerter();
                await a.ReadSettings();
                a.LoadSettings();
                await a.Data.GetList(a.Setting);
                a.DoCheck();
                a.DoTiles();

                a = null;


            }
            finally { deferral.Complete(); }


        }







    }


}
