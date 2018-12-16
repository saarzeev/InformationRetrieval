using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NetJSON;

namespace Model2
{
    public class PostingsSet
        {

        private static int counter = 0;
        //private SortedDictionary<string, List<Posting>> _termsDictionary = new SortedDictionary<string, List<Posting>>(); //terms, postings
        private SortedDictionary<string, List<CityPosting>> _citiesDictionary = new SortedDictionary<string, List<CityPosting>>(); //cities, postings
        private Mutex mutex = new Mutex();
        private int id;
        public int capacity = 100000;
        private string _path = "";
        private string _mergePath = "";
        private Dictionary<char, SortedDictionary<string, List<Posting>>> _allDictionarys;//= new Dictionary<char, SortedDictionary<string, List<Posting>>>();

        public PostingsSet(string destPath, bool isStemming)
        {
            this.initDictionary();
            id = counter;
            _mergePath = isStemming ? destPath + "\\postingWithStemming" : destPath + "\\posting";
            _path = destPath + "\\tmpPostingFiles";
            counter++;
        }

        public void initDictionary()
        {
            _allDictionarys = new Dictionary<char, SortedDictionary<string, List<Posting>>>();
            char c = 'a';
            for (int i = 0; i <= 25; i++)
            {
                _allDictionarys[c] = new SortedDictionary<string, List<Posting>>();
                c++;
            }
            _allDictionarys['#'] = new SortedDictionary<string, List<Posting>>();
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
                SortedDictionary<string, List<Posting>> relevantDict;
                if (term[0] <= 'z' && term[0] >= 'a')
                {
                    relevantDict = _allDictionarys[term[0]];
                }
                else {
                     relevantDict = _allDictionarys['#'];
                }

                if (relevantDict.ContainsKey(term))
                {
                    relevantDict[term].Add(posting);

                }
                else
                {
                    List<Posting> newList = new List<Posting>();
                    newList.Add(posting);
                    relevantDict.Add(term, newList);
                    capacity--;
                }
                //if (_termsDictionary.ContainsKey(term))
                //{
                //    _termsDictionary[term].Add(posting);

                //}
                //else
                //{
                //    List<Posting> newList = new List<Posting>();
                //    newList.Add(posting);
                //    _termsDictionary.Add(term, newList);
                //    capacity--;
                //}
            }
            else
            {
                return false;
            }
            return true;
        }

