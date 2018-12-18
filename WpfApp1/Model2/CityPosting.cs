using System;
using System.Collections.Generic;
using System.Text;

namespace Model2
{
    public class CityPosting 
    {
       
        public string docID;
        public StringBuilder gaps;
        

        //_city + "," + _country + "," +_currency + "," + _population + "," + relPath+ "," + docID + "," + [gaps]

        public CityPosting(string stringRep) //stringRep =relPath + docID + [gaps]
        {
            //city = city;
            string[] strArr = stringRep.Split(',');

            this.docID = strArr[0];

            StringBuilder strGaps = new StringBuilder(strArr[1].Remove(0, 1));
            int i = 1;
            while (!strArr[i].Contains("]"))
            {
                if (i == 1)
                {
                    i++;
                    continue;
                }
                strGaps.Append("," + strArr[i]);
                i++;
            }
            if (i == 1)
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

    }
}
