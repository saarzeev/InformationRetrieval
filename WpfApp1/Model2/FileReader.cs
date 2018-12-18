using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace Model2
{
    class FileReader
    {
        public string path { get; private set; }
        private Queue<string> _files = new Queue<string>();
        private string _currentFile = "";
        private long _docINdexInFile;
        public HashSet<string> stopWords;

        public FileReader(string filePath,string stopWordPath )
        {
            this.path = filePath;
            string[] allfiles = Directory.GetFiles(this.path, "*.*", SearchOption.AllDirectories);
            foreach (var file in allfiles)
            {
                _files.Enqueue(file);
            }
            UpdateStopWords(stopWordPath);
        }

        public void UpdateStopWords(string stopWordPath)
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
                this.stopWords = stopWord;
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
                using (var fileStream = File.OpenRead(_currentFile))
                {
                    using (var streamReader = new StreamReader(_currentFile))
                    {
                        String line;
                        while (!streamReader.EndOfStream && _currentFile != path + "\\stopwords.txt")
                        {
                            string docID = "";
                            string city = "";
                            bool notText = true;
                            while ((line = streamReader.ReadLine()) != null && line != "</DOC>")
                            {
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
                                    }
                                }
                                else
                                {
                                    doc.AppendFormat("{0}{1}", line, "\\n");
                                }

                            }

                            if (line != null)
                            {
                                retVal = new Doc( doc, docID, city);
                                notText = true;
                                docID = "";
                                city = "";
                                _docINdexInFile++;
                                retValList.Add(retVal);
                                doc = new StringBuilder("");
                            }
                        }
                    }
                }
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
