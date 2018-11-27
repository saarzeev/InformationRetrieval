
using System;
using System.Collections.Generic;
using System.Text;

namespace Model2
{
    public class Posting : IComparable
    {
        public string term;
        public int tf;
        public string docRelativePath;
        public int docID;
        public bool is100;
        public bool isLower;
        public StringBuilder gaps;

       

        public Posting(string docPath, long docID, Term term)
        {
            this.term = term.GetTerm;
            this.tf = term.Tf;
            this.docRelativePath = docPath;
            this.docID = (int)docID;
            this.is100 = term.IsIn100;
            this.isLower = term.IsLowerCase;
            this.gaps = getGaps(term.Positons);
        }

        public StringBuilder getPostingString()
        {
            StringBuilder posting = new StringBuilder(this.term + ",");
            posting.Append(this.docRelativePath + ",");
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
            //byte[] comma = Encode(',');
            //byte[] termString = Encoding.UTF8.GetBytes(term.GetTerm);
            //byte[] df = Encode(1);
            //byte[] docFullPathString = Encoding.UTF8.GetBytes(docFullPath);
            //byte[] dicIDInt = Encode((int)dicID);
            //byte[] is100 = Encode(term.IsIn100 ? 1 : 0);
            //byte[] leftpar = Encode('[');
            //byte[] tfByte = Encode(term.Tf);
            //byte[] rigthtpar = Encode(']');
            //byte[][] gaps = getGaps(term.Positons ,new byte[term.Positons.Count][]);
            //byte[] isLower = Encode(term.IsLowerCase ? 1 : 0);

            //int size = termString.Length + df.Length + docFullPathString.Length + dicIDInt.Length + is100.Length + leftpar.Length + tfByte.Length + rigthtpar.Length + isLower.Length + (7 * comma.Length);
            //foreach(byte[] gap in gaps)
            //{
            //    size += gap.Length;
            //}
            //size += comma.Length * (gaps.Length - 1);

            //byte[] posting = new byte[size];
            ////term,df,path,id,tf,is100,[gaps+comas],islower
            //int  i = 0;
            //for (int j = 0; j < termString.Length; i++, j++)
            //{
            //    posting[i] = termString[j];
            //}

            //for(int j = 0; j < comma.Length; i++, j++)
            //{
            //    posting[i] = comma[j];
            //}

            //for (int j = 0; j < df.Length; i++, j++)
            //{
            //    posting[i] = df[j];
            //}

            //for (int j = 0; j < comma.Length; i++, j++)
            //{
            //    posting[i] = comma[j];
            //}

            //for (int j = 0; j < docFullPathString.Length; i++, j++)
            //{
            //    posting[i] = docFullPathString[j];
            //}

            //for (int j = 0; j < comma.Length; i++, j++)
            //{
            //    posting[i] = comma[j];
            //}

            //for (int j = 0; j < dicIDInt.Length; i++, j++)
            //{
            //    posting[i] = dicIDInt[j];
            //}

            //for (int j = 0; j < comma.Length; i++, j++)
            //{
            //    posting[i] = comma[j];
            //}


            //for (int j = 0; j < tfByte.Length; i++, j++)
            //{
            //    posting[i] = tfByte[j];
            //}

            //for (int j = 0; j < comma.Length; i++, j++)
            //{
            //    posting[i] = comma[j];
            //}

            //for (int j = 0; j < is100.Length; i++, j++)
            //{
            //    posting[i] = is100[j];
            //}

            //for (int j = 0; j < comma.Length; i++, j++)
            //{
            //    posting[i] = comma[j];
            //}

            //for (int j = 0; j < leftpar.Length; i++, j++)
            //{
            //    posting[i] = leftpar[j];
            //}

            //for (int gap = 0; gap < gaps.Length; gap++)
            //{
            //    for (int j = 0; j < gaps[gap].Length; i++, j++)
            //    {
            //        posting[i] = gaps[gap][j];
            //    }

            //    if (gap + 1 < gaps.Length)
            //    {
            //        for (int j = 0; j < comma.Length; i++, j++)
            //        {
            //            posting[i] = comma[j];
            //        }
            //    }

            //}

            //for (int j = 0; j < rigthtpar.Length; i++, j++)
            //{
            //    posting[i] = rigthtpar[j];
            //}

            //for (int j = 0; j < comma.Length; i++, j++)
            //{
            //    posting[i] = comma[j];
            //}

            //for (int j = 0; j < isLower.Length; i++, j++)
            //{
            //    posting[i] = isLower[j];
            //}

            //return posting;

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
