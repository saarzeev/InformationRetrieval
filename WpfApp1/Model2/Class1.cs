using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model2
{
    class Class1
    {
        static void Main(string[] args)
        {
            Parse parse = new Parse();
            parse.FromFilesToDocs(@"C:\Users\nastia\source\repos\saarzeev\corpus", @"C:\Users\nastia\source\repos\saarzeev\InformationRetrieval\stopwords.txt" ,true);
            parse.FromFilesToDocs(@"C:\Users\nastia\source\repos\saarzeev\corpus", @"C:\Users\nastia\source\repos\saarzeev\InformationRetrieval\stopwords.txt", false);
            string[] delimiters = { " - ", " ", "(", ")", "<", ">", "[", "]", "{", "}", "^", ";", "\"", "'", "`", "|", "*", "#", "+", "?", "!", "&", "@", "\\", "," };
            ///TODO parse & index should singleton
        }
    }
}