        public void Add(string term, List<Posting> posting)
        {
                SortedDictionary<string, List<Posting>> relevantDict;
                if (term[0] <= 'z' && term[0] >= 'a')
                {
                    relevantDict = _allDictionarys[term[0]];
                }
                else
                {
                    relevantDict = _allDictionarys['#'];
                }

                if (relevantDict.ContainsKey(term))
                {
                    relevantDict[term].AddRange(posting);

                }
                else
                {
                    relevantDict.Add(term, posting);
                }
        }
          
        
        public void AddCity(string city, List<Posting> fullPosting  /*CityPosting posting*/)
        {
            List<CityPosting> range = new List<CityPosting>();
            foreach( Posting post in fullPosting)
            {
                range.Add(new CityPosting(post.path, post.dId, post.gaps));
            }
            if (_citiesDictionary.ContainsKey(city))
            {
                _citiesDictionary[city].AddRange(range);
            }
            else
            {
               // List<CityPosting> newList = new List<CityPosting>();
                //newList.Add(posting);
                _citiesDictionary.Add(city, range /*newList*/);
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
        public void DumpToDisk(bool shouldRunAsDifferentTask = true, bool isFinalPostingFile = false)
        {
            if (shouldRunAsDifferentTask)
            {
                Task writer = Task.Run(() =>
                {
                    PerformDumpToDisk();
                });
            }
            else
            {
                PerformDumpToDisk(isFinalPostingFile);
            }
        }

        private void PerformDumpToDisk(bool isFinalPostingFile = false)
        {
            //char lastChar = ' ';
            //char currChar = ' ';
            //StringBuilder postingString = new StringBuilder("");
            //string finalTerm = "";
            //List<string> orderedKeys = _termsDictionary.Keys.ToList();
            //orderedKeys.Sort((x, y) => string.Compare(x, y));
            //foreach (string term in orderedKeys)
            //{

            //    lastChar = currChar;
            //    currChar = term.ElementAt(0);
            //    if (lastChar != ' ' && !isSameFile(lastChar, currChar))
            //    {
            //        writePosting(postingString, lastChar, isFinalPostingFile);
            //        postingString.Clear();
            //    }

            //    List<Posting> list = _termsDictionary[term];
            //    _termsDictionary.Remove(term);
            //    list.Sort((x1, x2) => x2.CompareTo(x1)); //Descending order, from highest to lowest tf
            //    postingString.AppendFormat("{0},{1},",term, list.Count);
            //    //term,df,(relPath,docID,tf,is100,[gaps],isLower,)*
            //    foreach (Posting posting in list)
            //    {
            //        postingString.Append(posting.GetPostingString().Remove(0, term.Length + 1) + ",");
            //    }
            //    postingString.Append('\n');
            //    finalTerm = term;
            //    list = null;
            //}
            //if (finalTerm != "")
            //{
            //    writePosting(postingString, finalTerm.ElementAt(0), isFinalPostingFile);
            //}
            //postingString.Clear();
            foreach (char c in _allDictionarys.Keys)
            {
                if (isFinalPostingFile)
                {
                    if (_allDictionarys[c].Count > 0)
                    {
                        /*String currDict = NetJSON.NetJSON.Serialize(_allDictionarys[c]);*///JsonConvert.SerializeObject(_termsDictionary, Formatting.Indented);
                        //byte[] dictToWrite = Encoding.ASCII.GetBytes(currDict);
                        writePosting(_allDictionarys[c], c, isFinalPostingFile);
                        //_allDictionarys.Remove(c);
                    }
                }
                else {
                    //String currDict = NetJSON.NetJSON.Serialize(_allDictionarys[c]);//JsonConvert.SerializeObject(_termsDictionary, Formatting.Indented);
                    //byte[] dictToWrite = Encoding.ASCII.GetBytes(currDict);
                    writePosting(_allDictionarys[c], c, isFinalPostingFile);
                }

            }

            _allDictionarys = null;
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

        private void writePosting(SortedDictionary<string,List<Posting>> dictionary/*byte[] dictToWrite*//*StringBuilder postingString*/, char firstLetter, bool isFinalPostingFile = false)
        {
            string fileName = (firstLetter >= 'a' && firstLetter <= 'z') ? "" + firstLetter : "other";
            string postPath = (isFinalPostingFile ? _mergePath :_path)  + "\\" + fileName + (isFinalPostingFile ? "FINAL" : this.id.ToString() )+ ".gz";
            Zip(dictionary, postPath);
            //Zip(postingString, postPath);
        }
 
        /// <summary>
        /// Given a <paramref name="str"/> and a <paramref name="path"/>, compresses and writes <paramref name="str"/> to <paramref name="path"/>.
        /// <paramref name="compressionLevel"/> default is set to CommpressionLevel.Fastest
        /// </summary>
        /// <param name="str"></param>
        /// <param name="path"></param>
        /// <param name="compressionLevel"></param>
        public static void Zip(/*byte[] dictToWrite*//*StringBuilder str*/SortedDictionary<string, List<Posting>> dictionary, string path/*, CompressionLevel compressionLevel = CompressionLevel.Fastest*/)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Formatting = Formatting.Indented;
            using (StreamWriter sw = new StreamWriter(path))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, dictionary);
            }
            //// byte[] raw = Encoding.ASCII.GetBytes(str.ToString());
            // using (MemoryStream memory = new MemoryStream())
            // {
            //     using (GZipStream gzip = new GZipStream(memory,
            //         compressionLevel))
            //     {
            //         gzip.Write(dictToWrite, 0, dictToWrite.Length);
            //     }
            //     File.WriteAllBytes(path, memory.ToArray());
            // }
        }
        public static void Zip(byte[] dictToWrite/*StringBuilder str*/, string path, CompressionLevel compressionLevel = CompressionLevel.Fastest)
        {
            //JsonSerializer serializer = new JsonSerializer();
            //serializer.NullValueHandling = NullValueHandling.Ignore;
            //serializer.Formatting = Formatting.Indented;
            //using (StreamWriter sw = new StreamWriter(path))
            //using (JsonWriter writer = new JsonTextWriter(sw))
            //{
            //    serializer.Serialize(writer, dictionary);
            //}
            //// byte[] raw = Encoding.ASCII.GetBytes(str.ToString());
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory,
                    compressionLevel))
                {
                    gzip.Write(dictToWrite, 0, dictToWrite.Length);
                }
                File.WriteAllBytes(path, memory.ToArray());
            }
        }
        /// <summary>
        /// Given a file path in <paramref name="bytes"/>, returns a unzipped StringBuilder of the file
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        //public static byte[]/*StringBuilder*/ Unzip(byte[] bytes)
        //{
        //    using (GZipStream stream = new GZipStream(new MemoryStream(bytes),
        //     CompressionMode.Decompress))
        //    {
        //        const int size = 4096;
        //        byte[] buffer = new byte[size];
        //        using (MemoryStream memory = new MemoryStream())
        //        {
        //            int count = 0;
        //            do
        //            {
        //                count = stream.Read(buffer, 0, size);
        //                if (count > 0)
        //                {
        //                    memory.Write(buffer, 0, count);
        //                }
        //            }
        //            while (count > 0);
        //            return memory.ToArray();/*new StringBuilder(Encoding.ASCII.GetString(memory.ToArray()));*/
        //        }
        //    }
        //}
        /// <summary>
        /// Merges all temporary posting files into final posting files.
        /// Running this method during the population of the temporary posting files will result in data loss and undesired behaviour.
        /// </summary>
        public void mergeFiles(HashSet<string> _cities)
        {
           
            //term,relPath,docID,tf,is100,[gaps],isLower
            //term,df,(relPath,docID,tf,is100,[gaps],isLower,)*
            for (char c = 'a'; c <= 'z'; c++)
            {
                initDictionary();
                // _allDictionarys = new Dictionary<char, SortedDictionary<string, List<Posting>>>();
                SortedDictionary<string, List<Posting>> termsDictionary = new SortedDictionary<string, List<Posting>>();
                string[] allTempFilesOfLetter = Directory.GetFiles(_path, c + "*", SearchOption.AllDirectories);
                foreach (string file in allTempFilesOfLetter)
                {
                    //string dict = Encoding.ASCII.GetString(Unzip(File.ReadAllBytes(file)));
                    //termsDictionary = NetJSON.NetJSON.Deserialize<SortedDictionary<string, List<Posting>>>(dict);
                    using (StreamReader read = File.OpenText(file))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        termsDictionary = (SortedDictionary<string, List<Posting>>)serializer.Deserialize(read, 
                            typeof(SortedDictionary<string, List<Posting>>));
                    }
                    //dict = null;
                    foreach (string key in termsDictionary.Keys)
                    {
                        this.Add(key, termsDictionary[key]);
                        string toUpper = key.ToUpper();
                        bool isCity = _cities.Contains(toUpper);
                        if (isCity)
                        {
                            this.AddCity(toUpper, termsDictionary[key]);
                        }
                    }
                    //StringBuilder currFile = Unzip(File.ReadAllBytes(file));
                    //string[] lines = currFile.ToString().Split('\n');
                    //currFile.Clear();
                    //foreach (string line in lines)
                    //{
                    //    string[] brokenLine = line.Split(',');
                    //    string term = brokenLine[0];
                    //    int df = -1;
                    //    if (1 < brokenLine.Length && brokenLine[1] != "")
                    //    {
                    //        int.TryParse(brokenLine[1], out df);

                    //        for (int i = 2; i < brokenLine.Length; i++)
                    //        {
                    //            if (brokenLine[i] != "")
                    //            {
                    //                string ToUpper = term.ToUpper();
                    //                bool isCity = _cities.Contains(ToUpper);

                    //                StringBuilder cityPostingStr = new StringBuilder();
                    //                if (isCity)
                    //                {
                    //                    cityPostingStr.AppendFormat("{0},{1},", brokenLine[i], //relPath
                    //                                       brokenLine[i + 1]); //docID
                    //                }

                    //                StringBuilder postingStr = new StringBuilder(term + ","); //term
                    //                postingStr.AppendFormat("{0},{1},{2},{3},", brokenLine[i++], //relPath,
                    //                                    brokenLine[i++], //docID,
                    //                                    brokenLine[i++], //tf,
                    //                                    brokenLine[i++]  //is100,
                    //                                    );

                    //                while (!brokenLine[i].Contains("]"))
                    //                {
                    //                    if (isCity)
                    //                    {
                    //                        cityPostingStr.Append(brokenLine[i] + ",");
                    //                    }
                    //                    postingStr.Append(brokenLine[i++] + ",");
                    //                }

                    //                if (isCity)
                    //                {
                    //                    cityPostingStr.Append(brokenLine[i] + ",");
                    //                    this.AddCity(ToUpper, new CityPosting(cityPostingStr/*, city*/));
                    //                    cityPostingStr.Clear();
                    //                }
                    //                postingStr.Append(brokenLine[i++] + ","); //gap],
                    //                postingStr.Append(brokenLine[i]); //isLower

                    //                this.Add(term, new Posting(postingStr), limitCapacity: false);
                    //                postingStr.Clear();

                    //            }
                    //        }
                    //    }
                    //}
                    File.Delete(file);
                   
                }
                DumpToDisk(false, true);
                //At this point,_termsDictionary holds the entire postings collection for letter c.
            }
            WriteCitiesIndex();
        }

        private void WriteCitiesIndex()
        {
            StringBuilder postingString = new StringBuilder("");
            //string finalTerm = "";
            //List<string> orderedKeys = _citiesDictionary.Keys.ToList();
            //orderedKeys.Sort((x, y) => string.Compare(x, y));
            foreach (string city in _citiesDictionary.Keys)
            {
                //List<CityPosting> list = _citiesDictionary[city];
                //postingString.Append(city + ",");
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
                foreach (CityPosting posting in _citiesDictionary[city])
                {
                    if (isFirst)
                    {
                        isFirst = false;
                        postingString.AppendFormat("{0},{1},{2}", currCity.GetCountry, currCity.GetCurrency, currCity.GetPop);
                    }
                    postingString.AppendFormat (",{0},{1},{2}{3}{4}", posting.path ,posting.docID ,"[",posting.gaps.ToString(),"]");

                    
                }
                postingString.Append('\n');
                //_citiesDictionary.Remove(city);
            }
            _citiesDictionary.Clear();
            string cityIndexPath = _mergePath + "\\" + "CityIndex.gz";
            byte[] dictToWrite = Encoding.ASCII.GetBytes(postingString.ToString());
            postingString.Clear();
            Zip(dictToWrite, cityIndexPath);         
            dictToWrite = null;
            GC.Collect();
        }
    }
}
