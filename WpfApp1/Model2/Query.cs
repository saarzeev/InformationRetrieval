﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model2
{
    public class Query
    {
        private static int randomID = 0;
        private Dictionary<string, List<CityPosting>> cities;
        private bool isStemming;
        private bool withSemantic;
        private Dictionary<int, StringBuilder> queries;
       

        public Query(IList cities, bool isStemming, bool withSemantic)
        {
            this.cities = new Dictionary<string, List<CityPosting>>();
            if (cities != null)
            {
                for (int i = 0; i < cities.Count; i++)
                {
                    KeyValuePair<string, List<CityPosting>> keyValue = (KeyValuePair<string, List<CityPosting>>)cities[i];
                    this.cities.Add(keyValue.Key, keyValue.Value);
                }
            }
            this.isStemming = isStemming;
            this.withSemantic = withSemantic;
            queries = new Dictionary<int, StringBuilder>();
        }

        public bool IsStemming { get => isStemming; set => isStemming = value; }
        public bool WithSemantic { get => withSemantic; set => withSemantic = value; }
        public Dictionary<string, List<CityPosting>> Cities { get => cities; set => cities = value; }
        public Dictionary<int, StringBuilder> Queries { get => queries; set => queries = value; }


        public void runQueriesFromPath(string path)
        {
            StringBuilder query = new StringBuilder();
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (var fileStream = File.OpenRead(path))
                {
                    using (var streamReader = new StreamReader(path, Encoding.ASCII))
                    {
                        String line;
                        while (!streamReader.EndOfStream)
                        {
                            int queryID = 0;
                            bool Query = false;
                            bool queryEnd = false;
                            while ((line = streamReader.ReadLine()) != null && line != "</top>")
                            {
                               
                                if (!queryEnd && line.Length > 1)
                                {
                                    if (!Query)
                                    {
                                        //id
                                        if (line.StartsWith("<num>"))
                                        {
                                            string[] splited = line.Split(':');
                                            if (splited.Length > 0)
                                            {
                                                int.TryParse(splited.Last().Trim(), out queryID);
                                                Query = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //query
                                        if (line.StartsWith("<title>"))
                                        {
                                            //TODO maybe read titel
                                            //TODO maybe parse query if question or regular text
                                            //TODO mayby try read narr
                                            string[] del = { "<title>"," "};
                                            string[] splited = line.Split(del, StringSplitOptions.RemoveEmptyEntries);
                                            if (splited.Length > 0)
                                            {
                                                query.Append(String.Join(" ",splited));
                                                queryEnd = true;
                                            }
                                        }
                                    }
                                }
                            }

                            if (line != null)
                            {
                                this.queries.Add(queryID, query);
                                
                                query = new StringBuilder();
                                if (this.withSemantic)
                                {
                                    addSemantic(queryID);
                                }
                            }
                        }
                    }
                }
            }
            
        }

        public void runSingleQuery(string query)
        {
            this.queries.Add(randomID, new StringBuilder(query));
            if (this.withSemantic)
            {
                addSemantic(randomID);
            }
            randomID++;
        }
        public void addSemantic(int id)
        {
            //TODO
        }
    }
}
