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



        static void Main(string[] args)
        {
            //FileReader fr = new FileReader("C:\\Users\\nastia\\source\\repos\\saarzeev\\corpus");
            //_semaphore.Release(11);
            //while (fr.HasNext()) { }
            //Console.WriteLine( fr.ReadNextDoc());
            string s = ParsePresents("<<90 percent , bfdgfdgsj 1555" +
                "ddsfssfsfsfs   90             percent----");
            //string t = ParseRange("Between number and number (for example: between 18 and 24)");
            FromFilesToDocs("C:\\corpus");
          

        }

        public static void FromFilesToDocs(string path)
        {
            Task task;

            FileReader fr = new FileReader(path);
            task = Task.Run(() =>
            {

                while (fr.HasNext())
                {
                    _semaphore2.Wait();
                    Doc doc = fr.ReadNextDoc();
                    if (doc != null)
                    {
                        _docs.Enqueue(doc);
                        _semaphore1.Release();
                    }
                    else
                    {
                        _semaphore2.Release();
                    }
                }
                done = true;
            });

            while (!done)
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
            months.Add("january", "01"); months.Add("february", "02"); months.Add("march", "03");months.Add("april", "04"); months.Add("june", "06"); months.Add("july", "07"); months.Add("august", "08"); months.Add("september", "09"); months.Add("october", "10"); months.Add("november", "11"); months.Add("december", "12");
            
            IEnumerable<String> onlyText =
                splitedText
                .SkipWhile((word) => String.Compare(word, Resources.Resource.openText) != 0)
                .TakeWhile((word) => String.Compare(word, Resources.Resource.closeText) != 0)
                .Where((word) => String.Compare(word, "") != 0);

            string toParse = String.Join(" ", onlyText);
            toParse = ParsePresents(toParse);
            toParse = ParseRange(toParse);
            char[] delimiters = {' '};
            splitedText = toParse.ToString().Split(delimiters);

            Queue<int> dates = new Queue<int>();
            Queue<int> money = new Queue<int>();
            Queue<int> specificBigNums = new Queue<int>();
            Queue<int> bigNums = new Queue<int>();
            

           
            int pos = 0;
            foreach (string word in splitedText)
            {
                if (months.ContainsKey(word.ToLower()))
                {
                    dates.Enqueue(pos);
                }
                else if (word.ToLower().Contains(Resources.Resource.dollars.ToLower()) || word.Contains("$"))
                {
                    money.Enqueue(pos);
                }
                else if (word.ToLower() == Resources.Resource.thousand || word.ToLower() == Resources.Resource.million || 
                    word.ToLower() == Resources.Resource.billion || word.ToLower() == Resources.Resource.trillion)
                {
                    specificBigNums.Enqueue(pos);
                }
                else if (Regex.IsMatch(word, Resources.Resource.regexNumbers))
                {
                    bigNums.Enqueue(pos);
                }
                pos++;
            }
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

            Console.WriteLine(String.Join(" ", splitedText));
            
        }

        private static string numberBuilder(double number, double divider, string representativeLetter, string parsed ){

            double doubleFormat = number / divider;
            int intFormat = (int)doubleFormat;

            if (doubleFormat == intFormat)
            {
                parsed = intFormat + representativeLetter;
            }
            else
            {
                parsed = doubleFormat + representativeLetter;
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

                    parsed = numberBuilder(number,divider,representativeLetter,parsed);
                    splitedText[pos] = parsed;
                    return splitedText;
                }

                else if (double.TryParse(splitedText[pos], out double doubleNumber))
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

                    parsed = numberBuilder(doubleNumber,divider,representativeLetter,parsed);
                    splitedText[pos] = parsed;
                    return splitedText;
                }
            }
            return splitedText;
        }

        private static string[] ParseSpecificNumbers(int pos, string[] splitedText)
        {
            if (splitedText[pos] != " ")
            {
                string parsed = splitedText[pos];
                string representativeLetter = "B";
                double divider = 1000000000.0;

                if (splitedText[pos].ToLower() == Resources.Resource.thousand)
                {
                    divider = 1000.0;
                    representativeLetter = "K";
                }

                if (splitedText[pos].ToLower() == Resources.Resource.million)
                {
                    divider = 1000000.0;
                    representativeLetter = "M";
                }

                if (pos - 1 >= 0)
                {
                    if (int.TryParse(splitedText[pos - 1], out int number))
                    {
                        parsed = numberBuilder(number,divider,representativeLetter,parsed);
                        splitedText[pos - 1] = parsed;
                        splitedText[pos] = " ";
                        return splitedText;
                    }
                    else if (double.TryParse(splitedText[pos - 1], out double doubleNumber))
                    {
                        parsed = numberBuilder(doubleNumber,divider,representativeLetter,parsed);
                        splitedText[pos - 1] = parsed;
                        splitedText[pos] = " ";
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

        private static string ParseRange(string text)
        {
            string pattern = "(between)\\s(?<number>([0-9])+([.][0-9]+)*)\\s\\w+(and)\\s(?<number2>([0-9])+([.][0-9]+)*)";
            string replacement = "${number}-${number2}";
            return Regex.Replace(text, pattern, replacement, RegexOptions.IgnoreCase);
        }

    }
}
