using ConvertGeoNamesDBToMongoDB.Converter;
using System;
using System.Threading.Tasks;

namespace ConvertGeoNamesDBToMongoDB
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("usage: programm.exe initDbDirectory [citiesDBName]");
                return;
            }

            string initDirectory = args[0];
            string citiesDBName = string.Empty;
            if (args.Length == 2)
                citiesDBName = args[1];
            DBConverter converter = new DBConverter(initDirectory);
            if(!string.IsNullOrEmpty(citiesDBName))
            {
                converter.SetCitiesDBName(citiesDBName);
            }
            Console.WriteLine("Work begin");
            await converter.CreateJsonFromDBFiles();
        }
    }
}
