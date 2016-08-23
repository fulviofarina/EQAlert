using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Web.Syndication;
using Windows.Foundation;

namespace EQAlert
{
    public sealed class EQData
    {

        private Func<SyndicationItem, bool> createFeedAdder(Int16 mode, double minTime)
        {


            HashSet<EQ> output = null;

            if (mode == 1) output = this.eqls;
            else if (mode == 2) output = this.past24;



            Func<SyndicationItem, bool> feedAdder = o =>
            {
                EQ eq = null;
                bool added = false;
                try
                {
                    eq = new EQ(o);
                    if (eq.SinceMin <= minTime)
                    {
                        EQ find = output.FirstOrDefault(a => a.ID == eq.ID);
                        if (find == null)
                        {
                            output.Add(eq);
                            added = true;
                        }
                        else eq = null;
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
        public IAsyncAction GetList(EQSetting set)
        {

            return getList(set).AsAsyncAction();
        }

        public EQData()
        {
            uri = new Uri[2];


            uri[0] = new Uri("http://earthquake.usgs.gov/earthquakes/feed/v1.0/summary/all_hour.atom");

            uri[1] = new Uri("http://earthquake.usgs.gov/earthquakes/feed/v1.0/summary/all_day.atom");


            feeder = new Feeder();


            eqls = new HashSet<EQ>();



            past24 = new HashSet<EQ>();



        }


        private Feeder feeder = null;


        private async Task<bool> getList(EQSetting set)
        {



            feeder.Uri = uri[0];
            feeder.AdderMethod = createFeedAdder(1, set.MinTime/60);
            await feeder.SetFunc();


            feeder.Uri = uri[1];
            feeder.AdderMethod = createFeedAdder(2, set.MinTime/60);
            await feeder.SetFunc();


            eqls = new HashSet<EQ>(eqls.OrderBy(o => o.SinceMin).ToList()); //filter base on minToast
            eqls.TrimExcess();


            IEnumerable<EQ> aux = past24.OrderBy(o => o.SinceMin).ToList().Except(eqls).ToList();
            past24 = new HashSet<EQ>(aux.ToList()); //filter base on minToast

            past24.TrimExcess();





            return eqls.Count > 0;
        }


        public Uri[] UriPath
        {

            get { return uri; }
            set { uri = value; }
        }

        private Uri[] uri = null;
        private HashSet<EQ> eqls = null;

        private HashSet<EQ> past24 = null;


        public IEnumerable<EQ> EqLS
        {

            get { return eqls; }
            //  set { eqls = new HashSet<EQ>(value.ToList()); }
        }
        public IEnumerable<EQ> Past24
        {

            get { return past24; }
            //  set { eqls = new HashSet<EQ>(value.ToList()); }
        }

    }

}
