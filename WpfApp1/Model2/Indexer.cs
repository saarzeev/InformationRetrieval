using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
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
                        fullDictionary.Add(key, newTerm);
                    }
                terms.AppendLine(getPostingString(docDictionary[key], docPath[docPath.Length - 1], doc._indexInFile).ToString());
            }
            writePosting(terms.ToString(), docPath[docPath.Length - 1]);
            dictionaryMutex.ReleaseMutex();
        }

        //TODO zip
        private void writePosting(string postingString, string path)
        {
            string PostPath = _initialPathForPosting + "\\" + path + ".txt";

            //byte[] bytes = Zip(postingString);
            //if (writingStream.CanWrite)
            //{
            //    writingStream.Write(bytes,writingStream.Seek(0,SeekOrigin.End),bytes.Length);
            //    writingStream.WriteByte(33);
            //}
            //writingStream.Close();
            using (var file = File.Open(PostPath, FileMode.Append))
            {
                using (var stream = new StreamWriter(file))
                {
                    stream.Write(postingString);
                    stream.Close();
                    file.Close();
                }
                
            }

        }

        // docPath(last value of path)+docId(int)+tf(int)+is100(0-false 1-true)+gapPositins(int[])+isLowerCase(0-false 1-true)
        //TODO gaps dont work!
        public StringBuilder getPostingString(Term term, string docFullPath, long dicID)
        {
           
            StringBuilder posting = new StringBuilder("1," + docFullPath + ",");
            posting.Append(dicID + ",");
            posting.Append(term.Tf + ",");
            string is100 = term.IsIn100 ? "1" : "0";
            posting.Append(is100 + ",");
            posting.Append("[");
            posting.Append(getGaps(term.Positons));
            posting.Append("],");
            string isLower = term.IsLowerCase ? "1" : "0";
            posting.Append(is100 );
            return posting;
        }

        public StringBuilder getGaps(HashSet<int> positionsHash)
        {
            int[] positions = new int[positionsHash.Count];
            positionsHash.CopyTo( positions, 0);
            
            StringBuilder gaps = new StringBuilder();
            gaps.Append(positions[0]);
            
            for(int i = 1 ; i < positions.Length ; i++)
            {
                int gap = (positions[i] - positions[i - 1]);
                gaps.Append("," + gap);
            }
            return gaps;
        }
    }
} 