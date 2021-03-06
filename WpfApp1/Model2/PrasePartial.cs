
using System.Collections.Generic;
using System.Linq;


namespace Model2
{

    public partial class Parse
    {
        private enum Side
        {
            Left,
            Right
        }

        /// <summary>
        /// Parses "Between" phrases and Hyphen terms
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="splitedText"></param>
        /// <param name="numPositions"></param>
        /// <param name="addedTerms"></param>
        /// <returns></returns>
        public string[] ParseBetweenTerms(int pos, string[] splitedText, HashSet<int> numPositions, Dictionary<string, List<int>> addedTerms)
        {
            if (splitedText[pos].ToLower() == "between")
            {
                splitedText[pos] = BetweenAndTerm(ref pos, splitedText, numPositions, addedTerms);
            }
            else
            {
                if (splitedText[pos].ElementAt(0) == '-')
                {
                    splitedText[pos] = splitedText[pos].Remove(0, 1);
                }
                else if (splitedText[pos].ElementAt(splitedText[pos].Length - 1) == '-')
                {
                    splitedText[pos] = splitedText[pos].Remove(splitedText[pos].Length - 1, 1);
                }
                else
                {
                    splitedText[pos] = HyphenTerm(pos, splitedText, numPositions, addedTerms);
                }

            }
            return splitedText;
        }

        private string HyphenTerm(int pos, string[] splitedText, HashSet<int> numPositions, Dictionary<string, List<int>> addedTerms)
        {
            string concatHyphenTerm = "";
            string[] hyphenExpr = splitedText[pos].Split('-');
            if (hyphenExpr.Length == 2)
            { //Hyphen-terms with numbers should be number-parsed
                


                string[] leftSubstr;
                bool leftSubstrPossibleChange = true;
                if (pos - 1 >= 0)
                {
                    leftSubstr = new string[] { splitedText[pos - 1], hyphenExpr[0] };
                }
                else
                {
                    leftSubstr = new string[] { " ", hyphenExpr[0] };
                    leftSubstrPossibleChange = false;
                }

                string[] rightSubstr;
                bool rightSubstrPossibleChange = true;
                if (pos + 1 < splitedText.Length)
                {
                    rightSubstr = new string[] { hyphenExpr[1], splitedText[pos + 1] };
                }
                else
                {
                    rightSubstr = new string[] { hyphenExpr[1], " " };
                    rightSubstrPossibleChange = false;
                }

                hyphenExpr[0] = TreatHyphenTermsNumbers(pos, splitedText, leftSubstr, Side.Left, leftSubstrPossibleChange, numPositions);
                /*if(!(addedTerms.ContainsKey(hyphenExpr[0])))
                {
                    addedTerms[hyphenExpr[0]] = new List<int>();
                }
                addedTerms[hyphenExpr[0]].Add(pos);*/
                hyphenExpr[1] = TreatHyphenTermsNumbers(pos, splitedText, rightSubstr, Side.Right, rightSubstrPossibleChange, numPositions);
                /*if (!(addedTerms.ContainsKey(hyphenExpr[1])))
                {
                    addedTerms[hyphenExpr[1]] = new List<int>();
                }
                addedTerms[hyphenExpr[1]].Add(pos);*/

                for (int i = 0; i < hyphenExpr.Length; i++)
                {
                    concatHyphenTerm += (i + 1 < hyphenExpr.Length) ? hyphenExpr[i] + '-' : hyphenExpr[i];
                }

            }

            return concatHyphenTerm;
        }

        private string TreatHyphenTermsNumbers(int pos, string[] splitedText, string[] substr, Side side, bool possibleChange, HashSet<int> numPositions)
        {
            Queue<int> specificBigNums = new Queue<int>();
            Queue<int> bigNums = new Queue<int>();

            PopulateQueueWithPositions(substr, null, null, null, specificBigNums, bigNums, null);

            while (specificBigNums.Count != 0)
            {
                substr = ParseSpecificNumbers(specificBigNums.Dequeue(), substr, numPositions);
            }

            while (bigNums.Count != 0)
            {
                substr = ParseNumbers(bigNums.Dequeue(), substr, numPositions);
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
                if (substr[1] == " " && possibleChange)
                {
                    splitedText[pos + 1] = " ";
                }
                return substr[0];
            }
        }
        private string BetweenAndTerm(ref int pos, string[] splitedText, HashSet<int> numPositions, Dictionary<string, List<int>> addedTerms)
        {
            string concatBetweenTerm = "";
            int origPos = pos;
            string origBetween = splitedText[pos];
            bool commitChanges = false;

            concatBetweenTerm = (splitedText[pos] == "between") ? "between " : "Between ";
            pos++;
            if (pos < splitedText.Length && numPositions.Contains(pos))
            {
                concatBetweenTerm += splitedText[pos] + " ";
                pos++;

                while (pos < splitedText.Length && splitedText[pos].ToLower() == " ")
                {
                    pos++;
                }

                if (pos < splitedText.Length && splitedText[pos].ToLower() == "and")
                {
                    concatBetweenTerm += (splitedText[pos] == "and") ? "and " : "And ";
                    pos++;
                    if (pos < splitedText.Length && numPositions.Contains(pos))
                    {

                        concatBetweenTerm += splitedText[pos] + " ";
                        commitChanges = true;
                        while (pos > origPos)
                        {
                            /*if(splitedText[pos].ToLower() != "between" && splitedText[pos].ToLower() != "and" )
                            {
                                if (!addedTerms.ContainsKey(splitedText[pos]))
                                {
                                    addedTerms[splitedText[pos]] = new List<int>();
                                }
                                addedTerms[splitedText[pos]].Add(pos);
                            }*/
                            splitedText[pos] = " ";
                            pos--;
                        }
                        splitedText[pos] = concatBetweenTerm;
                    }
                }
            }

            return commitChanges ? concatBetweenTerm : origBetween;
        }

    }


}