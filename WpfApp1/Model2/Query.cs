using System;
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
        private HashSet<string> cities;
        private bool isStemming;
        private bool withSemantic;
        private Dictionary<int, StringBuilder> queries;

       public Query(HashSet<string> cities, bool isStemming, bool withSemantic)
        {
            this.cities = cities;
            this.isStemming = isStemming;
            this.withSemantic = withSemantic;
            queries = new Dictionary<int, StringBuilder>();
        }
  
        public bool IsStemming { get => isStemming; set => isStemming = value; }
        public bool WithSemantic { get => withSemantic; set => withSemantic = value; }
        public HashSet<string> Cities { get => cities; set => cities = value; }
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
                            bool narrative = false;
                            while ((line = streamReader.ReadLine()) != null && line != "</top>")
                            {
                                if (!narrative && line.Length > 1)
                                {
                                    if (!Query)
                                    {
                                        //id
                                        if (line.StartsWith("<num>"))
                                        {
                                            string[] splited = line.Split(':');
                                            if (splited.Length > 0)
                                            {
                                                int.TryParse(splited.Last().Trim(),out queryID) ;
                                                Query = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //query
                                        if (!line.StartsWith("<narr>"))
                                        {
                                            //TODO maybe read titel
                                            //TODO maybe parse query if question or regular text
                                            //TODO mayby try read narr
                                            if (!line.StartsWith("<"))
                                            {
                                                query.AppendFormat("{0}{1}", line, " ");
                                            }
                                               
                                        }
                                        //narative for now erelevant
                                        else
                                        {
                                            narrative = true;
                                        }
                                    }
                                }
                            }

                            if (line != null)
                            {
                                this.queries.Add(queryID, query);
                                query.Clear();
                            }
                        }
                    }
                }
            }
        }

        public void runSingleQuery(string query)
        {
            this.queries.Add(randomID, new StringBuilder(query));
            randomID++;
        }
        public void addSemantic()
        {
            //TODO
        }
    }
}
