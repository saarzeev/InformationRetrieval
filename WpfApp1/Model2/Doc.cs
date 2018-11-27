using System.Text;

namespace Model2
{
    class Doc
    {
        public string _path { get; }
        public StringBuilder _text { get; }
        public long _indexInFile { get; }
        public int max_tf;
        public string city;
        public int uniqWords;
        public int length;
        
    public Doc(string path, StringBuilder text, long indexInFile)
        {
            this.max_tf = 0;
            this.uniqWords = 0;
            this.city = "";
            this._path = path;
            this._text = text;
            this.length = 0;
            this._indexInFile = indexInFile; 
        }
    }
}
