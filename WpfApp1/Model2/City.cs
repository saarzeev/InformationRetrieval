﻿
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace Model2
{
    public class City : IComparable
    {
        private string _city;
        private string _country = "";
        private string _currency = "";
        private string _population = "";


        public City(string city)
        {
            this._city = city.Split(' ')[0];
            //Task ctor = Task.Run(() => {
                try
                {

                    var client = new RestClient("https://restcountries.eu");
                    var request = new RestRequest("rest/v2/capital/" + city, Method.GET);

                    // execute the request
                    var response = client.Execute(request);
                    string content = response.Content;
                    JArray joResponse = JArray.Parse(content);
                    _country = joResponse[0]["name"].ToString();
                    _population = joResponse[0]["population"].ToString();
                    _currency = joResponse[0]["currencies"][0]["code"].ToString();

                    
                }
                catch (Exception e) { }

                if(_country == "" && city.Split(' ').Length > 1)
                {
                    try
                    {

                        var client = new RestClient("https://restcountries.eu");
                        var request = new RestRequest("rest/v2/capital/" + city.Split(' ')[0], Method.GET);

                        // execute the request
                        var response = client.Execute(request);
                        string content = response.Content;
                        JArray joResponse = JArray.Parse(content);
                        _country = joResponse[0]["name"].ToString();
                        _population = joResponse[0]["population"].ToString();
                        _currency = joResponse[0]["currencies"][0]["code"].ToString();


                    }
                    catch (Exception e) { }
                }
                else
                {
                    _city = city.Split(' ')[0];
                }
            //});
            
            

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
