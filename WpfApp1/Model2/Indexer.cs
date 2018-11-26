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
    }
} 