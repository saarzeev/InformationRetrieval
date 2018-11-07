using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
            string s = ParsePresents("10.999 Percent bfdgfdgs");
            string t = ParseRange("Between number and number (for example: between 18 and 24)");
            //FromFilesToDocs("C:\\Users\\nastia\\source\\repos\\saarzeev\\corpus");
        }

        public static void FromFilesToDocs(string path)
        {
            Task task;

            FileReader fr = new FileReader(path);
            task = Task.Run(() => {

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

            while (!done) {
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
            string text = doc._text.Replace("\\n", " ");
            text = text.Replace(",", "");
            string[] splitedText = text.Split(' ');
           

            IEnumerable<String> onlyText =
                splitedText
                .SkipWhile((word) => String.Compare(word,"<TEXT>") != 0)
                .TakeWhile((word) => String.Compare(word, "</TEXT>") != 0)
                .Where((word) => String.Compare(word, "") != 0);

            text = String.Join(" ", onlyText);
            ParsePresents(text);
            ParseRange(text);
            char[] deli = { ' ', '-' };
            splitedText = text.Split(deli);

            Queue<int> moneyPos = new Queue<int>();
            Queue<int> datePos = new Queue<int>();
            Queue<int> bigNums = new Queue<int>();
            Queue<int> specificBigNums = new Queue<int>();
            Queue<int> fractions = new Queue<int>();//?

            List<string> months =new List<string>(){"January", "February", "March", "April",
                "May", "June", "July", "August", "September", "October", "November", "December" };
   
            int pos = 0; 
            foreach(string word in splitedText)
            {
                if(word == "Dollars" || word == "dollars" || word.Contains("$"))
                {
                    moneyPos.Enqueue(pos);
                }
                else if (months.Contains(word))
                {
                    datePos.Enqueue(pos);
                }
                else if(word == "Thousand" || word == "Million" || word == "Billion" || word == "Trillion")
                {
                    specificBigNums.Enqueue(pos);
                }
                else if (int.TryParse(word, out int iRes) || double.TryParse(word,out double dRes))
                {
                     bigNums.Enqueue(pos); 
                }
                else if (word.Contains("/"))
                {
                    fractions.Enqueue(pos);
                }
                pos++;
            }
        }
       
        private static void ParseNumbers(Doc doc)
        {
           
        }

        private static string ParsePresents(string text)
        {
            string pattern = "(?<number>([0-9])+([.][0-9]+)*)\\s(percentage|percent)";
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
