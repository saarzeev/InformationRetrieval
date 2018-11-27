
using Newtonsoft.Json.Linq;
using RestSharp;
using System;


namespace Model2
{
    class City : IComparable
    {
        private string _city;
        private string _country = "";
        private string _currency = "";
        private string _population = "";

        public City(string city)
        {
            this._city = city;

            try
            {
                var client = new RestClient("https://restcountries.eu");
                var request = new RestRequest("rest/v2/capital/" + _city, Method.GET);

                // execute the request
                var response = client.Execute(request);
                string content = response.Content;
                JArray joResponse = JArray.Parse(content);
                _country = joResponse[0]["name"].ToString();
                _population = joResponse[0]["population"].ToString();
                _currency = joResponse[0]["currencies"][0]["code"].ToString();
            }catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
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
            return "[" +
                    _city + ", " +
                    _country + ", " +
                    _currency + ", " +
                    _population + "]";
        }
    }
}
