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
            //FileReader fr = new FileReader("C:\\Users\\nastia\\source\\repos\\saarzeev\\corpus");
            //_semaphore.Release(11);
            //while (fr.HasNext()) { }
            //Console.WriteLine( fr.ReadNextDoc());
            //////string time = "93PM";
            //bool isss = System.DateTime.TryParse(time,out var ggg);
            //time = ggg.GetDateTimeFormats()[108];

            //string s = ParsePresents("<<90 percent , bfdgfdgsj 1555" +
            //    "ddsfssfsfsfs   90             percent----");

            //string t = ParseRange("Between number and number (for example: between 18 and 24)");
            Parse parse = new Parse();
            parse.FromFilesToDocs(@"C:\Users\nastia\source\repos\saarzeev\corpus", @"C:\Users\nastia\source\repos\saarzeev\InformationRetrieval\stopwords.txt" ,true);
            // FromFilesToDocs(@"C:\miniMiniCorpus", false);
            //string k = " 44444/24545";
            //double.TryParse(k, out double num);
            //Console.WriteLine(num);
            //string t = "1/2";
            //string n = "11.55/111";
            //string sa = "k/1";
            //string ss = "11/12/22";
            //string ffff = "raising the fat content of imported meat from 2O percent to 35%; in other words for every 1000 kgs of meat the fat content is 350 kgs.";
            //string ddddd = ParsePresents(ffff);
            //Console.WriteLine(Regex.IsMatch(k, Resources.Resource.regex_Fraction));
            //Console.WriteLine(Regex.IsMatch(t, Resources.Resource.regex_Fraction));
            //Console.WriteLine(Regex.IsMatch(n, Resources.Resource.regex_Fraction));
            //Console.WriteLine(Regex.IsMatch(sa, Resources.Resource.regex_Fraction));
            //Console.WriteLine(Regex.IsMatch(ss, Resources.Resource.regex_Fraction));*/
            //Parser(new Doc("ngnngngg", new StringBuilder(Resources.Resource.openText + " " + " 33/44z Dollars 10 3/4 dollars  1.7320 Dollars  22 3/4 Dollars $450,000 1,000,000 Dollars $450,000,000 54/88 million dollars 20.6 m Dollars $100 billion 100 bn Dollars 5 100/555 billion U.S. dollars 320 million U.S. dollars 1 trillion U.S. dollars" + " " + Resources.Resource.closeText), 0));
        }
    }
}
