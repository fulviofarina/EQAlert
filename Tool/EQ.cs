using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Web.Syndication;


namespace EQAlert
{
   

    public sealed class EQ
    {

        public EQ(SyndicationItem item)
        {
            try
            {

             
                string reg = item.Id.Split(':')[2].Trim();
                string tagId = item.Id.Split(':')[3].Trim();

                this.iD = reg + ":" + tagId;
                this.eventDate = item.LastUpdatedTime.ToUniversalTime();



            }
            catch (Exception ex)
            {

            }

            try
            {
         
            string[] magLoc = item.Title.Text.Substring(2).Split(',');
          
                this.location = magLoc[1].Trim() + ", " + magLoc[0].Split('-')[1].Trim();

                this.magnitude = Convert.ToDouble(magLoc[0].Split('-')[0].Trim());




            }
            catch (Exception ex)
            {
             
            }


           
            this.nr = 0;


            try
           {
      
            string coord = string.Empty;
            coord = item.ElementExtensions[0].NodeValue.ToString();
            string[] geo = coord.Split(' ');
            this.latitude = Convert.ToDouble(geo[0].Trim());
            this.longitude = Convert.ToDouble(geo[1].Trim());

                this.elevation = Convert.ToDouble(item.ElementExtensions[1].NodeValue);

            }
            catch (Exception ex)
            {
             
            }
            try
            {

         //   string aux = item.Links.FirstOrDefault().Uri.ToString();
           // int a = aux.LastIndexOf('/');
            this.uri = new Uri("http://earthquake.usgs.gov/earthquakes/eventpage/" + this.ID.Replace(":", null) +"#general_summary");

               }
            catch (Exception ex)
            {
             
            }

         
            try
            {
             //   string webimg = item.Summary.Text.Trim();
              //  webimg = item.Links.FirstOrDefault().BaseUri.ToString();
             //   int ind = webimg.IndexOf('"');
              //  webimg = webimg.Substring(ind + 1);
              //  ind = webimg.IndexOf('"');
              //  webimg = webimg.Substring(0, ind);
               this.imgUri = this.uri.ToString().Replace("#general_summary","#general_map");
            }
            catch (Exception ex)
            {
             
            }


        }

        private int nr = 0;
        public int Nr
        {
            get { return nr; }
            set { nr = value; }
        }

        private Uri uri = null;
        private string imgUri;
        private string location;
        private double latitude;
        private double longitude;
        private double magnitude;
        private DateTimeOffset eventDate;
        public double SinceMin
        {

            get
            {

                return DateTime.UtcNow.Subtract(this.EventDate.UtcDateTime).TotalMinutes;
            }

        }
        private double elevation;
        private string iD;

        private bool si = false;
        public bool SI
        {
            get { return si; }
            set { si = value; }
        }


        public string[] Summary
        {
            get
            {
                string[] lines = new string[5];
                string aux = string.Empty;
                for (int i = 0; i < lines.Count(); i++) lines[i] = aux;

                string tf = " min ";
                int since = Convert.ToInt32(this.SinceMin);
                if (since == 0)
                {
                    since = Convert.ToInt32(this.SinceMin * 60);
                    tf = " sec ";
                }
                else if (since >= 60)
                {
                    since = Convert.ToInt32(this.SinceMin / 60);
                    tf = " hr ";
                }


                string NS = "N: ";
                string EW = "E: ";
                if (latitude < 0)
                {
                    NS = "S: ";
                    latitude = Math.Abs(latitude);
                }
                if (longitude < 0)
                {
                    EW = "W: ";
                    longitude = Math.Abs(longitude);
                }
                double elev = (this.Elevation / 1000);
                string sis = " km";
                if (!si)
                {
                    elev = elev / 1.6;
                    sis = " mi";
                }


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
        public string Location
        {
            get { return location; }
            set
            {
                location = value;
            }

        }
        public double Magnitude
        {
            get { return magnitude; }
            set
            {
                magnitude = value;
            }

        }
        public double Longitude
        {
            get { return longitude; }
            set
            {
                longitude = value;
            }

        }
        public double Latitude
        {
            get { return latitude; }
            set
            {
                latitude = value;
            }

        }

        public double Elevation
        {
            get { return elevation; }
            set
            {
                elevation = value;
            }

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
