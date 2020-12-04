using System;
using System.Collections.Generic;
using System.Text;

namespace ConvertGeoNamesDBToMongoDB.Models
{
    [Serializable]
    public class City
    {
        public string CityAsciiName { get; set; }
        public string CityName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string CountryCode { get; set; }
        public string AdminCode1 { get; set; }
        public string AdminCode2 { get; set; }
        public string TimeZone { get; set; }
    }
}
