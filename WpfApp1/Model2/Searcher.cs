using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model2
{
    class Searcher
    {
        private string path;
        private bool isStemming;
        
        public Searcher(string path, bool isStemming)
        {
            this.path = path;
            this.isStemming = isStemming;
        }

        /// <summary>
        /// Given a List of strings, <paramref name="term"/>, returns the terms' posting StringBuilder
        /// </summary>
        /// <param name="terms"></param>
        /// <returns></returns>
        public List<string> GetTermsPosting(List<string> terms)
        {
            List<string> ans = new List<string>();
            Indexer.Instance(this.path, this.isStemming);
            if ( Indexer.fullDictionary == null || Indexer.fullDictionary.Count == 0)
            {
                Indexer.indexer.LoadDictionery();
            }
            //DateTime startingTime = DateTime.Now;
            for (int i = 0; i < terms.Count; i++)
            {
                string term = terms[i].ToLower();
                char firstChar = term[0];
                string firstLetter = "\\" + (term.ElementAt(0) >= 'a' && term.ElementAt(0) <= 'z' ? term.ElementAt(0).ToString() : "other");
                string postingPath = Indexer.indexer._initialPathForPosting + (Indexer.indexer.isStemming ? Indexer.indexer.postingWithStemmingDirectory : Indexer.indexer.postingDirectory) + ("\\" + firstLetter + "FINAL.txt");

                using (FileStream infile = new FileStream(postingPath, FileMode.Open, FileAccess.Read))
                {
                    StringBuilder posting = null;
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
