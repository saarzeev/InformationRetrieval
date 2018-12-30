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
        /// <summary>
        /// Given a List of strings, <paramref name="term"/>, returns the terms' posting StringBuilder
        /// </summary>
        /// <param name="terms"></param>
        /// <returns></returns>
        public List<string> GetTermsPosting(List<string> terms,Indexer index)
        {
            if (terms.Contains("winners"))
            {
                int i = 0;
            }
            List<string> ans = new List<string>();
            //terms.Sort();
            string currFirstLetter = "";

            for (int i = 0; i < terms.Count; i++)
            {
                string term = terms[i].ToLower();
               // char firstChar = term[0];
                currFirstLetter = GetFirstLetter(term);
                string postingPath = index.postingPathForSearch + ("\\" + currFirstLetter + "FINAL.txt");

                //TODO Maybe stay in the same file for every term within that file. (Sort query terms first)

                using (FileStream infile = new FileStream(postingPath, FileMode.Open, FileAccess.Read))
                {
                    if (Indexer.fullDictionary.ContainsKey(term))
                    {
                        long position = Indexer.fullDictionary[term].Position;
                        using (StreamReader file = new StreamReader(infile, Encoding.ASCII))
                        {
                            //while (true)
                            //{
                                //infile.Seek(0, SeekOrigin.Begin);
                                infile.Seek(position, SeekOrigin.Begin);
                                ans.Add(file.ReadLine());
                               /* if (i + 1 < terms.Count && GetFirstLetter(terms[i + 1]) == GetFirstLetter(terms[i]))
                                {
                                    i++;
                                    term = terms[i];
                                    if (Indexer.fullDictionary.ContainsKey(term))
                                    {
                                        position = Indexer.fullDictionary[term].Position;
                                    }
                                }
                                else
                                {
                                    break;
                                }*/
                            //}
                        }
                    }
                }
            }
            //Console.WriteLine("Search duration: " + (DateTime.Now - startingTime));
            return ans;
        }

        private string GetFirstLetter(string term)
        {
            return "\\" + (term.ElementAt(0) >= 'a' && term.ElementAt(0) <= 'z' ? term.ElementAt(0).ToString() : "other");
        }
    }
}
