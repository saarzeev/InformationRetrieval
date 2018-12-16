using System.Text;

namespace Model2
{
    public class Doc
    {
        public string _path { get; set; }
        public StringBuilder _text { get; set; }
        public string _indexInFile { get; set; }
        public int max_tf { get; set; }
        public string city { get; set; }
        public int uniqWords { get; set; }
        public int length { get; set; }
        public string countery { get; set; }
        public string language { get; set; }

        public Doc(string path, StringBuilder text, string index, string city, string countery, string language)
        {
            this.max_tf = 0;
            this.uniqWords = 0;
            this.city = city;
            this._path = path;
            this._text = text;
            this._indexInFile = index;
            this.countery = countery;
            this.language = language;
            this.length = 0;
        }
        
        public StringBuilder ToStringBuilder() {
            StringBuilder doc = new StringBuilder();
            doc.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7}", this._path, this._indexInFile, this.max_tf, this.uniqWords, this.length, this.city,this.language,this.countery);
            return doc;
        }

    }
}
