using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Web.Syndication;

namespace EQAlert
{
    public sealed class Feeder
    {
        public Feeder()
        {

            client = new SyndicationClient();
            client.BypassCacheOnRetrieve = false;
            client.SetRequestHeader("user-agent", "USER_AGENT");


        }


        private string status = string.Empty;
        private SyndicationFeed f = null;
        private SyndicationClient client = null;
        private Uri uri1;
        private object funcAdder = null;

        private async Task<bool> getfeed(Func<SyndicationItem, bool> feedAdd, Uri uri1)
        {
            try
            {
                setStatus("Feeding", false, false);

                f = await client.RetrieveFeedAsync(uri1);

                f.Items.Where(feedAdd).ToList();

            }
            catch (Exception ex)
            {
                setStatus(ex.Message, false, true);

            }


            if (f == null)
            {
                setStatus("Feed is empty", false, true);
                return false;
            }
            else return true;

        }
        private void setStatus(string header, bool starting, bool failed)
        {
            string ppp = "...";
            this.status = header + " starting" + ppp;
            if (failed) this.status = header + " failed!" + ppp;
            else if (!starting) this.status = header + " ok!";

        }




        public SyndicationClient Client
        {

            get { return client; }
            set { client = value; }
        }
        public SyndicationFeed Feed
        {

            get { return f; }
            set { f = value; }
        }
    
   
        public IAsyncOperation<bool> SetFunc ()
        {

                return getfeed( (Func<SyndicationItem, bool>) funcAdder, uri1).AsAsyncOperation<bool>();
            
          
        }
         public  object AdderMethod
        {
            get { return funcAdder; }
            set
            {
                funcAdder = value;

            }
        }

        public Uri Uri
        {
            get { return uri1; }

            set
            {
                uri1 = value;
              
            }
        }
     

    }
}
