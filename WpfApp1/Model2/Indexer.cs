using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;

namespace Model2
{

    internal class Indexer
    {
        static public ConcurrentDictionary<string, SimpleTerm> fullDictionary = new ConcurrentDictionary<string, SimpleTerm>();
        static public Mutex dictionaryMutex = new Mutex();
        private string _initialPathForPosting;
        private static Indexer indexer;
        public PostingsSet currenPostingSet;
        public List<PostingsSet> dead;
        public Mutex postingSetMutex = new Mutex();
        public bool isStemming;

        public static Indexer Instance(string path, bool isStemming)
        {

            if (indexer == null)
            {
                indexer = new Indexer(path, isStemming);
            }
            else
            {
                indexer._initialPathForPosting = path;
                indexer.isStemming = isStemming;
            }
            return indexer;

        }

        private Indexer(string path, bool isStemming)
        {
            this._initialPathForPosting = path;
            currenPostingSet = new PostingsSet(path, isStemming);
            dead = new List<PostingsSet>();
            System.IO.Directory.CreateDirectory(path + "\\postingWithStemming");
            System.IO.Directory.CreateDirectory(path + "\\posting");
            System.IO.Directory.CreateDirectory(path + "\\tmpPostingFiles");
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

        public void initIndex(Doc doc, SortedDictionary<string, Term> docDictionary)
        {
           Queue<Posting> postingOfDoc = setDocVocabularytoFullVocabulary(doc, docDictionary);

            this.postingSetMutex.WaitOne();
            while (postingOfDoc.Count > 0)
            {
                Posting posting = postingOfDoc.Dequeue();
                if (!currenPostingSet.Add(posting.term, posting)) {
                    //int nu = currenPostingSet.capacity;
                    currenPostingSet.DumpToDisk();
                    dead.Add(currenPostingSet);
                    currenPostingSet = new PostingsSet(this._initialPathForPosting, this.isStemming);
                    currenPostingSet.Add(posting.term, posting);
                }
            }
            this.postingSetMutex.ReleaseMutex();
        }

    }

} 