using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model2
{
    /// <summary>
    /// Indexer class
    /// </summary>
    public class Indexer
    {
        static public ConcurrentDictionary<string, SimpleTerm> fullDictionary;
        static public ConcurrentQueue<Doc> docsIndexer;
        static public Mutex dictionaryMutex = new Mutex();
        public string _initialPathForPosting;
        public string postingPathForSearch;
        public static Indexer indexer;
        public PostingsSet currenPostingSet;
        public List<PostingsSet> dead;
        public Mutex postingSetMutex = new Mutex();
        public bool isStemming;
        public string tmpDirectory = "\\tmpPostingFiles";
        public string postingDirectory = "\\posting";
        public string postingWithStemmingDirectory = "\\postingWithStemming";
        public HashSet<string> _cities = new HashSet<string>();
        public StringBuilder citiesIndex = new StringBuilder();
        public int termCount = 0;
        public int docsCount = 0;

        /// <summary>
        /// Get the Indexer's instance
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isStemming"></param>
        /// <returns></returns>
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


        /// <summary>
        /// Indexer destructor
        /// </summary>
        public static void DestructIndexer()
        {
            indexer = null;
        }

        /// <summary>
        /// Load Docs' dictionary from disk to memory
        /// </summary>
        public void LoadDocDictionary()
        {
            docsIndexer = new ConcurrentQueue<Doc>();
            string path = this.isStemming ? this._initialPathForPosting + postingWithStemmingDirectory : this._initialPathForPosting + postingDirectory;
            path += "\\docIndexer.txt";

            if (File.Exists(path))
            {
                string docDictionaryFile = (File.ReadAllText(path, Encoding.UTF8));
                string[] del = { "\r\n" };
                string[] lineByLine = docDictionaryFile.ToString().Split(del, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < lineByLine.Length; i++)
                {
                    if (lineByLine[i].Length > 1)
                    {
                        docsIndexer.Enqueue(new Doc(lineByLine[i]));
                    }
                }
            }
        }

        /// <summary>
        /// Loads the cities index from disk to memory
        /// </summary>
        public void LoadCitiesDictionary()
        {

            string path = this.isStemming ? this._initialPathForPosting + postingWithStemmingDirectory : this._initialPathForPosting + postingDirectory;
            path += Resources.Resource.cityIndexFileName;
            //_city,_country,_currency,population,(docID,[gaps],)*

            if (File.Exists(path))
            {
                string cityIndex = (File.ReadAllText(path, Encoding.UTF8));
                string[] del = { "\n" };
                string[] lineByLine = cityIndex.ToString().Split(del, StringSplitOptions.RemoveEmptyEntries);
                currenPostingSet = new PostingsSet(_initialPathForPosting, isStemming);

                for (int i = 0; i < lineByLine.Length; i++)
                {
                    if (lineByLine[i].Length > 1)
                    {
                        string[] splitted = lineByLine[i].Split(',');
                        string city = splitted[0];
                        string country = splitted[1];
                        string currency = splitted[2];
                        string population = splitted[3];

                        City tmpCity = new City(city, population, currency, country);
                        for (int j = 4; j < splitted.Length; j++)
                        {
                            StringBuilder cityPostingRep = new StringBuilder(splitted[j++]); //DocID

                            //[gap,*]
                            while (!splitted[j].Contains("]"))
                            {
                                cityPostingRep.AppendFormat(",{0}", splitted[j++]);
                            }
                            cityPostingRep.AppendFormat(",{0}", splitted[j]);

                            currenPostingSet.AddCity(city, new CityPosting(cityPostingRep));
                        }

                    }
                }
            }


        }


        /// <summary>
        /// Delete files and directories required for operation
        /// </summary>
        public void Reset()
        {
            if(Directory.Exists(_initialPathForPosting + tmpDirectory)) {
                Directory.Delete(_initialPathForPosting + tmpDirectory, true);
            }
            
            if (Directory.Exists(_initialPathForPosting + postingWithStemmingDirectory)) {
                Directory.Delete(_initialPathForPosting + postingWithStemmingDirectory, true);
            }
             
            if (Directory.Exists(_initialPathForPosting + postingDirectory)) {
                Directory.Delete(_initialPathForPosting + postingDirectory, true);
            }
           
            if(File.Exists(_initialPathForPosting + "\\show.txt"))
            {
                File.Delete(_initialPathForPosting + "\\show.txt");
            }
            if (File.Exists(_initialPathForPosting + "\\Stemmingshow.txt"))
            {
                File.Delete(_initialPathForPosting + "\\Stemmingshow.txt");
            }
        }

        private Indexer(string path, bool isStemming)
        {
            this._initialPathForPosting = path;
            currenPostingSet = new PostingsSet(path, isStemming);
            dead = new List<PostingsSet>();
            docsIndexer = new ConcurrentQueue<Doc>();
            fullDictionary = new ConcurrentDictionary<string, SimpleTerm>();
            System.IO.Directory.CreateDirectory(path + postingWithStemmingDirectory);
            System.IO.Directory.CreateDirectory(path + postingDirectory);
            System.IO.Directory.CreateDirectory(path + tmpDirectory);
        }

        /// <summary>
        /// Append Doc's dictionary to the full dictionary
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="docDictionary"></param>
        /// <returns></returns>
        public Queue<Posting> SetDocVocabularyToFullVocabulary(Doc doc, SortedDictionary<string, Term> docDictionary)
        {
           // string[] docPath = doc._path.Split('\\');
            Queue<Posting> docsPosting = new Queue<Posting>();
            docsIndexer.Enqueue(doc);
            foreach (string key in docDictionary.Keys)
            {
                if (fullDictionary.ContainsKey(key))
                {
                    if (docDictionary[key].IsLowerCase && !fullDictionary[key].IsLowerCase)
                    {
                        fullDictionary[key].IsLowerCase = true;
                    }
                    fullDictionary[key].Df++;
                    fullDictionary[key].addTf(docDictionary[key].Tf);
                    doc.entities.Add(new KeyValuePair<int, SimpleTerm>(docDictionary[key].Tf, fullDictionary[key]));
                }
                else
                {
                    SimpleTerm newTerm = new SimpleTerm(key/*, ""*/, docDictionary[key].IsLowerCase, docDictionary[key].Tf);
                    fullDictionary.TryAdd(key, newTerm);
                    doc.entities.Add(new KeyValuePair<int, SimpleTerm>(docDictionary[key].Tf, newTerm));
                }
                docsPosting.Enqueue(new Posting(/*docPath[docPath.Length - 1],*/ doc._docID, docDictionary[key]));
            }
            return docsPosting;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="docDictionary"></param>
        public void InitIndex(Doc doc, SortedDictionary<string, Term> docDictionary)
        {
           Queue<Posting> postingOfDoc = SetDocVocabularyToFullVocabulary(doc, docDictionary);

            this.postingSetMutex.WaitOne();
            while (postingOfDoc.Count > 0)
            {
                Posting posting = postingOfDoc.Dequeue();
                if (!currenPostingSet.Add(posting.term, posting)) {
                    currenPostingSet.DumpToDisk();
                    dead.Add(currenPostingSet);
                    currenPostingSet = new PostingsSet(this._initialPathForPosting, this.isStemming);
                    currenPostingSet.Add(posting.term, posting);
                }
            }
            this.postingSetMutex.ReleaseMutex();
        }


        /// <summary>
        /// Write dictionary to disk - the string representation can be used to rebuild the dictionary.
        /// </summary>
        public void WriteDictionary()
        {
            string path = this.isStemming ? this._initialPathForPosting + postingWithStemmingDirectory : this._initialPathForPosting + postingDirectory;
            StringBuilder dictionary = new StringBuilder();
            dictionary.AppendLine(path);
            foreach (SimpleTerm term in fullDictionary.Values)
            {
                dictionary.AppendLine(term.ToString());
            }
            path += "\\dictionary.txt";
            PostingsSet.Write(dictionary, path, System.IO.Compression.CompressionLevel.Fastest);
        }
    
        /// <summary>
        /// Loads the Terms' dictionary form disk to memory
        /// </summary>
        public void LoadDictionary()
        {
            string path = this.isStemming ? this._initialPathForPosting + postingWithStemmingDirectory : this._initialPathForPosting + postingDirectory;
            path += "\\dictionary.txt";
            if (File.Exists(path))  
            {
                //StringBuilder dictionary = PostingsSet.Unzip(File.ReadAllBytes(path));
                string dictionary = (File.ReadAllText(path, Encoding.UTF8));
                string[] del = {"\r\n"};
                string[] lineByLine = dictionary.ToString().Split(del, StringSplitOptions.RemoveEmptyEntries);
                this.postingPathForSearch = lineByLine[0];
                for (int i = 1; i < lineByLine.Length; i++)
                {
                    if (lineByLine[i].Length > 1)
                    {
                        SimpleTerm term = new SimpleTerm(lineByLine[i]);
                        fullDictionary[term.GetTerm] = term;
                    }
                }
            }
        }


        /// <summary>
        /// Write a representation of the Dictionary to disk
        /// </summary>
        public void GetDictionary()
        {
            SortedDictionary<string, Tuple<string, string>> dictionary = new SortedDictionary<string, Tuple<string,string>>();
            foreach(SimpleTerm term in fullDictionary.Values)
            {
                if (term.IsLowerCase) {
                    dictionary[term.GetTerm] = new Tuple<string, string>(term.Df.ToString(), term.Tf.ToString());
                }
                else
                {
                    dictionary[term.GetTerm.ToUpper()] = new Tuple<string, string>(term.Df.ToString(), term.Tf.ToString());
                }
            }
            string dictionaryPath = isStemming ? "\\Stemmingshow.txt" : "\\show.txt";
            using (StreamWriter file = new StreamWriter(_initialPathForPosting + dictionaryPath))
            {
                file.WriteLine("[{0} {1} {2}]","TERM", " DF " ,"TF");
                foreach (var entry in dictionary)
                {
                    file.WriteLine("[{0} {1} {2}]", entry.Key, entry.Value.Item1, entry.Value.Item2);
                }
            }
        }

        /// <summary>
        /// Writes the Docs index to the disk
        /// </summary>
        public void WriteDocPosting()
        {
            StringBuilder allDocPosting = new StringBuilder();
            foreach(Doc doc in docsIndexer)
            {
                doc.UpdateEntities();
                allDocPosting.AppendLine(doc.ToStringBuilder().ToString());
            }
            string path = this.isStemming ? this._initialPathForPosting + postingWithStemmingDirectory : this._initialPathForPosting + postingDirectory;
            path += "\\docIndexer.txt";
            PostingsSet.Write(allDocPosting, path);
            allDocPosting = null;
        }
    }
}