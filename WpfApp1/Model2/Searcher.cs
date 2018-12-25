using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model2
{
    public partial class Searcher
    {
        private string path;
        private bool isStemming;
        private HashSet<string> cities;

        private Query query;

        public Searcher(string path, bool isStemming, HashSet<string> cities)
        {
            this.path = path;
            this.isStemming = isStemming;
            this.cities = cities;
        }

        public Searcher(Query query)
        {
            this.query = query;
        }

        public Dictionary<int, HashSet<string>> parseQuery()
        {
            Dictionary<int, HashSet<string>> parsed = new Dictionary<int, HashSet<string>>();
            Parse parser = Parse.Instance();
            foreach (int querID in query.Queries.Keys)
            {
                parsed[querID] = parser.ParseQuery(query.Queries[querID], query.IsStemming);
            }
            return parsed;
        }

        W

        public Dictionary<int, List<Tuple<string, double>>> initSearch(Indexer indexer)
        {
            Dictionary<int, HashSet<string>> parsed = parseQuery();
            Dictionary<int, List<Tuple<string, double>>> rankingForQuery = new Dictionary<int, List<Tuple<string, double>>>();
            foreach (int queryId in parsed.Keys)
            {
                rankingForQuery.Add(queryId, new List<Tuple<string, double>>());
                List<string> posting = GetTermsPosting(parsed[queryId].ToList());
                //[docid: <term,df,tf,is100>
                HashSet<string> docs = new HashSet<string>();
                //remove by citises
                if (query.Cities != null && query.Cities.Count > 0)
                {
                    foreach (string city in query.Cities.Keys)
                    {
                        for (int i = 0; i < query.Cities[city].Count; i++)
                        {
                            if (!docs.Contains(query.Cities[city][i].docID))
                            {
                                docs.Add(query.Cities[city][i].docID);
                            }
                        }
                    }
                }
                Dictionary<string, Dictionary<string, Tuple<int, int, bool>>> allInfo = getAllInfoFromPosting(posting, docs);
                posting.Clear();
                //ranking
                Ranker ranker = new Ranker(indexer);
                foreach (string docId in allInfo.Keys)
                {
                    rankingForQuery[queryId].Add(ranker.rankingDocs(docId, parsed[queryId], allInfo[docId]));
                }
            }

            foreach(int item in rankingForQuery.Keys)
            {
                rankingForQuery[item].Sort((x, y) => x.Item2.CompareTo(y.Item2));
                rankingForQuery[item].Reverse();
                if (rankingForQuery[item].Count > 50) {
                    var ans = rankingForQuery[item].Take(50);
                    rankingForQuery[item].RemoveAll((x) => !ans.Contains(x));
                }
            }
            return rankingForQuery;
            //TODO puke somewere sorted by rank! only 50 rancks for each query
        }

        private Dictionary<string, Dictionary<string, Tuple<int, int, bool>>> getAllInfoFromPosting(List<string> posting, HashSet<string> docsByCities)
        {

            //[docid: <term,df,tf,is100>
            Dictionary<string, Dictionary<string, Tuple<int, int, bool>>> allInfo = new Dictionary<string, Dictionary<string, Tuple<int, int, bool>>>();
            for (int i = 0; i < posting.Count; i++)
            {
                string[] splited = posting[i].Split(',');
                int.TryParse(splited[1], out int df);
                //moves on the docID int the posting
                for (int j = 2; j + 2 < splited.Length; j = j + 4)
                {
                    if (docsByCities.Count == 0 || docsByCities.Contains(splited[j]))
                    {
                        if (!allInfo.Keys.Contains(splited[j]))
                        {
                            allInfo.Add(splited[j], new Dictionary<string, Tuple<int, int, bool>>());
                        }

                        int.TryParse(splited[j + 1], out int tf);
                        bool is100 = splited[j + 2] == "1" ? true : false;

                        allInfo[splited[j]].Add(splited[0], new Tuple<int, int, bool>(df, tf, is100));
                    }
                }

            }
            return allInfo;
        }

        public StringBuilder GetFile(string path)
        {
            //if (_cachedPath != path)
            //{
            //    _cachedFile = PostingsSet.Unzip(File.ReadAllBytes(path));
            //    _cachedPath = path;
            //}
            //return _cachedFile;
            return null;
        }
    }
}

