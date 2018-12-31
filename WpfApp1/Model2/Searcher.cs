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
        public Dictionary<int, HashSet<string>> parseSemi()
        {
            if (query.WithSemantic || query.SemiToQueries!=null )
            {
                Dictionary<int, HashSet<string>> parsed = new Dictionary<int, HashSet<string>>();
                Parse parser = Parse.Instance();
                foreach (int querID in query.SemiToQueries.Keys)
                {
                    if (query.SemiToQueries[querID].Length > 0)
                    {
                        parsed[querID] = parser.ParseQuery(query.SemiToQueries[querID], query.IsStemming);
                    }
                }
                return parsed;
            }
           
            return null;
        }

        public Dictionary<int, HashSet<string>> parseDescAndNarr()
        {
            if ( query.DescriptionAndNarrative != null)
            {
                Dictionary<int, HashSet<string>> parsed = new Dictionary<int, HashSet<string>>();
                Parse parser = Parse.Instance();
                foreach (int querID in query.DescriptionAndNarrative.Keys)
                {
                    if (query.DescriptionAndNarrative[querID].Length > 0)
                    {
                        parsed[querID] = parser.ParseQuery(query.DescriptionAndNarrative[querID], query.IsStemming);
                    }
                }
                return parsed;
            }

            return null;
        }


        public Dictionary<int, List<Tuple<string, double>>> initSearch(Indexer indexer)
        {
            //all parsed 
            Dictionary<int, HashSet<string>> parsedQuery = parseQuery();
            Dictionary<int, HashSet<string>> parsedSemi = parseSemi();
            Dictionary<int, HashSet<string>> parsedDescriptionAndNarr = parseDescAndNarr();

            //remove duplication from description and naarative
            if (parsedDescriptionAndNarr != null)
            {
                foreach (int queryId in parsedQuery.Keys)
                {
                    foreach (string word in parsedQuery[queryId])
                    {
                        if (parsedDescriptionAndNarr.Keys.Contains(queryId) && parsedDescriptionAndNarr[queryId].Contains(word))
                        {
                            parsedDescriptionAndNarr[queryId].Remove(word);
                        }
                    }

                }
            }
            Dictionary<int, List<Tuple<string, double>>> rankingForQuery = new Dictionary<int, List<Tuple<string, double>>>();

            foreach (int queryId in parsedQuery.Keys)
            {
                rankingForQuery.Add(queryId, new List<Tuple<string, double>>());
                List<string> posting = GetTermsPosting(parsedQuery[queryId].ToList(), indexer);

                //posting for semi
                List<string> postingSemi = new List<string>();
                if (parsedSemi != null && parsedSemi.Keys.Contains(queryId))
                {
                    postingSemi = GetTermsPosting(parsedSemi[queryId].ToList(), indexer);
                }
                //posting for description and narr 
                List<string> postingDescriptionAndNarrative = new List<string>();
                if (parsedDescriptionAndNarr != null && parsedDescriptionAndNarr.Keys.Contains(queryId))
                {
                    postingDescriptionAndNarrative = GetTermsPosting(parsedDescriptionAndNarr[queryId].ToList(), indexer);
                }
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

                Dictionary<string, Dictionary<string, Tuple<int, int, bool>>> allInfoOfQuery = getAllInfoFromPosting(posting, docs);
                Dictionary<string, Dictionary<string, Tuple<int, int, bool>>> allInfoOfSemi = new Dictionary<string, Dictionary<string, Tuple<int, int, bool>>>();
                Dictionary<string, Dictionary<string, Tuple<int, int, bool>>> allInfoOfDescAndNarr = new Dictionary<string, Dictionary<string, Tuple<int, int, bool>>>();
                posting.Clear();

                //all info for ranking for semi and add docs to check
                if (postingSemi.Count > 0) {
                    allInfoOfSemi = getAllInfoFromPosting(postingSemi, docs);
                    postingSemi.Clear();
                    //foreach (string docID in allInfoOfSemi.Keys)
                    //{
                    //    if (!allInfoOfQuery.ContainsKey(docID))
                    //    {
                    //        allInfoOfQuery.Add(docID, new Dictionary<string, Tuple<int, int, bool>>());
                    //    }
                    //}
                }

                //all info for ranking for description and narr and add docs to check
                if (postingDescriptionAndNarrative.Count > 0)
                {
                    allInfoOfDescAndNarr = getAllInfoFromPosting(postingDescriptionAndNarrative, docs);
                    postingDescriptionAndNarrative.Clear();
                    //foreach (string docID in allInfoOfDescAndNarr.Keys)
                    //{
                    //    if (!allInfoOfQuery.ContainsKey(docID))
                    //    {
                    //        allInfoOfQuery.Add(docID, new Dictionary<string, Tuple<int, int, bool>>());
                    //    }
                    //}
                }
          
                //ranking
                Ranker ranker = new Ranker(indexer);
                foreach (string docId in allInfoOfQuery.Keys)
                {
                    HashSet<string> parsedSemiforDoc = parsedSemi != null && parsedSemi.ContainsKey(queryId) ? parsedSemi[queryId] : null;
                    HashSet<string> parsedDescAndNarrforDoc = parsedDescriptionAndNarr != null && parsedDescriptionAndNarr.ContainsKey(queryId) ? parsedDescriptionAndNarr[queryId] : null;

                    rankingForQuery[queryId].Add(ranker.rankingDocs(docId, parsedQuery[queryId], allInfoOfQuery[docId],
                        parsedSemiforDoc, allInfoOfSemi.Keys.Contains(docId)? allInfoOfSemi[docId]:null,
                        parsedDescAndNarrforDoc, allInfoOfDescAndNarr.Keys.Contains(docId) ? allInfoOfDescAndNarr[docId] : null));
                }
            }

            foreach(int item in rankingForQuery.Keys)
            {
                rankingForQuery[item].Sort((x, y) => y.Item2.CompareTo(x.Item2));
                if (rankingForQuery[item].Count > 50) {
                    var ans = rankingForQuery[item].Take(50);
                    rankingForQuery[item].RemoveAll((x) => !ans.Contains(x));
                }
                string nu = Directory.GetCurrentDirectory();
                using (FileStream fs = new FileStream(Directory.GetCurrentDirectory() + @"\results.txt", FileMode.Append, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.ASCII))

                    {
                        foreach (var entry in rankingForQuery[item])
                        {
                            sw.WriteLine("{0} {1} {2} {3} {4} {5}", item, 0, entry.Item1, 1, 1.1, "a");
                        }
                    }
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
                        if (splited[j].Contains("winner"))
                        {
                            string docID = splited[j];
                        }
                        
                        if (!allInfo.Keys.Contains(splited[j]))
                        {
                            allInfo.Add(splited[j], new Dictionary<string, Tuple<int, int, bool>>());
                        }

                        int.TryParse(splited[j + 1], out int tf);
                        bool is100 = splited[j + 2] == "1" ? true : false;

                        if (!allInfo[splited[j]].Keys.Contains(splited[0])){
                            allInfo[splited[j]].Add(splited[0], new Tuple<int, int, bool>(df, tf, is100));
                        }
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

