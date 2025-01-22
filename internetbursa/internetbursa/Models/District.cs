using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace internetbursa.Models
{
    public class District
    {
        public int IlceId { get; set; }
        public string IlceAd { get; set; }

        public string Link { get; set; } // İlçeye ait tanıtım sitesi linki
    }
}