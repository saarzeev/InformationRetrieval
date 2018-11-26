using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            FileStream fs = new FileStream(_currentFile, FileMode.Open, FileAccess.Read);
            const Int32 BufferSize = 4096;
            using (var fileStream = File.OpenRead(_currentFile))
            using (var streamReader = new StreamReader(_currentFile))
            {

                String line;
                while (!streamReader.EndOfStream)
                {
                    while ((line = streamReader.ReadLine()) != null && line.CompareTo("</DOC>") != 0)
                    {
                        doc.Append(line);
                        doc.Append("\\n");
                    }

                    if (line != null)
                    {
                        doc.Append("</DOC>");
                        retVal = new Doc(this._currentFile, doc, this._docINdexInFile);
                        _docINdexInFile++;
                        retValList.Add(retVal);
                        doc = new StringBuilder();
                    }
                }
            }

            _currentFile = "";
            
            return retValList;
        }

        public bool HasNext()
        {
            return !(_files.Count == 0 && _currentFile == ""); //THIS IS SHITTY
        }
    }
}
