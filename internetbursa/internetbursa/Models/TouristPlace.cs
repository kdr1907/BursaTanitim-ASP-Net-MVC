using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace internetbursa.Models
{
    public class TouristPlace
    {
        public int YerId { get; set; }
        public string YerAd { get; set; }
        public string Aciklama { get; set; }

        public string Konum { get; set; }
    }
}