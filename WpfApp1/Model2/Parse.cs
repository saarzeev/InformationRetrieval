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

    class Parse
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
            string s = ParsePresents("<<90 percent, bfdgfdgsj 1555" +
                "ddsfssfsfsfs   90      percent");
            //string t = ParseRange("Between number and number (for example: between 18 and 24)");
            FromFilesToDocs("C:\\Users\\nastia\\source\\repos\\saarzeev\\corpus");
          

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

            Dictionary<string, string> months = new Dictionary<string, string>();
            months.Add("Jan", "01"); months.Add("Feb", "02"); months.Add("Mar", "03"); months.Add("Apr", "04"); months.Add("May", "05"); months.Add("Jun", "06"); months.Add("Jul", "07"); months.Add("Aug", "08"); months.Add("Sep", "09"); months.Add("Oct", "10"); months.Add("Nov", "11"); months.Add("Dec", "12");
            months.Add("January", "01"); months.Add("February", "02"); months.Add("March", "03"); months.Add("April", "04"); months.Add("June", "06"); months.Add("July", "07"); months.Add("August", "08"); months.Add("September", "09"); months.Add("October", "10"); months.Add("November", "11"); months.Add("December", "12");
            months.Add("JAN", "01"); months.Add("FEB", "02"); months.Add("MAR", "03"); months.Add("APR", "04"); months.Add("MAY", "05"); months.Add("JUN", "06"); months.Add("JUL", "07"); months.Add("AUG", "08"); months.Add("SEP", "09"); months.Add("OCT", "10"); months.Add("NOV", "11"); months.Add("DEC", "12");
            months.Add("JANUARY", "01"); months.Add("FEBRUARY", "02"); months.Add("MARCH", "03"); months.Add("APRIL", "04"); months.Add("JUNE", "06"); months.Add("JULY", "07"); months.Add("AUGUST", "08"); months.Add("SEPTEMBER", "09"); months.Add("OCTOBER", "10"); months.Add("NOVEMBER", "11"); months.Add("DECEMBER", "12");

            IEnumerable<String> onlyText =
                splitedText
                .SkipWhile((word) => String.Compare(word, "<TEXT>") != 0)
                .TakeWhile((word) => String.Compare(word, "</TEXT>") != 0)
                .Where((word) => String.Compare(word, "") != 0);

            string toParse = String.Join(" ", onlyText);
            ParsePresents(toParse);
            ParseRange(toParse);
            char[] delimiters = { ' ', '"' };
            splitedText = toParse.ToString().Split(delimiters);

            Queue<int> dates = new Queue<int>();
            Queue<int> money = new Queue<int>();
            Queue<int> specificBigNums = new Queue<int>();
            Queue<int> bigNums = new Queue<int>();
            
            int pos = 0;
            foreach (string word in splitedText)
            {
                if (months.ContainsKey(word))
                {
                    dates.Enqueue(pos);
                }
                else if (word == "Dollars" || word == "dollars" || word.Contains("$"))
                {
                    money.Enqueue(pos);
                }
                else if (word == "Thousand" || word == "Million" || word == "Billion" || word == "Trillion")
                {
                    specificBigNums.Enqueue(pos);
                }
                else if (int.TryParse(word, out int iRes) || double.TryParse(word, out double dRes))
                {
                    bigNums.Enqueue(pos);
                }
                pos++;
            }
            while (dates.Count != 0)
            {
                int position = dates.Dequeue();
                splitedText = ParseDate(position, months[splitedText[position]], splitedText);
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

        private static string[] ParseNumbers(int pos, string[] splitedText)
        {
            if (splitedText[pos] != " ")
            {
                string parsed = splitedText[pos];
                double divider = 1.0;
                string representativeLetter = "";
                if (int.TryParse(splitedText[pos], out int iRes))
                {
                    if (iRes >= 1000 && iRes < 1000000)
                    {
                        divider = 1000.0;
                        representativeLetter = "K";
                    }

                    else if (iRes >= 1000000 && iRes < 1000000000)
                    {
                        divider = 1000000.0;
                        representativeLetter = "M";
                    }

                    else if (iRes >= 1000000000)
                    {
                        divider = 1000000000.0;
                        representativeLetter = "B";
                    }
                    double doubleFormat = iRes / divider;
                    int intFormat = (int)doubleFormat;
                    if (doubleFormat == intFormat)
                    {
                        parsed = intFormat + representativeLetter;
                    }
                    else
                    {
                        parsed = doubleFormat + representativeLetter;
                    }
                    splitedText[pos] = parsed;
                    return splitedText;
                }
                else if (double.TryParse(splitedText[pos], out double dRes))
                {

                    if (dRes >= 1000 && dRes < 1000000)
                    {
                        divider = 1000.0;
                        representativeLetter = "K";
                    }

                    else if (dRes >= 1000000 && dRes < 1000000000)
                    {
                        divider = 1000000.0;
                        representativeLetter = "M";
                    }

                    else if (dRes >= 1000000000)
                    {
                        divider = 1000000000.0;
                        representativeLetter = "B";
                    }
                    double doubleFormat = dRes / divider;
                    int intFormat = (int)doubleFormat;
                    if (doubleFormat == intFormat)
                    {
                        parsed = intFormat + representativeLetter;
                    }
                    else
                    {
                        parsed = doubleFormat + representativeLetter;
                    }
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
                string parsed;
                string representativeLetter = "B";
                double divider = 1000000000.0;
                if (splitedText[pos] == "Thousand")
                {
                    divider = 1000.0;
                    representativeLetter = "K";
                }
                if (splitedText[pos] == "Million")
                {
                    divider = 1000000.0;
                    representativeLetter = "M";
                }

                if (pos - 1 >= 0)
                {
                    if (int.TryParse(splitedText[pos - 1], out int iRes))
                    {
                        double doubleFormat = iRes / divider;
                        int intFormat = (int)doubleFormat;
                        if (doubleFormat == intFormat)
                        {
                            parsed = intFormat + representativeLetter;
                        }
                        else
                        {
                            parsed = doubleFormat + representativeLetter;
                        }
                        splitedText[pos - 1] = parsed;
                        splitedText[pos] = " ";
                        return splitedText;
                    }
                    else if (double.TryParse(splitedText[pos - 1], out double dRes))
                    {
                        double doubleFormat = dRes / divider;
                        int intFormat = (int)doubleFormat;
                        if (doubleFormat == intFormat)
                        {
                            parsed = intFormat + representativeLetter;
                        }
                        else
                        {
                            parsed = doubleFormat + representativeLetter;
                        }
                        splitedText[pos - 1] = parsed;
                        splitedText[pos] = " ";
                        return splitedText;
                    }
                }
            }
            return splitedText;
        }

        private static string[] ParseMoney(int pos, string[] splitedText)
        {
            return splitedText;
        }

        private static string[] ParseDate(int pos, string format, string[] splitedText)
        {
            if (splitedText[pos] != " ")
            {
                string parsed;
                if (pos - 1 >= 0)
                {
                    if (int.TryParse(splitedText[pos - 1], out int res))
                    {
                        if (pos + 1 < splitedText.Length)
                        {
                            if (int.TryParse(splitedText[pos + 1], out int year))
                            {
                                if (year >= 1000 && year < 4000)
                                {
                                    if (res >= 10)
                                    {
                                        parsed = year + "-" + format + "-" + res;
                                    }
                                    else
                                    {
                                        parsed = year + "-" + format + "-" + "0" + res;
                                    }
                                    splitedText[pos - 1] = parsed;
                                    splitedText[pos] = " ";
                                    splitedText[pos + 1] = " ";
                                    return splitedText;
                                }
                            }
                        }
                        if (res >= 10)
                        {
                            parsed = format + "-" + res;
                        }
                        else
                        {
                            parsed = format + "-0" + res ;
                        }
                        splitedText[pos - 1] = parsed;
                        splitedText[pos] = " ";
                        return splitedText;
                    }
                }

                if (pos + 1 < splitedText.Length)
                {
                    if (int.TryParse(splitedText[pos + 1], out int res))
                    {
                        if ((pos + 2) < splitedText.Length) {
                            if (int.TryParse(splitedText[pos + 2], out int year))
                            {
                                if (res >= 10)
                                {
                                    parsed = year + "-" + format + "-" + res;
                                }
                                else
                                {
                                    parsed = year + "-" + format + "-0" + res;
                                }
                                splitedText[pos] = parsed;
                                splitedText[pos + 1] = " ";
                                splitedText[pos + 2] = " ";
                                return splitedText;
                            }
                        }
                    
                        if (res >= 10 && res <=31)
                        {
                            parsed = format + "-" + res;
                        }
                        else if( res >= 1000 && res <= 4000)
                        {
                            parsed = res + "-" + format;
                        }

                        else if (res < 10)
                        {
                            parsed = format + "-0" + res ;
                        }
                        else
                        {
                            return splitedText;
                        }
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
            string pattern = "(?<number>([0-9])+([.][0-9]+)*)(\\s)+(percentage|percent)";
            string replacement = "${number}%";
            return Regex.Replace(text, pattern, replacement, RegexOptions.IgnoreCase);
        }

        private static string ParseRange(string text)
        {
            string pattern = "(between)\\s(?<number>([0-9])+([.][0-9]+)*)\\s(and)\\s(?<number2>([0-9])+([.][0-9]+)*)";
            string replacement = "${number}-${number2}";
            return Regex.Replace(text, pattern, replacement, RegexOptions.IgnoreCase);
        }

    }
}
