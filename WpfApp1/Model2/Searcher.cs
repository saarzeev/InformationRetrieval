using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model2
{
   public class Searcher
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
            foreach(int querID in query.Queries.Keys)
            {
                parsed[querID] = parser.ParseQuery(query.Queries[querID], query.IsStemming);
            }
            return parsed;
        }

        /// <summary>
        /// Given a List of strings, <paramref name="term"/>, returns the terms' posting StringBuilder
        /// </summary>
        /// <param name="terms"></param>
        /// <returns></returns>
        public List<string> GetTermsPosting(List<string> terms)
        {
            List<string> ans = new List<string>();
           
            for (int i = 0; i < terms.Count; i++)
            {
                string term = terms[i].ToLower();
                char firstChar = term[0];
                string firstLetter = "\\" + (term.ElementAt(0) >= 'a' && term.ElementAt(0) <= 'z' ? term.ElementAt(0).ToString() : "other");
                string postingPath = Indexer.indexer.postingPathForSearch + ("\\" + firstLetter + "FINAL.txt");

                //TODO Maybe stay in the same file for every term within that file. (Sort query terms first)

                using (FileStream infile = new FileStream(postingPath, FileMode.Open, FileAccess.Read))
                {
                    if (Indexer.fullDictionary.ContainsKey(term))
                    {
                        long position = Indexer.fullDictionary[term].Position;
                        using (StreamReader file = new StreamReader(infile))
                        {
                            infile.Seek(position, SeekOrigin.Begin);
                            ans.Add(file.ReadLine());
                        }
                    }
                }
            }
            //Console.WriteLine("Search duration: " + (DateTime.Now - startingTime));
            return ans;
        }

        public void initSearch(Indexer indexer)
        {
           Dictionary<int, HashSet<string>> parsed = parseQuery();
           Dictionary<int, List<Tuple<string, double>>> rankingForQuery = new Dictionary<int, List<Tuple<string, double>>>();
           foreach(int queryId in parsed.Keys)
            {
                rankingForQuery.Add(queryId, new List<Tuple<string, double>>());
                 List<string> posting =  GetTermsPosting(parsed[queryId].ToList());
                //[[term:df]:[docid:tf:is100]]
                Dictionary<string, Dictionary<string, Tuple<int, int, bool>>> allInfo = getAllInfoFromPosting(posting);
                posting.Clear();
                //ranking
                Ranker ranker = new Ranker(indexer);
                foreach(string docId in allInfo.Keys)
                {
                    rankingForQuery[queryId].Add(ranker.rankingDocs(docId, parsed[queryId], allInfo[docId]));
                }
            }
           //TODO puke somewere sorted by rank! only 50 rancks for each query
        }

        private Dictionary<string, Dictionary<string, Tuple<int, int, bool>>> getAllInfoFromPosting(List<string> posting)
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
                    if (!allInfo.Keys.Contains(splited[j]))
                    {
                        allInfo.Add(splited[j], new Dictionary<string, Tuple<int, int, bool>>());
                    }

                    int.TryParse(splited[j + 1], out int tf);
                    bool is100 = splited[j + 2] == "1" ? true : false;

                    allInfo[splited[j]].Add(splited[0], new Tuple<int, int, bool>(df, tf, is100));
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
