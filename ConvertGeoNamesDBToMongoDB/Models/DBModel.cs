using System;
using System.Collections.Generic;
using System.Text;

namespace ConvertGeoNamesDBToMongoDB.Models
{
    [Serializable]
    public class DBModel
    {
        public List<Country> Countries { get; set; }
    }
}
