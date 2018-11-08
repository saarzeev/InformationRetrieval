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
        private long _currentPosition;

        public FileReader(string path)
        {
            this.path = path;
            string[] allfiles = Directory.GetFiles(this.path, "*.*", SearchOption.AllDirectories);
            foreach (var file in allfiles)
            {
                _files.Enqueue(file);
            }
        }

        public Doc ReadNextDoc()
        {
            Doc retVal;
            bool endOfFile = false;
            if (_currentFile == "")
            {
                _currentFile = _files.Dequeue();
                _currentPosition = 0;
            }

            StringBuilder doc = new StringBuilder("");

            FileStream fs = new FileStream(_currentFile, FileMode.Open, FileAccess.Read);
            const Int32 BufferSize = 4096;
            using (var fileStream = File.OpenRead(_currentFile))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            {
                fileStream.Seek(_currentPosition, 0);
                String line;
                while (!streamReader.EndOfStream && (line = streamReader.ReadLine()) != null && line.CompareTo("</DOC>") != 0)
                {
                    doc.Append(line);
                    doc.Append("\\n");
                }
                if (!streamReader.EndOfStream)
                {
                    doc.Append("</DOC>");
                    retVal = new Doc(this._currentFile, doc, this._currentPosition);
                    // Process line
                    _currentPosition = fileStream.Position;
                }
                else
                {
                    retVal = null;
                    endOfFile = true;
                }
            }

            

            if (endOfFile)
            {
                _currentFile = "";
            }
            return retVal;
        }

        public bool HasNext()
        {
            return !(_files.Count == 0 && _currentFile == ""); //THIS IS SHITTY
        }
    }
}
