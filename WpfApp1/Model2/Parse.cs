﻿using SearchEngine;
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
    /// <summary>
    /// Class is responsible for the parsing process
    /// </summary>
    public partial class Parse
    {
        private static Parse parse;
        private ConcurrentQueue<Doc> _docs;
        private HashSet<string> _cities;
        private Mutex _citiesMutex;
        private SemaphoreSlim _semaphore1;
        private SemaphoreSlim _semaphore2;
        private bool done = false;
        public static HashSet<string> stopWords;
        private HashSet<string> bigNumbersHash;
        private HashSet<String> counters = new HashSet<string>();
        private string destinationPath;
        public static List<Task> writingList = new List<Task>();
        public List<string> languagesD = new List<string>();


        string[] delimiters = {"=","_", "?" ," - ", " ", "(", ")", "<", ">", "[", "]", "{", "}", "^", ";", "\"", "'", "`", "|", "*", "#", "+", "?", "!", "&", "@", "," ,"---", "..", "...", " -- ", "\\n", "----", "$$" , "$$$" };
        char[] trimDelimiters = { '.', ':', '/' , '-'};
        Dictionary<string, string> months = new Dictionary<string, string>();

        public static Parse Instance(HashSet<string> StopWords = null)
        {
            if (StopWords == null)
            {
                if (parse == null)
                {
                    parse = new Parse();
                }
                return parse;
            }
            else
            {
                parse = new Parse(StopWords);
                return parse;
            }
           

        }

        public static void DestructParse()
        {
            parse = null;
        }

        private Parse()
        {
            _docs = new ConcurrentQueue<Doc>();
            _cities = new HashSet<string>();
            _citiesMutex = new Mutex();
            _semaphore1 = new SemaphoreSlim(0, 163);
            _semaphore2 = new SemaphoreSlim(163, 163);
            done = false;
            stopWords = new HashSet<string>();
            bigNumbersHash = new HashSet<string>();
            months = new Dictionary<string, string>();
            InitHeapVariables();
         }
        private Parse(HashSet<string> stopWords)
        {
            Parse.stopWords = stopWords;
            bigNumbersHash = new HashSet<string>();
            months = new Dictionary<string, string>();
            InitHeapVariables();

        }

        /// <summary>
        /// Invoke a new thread to iterate entire sub-tree, starting from the given <paramref name="filesPath"/>.
        /// Concurrently, main thread goes through Docs in _doc, and parses them.
        /// Docs are added to _docs.
        /// 
        /// </summary>
        /// <param name="filesPath"></param>
        /// <param name="destinationPath"></param>
        /// <param name="stopWordsPath"></param>
        /// <param name="shouldStem"></param>
        public void FromFilesToDocs(string filesPath, string destinationPath, string stopWordsPath, bool shouldStem)
        {
          
            Task task;
            this.destinationPath = destinationPath;
            FileReader fr = new FileReader(filesPath, stopWordsPath);
            stopWords = fr.stopWords;
            task = Task.Run(() =>
            {
                while (fr.HasNext())
                {

                    List<Doc> docs = fr.ReadNextDoc();
                    foreach (Doc doc in docs)
                    {
                        _semaphore2.Wait();
                        _docs.Enqueue(doc);
                        _semaphore1.Release();
                    }

                }
                done = true;
            });
            Task loadCities = Task.Run(() => { City.initCities(); });
            Task tasker1 = Task.Run(() =>
            {
                while (!done || _docs.Count > 0)
                {
                    _semaphore1.Wait();
                    Doc currentDoc;
                    _docs.TryDequeue(out currentDoc);
                    Parser(currentDoc, shouldStem);
                    _semaphore2.Release();
                }
            });

            Task tasker2 = Task.Run(() =>
            {
                while (!done || _docs.Count > 0)
                {
                    _semaphore1.Wait();
                    Doc currentDoc;
                    _docs.TryDequeue(out currentDoc);
                    Parser(currentDoc, shouldStem);
                    _semaphore2.Release();
                }
            });

            Task tasker3 = Task.Run(() =>
            {
                while (!done || _docs.Count > 0)
                {
                    _semaphore1.Wait();
                    Doc currentDoc;
                    _docs.TryDequeue(out currentDoc);
                    Parser(currentDoc, shouldStem);
                    _semaphore2.Release();
                }
            });

            Task tasker4 = Task.Run(() =>
            {
                while (!done || _docs.Count > 0)
                {
                    _semaphore1.Wait();
                    Doc currentDoc;
                    _docs.TryDequeue(out currentDoc);
                    Parser(currentDoc, shouldStem);
                    _semaphore2.Release();
                }
            });

            loadCities.Wait();
            task.Wait();
            tasker1.Wait();
            tasker2.Wait();
            tasker3.Wait();
            tasker4.Wait();
            Indexer indexer = Indexer.Instance(destinationPath, shouldStem);
            //final temp writing
           
           
            Task.WaitAll(writingList.ToArray());
            Task tasker5 = Task.Run(() => { indexer.currenPostingSet.DumpToDisk(true); });
            tasker5.Wait();
            // indexer.dead.Clear();
            // indexer.dead = null;
            //merging

            Task tasker6 = Task.Run(() => { indexer.currenPostingSet.mergeFiles(_cities); });
            Task tasker8 = Task.Run(() => { indexer.WriteDocPosting(); });
            tasker8.Wait();
            indexer.docsCount = Indexer.docsIndexer.Count();
            tasker6.Wait();
            Indexer.docsIndexer = null;
            Task tasker7 = Task.Run(() => { indexer.WriteDictionary(); });
            tasker7.Wait();
            indexer.termCount = Indexer.fullDictionary.Count();
            Indexer.fullDictionary.Clear();
        }

        private void InitHeapVariables()
        {
            bigNumbersHash.Add(Resources.Resource.thousand);
            bigNumbersHash.Add(Resources.Resource.million);
            bigNumbersHash.Add(Resources.Resource.billion);
            bigNumbersHash.Add(Resources.Resource.trillion);
            
            counters.Add(Resources.Resource.hundred);
            counters.Add(Resources.Resource.thousand);
            counters.Add(Resources.Resource.million);
            counters.Add(Resources.Resource.billion);
            counters.Add(Resources.Resource.trillion);

            months.Add("jan", "01"); months.Add("feb", "02"); months.Add("mar", "03"); months.Add("apr", "04"); months.Add("may", "05"); months.Add("jun", "06"); months.Add("jul", "07"); months.Add("aug", "08"); months.Add("sep", "09"); months.Add("oct", "10"); months.Add("nov", "11"); months.Add("dec", "12");
            months.Add("january", "01"); months.Add("february", "02"); months.Add("march", "03"); months.Add("april", "04"); months.Add("june", "06"); months.Add("july", "07"); months.Add("august", "08"); months.Add("september", "09"); months.Add("october", "10"); months.Add("november", "11"); months.Add("december", "12");
        }

        /// <summary>
        /// Given a <paramref name="doc"/>, parse doc.text.
        /// Parser standardizes dates, money phrases, numbers, and Between and '-' terms.
        /// Our added parsing rules are for times (3PM -> 15:00)
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="shouldStem"></param>
        private void Parser(Doc doc, bool shouldStem)
        {
            _citiesMutex.WaitOne();
            if (doc.city != "")
            {
                _cities.Add(doc.city.ToUpper());
            }
            _citiesMutex.ReleaseMutex();
            //saving suspicious words Indexes by theme
            Queue<int> dates = new Queue<int>();
            Queue<int> money = new Queue<int>();
            Queue<int> specificBigNums = new Queue<int>();
            Queue<int> bigNums = new Queue<int>();
            Queue<int> betweens = new Queue<int>();
            Queue<int> times = new Queue<int>();
            HashSet<int> numPositions = new HashSet<int>();
            Queue<int> cities = new Queue<int>();

            Dictionary<string, List<int>> addedTerms = new Dictionary<string, List<int>>();

            StringBuilder text = doc._text.Replace("'", "");
            doc._text = null;
            text = text.Replace("--", "-");
            string toParse = ParsePercent(text.ToString());
            text = null;
            string[] splitedText = toParse.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            splitedText = splitedText.Where((newWord) => ((newWord.Length > 1) && 
            (newWord == "between" || newWord == "Between" || newWord == "and" || !stopWords.Contains(newWord.ToLower())))).ToArray();
            if (splitedText.Length > 0)
            {
                splitedText[0] = " ";
                splitedText[splitedText.Length-1] = " ";
            }

            PopulateQueueWithPositions(splitedText, months, dates, money, specificBigNums, bigNums, betweens, times);

            //check and parse if the words meet the conditions

            while (dates.Count != 0)
            {
                int position = dates.Dequeue();
                splitedText = ParseDate(position, months[splitedText[position].ToLower()], splitedText);
            }
            while (times.Count != 0)
            {
                splitedText = ParseTimeTerms(times.Dequeue(), splitedText);
            }
            while (money.Count != 0)
            {
                splitedText = ParseMoney(money.Dequeue(), splitedText);
            }
            while (specificBigNums.Count != 0)
            {
                splitedText = ParseSpecificNumbers(specificBigNums.Dequeue(), splitedText, numPositions);
            }
            while (bigNums.Count != 0)
            {
                splitedText = ParseNumbers(bigNums.Dequeue(), splitedText, numPositions);
            }
          
            while (betweens.Count != 0)
            {
                splitedText = ParseBetweenTerms(betweens.Dequeue(), splitedText, numPositions, addedTerms);
            }

            numPositions.Clear();

            SortedDictionary<string,Term> docVovabulary = AddTermsToVocabulry(splitedText, shouldStem, doc, addedTerms);
            Indexer index = Indexer.Instance(destinationPath,shouldStem);
            index.InitIndex(doc ,docVovabulary, languagesD);
         
        }

        public HashSet<string> ParseQuery(StringBuilder query,bool shouldStem)
        {
            //saving suspicious words Indexes by theme
            Queue<int> dates = new Queue<int>();
            Queue<int> money = new Queue<int>();
            Queue<int> specificBigNums = new Queue<int>();
            Queue<int> bigNums = new Queue<int>();
            Queue<int> betweens = new Queue<int>();
            Queue<int> times = new Queue<int>();
            HashSet<int> numPositions = new HashSet<int>();
            Queue<int> cities = new Queue<int>();

            Dictionary<string, List<int>> addedTerms = new Dictionary<string, List<int>>();

            StringBuilder text = query.Replace("'", "");
            query = null;
            text = text.Replace("--", "-");
            string toParse = ParsePercent(text.ToString());
            text = null;
            string[] splitedText = toParse.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            splitedText = splitedText.Where((newWord) => ((newWord.Length > 1) &&
            (newWord == "between" || newWord == "Between" || newWord == "and" || !stopWords.Contains(newWord.ToLower())))).ToArray();
            
            PopulateQueueWithPositions(splitedText, months, dates, money, specificBigNums, bigNums, betweens, times);

            //check and parse if the words meet the conditions

            while (dates.Count != 0)
            {
                int position = dates.Dequeue();
                splitedText = ParseDate(position, months[splitedText[position].ToLower()], splitedText);
            }
            while (times.Count != 0)
            {
                splitedText = ParseTimeTerms(times.Dequeue(), splitedText);
            }
            while (money.Count != 0)
            {
                splitedText = ParseMoney(money.Dequeue(), splitedText);
            }
            while (specificBigNums.Count != 0)
            {
                splitedText = ParseSpecificNumbers(specificBigNums.Dequeue(), splitedText, numPositions);
            }
            while (bigNums.Count != 0)
            {
                splitedText = ParseNumbers(bigNums.Dequeue(), splitedText, numPositions);
            }

            while (betweens.Count != 0)
            {
                splitedText = ParseBetweenTerms(betweens.Dequeue(), splitedText, numPositions, addedTerms);
            }

            numPositions.Clear();

            HashSet<string> parsed = new HashSet<string>();
            StemmerInterface stm = new Stemmer();
            foreach (string word in splitedText)
            {
                string toLower = word.ToLower();
                if (toLower.Length > 1 && toLower != "betweens" && toLower != "and")
                {
                   bool isNumber = Char.IsDigit(toLower[0]);
                   parsed.Add((shouldStem && !isNumber) ? stm.stemTerm(toLower) : toLower);
                }
            }
            return parsed;
        }

        /// <summary>
        /// Populates given queues with position of suspected actionable terms;
        /// </summary>
        /// <param name="splitedText"></param>
        /// <param name="months"></param>
        /// <param name="dates"></param>
        /// <param name="money"></param>
        /// <param name="specificBigNums"></param>
        /// <param name="bigNums"></param>
        /// <param name="betweens"></param>
        /// <param name="times"></param>
        private void PopulateQueueWithPositions(string[] splitedText,
                                                Dictionary<string, string> months,
                                                Queue<int> dates,
                                                Queue<int> money,
                                                Queue<int> specificBigNums,
                                                Queue<int> bigNums,
                                                Queue<int> betweens,
                                                Queue<int> times = null)
        {
            int pos = 0;

            foreach (string word in splitedText)
            {
                    string newWord = word;
                    if (word.ToLower() != "u.s.")
                    {
                        newWord = newWord.TrimStart(trimDelimiters).TrimEnd(trimDelimiters);
                        splitedText[pos] = newWord;
                    }
                    if (newWord.Length > 1)
                    {
                        if (bigNums != null && newWord[0] <= 57 && newWord[0] >= 48 && Regex.IsMatch(newWord, Resources.Resource.regex_Numbers))
                        {
                            bigNums.Enqueue(pos);
                        }
                        else if (betweens != null && (newWord.ToLower().Contains("-") || newWord.ToLower() == "between"))
                        {
                            betweens.Enqueue(pos);
                        }
                        else if (dates != null && months.ContainsKey(newWord.ToLower()))
                        {
                            dates.Enqueue(pos);
                        }
                        else if (money != null && (newWord.ToLower() == Resources.Resource.dollars.ToLower() || newWord[0] == '$'))
                        {
                            money.Enqueue(pos);
                        }
                        else if (specificBigNums != null && bigNumbersHash.Contains(newWord.ToLower()))
                        {
                            specificBigNums.Enqueue(pos);
                        }
                        else if (times != null && Regex.IsMatch(newWord, Resources.Resource.regex_PM_AM))
                        {
                            times.Enqueue(pos);
                        }
                    }
                pos++;
           }
        }

        private bool isFraction(string suspect)
        {
            return Regex.IsMatch(suspect, Resources.Resource.regex_Fraction);
        }


        /// <summary>
        /// Generate string for number (int/double/fraction) with its representative letter or string
        /// </summary>
        /// <param name="number"></param>
        /// <param name="divider"></param>
        /// <param name="frac"></param>
        /// <param name="representativeString"></param>
        /// <returns> parsed number</returns>
        private string numberBuilder(double number, double divider, string frac, string representativeString) {

            double doubleFormat = number / divider;
            int intFormat = (int)doubleFormat;
            string parsed = "";
            if (frac != "")
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


        /// <summary>
        /// Checks if the suspicious word is a number and parses it according to the requirements
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="splitedText"></param>
        /// <param name="numPositions"></param>
        /// <returns></returns>
        public string[] ParseNumbers(int pos , string[] splitedText, HashSet<int> numPositions)
        {
            if (splitedText[pos] != " ")
            {
                string parsed = splitedText[pos];
                double divider = 1.0;
                string representativeLetter = "";
                string frac = "";

                if (pos + 1 < splitedText.Length)
                {
                    if (isFraction(splitedText[pos + 1]))
                    {
                        frac = splitedText[pos + 1];
                        splitedText[pos + 1] = " ";
                    }
                }

                // dispensing cases - depending on the size of the number(int)
                if (double.TryParse(splitedText[pos], out double number))
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

                    parsed = numberBuilder(number, divider, frac, representativeLetter);
                    splitedText[pos] = parsed;
                    numPositions.Add(pos);
                    return splitedText;
                }
            }
            return splitedText;
        }

        /// <summary>
        /// checks if the suspicious word is a expression to represent a large number and parse it to requirements
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="splitedText"></param>
        /// <param name="numPositions"></param>
        /// <returns></returns>
        private string[] ParseSpecificNumbers(int pos, string[] splitedText, HashSet<int> numPositions)
        {
            string frac = "";
            if (splitedText[pos] != " ")
            {
                string parsed = splitedText[pos];
                string representativeLetter = "B";
                double divider = 1000000000.0;

                if (splitedText[pos].ToLower() == Resources.Resource.thousand)
                {
                    representativeLetter = "K";
                    divider = 1000.0;
                }

                if (splitedText[pos].ToLower() == Resources.Resource.million)
                {
                    representativeLetter = "M";
                    divider = 1000000.0;
                }

                if (pos - 1 >= 0)
                {
                    // number - fraction - word 
                    if (isFraction(splitedText[pos - 1]))
                    {
                        frac = splitedText[pos - 1];
                        if (pos - 2 >= 0)
                        {
                            if (int.TryParse(splitedText[pos - 2], out int numberBeforeFrac))
                            {
                                parsed = numberBuilder(numberBeforeFrac, divider, frac, representativeLetter);
                                splitedText[pos - 2] = parsed;
                                splitedText[pos - 1] = " ";
                                splitedText[pos] = " ";
                                numPositions.Add(pos - 2);
                                return splitedText;
                            }
                        }
                    }
                    // number - word 
                    else if (double.TryParse(splitedText[pos - 1], out double doubleNumber))
                    {
                        parsed = numberBuilder(doubleNumber, divider, frac, representativeLetter);
                        splitedText[pos - 1] = parsed;
                        splitedText[pos] = " ";
                        numPositions.Add(pos - 1);
                        return splitedText;
                    }
                }
            }
            return splitedText;
        }


        /// <summary>
        /// generate string for Date :(year-month-day)
        /// </summary>
        /// <param name="day"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="parsed"></param>
        /// <returns></returns>
        private string BuildDayAndYear(int day, int year, string month, string parsed) {

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

        /// <summary>
        /// generate string for Date :(month-day or year-month)
        /// </summary>
        /// <param name="dayOrYear"></param>
        /// <param name="month"></param>
        /// <param name="parsed"></param>
        /// <returns></returns>

        private string BuildDayOrYear(int dayOrYear, string month, string parsed) {

            if (dayOrYear >= 10 && dayOrYear <= 31)
            {
                parsed = month + "-" + dayOrYear;
            }

            else if (dayOrYear >= 1000 && dayOrYear <= 4000)
            {
                parsed = dayOrYear + "-" + month;
            }

            else if (dayOrYear < 10)
            {
                parsed = month + "-0" + dayOrYear;
            }
            return parsed;
        }

        /// <summary>
        /// checks if the expression to represent a months  has another date expression in its vicinity, and parse it to requirements
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="month"></param>
        /// <param name="splitedText"></param>
        /// <returns></returns>
        private string[] ParseDate(int pos, string month, string[] splitedText)
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
                            //day-month-year
                            if (int.TryParse(splitedText[pos + 1], out int year))
                            {
                                if (year >= 1000 && year < 4000)
                                {
                                    parsed = BuildDayAndYear((int)day, (int)year, month, parsed);

                                    splitedText[pos - 1] = parsed;
                                    splitedText[pos] = " ";
                                    splitedText[pos + 1] = " ";
                                    return splitedText;
                                }
                            }
                        }

                        //day-month
                        parsed = BuildDayOrYear((int)day, month, parsed);

                        splitedText[pos - 1] = parsed;
                        splitedText[pos] = " ";
                        return splitedText;
                    }
                }

                if (pos + 1 < splitedText.Length)
                {              
                    if (int.TryParse(splitedText[pos + 1], out int day))
                    {
                        if ((pos + 2) < splitedText.Length)
                        {
                            if (int.TryParse(splitedText[pos + 2], out int year))
                            {
                                if (year >= 1000 && year < 4000)
                                {

                                    //month-day-year
                                    parsed = BuildDayAndYear((int)day, (int)year, month, parsed);

                                    splitedText[pos] = parsed;
                                    splitedText[pos + 1] = " ";
                                    splitedText[pos + 2] = " ";
                                    return splitedText;
                                }
                            }
                        }
                        //month-day
                        parsed = BuildDayOrYear((int)day, month, parsed);

                        splitedText[pos] = parsed;
                        splitedText[pos + 1] = " ";
                        return splitedText;
                    }
                }
            }
            return splitedText;
        }


        /// <summary>
        /// Parses "Dollar" expressions and "$price" terms
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="splitedText"></param>
        /// <returns></returns>
        public string[] ParseMoney(int pos, string[] splitedText)
        {
            List<int> posToRemove = new List<int>();
            string canonizedStr = DollarSignToCanonicalForm(ref pos, splitedText, ref posToRemove);
            string[] splitedMoneyExpr = canonizedStr.Split(' ');
            string frac = "";
            bool commitChangesToSplittedText = false;
            if (Regex.IsMatch(splitedMoneyExpr[0], Resources.Resource.regex_Fraction))
            {
                frac = " " + splitedMoneyExpr[0];
                if (pos - 1 >= 0)
                {
                    splitedMoneyExpr[0] = splitedText[pos - 1];
                }
               
                splitedText[pos] = canonizedStr;
                commitChangesToSplittedText = true;
            }
            double sum = 0;
            if (Regex.IsMatch(splitedMoneyExpr[0], Resources.Resource.regex_Numbers) && double.TryParse(splitedMoneyExpr[0], out sum))
            {
            
                splitedText[pos] = canonizedStr;
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
                    splitedText[pos] = sum.ToString() + frac + " M " + Resources.Resource.dollars;
                    commitChangesToSplittedText = true;
                }
                else
                {
                    splitedText[pos] = sum.ToString() + frac + " " + Resources.Resource.dollars;
                    commitChangesToSplittedText = true;
                }
            }

            if (commitChangesToSplittedText)
            {
                foreach (int i in posToRemove)
                {
                    splitedText[i] = " ";
                }
            }
            return splitedText;
        }

        private string DollarSignToCanonicalForm(ref int pos, string[] splitedText, ref List<int> posToDelete) //Canonical form == NUMBER (counter) Dollars
        {
            
            string frac = "";
            string canonizedStr = "";

            if (splitedText[pos].Contains('$'))
            {
                canonizedStr += splitedText[pos].Remove(splitedText[pos].IndexOf("$"), 1);
                pos++; //skip the figure after the $ sign

                if (pos < splitedText.Length && counters.Contains(splitedText[pos].ToLower()))
                { //this limits counters to 1.
                    canonizedStr += " " + splitedText[pos];
                    posToDelete.Add(pos);
                }

                canonizedStr += " " + Resources.Resource.dollars;

                pos--;
            }
            else if (pos - 1 >= 0)
            {
                if (splitedText[pos - 1].ToLower() == "u.s.")
                {
                    posToDelete.Add(pos);
                    posToDelete.Add(pos - 1);
                    if (pos - 3 >= 0 && counters.Contains(splitedText[pos - 2]))
                    { //has a counter, it is of form NUM billion U.S. Dollars
                        canonizedStr = splitedText[pos - 3] + " " + splitedText[pos - 2] + " " + " " + Resources.Resource.dollars;
                        posToDelete.Add(pos - 2);
                        pos = pos - 3;
                    }
                    else if (pos - 2 >= 0) //Does't have a counter. It is of form NUM "frac" U.S. Dollars
                    {
                        canonizedStr = splitedText[pos - 2] + " " + Resources.Resource.dollars;
                        pos = pos - 2;
                    }
                }
                else if (pos - 2 >= 0 && splitedText[pos - 1].ToLower() == "m")
                {
                    posToDelete.Add(pos);
                    posToDelete.Add(pos - 1);
                    canonizedStr = splitedText[pos - 2] + " " + Resources.Resource.million + " " + Resources.Resource.dollars;
                    pos = pos - 2;
                }
                else if (pos - 2 >= 0 && splitedText[pos - 1].ToLower() == "bn")
                {
                    posToDelete.Add(pos);
                    posToDelete.Add(pos - 1);
                    canonizedStr = splitedText[pos - 2] + " " + Resources.Resource.billion + " " + Resources.Resource.dollars;
                    pos = pos - 2;
                }
                else if (pos - 2 >= 0 && (splitedText[pos - 1].ToLower() == Resources.Resource.billion ||
                    splitedText[pos - 1].ToLower() == Resources.Resource.trillion || splitedText[pos - 1].ToLower() == Resources.Resource.million))
                {
                    canonizedStr = splitedText[pos - 2] + " " + splitedText[pos - 1] + " " + Resources.Resource.dollars;
                    posToDelete.Add(pos);
                    posToDelete.Add(pos - 1);
                    pos = pos - 2;
                }

                else if (pos - 1 >= 0 && Regex.IsMatch(splitedText[pos - 1], Resources.Resource.regex_Fraction))
                {
                    canonizedStr = splitedText[pos - 1] + " " + Resources.Resource.dollars;
                    posToDelete.Add(pos);

                    pos = pos - 1;
                }

                else if (pos - 1 >= 0 && (Regex.IsMatch(splitedText[pos - 1], Resources.Resource.regex_Numbers) || Regex.IsMatch(splitedText[pos - 1], Resources.Resource.regex_Fraction)))
                {
                    canonizedStr = splitedText[pos - 1] + " " + Resources.Resource.dollars;
                    posToDelete.Add(pos);

                    pos = pos - 1;
                }

            }

            return canonizedStr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="splitedText"></param>
        /// <returns></returns>
        public string[] ParseTimeTerms(int pos, string[] splitedText) {
            if (splitedText[pos] != " ")
            {
                //same word
                string replacedDot = splitedText[pos].Contains('.') ? splitedText[pos].Replace('.', ':') : splitedText[pos];

                if (replacedDot.Contains(':') || Regex.IsMatch(replacedDot, Resources.Resource.regex_PM_AM_withHoursOnly)) {
                    if (System.DateTime.TryParse(replacedDot, out var dateTime))
                    {
                        splitedText[pos] = dateTime.GetDateTimeFormats()[109];
                        return splitedText;
                    }
                }

                else if (pos - 1 >= 0) {
                    //time with .
                    bool isTimeWithDot = (splitedText[pos] == "PM" || splitedText[pos] == "AM") && double.TryParse(splitedText[pos - 1], out double suspectedTimeWithDot); 
                    replacedDot = isTimeWithDot ? splitedText[pos - 1].Replace('.', ':') : splitedText[pos - 1];
                    // 2 words
                    if (replacedDot.Contains(':')) {

                        string suspectedTime = replacedDot + " " + splitedText[pos];
                        if (System.DateTime.TryParse(suspectedTime, out var dateTime))
                        {
                            splitedText[pos - 1] = dateTime.GetDateTimeFormats()[109];
                            splitedText[pos] = " ";
                            return splitedText;
                        }
                    }

                }
            }
            return splitedText;
        }

    //parse expressions that formated: <number-percentage|percent> to another format: <number%>
    private string ParsePercent(string text)
    {
        string pattern = "(?<number>([0-9])+([.][0-9]+)*)(\\s|-)+(percentage|percent)";
        string replacement = "${number}%";
        return Regex.Replace(text, pattern, replacement, RegexOptions.IgnoreCase);
    }
        private SortedDictionary<string, Term> AddTermsToVocabulry(string[] splitedText, bool shouldStem, Doc doc, Dictionary<string, List<int>> addedTerms)
        {
            addedTerms = new Dictionary<string, List<int>>();
            StemmerInterface stm = new Stemmer();
            SortedDictionary<string, Term> thisDocVocabulary = new SortedDictionary<string, Term>();
            int pos = 0;
            foreach (string word in splitedText)
            {
                string toLower = word.ToLower();
                if (toLower.Length > 1 && toLower != "between" && toLower != "and" )
                {

                    bool isNumber = Char.IsDigit(word[0]);
                    string term = (shouldStem && !isNumber) ? stm.stemTerm(word).ToLower() : word.ToLower();
                    if (thisDocVocabulary.ContainsKey(term))
                    {
                        if (isNumber)
                        {
                            thisDocVocabulary[term].addPosition(pos);
                        }
                        else
                        {
                            thisDocVocabulary[term].addPosition(pos, word[0]);
                        }
                    }
                    else
                    {
                        Term newTerm;
                        if (isNumber)
                        {
                             newTerm = new Term(term, pos);
                        }
                        else {
                             newTerm = new Term(term, pos, word[0]);
                        }
                        
                        thisDocVocabulary.Add(term, newTerm);
                    }
                    pos++;
                }
            }

            List<string> addTermsList = addedTerms.Keys.ToList();
            for (int i = 0; i < addTermsList.Count; i++)
            {
                string word = addTermsList[i];
                string toLower = word.ToLower();
                if (toLower.Length > 1 && toLower != "between" && toLower != "and")
                {

                    bool isNumber = Char.IsDigit(word[0]);
                    string term = (shouldStem && !isNumber) ? stm.stemTerm(word).ToLower() : word.ToLower();
                    if (thisDocVocabulary.ContainsKey(term))
                    {
                        if (isNumber)
                        {
                            for(int j = 0; j< addedTerms[addTermsList[i]].Count; j++)
                            {
                                thisDocVocabulary[term].addPosition(addedTerms[addTermsList[i]][j]);
                            }
                        }
                        else
                        {
                            for (int j = 0; j < addedTerms[addTermsList[i]].Count; j++)
                            {
                                thisDocVocabulary[term].addPosition(addedTerms[addTermsList[i]][j], word[0]);
                            }
                        }
                    }
                    else
                    {
                        Term newTerm;
                        if (isNumber)
                        {
                            newTerm = new Term(term, addedTerms[addTermsList[i]][0]);
                            thisDocVocabulary.Add(term, newTerm);

                            for (int j = 1; j < addedTerms[addTermsList[i]].Count ; j++)
                            {
                                thisDocVocabulary[term].addPosition(addedTerms[addTermsList[i]][j]);
                            }
                        }
                        else
                        {
                            newTerm = new Term(term, addedTerms[addTermsList[i]][0], word[0]);
                            thisDocVocabulary.Add(term, newTerm);

                            for (int j = 1; j < addedTerms[addTermsList[i]].Count; j++)
                            {
                                thisDocVocabulary[term].addPosition(addedTerms[addTermsList[i]][j], word[0]);
                            }
                        }

                        
                    }
                }
            }

            if (thisDocVocabulary != null && thisDocVocabulary.Count() > 0)
            {
                doc.uniqWords = thisDocVocabulary.Count();
                doc.max_tf = thisDocVocabulary.Values.OrderBy((Term) => Term.Tf).Last().Tf;
                doc.length = pos;
                
            }
            
            return thisDocVocabulary;
        }

      
    }

}
