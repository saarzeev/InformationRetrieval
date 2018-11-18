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

        static private ConcurrentQueue<Doc> _docs = new ConcurrentQueue<Doc>();
        static private SemaphoreSlim _semaphore1 = new SemaphoreSlim(0, 10);
        static private SemaphoreSlim _semaphore2 = new SemaphoreSlim(10, 10);
        static private Mutex mutex = new Mutex();
        static private bool done = false;
        static private HashSet<int> numPositions = new HashSet<int>();

        static void Main(string[] args)
        {
            //FileReader fr = new FileReader("C:\\Users\\nastia\\source\\repos\\saarzeev\\corpus");
            //_semaphore.Release(11);
            //while (fr.HasNext()) { }
            //Console.WriteLine( fr.ReadNextDoc());
            string s = ParsePresents("<<90 percent , bfdgfdgsj 1555" +
                "ddsfssfsfsfs   90             percent----");
            //string t = ParseRange("Between number and number (for example: between 18 and 24)");
          FromFilesToDocs(@"C:\Users\nastia\Source\Repos\saarzeev\corpus");
            string k = " 44444/24545";
            double.TryParse(k, out double num);
            Console.WriteLine(num);
            string t = "1/2";
            string n = "11.55/111";
            string sa = "k/1";
            string ss = "11/12/22";
            string ffff = "raising the fat content of imported meat from 2O percent to 35%; in other words for every 1000 kgs of meat the fat content is 350 kgs.";
            string ddddd = ParsePresents(ffff);
            Console.WriteLine(Regex.IsMatch(k, Resources.Resource.regex_Fraction));
            Console.WriteLine(Regex.IsMatch(t, Resources.Resource.regex_Fraction));
            Console.WriteLine(Regex.IsMatch(n, Resources.Resource.regex_Fraction));
            Console.WriteLine(Regex.IsMatch(sa, Resources.Resource.regex_Fraction));
            Console.WriteLine(Regex.IsMatch(ss, Resources.Resource.regex_Fraction));
            Parser(new Doc("ngnngngg", new StringBuilder(Resources.Resource.openText+ " " + "3/4z Dollars 10 3/4 dollars  1.7320 Dollars  22 3/4 Dollars $450,000 1,000,000 Dollars $450,000,000 54/88 million dollars 20.6 m Dollars $100 billion 100 bn Dollars 5 100/555 billion U.S. dollars 320 million U.S. dollars 1 trillion U.S. dollars" + " " + Resources.Resource.closeText), 0));
        }

          public static void FromFilesToDocs(string path)
        {
            Task task;

            FileReader fr = new FileReader(path);
            task = Task.Run(() =>
            {
                while (fr.HasNext())
                {
                   
                    List<Doc> docs = fr.ReadNextDoc();
                    foreach(Doc doc in docs) {
                        _semaphore2.Wait();
                        _docs.Enqueue(doc);
                        _semaphore1.Release();
                    }
                 
                }
                done = true;
            });
            //TO-DO
            while (!done || _docs.Count > 0 )
            {
                _semaphore1.Wait();
                Doc currentDoc;
                _docs.TryDequeue(out currentDoc);
                Parser(currentDoc);
                _semaphore2.Release();
            }
            task.Wait();
        }

        private static void Parser(Doc doc)
        {
            StringBuilder text = doc._text.Replace("\\n", " ");
            text = text.Replace(",", "");
            string[] splitedText = text.ToString().Split(' ');

            //Substitute month' names with numbers
            Dictionary<string, string> months = new Dictionary<string, string>();
            months.Add("jan", "01"); months.Add("feb", "02"); months.Add("mar", "03"); months.Add("apr", "04"); months.Add("may", "05"); months.Add("jun", "06"); months.Add("jul", "07"); months.Add("aug", "08"); months.Add("sep", "09"); months.Add("oct", "10"); months.Add("nov", "11"); months.Add("dec", "12");
            months.Add("january", "01"); months.Add("february", "02"); months.Add("march", "03"); months.Add("april", "04"); months.Add("june", "06"); months.Add("july", "07"); months.Add("august", "08"); months.Add("september", "09"); months.Add("october", "10"); months.Add("november", "11"); months.Add("december", "12");

            IEnumerable<String> onlyText =
                splitedText
                .SkipWhile((newWord) => String.Compare(newWord, Resources.Resource.openText) != 0)
                .TakeWhile((newWord) => String.Compare(newWord, Resources.Resource.closeText) != 0)
                .Where((newWord) => String.Compare(newWord, "") != 0);

            string toParse = String.Join(" ", onlyText);
            toParse = ParsePresents(toParse);
            char[] delimiters = { ' ' };
            splitedText = toParse.ToString().Split(delimiters);

            Queue<int> dates = new Queue<int>();
            Queue<int> money = new Queue<int>();
            Queue<int> specificBigNums = new Queue<int>();
            Queue<int> bigNums = new Queue<int>();
            Queue<int> betweens = new Queue<int>();

            PopulateQueueWithPositions(splitedText, months, dates, money, specificBigNums, bigNums, betweens);

            //while (betweens.Count != 0)
            //{
            //    splitedText = ParseBetweenTerms(betweens.Dequeue(), splitedText);
            //}
            while (dates.Count != 0)
            {
                int position = dates.Dequeue();
                splitedText = ParseDate(position, months[splitedText[position].ToLower()], splitedText);
            }
            while (money.Count != 0)
            {
                splitedText = ParseMoney(money.Dequeue(), splitedText);
            }
            while (specificBigNums.Count != 0)
            {
                splitedText = ParseSpecificNumbers(specificBigNums.Dequeue(), splitedText);
            }
            while (bigNums.Count != 0)
            {
                splitedText = ParseNumbers(bigNums.Dequeue(), splitedText);
            }

            numPositions.Clear();
            Console.WriteLine(doc._path + "\n" + String.Join(" ", splitedText));

        }

        private static void PopulateQueueWithPositions(string[] splitedText, Dictionary<string, string> months, Queue<int> dates, Queue<int> money, Queue<int> specificBigNums, Queue<int> bigNums, Queue<int> betweens)
        {
            int pos = 0;
            foreach (string word in splitedText)
            {
                string newWord = word;

                if ((word.Length - 1 >= 0) && word[word.Length - 1] == '.' && word.ToLower() != "u.s.")
                {
                    newWord = word.Substring(0, word.Length - 1);
                    splitedText[pos] = newWord;
                }

                if (dates != null && months.ContainsKey(newWord.ToLower()))
                {
                    dates.Enqueue(pos);
                }
                if (money != null && (newWord.ToLower().Contains(Resources.Resource.dollars.ToLower()) || newWord.Contains("$")))
                {
                    money.Enqueue(pos);
                }
                if (specificBigNums != null &&
                    (newWord.ToLower() == Resources.Resource.thousand || newWord.ToLower() == Resources.Resource.million ||
                    newWord.ToLower() == Resources.Resource.billion || newWord.ToLower() == Resources.Resource.trillion))
                {
                    specificBigNums.Enqueue(pos);
                }
                if (bigNums != null && Regex.IsMatch(newWord, Resources.Resource.regex_Numbers))
                {
                    bigNums.Enqueue(pos);
                }
                if (betweens != null && (newWord.ToLower().Contains("between") || newWord.ToLower().Contains("-")))
                {
                    betweens.Enqueue(pos);
                }
                pos++;
            }
        }

        private static bool isFraction(string suspect)
        {
           return Regex.IsMatch(suspect, Resources.Resource.regex_Fraction);
        }

        private static string numberBuilder(double number, double divider, string frac ,string representativeString ){

            double doubleFormat = number / divider;
            int intFormat = (int)doubleFormat;
            string parsed = "";
            if(frac != "")
            {
                frac = " " + frac;
            }
            if (doubleFormat == intFormat)
            {
                parsed = intFormat + frac + representativeString;
            }
            else
            {
                parsed = doubleFormat + frac + representativeString;
            }

            return parsed;

        }                     
        
        private static string[] ParseNumbers(int pos, string[] splitedText)
        {
            if (splitedText[pos] != " ")
            {
                string parsed = splitedText[pos];
                double divider = 1.0;
                string representativeLetter = "";
                string frac = "";

                if(pos+1 < splitedText.Length)
                {
                    if (isFraction(splitedText[pos + 1]))
                    {
                        frac = splitedText[pos + 1];
                        splitedText[pos + 1] = " ";
                    }
                }

                if (int.TryParse(splitedText[pos], out int number))
                {
                    if (number >= 1000 && number < 1000000)
                    {
                        divider = 1000.0;
                        representativeLetter = "K";
                    }

                    else if (number >= 1000000 && number < 1000000000)
                    {
                        divider = 1000000.0;
                        representativeLetter = "M";
                    }

                    else if (number >= 1000000000)
                    {
                        divider = 1000000000.0;
                        representativeLetter = "B";
                    }

                    parsed = numberBuilder(number,divider,frac,representativeLetter);
                    splitedText[pos] = parsed;
                    numPositions.Add(pos);
                    return splitedText;
                }

                else if (frac == "" && double.TryParse(splitedText[pos], out double doubleNumber))
                {

                    if (doubleNumber >= 1000 && doubleNumber < 1000000)
                    {
                        divider = 1000.0;
                        representativeLetter = "K";
                    }

                    else if (doubleNumber >= 1000000 && doubleNumber < 1000000000)
                    {
                        divider = 1000000.0;
                        representativeLetter = "M";
                    }

                    else if (doubleNumber >= 1000000000)
                    {
                        divider = 1000000000.0;
                        representativeLetter = "B";
                    }

                    parsed = numberBuilder(doubleNumber,divider,frac,representativeLetter);
                    splitedText[pos] = parsed;
                    numPositions.Add(pos);
                    return splitedText;
                }
            }
            return splitedText;
        }

        private static string[] ParseSpecificNumbers(int pos, string[] splitedText)
        {
            string frac = "";
            if (splitedText[pos] != " ")
            {
                string parsed = splitedText[pos];
                string representativeLetter = "B";

                if (splitedText[pos].ToLower() == Resources.Resource.thousand)
                {
                    representativeLetter = "K";
                }

                if (splitedText[pos].ToLower() == Resources.Resource.million)
                {
                    representativeLetter = "M";
                }
                
                if (pos - 1 >= 0)
                {
                    if (isFraction(splitedText[pos - 1]))
                    {
                        frac = splitedText[pos - 1];

                        if (pos - 2 >= 0)
                        {
                            if (int.TryParse(splitedText[pos - 2], out int numberBeforeFrac))
                            {
                                parsed = numberBuilder(numberBeforeFrac, 1.0, frac, representativeLetter);
                                splitedText[pos-2] = parsed;
                                splitedText[pos-1] = " ";
                                splitedText[pos] = " ";
                                numPositions.Add(pos-2);
                                return splitedText;
                            }
                        }
                    }

                   else if (int.TryParse(splitedText[pos - 1], out int number))
                    {
                        parsed = number + representativeLetter;
                        splitedText[pos - 1] = parsed;
                        splitedText[pos] = " ";
                        numPositions.Add(pos-1);
                        return splitedText;
                    }
                    else if (double.TryParse(splitedText[pos - 1], out double doubleNumber))
                    {
                        parsed = doubleNumber + representativeLetter;
                        splitedText[pos - 1] = parsed;
                        splitedText[pos] = " ";
                        numPositions.Add(pos - 1);
                        return splitedText;
                    }
                }
            }
            return splitedText;
        }

        private static string BuildDayAndYear(int day, int year, string month, string parsed){

            if (day >= 10)
            {
                parsed = year + "-" + month + "-" + day;
            }
            else
            {
                parsed = year + "-" + month + "-" + "0" + day;
            }
             return parsed;
        }

        private static string BuildDayOrYear(int dayOrYear, string month, string parsed){
            
             if (dayOrYear >= 10 && dayOrYear <= 31)
             {
                parsed = month + "-" + dayOrYear;
             }

             else if(dayOrYear >= 1000 && dayOrYear <= 4000)
             {
                parsed = dayOrYear + "-" + month;
             }

             else if (dayOrYear < 10)
             {
                parsed = month + "-0" + dayOrYear;
             }
             return parsed;
        }

        private static string[] ParseDate(int pos, string month, string[] splitedText)
        {
            if (splitedText[pos] != " ")
            {
                string parsed = splitedText[pos];
                if (pos - 1 >= 0)
                {
                    if (int.TryParse(splitedText[pos - 1], out int day))
                    {
                        if (pos + 1 < splitedText.Length)
                        {
                            if (int.TryParse(splitedText[pos + 1], out int year))
                            {
                                if (year >= 1000 && year < 4000)
                                {
                                    parsed = BuildDayAndYear(day,year,month,parsed);

                                    splitedText[pos - 1] = parsed;
                                    splitedText[pos] = " ";
                                    splitedText[pos + 1] = " ";
                                    return splitedText;
                                }
                            }
                        }

                        parsed = BuildDayOrYear(day,month,parsed);

                        splitedText[pos - 1] = parsed;
                        splitedText[pos] = " ";
                        return splitedText;
                    }
                }

                if (pos + 1 < splitedText.Length)
                {
                    if (int.TryParse(splitedText[pos + 1], out int day))
                    {
                        if ((pos + 2) < splitedText.Length) {

                            if (int.TryParse(splitedText[pos + 2], out int year))
                            {    
                                if (year >= 1000 && year < 4000){

                                    parsed = BuildDayAndYear(day,year,month,parsed);

                                    splitedText[pos] = parsed;
                                    splitedText[pos + 1] = " ";
                                    splitedText[pos + 2] = " ";
                                    return splitedText;
                                }
                            }
                        }
                        parsed = BuildDayOrYear(day,month,parsed);

                        splitedText[pos] = parsed;
                        splitedText[pos + 1] = " ";
                        return splitedText;
                    }
                }
            }
            return splitedText;
        }
       
        private static string ParsePresents(string text)
        {
            string pattern = "(?<number>([0-9])+([.][0-9]+)*)(\\s|-)+(percentage|percent)";
            string replacement = "${number}%";
            return Regex.Replace(text, pattern, replacement, RegexOptions.IgnoreCase);
        }

        public static string[] ParseMoney(int pos, string[] splitedText)
        {
            string[] temp = new string[splitedText.Length];
            Array.Copy(splitedText, temp, splitedText.Length);
            temp = DollarSignToCanonicalForm(ref pos, temp);
            string[] splitedMoneyExpr = temp[pos].Split(' ');
            string frac = "";
            if(Regex.IsMatch(splitedMoneyExpr[0], Resources.Resource.regex_Fraction))
            {
                frac = " " + splitedMoneyExpr[0];
                if(pos - 1 >= 0)
                {
                    splitedMoneyExpr[0] = temp[pos - 1];
                }
                splitedText = temp;
            }
            double sum = 0;

            if (Regex.IsMatch(splitedMoneyExpr[0], Resources.Resource.regex_Numbers) && double.TryParse(splitedMoneyExpr[0], out sum))
            { 
                splitedText = temp;
                if (frac != "")
                {
                    splitedText[pos] = " ";
                    pos = pos - 1; 
                }
                if (splitedMoneyExpr[1] == Resources.Resource.hundred)
                {
                    sum = sum * 100;
                }
                else if (splitedMoneyExpr[1] == Resources.Resource.thousand)
                {
                    sum = sum * 1000;
                }
                else if (splitedMoneyExpr[1] == Resources.Resource.million)
                {
                    sum = sum * 1000000;
                }
                else if (splitedMoneyExpr[1] == Resources.Resource.billion)
                {
                    sum = sum * 1000000000;
                }
                else if (splitedMoneyExpr[1] == Resources.Resource.trillion)
                {
                    sum = sum * 1000000000000;
                }

                if (sum >= 1000000)
                {
                    sum = sum / 1000000;
                    splitedText[pos] = sum.ToString() + frac +  " M " + Resources.Resource.dollars;
                }
                else
                {
                    splitedText[pos] = sum.ToString() + frac +  " " + Resources.Resource.dollars;
                }
            }
            return splitedText;
        }
        //to-do fraction dollars && fraction milion/billion... dollars
        private static string[] DollarSignToCanonicalForm(ref int pos, string[] splitedText) //Canonical form == NUMBER (counter) Dollars
        {
            List<String> counters = new List<string>();
            counters.Add(Resources.Resource.hundred);
            counters.Add(Resources.Resource.thousand);
            counters.Add(Resources.Resource.million);
            counters.Add(Resources.Resource.billion);
            counters.Add(Resources.Resource.trillion);
            string frac = "";

            if (splitedText[pos].Contains('$'))
            {
                splitedText[pos] = splitedText[pos].Remove(splitedText[pos].IndexOf("$"), 1);
                pos++; //skip the figure after the $ sign

                if (pos < splitedText.Length && counters.Contains(splitedText[pos].ToLower()))
                { //this limits counters to 1.
                    splitedText[pos - 1] = splitedText[pos - 1] + " " + splitedText[pos];
                    splitedText[pos] = " ";
                }

                splitedText[pos - 1] = splitedText[pos - 1] + " " + Resources.Resource.dollars;

                pos--;
            }
            else if (pos - 1 >= 0)
            {
                if (splitedText[pos - 1].ToLower() == "u.s.")
                {
                    splitedText[pos] = " "; //Remove Dollar
                    splitedText[pos - 1] = " "; //Remove U.S.
                    if (pos - 3 >= 0 && counters.Contains(splitedText[pos - 2]))
                    { //has a counter, it is of form NUM billion U.S. Dollars
                        splitedText[pos - 3] = splitedText[pos - 3] + " " + splitedText[pos - 2] + " " + " " + Resources.Resource.dollars;
                        splitedText[pos - 2] = " "; //Remove Counter
                        pos = pos - 3;
                    }
                    else if (pos - 2 >= 0) //Does't have a counter. It is of form NUM "frac" U.S. Dollars
                    {
                        splitedText[pos - 2] = splitedText[pos - 2] + " " + Resources.Resource.dollars;
                        pos = pos - 2;
                    }
                }
                else if (pos - 2 >= 0 && splitedText[pos - 1].ToLower() == "m")
                {
                    splitedText[pos] = " ";
                    splitedText[pos - 1] = " ";
                     splitedText[pos - 2] = splitedText[pos - 2] + " " + Resources.Resource.million + " " + Resources.Resource.dollars;
                    pos = pos - 2;
                }
                else if (pos - 2 >= 0 && splitedText[pos - 1].ToLower() == "bn")
                {
                    splitedText[pos] = " ";
                    splitedText[pos - 1] = " ";
                    splitedText[pos - 2] = splitedText[pos - 2] + " " + Resources.Resource.billion + " " + Resources.Resource.dollars;
                    pos = pos - 2;
                }
                else if (pos - 2 >= 0 && (splitedText[pos - 1].ToLower() == Resources.Resource.billion ||
                    splitedText[pos - 1].ToLower() == Resources.Resource.trillion || splitedText[pos - 1].ToLower() == Resources.Resource.million))
                {
                    splitedText[pos - 2] = splitedText[pos - 2] + " " + splitedText[pos - 1] + " " + Resources.Resource.dollars;
                    splitedText[pos] = " ";
                    splitedText[pos - 1] = " ";
                    pos = pos - 2;
                }

               else if (pos - 1 >= 0 && Regex.IsMatch(splitedText[pos - 1],Resources.Resource.regex_Fraction))
                {
                        splitedText[pos - 1] = splitedText[pos - 1] + " " + Resources.Resource.dollars;
                        splitedText[pos] = " ";
                       
                        pos = pos - 1;
                }

                else if (pos - 1 >= 0 && (Regex.IsMatch(splitedText[pos - 1], Resources.Resource.regex_Numbers) ||
                  Regex.IsMatch(splitedText[pos - 1], Resources.Resource.regex_Fraction)))
                {
                    splitedText[pos - 1] = splitedText[pos - 1] + " " + Resources.Resource.dollars;
                    splitedText[pos] = " ";
                    pos = pos - 1;
                }

            }

            return splitedText;
        }

    }
}
