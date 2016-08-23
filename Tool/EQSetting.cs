using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQAlert
{
    public sealed class EQSetting
    {




        public EQSetting()
        {

        }
        private double radio = 180;
        public double Radio
        {
            get { return radio; }
            set
            {
                radio = value;
            }

        }
        public int CheckEq
        {
            get { return checkEq; }
            set
            {
                checkEq = value;
            }

        }

        private int checkEq = 10;

        public double ExpireMin
        {
            get { return expireMin; }
            set
            {
                expireMin = value;
            }

        }

        private double expireMin = 1;

        public bool World
        {
            get { return world; }
            set
            {
                world = value;
            }

        }

        private bool noSound;
        public bool NoSound
        {
            get { return noSound; }
            set
            {
                noSound = value;
            }

        }
        private bool sI;
        public bool SI
        {
            get { return sI; }
            set
            {
                sI = value;
            }

        }


        private bool world = true;
        private double minToast = 30;
        private double filterMag = 1;
        private double filterMagToast = 5;

        private bool orderbyMag = false;
        public bool OrderbyMag
        {
            get { return orderbyMag; }
            set
            {
                orderbyMag = value;
            }

        }
        private bool runBkg = false;
        private int bkgInterval = 15;
        public bool RunBkg
        {
            get { return runBkg; }
            set
            {
                runBkg = value;
            }

        }
        public int BkgInterval
        {
            get { return bkgInterval; }
            set
            {
                bkgInterval = value;
            }

        }



        private double minTime = 1440;
        public double MinTime
        {
            get { return minTime; }
            set
            {
                minTime = value;
            }

        }


        public double MinToast
        {
            get { return minToast; }
            set
            {
                minToast = value;
            }

        }
        public double FilterMag
        {
            get { return filterMag; }
            set
            {
                filterMag = value;
            }

        }
        public double FilterMagToast
        {
            get { return filterMagToast; }
            set
            {
                filterMagToast = value;
            }

        }
    }

}
