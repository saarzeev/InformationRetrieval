using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Model2
{
    /// <summary>
    /// Query class
    /// </summary>
    public class Query
    {
        private static int randomID = 0;
        private Dictionary<string, List<CityPosting>> cities;
        private bool isStemming;
        private bool withSemantic;
        private Dictionary<int, StringBuilder> queries;
        private static Dictionary<string, List<KeyValuePair<string, double>>> semanticsDict; //origWord -> (simWord, score)*

        public bool IsStemming { get => isStemming; set => isStemming = value; }
        public bool WithSemantic { get => withSemantic; set => withSemantic = value; }
        public Dictionary<string, List<CityPosting>> Cities { get => cities; set => cities = value; }
        public Dictionary<int, StringBuilder> Queries { get => queries; set => queries = value; }

        /// <summary>
        /// Query C'tor
        /// </summary>
        /// <param name="cities"></param>
        /// <param name="isStemming"></param>
        /// <param name="withSemantic"></param>
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

            InitSemantics();
        }

        /// <summary>
        /// Initializes the semanticsDict dictionary with values from semantics.json file
        /// </summary>
        private void InitSemantics()
        {
            if (semanticsDict == null)
            {
                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(".json"));
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    string result = reader.ReadToEnd();
                    semanticsDict = JsonConvert.DeserializeObject<Dictionary<string, List<KeyValuePair<string, double>>>>(result);
                };
            }
        }

        /// <summary>
        /// Run all queries from the given path
        /// </summary>
        /// <param name="path">Path from which queries will be loaded</param>
        public void runQueriesFromPath(string path)
        {
            StringBuilder query = new StringBuilder();
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (var fileStream = File.OpenRead(path))
                {
                    using (var streamReader = new StreamReader(path, Encoding.UTF8))
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
                                    AddSemantic(queryID);
                                }
                            }
                        }
                    }
                }
            }
            
        }

        /// <summary>
        /// Runa single query
        /// </summary>
        /// <param name="query"></param>
        public void runSingleQuery(string query)
        {
            this.queries.Add(randomID, new StringBuilder(query));
            if (this.withSemantic)
            {
                AddSemantic(randomID);
            }
            randomID++;
        }

        /// <summary>
        /// Adds similar words to original query
        /// </summary>
        /// <param name="id"> query ID, to which the sematics will be added</param>
        public void AddSemantic(int id)
        {
            //split query into terms, to find similar words in semanticsDict.
            string[] splitted = queries[id].ToString().Split(' ');
            for (int i = 0; i < splitted.Length; i++)
            {
                string word = splitted[i];
                if (semanticsDict.ContainsKey(word))
                {
                    List<KeyValuePair<string, double>> words = semanticsDict[word];
                    for (int j = 0; j < words.Count; j++)
                    {
                        queries[id].AppendFormat(" {0}", words[j].Key);
                    }
                }
            }
        }
    }
}
