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

        public List<HashSet<string>> parseQuery()
        {
            List<HashSet<string>> parsed = new List<HashSet<string>>();
            Parse parser = Parse.Instance();
            foreach(StringBuilder quer in query.Queries.Values)
            {
                parsed.Add(parser.ParseQuery(quer,query.IsStemming));
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

        public void initSearch()
        {
           List<HashSet<string>> parsed = parseQuery();

            for(int i = 0; i < parsed.Count(); i++)
            {
                //TODO renker? dictionary?
                GetTermsPosting(parsed[i].ToList());
            }
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
