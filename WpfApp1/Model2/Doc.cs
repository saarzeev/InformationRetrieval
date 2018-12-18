using System;
using System.Collections.Generic;
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
        //public SortedDictionary<int, Term> entities = new SortedDictionary<int, Term>(new DescendingComparer<int>());
        public List<KeyValuePair<int, SimpleTerm>> entities = new List<KeyValuePair<int, SimpleTerm>>();
        //public string countery;
        //public string language { get; set; }

        /// <summary>
        /// C'tor
        /// </summary>
        /// <param name="path"></param>
        /// <param name="text"></param>
        /// <param name="index"></param>
        /// <param name="city"></param>
        /// <param name="countery"></param>
        /// <param name="language"></param>
        public Doc(string path, StringBuilder text, string index, string city, string countery = "", string language = "")
        {
            this.max_tf = 0;
            this.uniqWords = 0;
            this.city = city;
            this._path = path;
            this._text = text;
            this._indexInFile = index;
            //this.countery = countery;
            //this.language = language;
            this.length = 0;
        }
        
        /// <summary>
        /// Returns a StringBuilder representation of the Doc
        /// </summary>
        /// <returns></returns>
        public StringBuilder ToStringBuilder() {
            StringBuilder doc = new StringBuilder();
            doc.AppendFormat("{0},{1},{2},{3},{4},{5}", this._path, this._indexInFile, this.max_tf, this.uniqWords, this.length, this.city/*,this.language,this.countery*/);

            for (int i = 0; i < entities.Count; i++)
            {
                doc.Append("," + entities[i].Value.GetTerm);
            }

            return doc;
        }

        /// <summary>
        /// Updates the Entities dictionary to the Top-5 (Calculated by tf in doc) entities
        /// </summary>
        public void UpdateEntities()
        {
            //Remove terms which are not entities
            entities.RemoveAll(item => item.Value.IsLowerCase);

            //Sort by Tf descending order
            entities.Sort((emp1, emp2) => emp2.Key.CompareTo(emp1.Key));

            //take only top 5
            entities = new List<KeyValuePair<int, SimpleTerm>>(entities.GetRange(0, entities.Count > 5 ? 5 : entities.Count));
                
        }

    }
}
