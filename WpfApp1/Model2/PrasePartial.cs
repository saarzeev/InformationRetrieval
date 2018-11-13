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
        public static string[] ParseMoney ( int pos, string[] splitedText )
        {
            splitedText = DollarSignToCanonicalForm(ref pos, splitedText);
            string[] splitedMoneyExpr = splitedText[pos].Split(' ');
            double sum = 0;
            string inPos = splitedText[pos];
            if ( double.TryParse(splitedMoneyExpr[0], out sum) )
            {
                if ( splitedMoneyExpr[1] == Resources.Resource.hundred )
                {
                    sum = sum * 100;
                }
                else if ( splitedMoneyExpr[1] == Resources.Resource.thousand )
                {
                    sum = sum * 1000;
                }
                else if ( splitedMoneyExpr[1] == Resources.Resource.million )
                {
                    sum = sum * 1000000;
                }
                else if ( splitedMoneyExpr[1] == Resources.Resource.billion )
                {
                    sum = sum * 1000000000;
                }
                else if ( splitedMoneyExpr[1] == Resources.Resource.trillion )
                {
                    sum = sum * 1000000000000;
                }

                if ( sum >= 1000000 )
                {
                    sum = sum / 1000000;
                    splitedText[pos] = sum.ToString() + " M " + Resources.Resource.dollars;
                }
                else
                {
                    splitedText[pos] = sum.ToString() + " " + Resources.Resource.dollars;
                }
                inPos = splitedText[pos];
            }
           
            
            return splitedText;

        }

        private static string[] DollarSignToCanonicalForm ( ref int pos, string[] splitedText ) //Canonical form == NUMBER (counter) Dollars
        {
            List<String> counters = new List<string>();
            counters.Add(Resources.Resource.hundred);
            counters.Add(Resources.Resource.thousand);
            counters.Add(Resources.Resource.million);
            counters.Add(Resources.Resource.billion);
            counters.Add(Resources.Resource.trillion);

            if ( splitedText[pos].Contains('$') )
            {
                splitedText[pos] = splitedText[pos].Remove(splitedText[pos].IndexOf("$"), 1);
                pos++; //skip the figure after the $ sign

                if ( pos < splitedText.Length && counters.Contains(splitedText[pos].ToLower()) )
                { //this limits counters to 1.
                    splitedText[pos - 1] = splitedText[pos - 1] + " " + splitedText[pos];
                    splitedText[pos] = " ";
                }

                splitedText[pos - 1] = splitedText[pos - 1] + " "+ Resources.Resource.dollars;

                pos--;
            }
            else if ( pos - 1 >= 0 )
            {
                if ( splitedText[pos - 1].ToLower() == "u.s." )
                {
                    splitedText[pos] = " "; //Remove Dollar
                    splitedText[pos - 1] = " "; //Remove U.S.
                    if ( pos - 3 >= 0 && counters.Contains(splitedText[pos - 2]) )
                    { //has a counter, it is of form NUM billion U.S. Dollars
                        splitedText[pos - 3] = splitedText[pos - 3] + " " + splitedText[pos - 2] + " " + " " + Resources.Resource.dollars;
                        splitedText[pos - 2] = " "; //Remove Counter

                        pos = pos - 3;
                    }
                    else if ( pos - 2 >= 0 ) //Does't have a counter. It is of form NUM U.S. Dollars
                    {
                        splitedText[pos - 2] = splitedText[pos - 2] + " " + Resources.Resource.dollars;
                        pos = pos - 2;
                    }
                }
                else if ( pos - 2 >= 0 && splitedText[pos - 1].ToLower() == "m" )
                {
                    splitedText[pos] = " ";
                    splitedText[pos - 1] = " ";
                    splitedText[pos - 2] = splitedText[pos - 2] + " " + " " + Resources.Resource.million + " " + Resources.Resource.dollars;
                    pos = pos - 2;
                }
                else if ( pos - 2 >= 0 && splitedText[pos - 1].ToLower() == "bn" )
                {
                    splitedText[pos] = " ";
                    splitedText[pos - 1] = " ";
                    splitedText[pos - 2] = splitedText[pos - 2] + " " + Resources.Resource.billion + " " + Resources.Resource.dollars;
                    pos = pos - 2;
                }
                else if ( pos - 2 >= 0 && ( splitedText[pos - 1].ToLower() == Resources.Resource.billion || 
                    splitedText[pos - 1].ToLower() == Resources.Resource.trillion || splitedText[pos - 1].ToLower() == Resources.Resource.million) )
                {
                    splitedText[pos - 2] = splitedText[pos - 2] + " " + splitedText[pos - 1] + " " + Resources.Resource.dollars;
                    splitedText[pos] = " ";
                    splitedText[pos - 1] = " ";
                    pos = pos - 2;
                }
                else if ( pos - 1 >= 0 && Regex.IsMatch(splitedText[pos - 1], " " + Resources.Resource.regexNumbers) )
                {
                    splitedText[pos - 1] = splitedText[pos - 1] + " " + Resources.Resource.dollars;
                    splitedText[pos] = " ";
                }


            }

            return splitedText;
        }
    }
}