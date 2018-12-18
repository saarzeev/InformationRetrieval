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
    public class PostingsSet
        {

        private static int counter = 0;
        private Dictionary<string, List<Posting>> _termsDictionary = new Dictionary<string, List<Posting>>(); //terms, postings
        private Dictionary<string, List<CityPosting>> _citiesDictionary = new Dictionary<string, List<CityPosting>>(); //cities, postings
        private Mutex mutex = new Mutex();
        private int id;
        public int capacity = 100000;
        private string _path = "";
        private string _mergePath = "";

        public PostingsSet(string destPath, bool isStemming)
        {
            id = counter;
            _mergePath = isStemming ? destPath + "\\postingWithStemming" : destPath + "\\posting";
            _path = destPath + "\\tmpPostingFiles";
            counter++;
        }
        /// <summary>
        /// Given a term and its posting, adds them to the data structure (It is NOT thread safe).
        /// Returns true if successfuly added the term and posting (there was enough capacity).
        /// Returns false otherwise.
        /// </summary>
        /// <param name="term"></param>
        /// <param name="posting"></param>
        /// <param name="limitCapacity"></param>
        /// <returns></returns>
        public bool Add(string term, Posting posting, bool limitCapacity = true)
        {
            if (!limitCapacity || hasCapacity())
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
                return false;
            }
            return true;
        }

        public void AddCity(string city, CityPosting posting)
        {

            if (_citiesDictionary.ContainsKey(city))
            {
                _citiesDictionary[city].Add(posting);
            }
            else
            {
                List<CityPosting> newList = new List<CityPosting>();
                newList.Add(posting);
                _citiesDictionary.Add(city, newList);
            }
        }

        public bool hasCapacity()
        {
            return capacity > 0;
        }

        /// <summary>
        /// A wrapper method.
        /// Writes to path the entire collection of postings in the following format:
        /// term,df,(relPath,docID,tf,is100,[gaps],isLower,)*
        /// When done, collection is set to null .
        /// </summary>
        public void DumpToDisk(bool final = false)
        {
            if (final)
            {
                PerformDumpToDisk();

            }
            else
            {
                Task writer = new Task(() =>
                    {
                        PerformDumpToDisk();
                    });

                Parse.writingList.Add(writer);
                writer.Start();
            }
        }

        private void PerformDumpToDisk()
        {
            char lastChar = ' ';
            char currChar = ' ';
            StringBuilder postingString = new StringBuilder("");
            string finalTerm = "";
            List<string> orderedKeys = _termsDictionary.Keys.ToList();
            orderedKeys.Sort((x, y) => string.Compare(x, y));
            foreach (string term in orderedKeys)
            {

                lastChar = currChar;
                currChar = term.ElementAt(0);
                if (lastChar != ' ' && !isSameFile(lastChar, currChar))
                {
                    writePosting(postingString, lastChar);
                    postingString.Clear();
                }

                List<Posting> list = _termsDictionary[term];
                _termsDictionary.Remove(term);
                list.Sort((x1, x2) => x2.CompareTo(x1)); //Descending order, from highest to lowest tf
                postingString.AppendFormat("{0},{1},",term, list.Count);
                //term,df,(relPath,docID,tf,is100,[gaps],isLower,)*
                foreach (Posting posting in list)
                {
                    postingString.Append(posting.GetPostingString().Remove(0, term.Length + 1) + ",");
                }
                postingString.Append('\n');
                finalTerm = term;
                list = null;
            }
            if (finalTerm != "")
            {
                writePosting(postingString, finalTerm.ElementAt(0));
            }
            postingString.Clear();
            _termsDictionary = null;
            GC.Collect();
        }

        private void PerformDumpToDiskLineByLine ()
        {
           
            char lastChar = ' ';
            char currChar = ' ';
            StringBuilder postingString = new StringBuilder("");
            string finalTerm = "";
            List<string> orderedKeys = _termsDictionary.Keys.ToList();
            orderedKeys.Sort((x, y) => string.Compare(x, y));
            long pos = 0;
            foreach (string term in orderedKeys)
            {
                Indexer.fullDictionary[term].Position = pos;
                lastChar = currChar;
                currChar = term.ElementAt(0);
                if (lastChar != ' ' && !isSameFile(lastChar, currChar))
                {
                    pos = 0;
                    Indexer.fullDictionary[term].Position = pos;
                    string fileName = (lastChar >= 'a' && lastChar <= 'z') ? "" + lastChar : "other";
                    string postPath = (_mergePath  + "\\" + fileName + "FINAL.txt");
                    using (FileStream fs = new FileStream(postPath, FileMode.Append, FileAccess.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.Write(postingString);
                        }
                    }
                    postingString.Clear();
                }

                List<Posting> list = _termsDictionary[term];
                _termsDictionary.Remove(term);
                list.Sort((x1, x2) => x2.CompareTo(x1)); //Descending order, from highest to lowest tf
                postingString.AppendFormat("{0},{1},", term, list.Count);
                //term,df,(relPath,docID,tf,is100,[gaps],isLower,)*
                foreach (Posting posting in list)
                {
                    postingString.Append(posting.GetPostingStringFinal().Remove(0, term.Length + 1) + ",");
                }
                postingString.Append('\n');
                pos = postingString.Length + 1;
                finalTerm = term;
                list = null;
            }
            if (finalTerm != "")
            {
                string fileName = (lastChar >= 'a' && lastChar <= 'z') ? "" + lastChar : "other";
                string postPath = (_mergePath + "\\" + fileName + "FINAL.txt");
                using (FileStream fs = new FileStream(postPath, FileMode.Append, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.Write(postingString);
                    }
                }
            }
            postingString.Clear();
            _termsDictionary = null;
            GC.Collect();
        }
        
        /// <summary>
        /// Returns whether or not the current term should be in the same file with the previous one.
        /// </summary>
        /// <param name="lastChar"></param>
        /// <param name="currChar"></param>
        /// <returns></returns>
        private static bool isSameFile(char lastChar, char currChar)
        {            
            return ((currChar >= 'a' && currChar <= 'z') && lastChar == currChar) || //they both begin with the same letter
                                    ((!(currChar >= 'a' && currChar <= 'z') && !(lastChar >= 'a' && lastChar <= 'z'))); //they both begin with a special char that is not a letter
        }

        private void writePosting(StringBuilder postingString, char firstLetter, bool isFinalPostingFile = false)
        {
            string fileName = (firstLetter >= 'a' && firstLetter <= 'z') ? "" + firstLetter : "other";
            string postPath = (isFinalPostingFile ? _mergePath :_path)  + "\\" + fileName + (isFinalPostingFile ? "FINAL" : this.id.ToString() )+ ".gz";

            Zip(postingString, postPath);
        }

        /// <summary>
        /// Given a <paramref name="str"/> and a <paramref name="path"/>, compresses and writes <paramref name="str"/> to <paramref name="path"/>.
        /// <paramref name="compressionLevel"/> default is set to CommpressionLevel.Fastest
        /// </summary>
        /// <param name="str"></param>
        /// <param name="path"></param>
        /// <param name="compressionLevel"></param>
        public static void Zip(StringBuilder str, string path, CompressionLevel compressionLevel = CompressionLevel.Fastest)
        {
            byte[] raw = Encoding.UTF8.GetBytes(str.ToString());
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory,
                    compressionLevel))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                File.WriteAllBytes(path, memory.ToArray());
            }
        }

        /// <summary>
        /// Given a file path in <paramref name="bytes"/>, returns a unzipped StringBuilder of the file
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static StringBuilder Unzip(byte[] bytes)
        {
            using (GZipStream stream = new GZipStream(new MemoryStream(bytes),
             CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return new StringBuilder(Encoding.UTF8.GetString(memory.ToArray()));
                }
            }
        }
        /// <summary>
        /// Merges all temporary posting files into final posting files.
        /// Running this method during the population of the temporary posting files will result in data loss and undesired behaviour.
        /// </summary>
        public void mergeFiles(HashSet<string> _cities)
        {
            //term,relPath,docID,tf,is100,[gaps],isLower
            //term,df,(relPath,docID,tf,is100,[gaps],isLower,)*
            for (char c = 'z'; c >= 'a'; c--)
            {
                _termsDictionary = new Dictionary<string, List<Posting>>();
                string[] allTempFilesOfLetter = Directory.GetFiles(_path, c + "*", SearchOption.AllDirectories);
                foreach (string file in allTempFilesOfLetter)
                {
                    StringBuilder currFile = Unzip(File.ReadAllBytes(file));
                    string[] lines = currFile.ToString().Split('\n');
                    currFile.Clear();
                    foreach (string line in lines)
                    {
                        string[] brokenLine = line.Split(',');
                        string term = brokenLine[0];
                        int df = -1;
                        if (1 < brokenLine.Length && brokenLine[1] != "")
                        {
                            int.TryParse(brokenLine[1],out df);

                            for (int i = 2; i < brokenLine.Length; i++)
                            {
                                if (brokenLine[i] != "")
                                {
                                    string ToUpper = term.ToUpper();
                                    bool isCity = _cities.Contains(ToUpper);

                                    StringBuilder cityPostingStr = new StringBuilder();
                                    if (isCity)
                                    { 
                                        cityPostingStr.AppendFormat("{0},",brokenLine[i]); //docID
                                    }

                                    StringBuilder postingStr = new StringBuilder(term + ","); //term
                                    postingStr.AppendFormat("{0},{1},{2},", 
                                                        brokenLine[i++] , //docID,
                                                        brokenLine[i++] , //tf,
                                                        brokenLine[i++]  //is100,
                                                        );

                                    while (!brokenLine[i].Contains("]"))
                                    {
                                        if (isCity)
                                        {
                                            cityPostingStr.Append(brokenLine[i] + ",");
                                        }
                                        i++;
                                    }

                                    if (isCity)
                                    {
                                        cityPostingStr.Append(brokenLine[i] + ",");//gap],
                                        this.AddCity(ToUpper, new CityPosting(cityPostingStr));
                                        cityPostingStr.Clear();
                                    }
                                    i++;
                                    postingStr.Append(brokenLine[i]); //isLower
                                    this.Add(term, new Posting(postingStr), limitCapacity: false);
                                    postingStr.Clear();
                                    
                                }
                            }
                        }
                    }
                    File.Delete(file);
                }

                //At this point,_termsDictionary holds the entire postings collection for letter c.

                PerformDumpToDiskLineByLine();
            }
            WriteCitiesIndex();
        }

        private void WriteCitiesIndex()
        {
            StringBuilder postingString = new StringBuilder("");
            string finalTerm = "";
            List<string> orderedKeys = _citiesDictionary.Keys.ToList();
            orderedKeys.Sort((x, y) => string.Compare(x, y));
            foreach (string city in orderedKeys)
            {
                List<CityPosting> list = _citiesDictionary[city];
                postingString.Append(city + ",");
                City currCity = null;
                mutex.WaitOne();
                if (!City.citiesInfo.ContainsKey(city))
                {
                   new City(city);
                }

                if (City.citiesInfo.ContainsKey(city))
                {
                    currCity = City.citiesInfo[city];
                }
                mutex.ReleaseMutex();
                //_city,_country,_currency,population,(relPath,docID,[gaps],)*
                bool isFirst = true;
                foreach (CityPosting posting in list)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                        postingString.AppendFormat("{0},{1},{2}", currCity.GetCountry, currCity.GetCurrency, currCity.GetPop);
                    }
                    postingString.AppendFormat (",{0},{1}{2}{3}", posting.docID ,"[",posting.gaps.ToString(),"]");
                }
                postingString.Append('\n');
                _citiesDictionary.Remove(city);
                list.Clear();
            }
            _citiesDictionary.Clear();
            orderedKeys = null;
            string cityIndexPath = _mergePath + "\\" + "CityIndex.gz";
            Zip(postingString, cityIndexPath);
            postingString.Clear();
            GC.Collect();
        }
    }
}
