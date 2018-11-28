using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Model2
{
    class PostingsSet
        {

        private static int counter = 0;
        private Mutex mutex = new Mutex();
        private Dictionary<string, List<Posting>> _termsDictionary = new Dictionary<string, List<Posting>>(); //terms, postings
        private int id;
        private static PostingsSet ps;
        private int capacity = 5000;
        private string _path = "";



        public PostingsSet(string destPath)
        {
            id = counter;
            _path = destPath;
            counter++;
        }
        /// <summary>
        /// Given a term and its posting, adds them to the data structure (It is NOT thread safe).
        /// Returns true if successfuly added the term and posting (there was enough capacity).
        /// Returns false otherwise.
        /// </summary>
        /// <param name="term"></param>
        /// <param name="posting"></param>
        /// <returns></returns>
        public bool Add(string term, Posting posting)
        {
            if (hasCapacity())
            {
                if (_termsDictionary.ContainsKey(term))
                {
                    _termsDictionary[term].Add(posting);
                    capacity--;
                }
                else
                {
                    List<Posting> newList = new List<Posting>();
                    newList.Add(posting);
                    _termsDictionary.Add(term, newList);
                   
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public bool hasCapacity()
        {
            return capacity > 0;
        }


        public void DumpToDisk()
        {
            Task writer = Task.Run(() =>
            {
                foreach (string term in _termsDictionary.Keys)
                {
                    List<Posting> list = _termsDictionary[term];
                    list.Sort((x1, x2) => x2.CompareTo(x1)); //Descending order, from highest to lowest tf
                    StringBuilder postingString = new StringBuilder("");

                    //term,df,(relPath,docID,tf,is100,[gaps],isLower,)*
                    foreach (Posting posting in list)
                    {
                        postingString.Append(posting.getPostingString().Remove(0, term.Length + 1) + ",");
                    }
                    string res = term + "," + list.Count + "," + postingString.ToString();
                    //writePosting((term + "," + df + "," + postingString), term.ElementAt(0));
                }
                _termsDictionary = null;
            });
        }

        private void writePosting(string postingString, char firstLetter)
        {
            string fileName = ((firstLetter >= 'a' && firstLetter <= 'z') || (firstLetter >= 'A' && firstLetter <= 'Z')) ? "" + firstLetter : "other";
            string PostPath = _path + "\\" + fileName + id + ".txt";
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

        public void mergeFiles()
        {
            /*var fileArray = Directory.EnumerateFiles(this._initialPathForPosting, "*.txt");
            Queue<string> files = new Queue<string>(fileArray);
            while (files.Count > 1)
            {
                var bytes = File.ReadAllBytes(files.Dequeue());
                string unziped = Unzip(bytes);

            }*/


        }

    }
}
