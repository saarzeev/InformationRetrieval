
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model2
{
    public class City : IComparable
    {
        private string _city;
        private string _country = "";
        private string _currency = "";
        private string _population = "";

        public static Dictionary<string, Tuple<string, string, string>> citiesInfo;
        public static bool isInit = false;
        public static void initCities()
        {
            citiesInfo = new Dictionary<string, Tuple<string, string, string>>();
            try
            {

                var client = new RestClient("https://restcountries.eu");
                var request = new RestRequest("rest/v2/all?fields=capital;name;population;currencies", Method.GET);

                // execute the request
                var response = client.Execute(request);
                string content = response.Content;
                JArray joResponse = JArray.Parse(content);
                //_country = joResponse[0]["name"].ToString();
                //_population = joResponse[0]["population"].ToString();
                //_currency = joResponse[0]["currencies"][0]["code"].ToString();
                isInit = true;
                foreach (JObject item in joResponse)
                {
                    try
                    {
                        string city = item["capital"].ToString().ToUpper();
                        string country = item["name"].ToString();
                        string population = item["population"].ToString();
                        string currencies = item["currencies"][0]["code"].ToString();

                        citiesInfo.Add(city, new Tuple<string, string, string>(country, population, currencies));
                    }
                    catch { }                
                }

            }
            catch (Exception e) { }
        }

        public City(string city)
        {
            if (!isInit)
            {
                initCities();
            }

            _city = city.ToUpper();
            if (citiesInfo.ContainsKey(_city))
            {
                Tuple<string, string, string> info = citiesInfo[_city];
                _country = info.Item1;
                _population = info.Item2;
                _currency = info.Item3;
            }
            //TODO - Pass population through the parseBigNumbers();
        }

        public int CompareTo(object obj)
        {
            return _city.CompareTo(obj);
        }

        public override bool Equals(object obj)
        {
            if(obj is City)
            {
                return (((City)obj)._city.Equals(this._city));
            }
            return false;
        }
        public override int GetHashCode()
        {
            return _city.GetHashCode();
        }
        public override string ToString()
        {
            return 
                    _city + "," +
                    _country + "," +
                    _currency + "," +
                    _population;
        }

        /// <summary>
        /// Returns the city's name
        /// </summary>
        /// <returns>The city's name</returns>
        public string getCity()
        {
            return _city;
        }
    }
}
