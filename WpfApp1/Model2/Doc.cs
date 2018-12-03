using System.Text;

namespace Model2
{
    public class Doc
    {
        public string _path { get; }
        public StringBuilder _text;
        public string _indexInFile;
        public int max_tf;
        public string city;
        public int uniqWords;
        public int length;
        public string country;
        public string language;

        public Doc(string path, StringBuilder text, string index, string city, string countery, string lanuage)
        {
            this.max_tf = 0;
            this.uniqWords = 0;
            this.city = city;
            this._path = path;
            this._text = text;
            this._indexInFile = index;
            this.country = country;
            this.language = language;
            this.length = 0;
        }
        
        public StringBuilder ToStringBuilder() {
            StringBuilder doc = new StringBuilder();
            doc.AppendFormat("{0},{1},{2},{3},{4},{5}", this._path, this._indexInFile, this.max_tf, this.uniqWords, this.length, this.city);
            return doc;
        }

    }
}
