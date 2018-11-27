
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
        
        public void setDocVocabularytoFullVocabulary(Doc doc ,SortedDictionary<string, Term> docDictionary)
        {
            string[] docPath = doc._path.Split('\\');
            StringBuilder terms = new StringBuilder();

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
                terms.AppendLine(getPostingString(docDictionary[key], docPath[docPath.Length - 1], doc._indexInFile).ToString());
            }
            writePosting(terms.ToString(), docPath[docPath.Length - 1]);
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
        //TODO gaps dont work!
        public StringBuilder getPostingString(Term term, string docFullPath, long dicID)
        {

            StringBuilder posting = new StringBuilder(term.GetTerm + ",");
            posting.Append("1," + docFullPath + ",");
            posting.Append(dicID + ",");
            posting.Append(term.Tf + ",");
            string is100 = term.IsIn100 ? "1" : "0";
            posting.Append(is100 + ",");
            posting.Append("[");
            posting.Append(getGaps(term.Positons));
            posting.Append("],");
            string isLower = term.IsLowerCase ? "1" : "0";
            posting.Append(isLower);
            return posting;
            //byte[] comma = Encode(',');
            //byte[] termString = Encoding.UTF8.GetBytes(term.GetTerm);
            //byte[] df = Encode(1);
            //byte[] docFullPathString = Encoding.UTF8.GetBytes(docFullPath);
            //byte[] dicIDInt = Encode((int)dicID);
            //byte[] is100 = Encode(term.IsIn100 ? 1 : 0);
            //byte[] leftpar = Encode('[');
            //byte[] tfByte = Encode(term.Tf);
            //byte[] rigthtpar = Encode(']');
            //byte[][] gaps = getGaps(term.Positons ,new byte[term.Positons.Count][]);
            //byte[] isLower = Encode(term.IsLowerCase ? 1 : 0);

            //int size = termString.Length + df.Length + docFullPathString.Length + dicIDInt.Length + is100.Length + leftpar.Length + tfByte.Length + rigthtpar.Length + isLower.Length + (7 * comma.Length);
            //foreach(byte[] gap in gaps)
            //{
            //    size += gap.Length;
            //}
            //size += comma.Length * (gaps.Length - 1);

            //byte[] posting = new byte[size];
            ////term,df,path,id,tf,is100,[gaps+comas],islower
            //int  i = 0;
            //for (int j = 0; j < termString.Length; i++, j++)
            //{
            //    posting[i] = termString[j];
            //}

            //for(int j = 0; j < comma.Length; i++, j++)
            //{
            //    posting[i] = comma[j];
            //}

            //for (int j = 0; j < df.Length; i++, j++)
            //{
            //    posting[i] = df[j];
            //}

            //for (int j = 0; j < comma.Length; i++, j++)
            //{
            //    posting[i] = comma[j];
            //}

            //for (int j = 0; j < docFullPathString.Length; i++, j++)
            //{
            //    posting[i] = docFullPathString[j];
            //}

            //for (int j = 0; j < comma.Length; i++, j++)
            //{
            //    posting[i] = comma[j];
            //}

            //for (int j = 0; j < dicIDInt.Length; i++, j++)
            //{
            //    posting[i] = dicIDInt[j];
            //}

            //for (int j = 0; j < comma.Length; i++, j++)
            //{
            //    posting[i] = comma[j];
            //}


            //for (int j = 0; j < tfByte.Length; i++, j++)
            //{
            //    posting[i] = tfByte[j];
            //}

            //for (int j = 0; j < comma.Length; i++, j++)
            //{
            //    posting[i] = comma[j];
            //}

            //for (int j = 0; j < is100.Length; i++, j++)
            //{
            //    posting[i] = is100[j];
            //}

            //for (int j = 0; j < comma.Length; i++, j++)
            //{
            //    posting[i] = comma[j];
            //}

            //for (int j = 0; j < leftpar.Length; i++, j++)
            //{
            //    posting[i] = leftpar[j];
            //}

            //for (int gap = 0; gap < gaps.Length; gap++)
            //{
            //    for (int j = 0; j < gaps[gap].Length; i++, j++)
            //    {
            //        posting[i] = gaps[gap][j];
            //    }

            //    if (gap + 1 < gaps.Length)
            //    {
            //        for (int j = 0; j < comma.Length; i++, j++)
            //        {
            //            posting[i] = comma[j];
            //        }
            //    }

            //}

            //for (int j = 0; j < rigthtpar.Length; i++, j++)
            //{
            //    posting[i] = rigthtpar[j];
            //}

            //for (int j = 0; j < comma.Length; i++, j++)
            //{
            //    posting[i] = comma[j];
            //}

            //for (int j = 0; j < isLower.Length; i++, j++)
            //{
            //    posting[i] = isLower[j];
            //}

            //return posting;

        }
        public StringBuilder getGaps(HashSet<int> positionsHash)
        {
            int[] positions = new int[positionsHash.Count];
            positionsHash.CopyTo(positions, 0);

            StringBuilder gaps = new StringBuilder();
            gaps.Append(positions[0]);
            for (int i = 1; i < positions.Length; i++)
            {
                int gap = (positions[i] - positions[i - 1]);
                gaps.Append("," + gap);
            }
            return gaps;
        }

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