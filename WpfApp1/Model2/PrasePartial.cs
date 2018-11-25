using SearchEngine;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Model2
{

    public partial class Parse
    {
        private enum Side
        {
            Left,
            Right
        }

        public  string[] ParseBetweenTerms(int pos, string[] splitedText)
        {
            if (splitedText[pos].ToLower() == "between")
            {
                splitedText[pos] = BetweenAndTerm(ref pos, splitedText);
            }
            else
            {
               // splitedText[pos] = HyphenTerm(pos,splitedText);
            }
            return splitedText;
        }

        private  string HyphenTerm(int pos, string[] splitedText)
        {
            string concatHyphenTerm = "";
            string[] hyphenExpr = splitedText[pos].Split('-');
            if (hyphenExpr.Length == 2)
            { //Hyphen-terms with numbers should be number-parsed
                string[] leftSubstr = { splitedText[pos - 1], hyphenExpr[0] };
                // OUT OF RANGE TO-Do!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                string[] rightSubstr = { hyphenExpr[1], splitedText[pos + 1] };

                hyphenExpr[0] = TreatHyphenTermsNumbers(pos, splitedText, leftSubstr, Side.Left);
                hyphenExpr[1] = TreatHyphenTermsNumbers(pos, splitedText, rightSubstr, Side.Right);

                for (int i = 0; i < hyphenExpr.Length; i++)
                {
                    concatHyphenTerm += (i + 1 < hyphenExpr.Length) ? hyphenExpr[i] + '-' : hyphenExpr[i];
                }

            }

            return concatHyphenTerm;
        }

        private  string TreatHyphenTermsNumbers(int pos, string[] splitedText, string[] substr, Side side)
        {
            Queue<int> specificBigNums = new Queue<int>();
            Queue<int> bigNums = new Queue<int>();

            PopulateQueueWithPositions(substr, null, null, null, specificBigNums, bigNums, null);

            while (specificBigNums.Count != 0)
            {
                substr = ParseSpecificNumbers(specificBigNums.Dequeue(), substr);
            }

            while (bigNums.Count != 0)
            {
                substr = ParseNumbers(bigNums.Dequeue(), substr);
            }


            if (side == Side.Left)
            {
                if (substr[1] == " ")
                {
                    substr[1] = substr[0];
                    splitedText[pos - 1] = " ";
                }
                return substr[1];
            }
            else //side == Side.Right
            {
                if (substr[1] == " ")
                {
                    splitedText[pos + 1] = " ";
                }
                return substr[0];
            }
        }

        private  string BetweenAndTerm(ref int pos, string[] splitedText)
        {
            string concatBetweenTerm = "";
            int origPos = pos;
            concatBetweenTerm = (splitedText[pos] == "between") ? "between " : "Between ";
            pos++;
            //if (numPositions.Contains(pos))
            //{
            //    concatBetweenTerm += splitedText[pos] + " ";
            //    pos++;

            //    while (splitedText[pos].ToLower() == " ")
            //    {
            //        pos++;
            //    }

            //    if (splitedText[pos].ToLower() == "and")
            //    {
            //        concatBetweenTerm += (splitedText[pos] == "and") ? "and " : "And ";
            //        pos++;
            //        //if (numPositions.Contains(pos)) { 
                    
            //        //    concatBetweenTerm += splitedText[pos] + " ";

            //        //    while (pos > origPos)
            //        //    {
            //        //        splitedText[pos] = " ";
            //        //        pos--;
            //        //    }
            //        //    splitedText[pos] = concatBetweenTerm;
            //        //}
            //    }
            //}

            return concatBetweenTerm;
        }

        private  void AddTermsToVocabulry(string[] splitedText, bool shouldStem)
        {
            StemmerInterface stm = new Stemmer();
            foreach (string term in splitedText)
            { 
                _vocabulary.Add(shouldStem && term != " " ? stm.stemTerm(term) : term);
            }
        }
    }

    
}