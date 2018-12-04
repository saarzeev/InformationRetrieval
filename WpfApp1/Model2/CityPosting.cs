using System;
using System.Collections.Generic;
using System.Text;

namespace Model2
{
    public class CityPosting : IComparable
    {
        public string city;
        public string country;
        public string currency;
        public string population;
        public string docRelativePath;
        public string docID;
        public StringBuilder gaps;
        

        //_city + "," + _country + "," +_currency + "," + _population + "," + relPath+ "," + docID + "," + [gaps]

        public CityPosting(string stringRep)
        {
            string[] strArr = stringRep.Split(',');
            this.city = strArr[0];
            this.country = strArr[1];
            this.currency = strArr[2];
            this.population = strArr[3];
            this.docRelativePath = strArr[4];
            //double docId = Parse.QuickDoubleParse(strArr[4]);
            //if (!int.TryParse(strArr[2], out this.docID) || !int.TryParse(strArr[3], out this.tf))
            //if (docId == Double.NaN)
            //{
            //    throw new System.ArgumentException("Parameter parsing failed.", "stringRep");
            //}
            this.docID = strArr[2];

            StringBuilder strGaps = new StringBuilder(strArr[6].Remove(0, 1));
            int i = 6;
            while (!strArr[i].Contains("]"))
            {
                if (i == 6)
                {
                    i++;
                    continue;
                }
                strGaps.Append("," + strArr[i]);
                i++;
            }
            if (i == 6)
            {
                char[] trim = new char[] { '[', ']' };
                strGaps = new StringBuilder(strArr[i].Trim(trim));
            }
            else
            {
                strGaps.Append("," + strArr[i].Remove(strArr[i].IndexOf(']')));
            }

            i++;

            this.gaps = strGaps;
        }

        public CityPosting(StringBuilder str):this(str.ToString())
        {
        }

        public CityPosting(string docPath, string docID, Term term)
        {
            this.city = term.GetTerm;
            this.docRelativePath = docPath;
            this.docID = docID;
            this.gaps = getGaps(term.Positons);
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
            posting.Append(this.country + ",");
            posting.Append(this.currency + ",");
            posting.Append(this.population + ",");
            posting.Append(this.docRelativePath + ",");
            posting.Append(this.docID + ",");
            posting.Append("[");
            posting.Append(this.gaps);
            posting.Append("]");

            return posting;
        }

        public int CompareTo(object obj)
        {
            return this.city.CompareTo(((CityPosting)obj).city);
        }
    }
}
