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
                //TODO stopWords is in the corpus 
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

            FileStream fs = new FileStream(_currentFile, FileMode.Open, FileAccess.Read);
            using (var fileStream = File.OpenRead(_currentFile))
            using (var streamReader = new StreamReader(_currentFile))
            {

                String line;
               

                while (!streamReader.EndOfStream && _currentFile != path + "\\stopwords.txt")
                {
                    string docID = "";
                    string countery = "";
                    string city = "";
                    string language = "";
                    bool notText = true;
                    bool firstAfterText = false;
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
                            //countery
                            else if (line.StartsWith("<F P=101>"))
                            {
                                string[] del = { "<F P=101>", "</F P=101>", "</F>" ," "};
                                string[] splited = line.Split(del, StringSplitOptions.RemoveEmptyEntries);
                                if (splited.Length > 0)
                                {
                                    countery = splited[0];
                                }
                            }
                            //city
                            else if (line.StartsWith("<F P=104>"))
                            {
                                string[] del = { "<F P=104>", "</F P=104>", "</F>" ," "};
                                string[] splited = line.Split(del,StringSplitOptions.RemoveEmptyEntries);
                                if (splited.Length > 0) { city = splited[0]; }
                            }
                            else if(line == "<TEXT>")
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
                                if (line.StartsWith("Language")){
                                    string[] del = { "Language:","<F P=105>", "</F P=105>", "</F>", " "};
                                    string[] splited = line.Split(del, StringSplitOptions.RemoveEmptyEntries);
                                    if (splited.Length > 0)
                                    {
                                        language = splited[0];
                                    }
                                }
                                firstAfterText = false;
                            }
                            doc.AppendFormat("{0}{1}",line,"\\n");
                        }
                       
                    }

                    if (line != null)
                    {
                        retVal = new Doc(this._currentFile, doc, docID, city, countery, language);
                        notText = true;
                        firstAfterText = false;
                        docID = "";
                        countery = "";
                        city = "";
                        language = "";
                        _docINdexInFile++;
                        retValList.Add(retVal);
                        doc = new StringBuilder();
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
