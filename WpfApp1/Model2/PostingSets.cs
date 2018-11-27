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
    class PostingSets
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
            /// Given a term and its posting, adds them to the data structure (Thread safe).
            /// Returns true if successfuly added the term and posting (there was enough capacity).
            /// Returns false otherwise.
            /// </summary>
            /// <param name="term"></param>
            /// <param name="posting"></param>
            /// <returns></returns>
            public bool Add(string term, Posting posting)
            {
                mutex.WaitOne();
                if (hasCapacity())
                {
                    if (_termsDictionary.ContainsKey(term))
                    {
                        _termsDictionary[term].Add(posting);
                    }
                    else
                    {
                        List<Posting> newList = new List<Posting>();
                        newList.Add(posting);
                        _termsDictionary.Add(term, newList);
                        capacity--;
                    }
                }
                else
                {
                    mutex.ReleaseMutex();
                    dumpToDisk();
                    return false;
                }
                mutex.ReleaseMutex();
                return true;
            }

            public bool hasCapacity()
            {
                return capacity > 0;
            }


            public void dumpToDisk()
            {
                Task writer = Task.Run(() =>
                {
                    foreach (string term in _termsDictionary.Keys)
                    {
                        List<Posting> list = _termsDictionary[term];
                        list.OrderByDescending(posting => posting.tf);
                        foreach (Posting posting in list)
                        {
                            writePosting(posting.getPostingString().ToString(), posting.term.ElementAt(0));
                        }

                    }
                });
            }

            private void writePosting(string postingString, char firstLetter)
            {
                string fileName = ((firstLetter >= 'a' && firstLetter <= 'z') || (firstLetter >= 'A' && firstLetter <= 'Z')) ? "" + firstLetter : "other";
                string PostPath = _path + "\\" + fileName + ".txt";
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
    }
}
