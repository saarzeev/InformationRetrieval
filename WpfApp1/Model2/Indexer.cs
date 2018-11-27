
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System;

namespace Model2
{

    internal class Indexer
    {
        static public ConcurrentDictionary<string, SimpleTerm> fullDictionary = new ConcurrentDictionary<string, SimpleTerm>();
        static public Mutex dictionaryMutex = new Mutex();
        private string _initialPathForPosting;

        public Indexer(string path)
        {
            this._initialPathForPosting = path;
        }

        public Queue<Posting> setDocVocabularytoFullVocabulary(Doc doc, SortedDictionary<string, Term> docDictionary)
        {
            string[] docPath = doc._path.Split('\\');
            Queue<Posting> docsPosting = new Queue<Posting>();
            foreach (string key in docDictionary.Keys)
            {
                if (fullDictionary.ContainsKey(key))
                {
                    string posting = fullDictionary[key].PostingPath;
                    if (docDictionary[key].IsLowerCase && !fullDictionary[key].IsLowerCase)
                    {
                        fullDictionary[key].IsLowerCase = true;
                    }
                    fullDictionary[key].Df++;
                }
                else
                {
                    SimpleTerm newTerm = new SimpleTerm(key, "", docDictionary[key].IsLowerCase);
                    fullDictionary.TryAdd(key, newTerm);
                }
                docsPosting.Enqueue(new Posting(docPath[docPath.Length - 1], doc._indexInFile, docDictionary[key]));
            }
            return docsPosting;
        }



        // docPath(last value of path)+docId(int)+tf(int)+is100(0-false 1-true)+gapPositins(int[])+isLowerCase(0-false 1-true)
    }

} 