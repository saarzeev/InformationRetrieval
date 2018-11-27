
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
        
        public Queue<Posting> setDocVocabularytoFullVocabulary(Doc doc ,SortedDictionary<string, Term> docDictionary)
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
                            fullDictionary[key].IsLowerCase = true ;
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

        private void writePosting(string postingString, string path)
        {
            string PostPath = _initialPathForPosting + "\\" + path + ".txt";
            dictionaryMutex.WaitOne();
            using (var file = File.Open(PostPath, FileMode.Append))
            {
                using (BinaryWriter writer = new BinaryWriter(file))
                {
                    var bytes = Zip(postingString);
                    writer.Write(bytes);
                    writer.Close();
                    file.Close();
                }
            }
            dictionaryMutex.ReleaseMutex();
        }

        public static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }
                return mso.ToArray();
            }
        }

        public static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }
                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        // docPath(last value of path)+docId(int)+tf(int)+is100(0-false 1-true)+gapPositins(int[])+isLowerCase(0-false 1-true)
       

    public void mergeFiles()
        {
            var fileArray = Directory.EnumerateFiles(this._initialPathForPosting, "*.txt");
            Queue<string> files = new Queue<string>(fileArray);
            while (files.Count > 1)
            {
                var bytes = File.ReadAllBytes(files.Dequeue());
                string unziped = Unzip(bytes);
                
            }
            

        }

    }
} 