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

        public string ReadNextDoc()
        {
            if (_currentFile == "")
            {
                _currentFile = _files.Dequeue();
                _currentPosition = 0;
            }

            string doc = "";

            FileStream fs = new FileStream(_currentFile, FileMode.Open, FileAccess.Read);
            const Int32 BufferSize = 4096;
            using (var fileStream = File.OpenRead(_currentFile))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            {
                String line;
                while (!streamReader.EndOfStream && (line = streamReader.ReadLine()) != null && line.CompareTo("</DOC>") != 0)
                {
                    doc += line;
                }

                doc += "</DOC>";
                // Process line
                _currentPosition = fileStream.Position;
            }


            if (_currentPosition == fs.Length)
            {
                _currentFile = "";
            }

            return doc;
        }

        public bool HasNext()
        {
            return _files.Count != 0; //THIS IS SHITTY
        }
    }
}
