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

        public Tuple<string, double> rankingDocs(string docId, HashSet<string> query, Dictionary<string, Tuple<int, int, bool>> term_Df_TF_Is100,
             HashSet<string> semiToQuery, Dictionary<string, Tuple<int, int, bool>> term_Df_TF_Is100OfSemi,
             HashSet<string> descAndNarrWords, Dictionary<string, Tuple<int, int, bool>> term_Df_TF_Is100OfDescAndNarr)
        {
            //bm25
            double bm25 = getSuperDuperBm25(docId, query, term_Df_TF_Is100, semiToQuery, term_Df_TF_Is100OfSemi, descAndNarrWords, term_Df_TF_Is100OfDescAndNarr);
            return new Tuple<string, double>(docId, bm25);
            //return [doc id: rank]
        }

        private double getSuperDuperBm25(string docId, HashSet<string> query, Dictionary<string, Tuple<int, int, bool>> term_Df_TF_Is100,
             HashSet<string> semiToQuery, Dictionary<string, Tuple<int, int, bool>> term_Df_TF_Is100OfSemi,
             HashSet<string> descAndNarrWords, Dictionary<string, Tuple<int, int, bool>> term_Df_TF_Is100OfDescAndNarr)
        {
            double b = 0.75;
            double k = 1.3;
            int docLength = indexer.docIndexDictionary[docId].length;
            double avergeDocLength = indexer.getAverDocLength();
            double numberOfDocs = indexer.docIndexDictionary.Keys.Count;
            double ans = 0.0;
            foreach(string word in query)
            {
                if (term_Df_TF_Is100 != null && term_Df_TF_Is100.Keys.Contains(word)) {
                    //bm25
                    double a = ((double)1 / (double)query.Count);
                    double bb = ((k + 1) * term_Df_TF_Is100[word].Item2);
                    double c = term_Df_TF_Is100[word].Item2;
                    double d = ((1 - b) + (b * (docLength) / avergeDocLength));
                    double e = numberOfDocs + 1;
                    double f = term_Df_TF_Is100[word].Item1;
                    ans += a * (bb / (c + (k * d))) * Math.Log(e / f);
                }
                ans += 0;
            }
            if (semiToQuery != null)
            {
                //k = 1.8;
                foreach (string word in semiToQuery)
                {
                    if (term_Df_TF_Is100OfSemi != null && term_Df_TF_Is100OfSemi.Keys.Contains(word))
                    {
                        //bm25
                        double a = ((double)1 / (double)query.Count);
                        double bb = ((k + 1) * term_Df_TF_Is100OfSemi[word].Item2);
                        double c = term_Df_TF_Is100OfSemi[word].Item2;
                        double d = ((1 - b) + (b * (docLength) / avergeDocLength));
                        double e = numberOfDocs + 1;
                        double f = term_Df_TF_Is100OfSemi[word].Item1;
                        double j = (0.105) * ((a * (bb / (c + (k * d))) * Math.Log(e / f)));
                        ans += j;
                    }
                    ans += 0;
                }
            }

            if (descAndNarrWords != null)
            {
                k = 1.85;
                b = 0.75;
                foreach (string word in descAndNarrWords)
                {
                    if (term_Df_TF_Is100OfDescAndNarr != null && term_Df_TF_Is100OfDescAndNarr.Keys.Contains(word))
                    {
                        //bm25
                        double a = ((double)1 / (double)query.Count);
                        double bb = ((k + 1) * term_Df_TF_Is100OfDescAndNarr[word].Item2);
                        double c = term_Df_TF_Is100OfDescAndNarr[word].Item2;
                        double d = ((1 - b) + (b * (docLength) / avergeDocLength));
                        double e = numberOfDocs + 1;
                        double f = term_Df_TF_Is100OfDescAndNarr[word].Item1;
                        ans += 0.65 * (a * (bb / (c + (k * d))) * Math.Log(e / f));
                    }
                    ans += 0;
                }
            }
            return ans;
        }
    }
}
