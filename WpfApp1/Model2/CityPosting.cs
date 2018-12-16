using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Model2
{
    public class CityPosting /*: IComparable*/
    {
        /*public string city;
        public string country;
        public string currency;
        public string population;*/
        //public City city;
        public string path { get; set; }
        public string docID { get; set; }
        public HashSet<int> gaps { get; set; }
        //public StringBuilder gaps;

        [JsonConstructor]
        public CityPosting(string docRelativePath, string docID, HashSet<int> gaps)
        {
            this.path = docRelativePath;
            this.docID = docID;
            this.gaps = gaps;
        }

        public CityPosting(string stringRep/*, City city*/) //stringRep =relPath + docID + [gaps]
        {
            //city = city;
            string[] strArr = stringRep.Split(',');

            this.path = strArr[0];
            //double docId = Parse.QuickDoubleParse(strArr[4]);
            //if (!int.TryParse(strArr[2], out this.docID) || !int.TryParse(strArr[3], out this.tf))
            //if (docId == Double.NaN)
            //{
            //    throw new System.ArgumentException("Parameter parsing failed.", "stringRep");
            //}
            this.docID = strArr[1];

            StringBuilder strGaps = new StringBuilder(strArr[2].Remove(0, 1));
            int i = 2;
            while (!strArr[i].Contains("]"))
            {
                if (i == 2)
                {
                    i++;
                    continue;
                }
                strGaps.Append("," + strArr[i]);
                i++;
            }
            if (i == 2)
            {
                char[] trim = new char[] { '[', ']' };
                strGaps = new StringBuilder(strArr[i].Trim(trim));
            }
            else
            {
                strGaps.Append("," + strArr[i].Remove(strArr[i].IndexOf(']')));
            }

            i++;

           // this.gaps = strGaps;
        }

        public CityPosting(StringBuilder str/*, City city*/):this(str.ToString()/*, city*/)
        {
        }

        public CityPosting(string docPath, string docID, Term term)
        {
            //this.city = term.GetTerm;
            /*if (City.citiesInfo.ContainsKey(term.GetTerm.ToUpper()))
            {
                this.city = City.citiesInfo[term.GetTerm.ToUpper()];
            }
            else
            {
                this.city = new City(term.GetTerm.ToUpper());
            }*/
            this.path = docPath;
            this.docID = docID;
            this.gaps = term.Positons; /*getGaps(term.Positons);*/
        }

        private StringBuilder getGaps(HashSet<int> positionsHash)
        {
            int[] positions = new int[positionsHash.Count];
            positionsHash.CopyTo(positions, 0);

            StringBuilder gaps = new StringBuilder();
            gaps.Append(positions[0]);
            for (int i = 1; i < positions.Length; i++)
            {
                int gap = (positions[i] - positions[i - 1]);
                gaps.Append("," + gap);
            }
            return gaps;
        }

        public StringBuilder ToStringBuilder()
        {
            StringBuilder posting = new StringBuilder();
            /*posting.Append(this.city.GetCountry + ",");
            posting.Append(this.city.GetCurrency + ",");
            posting.Append(this.city.GetPop + ",");*/
            posting.Append(this.path + ",");
            posting.Append(this.docID + ",");
            posting.Append("[");
            posting.Append(this.gaps);
            posting.Append("]");

            return posting;
        }

        /*public int CompareTo(object obj)
        {
            return this.city.CompareTo(((CityPosting)obj).city.GetCity);
        }*/
    }
}
