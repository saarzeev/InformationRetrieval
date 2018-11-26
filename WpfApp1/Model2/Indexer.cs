using System.Collections.Generic;
using System.Threading;

namespace Model2
{

    internal class Indexer
    {
        static public Dictionary<string, SimpleTerm> fullDictionary = new Dictionary<string, SimpleTerm>();
        static public Mutex dictionaryMutex = new Mutex();
        private string _initialPathForPosting;

        public Indexer(string path)
        {
            this._initialPathForPosting = path;
        }
        
        public void setDocVocabularytoFullVocabulary(Doc doc ,Dictionary<string, Term> docDictionary)
        {
            dictionaryMutex.WaitOne();
            foreach (string key in docDictionary.Keys)
            {
                    if (fullDictionary.ContainsKey(key))
                    {
                        string posting = fullDictionary[key].PostingPath;
                        if (docDictionary[key].IsLowerCase && !fullDictionary[key].IsLowerCase)
                        {
                            fullDictionary[key].IsLowerCase = true ;
                        }
                        fullDictionary[key].Df++;
                    //TODO update posting
                    }
                    else
                    {
                        string posting = "";//TODO where and how we choose posting path
                        SimpleTerm newTerm = new SimpleTerm(key, posting, docDictionary[key].IsLowerCase);
                        fullDictionary.Add(key, newTerm);
                    }
            }
            dictionaryMutex.ReleaseMutex();
        }


        // docPath(last value of path)+docId(int)+tf(int)+is100(0-true 1-false)+gapPositins(int[])+isLowerCase(0-true 1-false)
        //public void string getPostingString(Term term, Doc doc)
        //{
        //    string[] docPath = doc._path.Split('\\');
        //    str
             
        //}
    }
} 