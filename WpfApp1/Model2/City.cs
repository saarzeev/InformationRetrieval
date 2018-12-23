
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
        private string _pop = "";
        private string _currency = "";
        private string _country = "";
        public City(string city, string pop = "", string currency = "", string country = "")
        {
            _city = city;
            _pop = pop;
            _currency = currency;
            _country = country;
            if(City.citiesInfo == null)
            {
                City.citiesInfo = new Dictionary<string, City>();
            }
            if (!City.citiesInfo.ContainsKey(_city))
            {
                City.citiesInfo.Add(_city, this);
            }
        }

        public static Dictionary<string, City> citiesInfo;
        public static bool isInit = false;

        public string GetCity { get => _city; set => _city = value; }
        public string GetPop { get => _pop; set => _pop = value; }
        public string GetCurrency { get => _currency; set => _currency = value; }
        public string GetCountry { get => _country; set => _country = value; }

        public static void initCities()
        {
            citiesInfo = new Dictionary<string, City>();
            try
            {

                var client = new RestClient("https://restcountries.eu");
                var request = new RestRequest("rest/v2/all?fields=capital;name;population;currencies", Method.GET);

               
                var response = client.Execute(request);
                string content = response.Content;
                JArray joResponse = JArray.Parse(content);
               
                isInit = true;
                foreach (JObject item in joResponse)
                {
                    try
                    {
                        string city = item["capital"].ToString().ToUpper();
                        string country = item["name"].ToString();
                        string population = item["population"].ToString();
                        string currencies = item["currencies"][0]["code"].ToString();

                        string[] popArr = new string[] { population };
                        if (!City.citiesInfo.ContainsKey(city))
                        {
                            new City(city, Parse.Instance().ParseNumbers(0, popArr, new HashSet<int>())[0], currencies, country);
                        }
                    }
                    catch { }
                }

            }
            catch (Exception e) { }
        }
        public override int GetHashCode()
        {
            return _city.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return _city.Equals(obj);
        }

        public int CompareTo(object obj)
        {
            if(obj!= null && obj is City) {
                return _city.CompareTo(((City)obj).GetCity);
            }
            return -1;
        }
    }
}
