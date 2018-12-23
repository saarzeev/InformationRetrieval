using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model2
{
    public class Ranker
    {
        Indexer indexer;
        public Ranker(Indexer indexer)
        {
            this.indexer = indexer;
        }

        public Tuple<string, double> rankingDocs(string docId, HashSet<string> query, Dictionary<string, Tuple<int, int, bool>> term_Df_TF_Is100)
        {
            //TODO isCity?!

            //bm25
            double bm25 = getSuperDuperBm25(docId, query, term_Df_TF_Is100);
            return new Tuple<string, double>(docId, bm25);
            //return [doc id: rank]
        }

        private double getSuperDuperBm25(string docId, HashSet<string> query, Dictionary<string, Tuple<int, int, bool>> term_Df_TF_Is100)
        {
            double b = 0.75;
            double k = 1.2;
            int docLength = indexer.docIndexDictionary[docId].length;
            double avergeDocLength = indexer.getAverDocLength();
            double numberOfDocs = indexer.docIndexDictionary.Keys.Count;
            double ans = 0.0;
            foreach(string word in query)
            {
                if (term_Df_TF_Is100.Keys.Contains(word)) {
                    //bm25
                    double a = ((double)1 / (double)query.Count);
                    double bb = ((k + 1) * term_Df_TF_Is100[word].Item2);
                    double c = term_Df_TF_Is100[word].Item2;
                    double d = ((1 - b) + (b * (docLength) / avergeDocLength));
                    double e = numberOfDocs + 1;
                    double f = term_Df_TF_Is100[word].Item1;
                    ans += a * (bb / (c + (k * d))) * Math.Log(e / f);

                    //is100
                    ans *= term_Df_TF_Is100[word].Item3 ? 0.4 * ((docLength / 100)) : 1;

                    //tf/uniqWords
                    ans *= (double)term_Df_TF_Is100[word].Item2 / (double)indexer.docIndexDictionary[docId].uniqWords;
                }
                ans += 0;
            }
            return ans;
        }

    }
}
