using System;
using System.Collections.Generic;
using System.Text;

namespace ConvertGeoNamesDBToMongoDB.Models
{
    [Serializable]
    public class District
    {
        public string DistrictName { get; set; }
        public string DistrictAsciiName { get; set; }
        public string DistrictCode { get; set; }
        public List<City> Cities { get; set; }
    }
}
