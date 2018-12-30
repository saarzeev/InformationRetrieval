using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

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
        private Dictionary<int, StringBuilder> semiToQueries;
        private Dictionary<int, StringBuilder> descriptionAndNarrative;
        private static Dictionary<string, List<KeyValuePair<string, double>>> semanticsDict; //origWord -> (simWord, score)*

        public bool IsStemming { get => isStemming; set => isStemming = value; }
        public bool WithSemantic { get => withSemantic; set => withSemantic = value; }
        public Dictionary<string, List<CityPosting>> Cities { get => cities; set => cities = value; }
        public Dictionary<int, StringBuilder> Queries { get => queries; set => queries = value; }
        public Dictionary<int, StringBuilder> SemiToQueries { get => semiToQueries; set => semiToQueries = value; }
        public Dictionary<int, StringBuilder> DescriptionAndNarrative { get => descriptionAndNarrative; set => descriptionAndNarrative = value; }

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
            if (withSemantic)
            {
                semiToQueries = new Dictionary<int, StringBuilder>();
                InitSemantics();
            }
           
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
                using (StreamReader reader = new StreamReader(stream,Encoding.ASCII))
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
            this.descriptionAndNarrative = new Dictionary<int, StringBuilder>();
            String[] additionalStopWords = { "etc.", "i.e", "considered", "information", "documents", "document",
                "discussing", "discuss", "following", "issues", "identify", "find", "so-called","impact"};
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
                            bool description = false;
                            bool narr = false;
                            StringBuilder narrative = new StringBuilder("") ;
                            StringBuilder desc = new StringBuilder("");
                            while ((line = streamReader.ReadLine()) != null && line != "</top>")
                            {
                               
                                if (line.Length > 1)
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
                                            //TODO mayby try read narr
                                            string[] del = { "<title>"," "};
                                            string[] splited = line.Split(del, StringSplitOptions.RemoveEmptyEntries);
                                            if (splited.Length > 0)
                                            {
                                                query.Append(String.Join(" ",splited));
                                                continue;
                                                //queryEnd = true;
                                            }
                                        }

                                        if (description|| line.StartsWith("<desc>"))
                                        {
                                            description = true;

                                            if (narr || line.StartsWith("<narr>"))
                                            {
                                                narr = true;
                                                narrative.AppendLine(line);
                                                continue;
                                            }
                                            else
                                            {
                                                if(!line.StartsWith("<desc>") /*&& line.Length > 1*/)
                                                {
                                                    string[] splited = line.ToLower().Split(additionalStopWords, StringSplitOptions.RemoveEmptyEntries);
                                                    if (splited.Length > 0)
                                                    {
                                                        desc.AppendLine(" " + String.Join(" ", splited));
                                                        // query.Append(" " + String.Join(" ", splited));
                                                        continue; 
                                                        //queryEnd = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (line != null)
                            {
                                string [] narativeSplitted = this.GetNarrative(narrative.ToString());
                                //query.Append(" " + String.Join(" ", narativeSplitted));
                                this.queries.Add(queryID, query);
                                String[] arr = { "\r\n", "\n\r" , " "};
                                this.AddNarr(queryID, desc.ToString().Split(arr, StringSplitOptions.RemoveEmptyEntries));
                                this.AddNarr(queryID, narativeSplitted);
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
                    if (!semiToQueries.ContainsKey(id))
                    {
                        semiToQueries.Add(id, new StringBuilder(""));
                    }
;                   for (int j = 0; j < words.Count; j++)
                    {
                        semiToQueries[id].AppendFormat("{0} ", words[j].Key);
                    }
                }
            }
        }

        public void AddNarr(int id,string[] narr)
        {
            for (int i = 0; i < narr.Length; i++)
            {
                string word = narr[i];
                    if (!descriptionAndNarrative.ContainsKey(id))
                    {
                        descriptionAndNarrative.Add(id, new StringBuilder(""));
                    }
                   descriptionAndNarrative[id].AppendFormat("{0} ", word);
            }
        }

        public string[] GetNarrative(string narrative)
        {
            string[] splitedRelevant;
            String[] additionalStopWords = { "etc.", "i.e", "considered", "information", "documents", "document",
                "discussing", "discuss", "following", "issues", "identify", "find", "so-called","impact", " ",
                "<narr>","Narrative","relevant" ,"focus", "topic", "shows", "mention","requierd" ,"mentions", "purpose", "includ", "concentrate","factor","\r\n"};
            String[] arr = { "\r\n" , "\n\r" };
            if (narrative.Contains("relevant:") || narrative.Contains("Relevant:"))
            {
                string[] splited = narrative.Split(arr, StringSplitOptions.RemoveEmptyEntries);
                var relevant = splited.SkipWhile(word => word.Contains("are relevant")).TakeWhile(word => !word.Contains("not relevant") && !word.Contains("non relevant") && !word.Contains("non-relevant")).ToArray();
                
                splitedRelevant = String.Join(" ", relevant).Split(additionalStopWords, StringSplitOptions.RemoveEmptyEntries);
            }

            else
            {
                string[] splitedByDot = narrative.Split('.');
                var relevant = splitedByDot.Where(word => !word.Contains("not relevant") && !word.Contains("non relevant") && !word.Contains("non-relevant")).ToArray();
                splitedRelevant = String.Join(" ", relevant).Split(additionalStopWords, StringSplitOptions.RemoveEmptyEntries);
            }
            return splitedRelevant;
        }
    }
}
