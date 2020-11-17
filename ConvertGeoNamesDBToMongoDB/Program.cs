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
                Console.WriteLine("usage: programm.exe initDbDirecotry");
                return;
            }
            string initDirectory = args[0];
            DBConverter converter = new DBConverter(initDirectory);
            Console.WriteLine("Work begin");
            await converter.CreateJsonFromDBFiles();
        }
    }
}
