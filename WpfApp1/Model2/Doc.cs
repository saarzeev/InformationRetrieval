using System;
using System.Collections.Generic;
using System.Text;

namespace Model2
{
    public class Doc
    {
        public string _path { get; }
        public StringBuilder _text;
        public string _docID;
        public int max_tf;
        public string city;
        public int uniqWords;
        public int length;
        //public SortedDictionary<int, Term> entities = new SortedDictionary<int, Term>(new DescendingComparer<int>());
        public List<KeyValuePair<int, SimpleTerm>> entities = new List<KeyValuePair<int, SimpleTerm>>();
        //public string countery;
        public string language { get; set; }

        /// <summary>
        /// C'tor
        /// </summary>
        /// <param name="path"></param>
        /// <param name="text"></param>
        /// <param name="docID"></param>
        /// <param name="city"></param>
        /// <param name="countery"></param>
        /// <param name="language"></param>
        public Doc(/*string path,*/ StringBuilder text, string docID, string city, /*string countery = "",*/ string language = "")
        {
            this.max_tf = 0;
            this.uniqWords = 0;
            this.city = city;
            this._text = text;
            this._docID = docID;
            this.length = 0;
            this.language = language;
        }
        
        /// <summary>
        /// C'tor - Construct a Doc, given a string. Needs a similar formating as the one in ToStringBuilder().
        /// </summary>
        /// <param name="strRep"></param>
        public Doc(string strRep)
        {
            string[] splitted = strRep.Split(',');

            this._docID = splitted[0];
            Int32.TryParse(splitted[1], out this.max_tf);
            Int32.TryParse(splitted[2], out this.uniqWords);
            Int32.TryParse(splitted[3], out this.length);
            this.city = splitted[4];
            this.language = splitted[5];

            entities = new List<KeyValuePair<int, SimpleTerm>>();
            int i = 6;
            while(i < splitted.Length)
            {
                int tf;
                Int32.TryParse(splitted[i++], out tf);
                string[] simpleTerm = new string[] { splitted[i++], // term,
                                                     splitted[i++], // Df
                                                     splitted[i++], // max Tf
                                                     splitted[i++], // Position
                                                     splitted[i++], }; //isLower
                entities.Add(new KeyValuePair<int, SimpleTerm>(tf, new SimpleTerm(String.Join(",", simpleTerm))));
            }

        }
        /// <summary>
        /// Returns a StringBuilder representation of the Doc
        /// </summary>
        /// <returns></returns>
        public StringBuilder ToStringBuilder() {
            StringBuilder doc = new StringBuilder();
            doc.AppendFormat("{0},{1},{2},{3},{4},{5}", this._docID, this.max_tf, this.uniqWords, this.length, this.city, this.language);

            for (int i = 0; i < entities.Count; i++)
            {
                doc.Append("," + entities[i].Key + "," + entities[i].Value.ToString());
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
