
using System;
using System.Collections.Generic;
using System.Text;

namespace Model2
{
    /// <summary>
    /// Posting class
    /// </summary>
    public class Posting : IComparable
    {
        public string term;
        public int tf;
        public string docID;
        public bool is100;
        public bool isLower;
        public StringBuilder gaps;

       
        /// <summary>
        /// Posting c'tor
        /// </summary>
        /// <param name="docPath"></param>
        /// <param name="docID"></param>
        /// <param name="term"></param>
        public Posting(/*string docPath,*/ string docID, Term term)
        {
            this.term = term.GetTerm;
            this.tf = term.Tf;
            this.docID = docID;
            this.is100 = term.IsIn100;
            this.isLower = term.IsLowerCase;
            this.gaps = getGaps(term.Positons);
        }
        /// <summary>
        /// Given a string representation, constructs a Posting object
        /// Format is:
        /// term,relPath,docID,tf,is100,[gaps],isLower
        /// </summary>
        /// <param name="stringRep"></param>
        public Posting(string stringRep)
        {
            string[] strArr = stringRep.Split(',');
            this.term = strArr[0];
            if (!int.TryParse(strArr[2], out this.tf))
            {
                throw new System.ArgumentException("Parameter parsing failed.", "stringRep");
            }
            this.docID = strArr[1];
            this.is100 = (strArr[3] == "1");
            if (strArr[4][0] == '[')
            {
                StringBuilder strGaps = new StringBuilder(strArr[4].Remove(0, 1));
                int i = 4;
                while (!strArr[i].Contains("]"))
                {
                    if (i == 4)
                    {
                        i++;
                        continue;
                    }
                    strGaps.Append("," + strArr[i]);
                    i++;
                }
                if (i == 4)
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

                this.isLower = (strArr[i] == "1");
            }
            else
            {
                this.isLower = (strArr[4] == "1");
            }

        }
        /// <summary>
        /// Given a StringBuilder representation, constructs a Posting object
        /// Format is:
        /// term,relPath,docID,tf,is100,[gaps],isLower
        /// </summary>
        /// <param name="str"></param>
        public Posting(StringBuilder str) : this(str.ToString()){}

        /// <summary>
        /// Get posting's string representation.
        /// Format is:
        /// term,relPath,docID,tf,is100,[gaps],isLower
        /// </summary>
        /// <returns></returns>
        public StringBuilder GetPostingString()
        {
            StringBuilder posting = new StringBuilder(this.term + ",");
            posting.Append(this.docID + ",");
            posting.Append(this.tf + ",");
            string is100 = this.is100 ? "1" : "0";
            posting.Append(is100 + ",");
            posting.Append("[");
            posting.Append(this.gaps);
            posting.Append("],");
            string isLower = this.isLower ? "1" : "0";
            posting.Append(isLower);
            return posting;
        }
        public StringBuilder GetPostingStringFinal()
        {
            StringBuilder posting = new StringBuilder(this.term + ",");
            posting.Append(this.docID + ",");
            posting.Append(this.tf + ",");
            string is100 = this.is100 ? "1" : "0";
            posting.Append(is100 + ",");
            string isLower = this.isLower ? "1" : "0";
            posting.Append(isLower);
            return posting;
        }

        public StringBuilder getGaps(HashSet<int> positionsHash)
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

        public int CompareTo(object obj)
        {
          return this.tf.CompareTo(((Posting)obj).tf);
        }
    }
}
