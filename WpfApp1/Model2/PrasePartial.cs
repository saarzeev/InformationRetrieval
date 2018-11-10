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

            List<String> counters = new List<string>();
            counters.Add("hunderd");
            counters.Add("thousand");
            counters.Add("million");
            counters.Add("billion");
            if ( splitedText[pos].Contains('$') )
            {
                //splitedText[pos].remove(splitedText[pos].IndexOf("$"), 1);
                //int i = pos;
                //string pattern = "(([0-9])+([.][0-9]+)*))";
                //if ()
                //   while ( Regex.IsMatch(splitedText[))
            }
            return splitedText;

        }
    }
}