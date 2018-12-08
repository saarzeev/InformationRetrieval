
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model2
{
    public class City
    {

        public static Dictionary<string, Tuple<string, string, string>> citiesInfo;
        public static bool isInit = false;
        public static void initCities()
        {
            citiesInfo = new Dictionary<string, Tuple<string, string, string>>();
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
                        citiesInfo.Add(city, new Tuple<string, string, string>(country, population, currencies));
                    }
                    catch { }
                }

            }
            catch (Exception e) { }
        }
    }
}
