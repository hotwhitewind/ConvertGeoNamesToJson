using System;
using System.Collections.Generic;
using System.Text;

namespace ConvertGeoNamesDBToMongoDB.Models
{
    [Serializable]
    public class Country
    {
        public int Id { get; set; }
        public string CountryName { get; set; }
        public string CountryISOCode { get; set; }
        public List<State> States { get; set; }
        public List<City> Cities { get; set; }
    }
}
