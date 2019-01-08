using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace Model2
{
    public class FileReader
    {
        public string path { get; private set; }
        public Queue<string> _files = new Queue<string>();
        private string _currentFile = "";
        private long _docINdexInFile;
        public HashSet<string> stopWords;

        public FileReader(string filePath, string stopWordPath)
        {
            this.path = filePath;
            string[] allfiles = Directory.GetFiles(this.path, ".", SearchOption.AllDirectories);
            foreach (var file in allfiles)
            {
                _files.Enqueue(file);
            }
            this.stopWords = UpdateStopWords(stopWordPath);
        }

        public static HashSet<string> UpdateStopWords(string stopWordPath)
        {
            using (var streamReader = new StreamReader(stopWordPath))
            {
                String line;
                HashSet<string> stopWord = new HashSet<string>();
                while (!streamReader.EndOfStream)
                {
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        line = line.Replace("'", "");
                        stopWord.Add(line.ToLower());
                    }

                }
                return stopWord;
            }
        }
        public List<Doc> ReadNextDoc()
        {
            Doc retVal;
            List<Doc> retValList = new List<Doc>();
            if (_currentFile == "")
            {
                _currentFile = _files.Dequeue();
                _docINdexInFile = 1;
            }

            StringBuilder doc = new StringBuilder("");

            using (FileStream fs = new FileStream(_currentFile, FileMode.Open, FileAccess.Read))
            {
                //using (var fileStream = File.OpenRead(_currentFile))
                //{
                using (var streamReader = new StreamReader(_currentFile, Encoding.UTF8))
                {
                    if (_currentFile != path + "\\stopwords.txt")
                    {
                        string line = "";
                        //while (!streamReader.EndOfStream && _currentFile != path + "\\stopwords.txt")
                        //{
                        string docID = "";
                        string city = "";
                        bool notText = true;
                        string language = "";
                        bool firstAfterText = false;
                        string[] file = streamReader.ReadToEnd().Split('\n');
                        int numOfLine = 0;
                        //while ((line = streamReader.ReadLine()) != null && line != "</DOC>")
                        //for (int i = 0; i < file.Length; i++)
                        while (numOfLine < file.Length)
                        {
                            
                            
                            while (line != "</DOC>" && numOfLine < file.Length)
                            {
                                line = file[numOfLine];
                                numOfLine++;
                                if (notText)
                                {
                                    //id
                                    if (line.StartsWith("<DOCNO>"))
                                    {
                                        string[] del = { "<DOCNO>", "</DOCNO>", " " };
                                        string[] splited = line.Split(del, StringSplitOptions.RemoveEmptyEntries);
                                        if (splited.Length > 0)
                                        {
                                            docID = splited[0];
                                        }
                                    }
                                    //city
                                    else if (line.StartsWith("<F P=104>"))
                                    {
                                        string[] del = { "<F P=104>", "</F P=104>", "</F>", " " };
                                        string[] splited = line.Split(del, StringSplitOptions.RemoveEmptyEntries);
                                        if (splited.Length > 0) { city = splited[0]; }
                                    }
                                    else if (line == "<TEXT>")
                                    {
                                        notText = false;
                                        doc.Append(line);
                                        firstAfterText = true;
                                    }
                                }
                                else
                                {
                                    if (firstAfterText)
                                    {
                                        if (line.StartsWith("Language"))
                                        {
                                            string[] del = { "Language:", "<F P=105>", "</F P=105>", "</F>", " " };
                                            string[] splited = line.Split(del, StringSplitOptions.RemoveEmptyEntries);
                                            if (splited.Length > 0)
                                            {
                                                language = splited[0];
                                            }
                                        }
                                        firstAfterText = false;
                                    }
                                    doc.AppendFormat("{0}{1}", line, "\\n");
                                }

                            }
                            if (docID != "") {
                                retVal = new Doc(doc, docID, city, language);
                                notText = true;
                                docID = "";
                                city = "";
                                language = "";
                                retValList.Add(retVal);
                            }
                            doc = new StringBuilder("");
                            if (numOfLine < file.Length) { line = file[numOfLine]; }
                            // _docINdexInFile++;

                        }
                    }
                    //}
                }
                //}
            }
            _currentFile = "";
            return retValList;
        }

        /// <summary>
        /// Returns whether or not there is another file to read.
        /// </summary>
        /// <returns></returns>
        public bool HasNext()
        {
            return !(_files.Count == 0 && _currentFile == "");
        }
    }
}