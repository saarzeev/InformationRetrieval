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
        public static string[] ParseBetweenTerms(int pos, string[] splitedText)
        {
            if (splitedText[pos].ToLower() == "between")
            {
                splitedText[pos] = BetweenAndTerm(ref pos, splitedText);
            }
            else
            {
                splitedText[pos] = HyphenTerm(pos,splitedText);
            }
            return splitedText;
        }

        private static string HyphenTerm(int pos, string[] splitedText)
        {
            string concatHyphenTerm = "";
            string[] hyphenExpr = splitedText[pos].Split('-');
            if (hyphenExpr.Length == 2/* || //hyphen term is of length 2, or of form word-word-word.
                (hyphenExpr.Length == 3 && !
                (Regex.IsMatch(hyphenExpr[0], Resources.Resource.regex_Numbers) ||
                Regex.IsMatch(hyphenExpr[1], Resources.Resource.regex_Numbers) ||
                Regex.IsMatch(hyphenExpr[2], Resources.Resource.regex_Numbers)))*/){
                string[] substr = { splitedText[pos - 1], hyphenExpr[0], hyphenExpr[1], splitedText[pos + 1] };
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

                for (int i = 0;  i < substr.Length; i++)
                {
                    if (substr[i] == " ")
                    {
                        substr[i] = "";
                    }
                 hyphenExpr[i] = String.Join(" ", substr);
                }

                

                for (int i = 0; i < hyphenExpr.Length; i++)
                {
                    concatHyphenTerm += (i + 1 < hyphenExpr.Length) ? hyphenExpr[i] + '-' : hyphenExpr[i];
                }

            }

            return concatHyphenTerm;
        }

        private static string BetweenAndTerm(ref int pos, string[] splitedText)
        {
            string concatBetweenTerm = "";
            int origPos = pos;
            concatBetweenTerm = (splitedText[pos] == "between") ? "between " : "Between ";
            pos++;
            if (numPositions.Contains(pos))
            {
                concatBetweenTerm += splitedText[pos] + " ";
                pos++;

                while (splitedText[pos].ToLower() == " ")
                {
                    pos++;
                }

                if (splitedText[pos].ToLower() == "and")
                {
                    concatBetweenTerm += (splitedText[pos] == "and") ? "and " : "And ";
                    pos++;
                    if (numPositions.Contains(pos)) { 
                    
                        concatBetweenTerm += splitedText[pos] + " ";

                        while (pos > origPos)
                        {
                            splitedText[pos] = " ";
                            pos--;
                        }
                        splitedText[pos] = concatBetweenTerm;
                    }
                }
            }

            return concatBetweenTerm;
        }
    }
}