using System;
using System.Collections.Generic;
using System.Text;

namespace ConvertGeoNamesDBToMongoDB.Models
{
    public class State
    {
        public string StateName { get; set; }
        public string StateAsciiName { get; set; }
        public string StateCode { get; set; }
        public List<District> Districts { get; set; }
        public List<City> Cities { get; set; }
    }
}
