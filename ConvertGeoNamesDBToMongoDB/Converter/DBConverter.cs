using ConvertGeoNamesDBToMongoDB.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConvertGeoNamesDBToMongoDB.Converter
{
    public class DBConverter
    {
        private string _dbDirectory;

        public DBConverter(string dbDirectory)
        {
            if(!Directory.Exists(dbDirectory))
            {
                throw new Exception("Init directory not exist");
            }
            if(!File.Exists($"{dbDirectory}/admin1CodesASCII.txt") ||
                !File.Exists($"{dbDirectory}/admin2Codes.txt") ||
                !File.Exists($"{dbDirectory}/cities500.txt") ||
                !File.Exists($"{dbDirectory}/countryInfo.txt"))
            {
                throw new Exception("Not all need DB files exist");
            }
            _dbDirectory = dbDirectory;
        }

        public async Task CreateJsonFromDBFiles()
        {
            //подготовим промежуточные данные
            Dictionary<string, Country> countryMap = new Dictionary<string, Country>();
            Dictionary<string, State> stateMap = new Dictionary<string, State>();
            Dictionary<string, District> districtMap = new Dictionary<string, District>();

            int primaryIndex = 0;
            using (var file = new FileStream($"{_dbDirectory}/countryInfo.txt", FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(file))
                {
                    string currentCountry = "";
                    do
                    {
                        currentCountry = await reader.ReadLineAsync();
                        if (!string.IsNullOrEmpty(currentCountry) && currentCountry[0] != '#')
                        {
                            var tables = currentCountry.Split('\t');
                            countryMap.Add(tables[0], new Country
                            {
                                Id = primaryIndex++,
                                CountryISOCode = tables[0],
                                CountryName = tables[4],
                                States = new List<State>(),
                                Cities = new List<City>()
                            });
                        }
                    } while (!string.IsNullOrEmpty(currentCountry));
                }
            }

            using (var file = new FileStream($"{_dbDirectory}/admin1CodesASCII.txt", FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(file))
                {
                    string currentState = "";
                    do
                    {
                        currentState = await reader.ReadLineAsync();
                        if (!string.IsNullOrEmpty(currentState) && currentState[0] != '#')
                        {
                            var tables = currentState.Split('\t');
                            stateMap.Add(tables[0], new State
                            {
                                Id = primaryIndex++,
                                StateCode = tables[0].Split('.')[1],
                                StateName = tables[1],
                                StateAsciiName = tables[2],
                                Districts = new List<District>(),
                                Cities = new List<City>()
                            });
                        }
                    } while (!string.IsNullOrEmpty(currentState));
                }
            }

            using (var file = new FileStream($"{_dbDirectory}/admin2Codes.txt", FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(file))
                {
                    string currentDistrict = "";
                    do
                    {
                        currentDistrict = await reader.ReadLineAsync();
                        if (!string.IsNullOrEmpty(currentDistrict) && currentDistrict[0] != '#')
                        {
                            var tables = currentDistrict.Split('\t');
                            districtMap.Add(tables[0], new District
                            {
                                Id = primaryIndex++,
                                DistrictCode = tables[0].Split('.')[2],
                                DistrictName = tables[1],
                                DistrictAsciiName = tables[2],
                                Cities = new List<City>()
                            });
                        }
                    } while (!string.IsNullOrEmpty(currentDistrict));
                }
            }

            //далее читаем основной файл с городами и заполняем класс DBModel
            DBModel dBModel = new DBModel();
            dBModel.Countries = new List<Country>();
            var separator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator[0];
            
            using (var file = new FileStream($"{_dbDirectory}/cities500.txt", FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(file))
                {
                    string currentCity = "";
                    do
                    {
                        currentCity = await reader.ReadLineAsync();
                        if (!string.IsNullOrEmpty(currentCity) && currentCity[0] != '#')
                        {
                            var tables = currentCity.Split('\t');
                            //получили данные о городе, заполним 
                            try
                            {
                                City newCity = new City
                                {
                                    Id = primaryIndex++,
                                    CityName = tables[1],
                                    CityAsciiName = tables[2],
                                    Latitude = Convert.ToDouble(tables[4].Replace('.', separator)),
                                    Longitude = Convert.ToDouble(tables[5].Replace('.', separator)),
                                    CountryCode = tables[8],
                                    AdminCode1 = tables[10],
                                    AdminCode2 = tables[11],
                                    TimeZone = tables[17]
                                };
                                //ищем страну
                                if (countryMap.ContainsKey(newCity.CountryCode))
                                {
                                    var country = dBModel.Countries.Where(c => c.CountryISOCode == newCity.CountryCode).FirstOrDefault();
                                    if (country == null)
                                    {
                                        //добавим страну в список
                                        dBModel.Countries.Add(countryMap[newCity.CountryCode]);
                                        country = countryMap[newCity.CountryCode];
                                    }
                                    if (!string.IsNullOrEmpty(newCity.AdminCode1))
                                    {
                                        //город относится к области или штату
                                        //ищем область или штат
                                        if (stateMap.ContainsKey($"{newCity.CountryCode}.{newCity.AdminCode1}"))
                                        {
                                            var state = country.States.Where(c => c.StateCode == newCity.AdminCode1).FirstOrDefault();
                                            if(state == null)
                                            {
                                                //добавим штат или область в список
                                                country.States.Add(stateMap[$"{newCity.CountryCode}.{newCity.AdminCode1}"]);
                                                state = stateMap[$"{newCity.CountryCode}.{newCity.AdminCode1}"];
                                            }
                                            if(!string.IsNullOrEmpty(newCity.AdminCode2))
                                            {
                                                //город относится к району, найдем район
                                                if (districtMap.ContainsKey($"{newCity.CountryCode}.{newCity.AdminCode1}.{newCity.AdminCode2}"))
                                                {
                                                    var district = state.Districts.Where(c => c.DistrictCode == newCity.AdminCode2).FirstOrDefault();
                                                    if(district == null)
                                                    {
                                                        //добавим район в список
                                                        state.Districts.Add(districtMap[$"{newCity.CountryCode}.{newCity.AdminCode1}.{newCity.AdminCode2}"]);
                                                        district = districtMap[$"{newCity.CountryCode}.{newCity.AdminCode1}.{newCity.AdminCode2}"];
                                                    }
                                                    //добавим город в район
                                                    district.Cities.Add(newCity);
                                                }    
                                            }
                                            else
                                            {
                                                //город относится к области просто добавим в область
                                                state.Cities.Add(newCity);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //город не относится к штату или области
                                        //просто добавим в список страны
                                        country.Cities.Add(newCity);
                                    }

                                }
                            }
                            catch (Exception ex)
                            {
                                ////TODO вставить логирование!!!
                                Console.WriteLine($"ERROR! {ex.Message} ({ex.StackTrace})");
                            }
                        }
                    } while (!string.IsNullOrEmpty(currentCity));
                }
            }
            using (FileStream fs = new FileStream("resultDB.json", FileMode.OpenOrCreate))
            {                
                await JsonSerializer.SerializeAsync<List<Country>>(fs, dBModel.Countries, new JsonSerializerOptions { 
                    WriteIndented = true,                    
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
                Console.WriteLine("Data has been saved to file");
            }
        }
    }
}
