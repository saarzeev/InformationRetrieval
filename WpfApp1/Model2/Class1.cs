
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Model2
{
    class Class1
    {
        static void Main(string[] args)
        {
            // Parse parse = new Parse();
            //City.initCities();
            //Parse.Instance().FromFilesToDocs(@"d:\documents\users\kovalkov\Downloads\mini", @"d:\documents\users\kovalkov\Downloads\miniOutput", @"d:\documents\users\kovalkov\Downloads\mini\stopwords.txt", false);
            //DateTime startingTime = DateTime.Now;
            Parse.Instance().FromFilesToDocs(@"C:\minicorpus",@"c:\minicorpusoutput", @"C:\minicorpus\stopwords.txt", false);
            //Console.WriteLine(DateTime.Now - startingTime);
            Searcher searcher = new Searcher(@"c:\minicorpusoutput", false);
            List<string> nu = new List<string>();
            nu.AddRange(new string[] { "fellow", "founder-states", "potala", "szymanski", "szymanski", "szymanski", "szymanski" });
            searcher.GetTermsPosting(nu);
            ////string[] delimiters = { " - ", " ", "(", ")", "<", ">", "[", "]", "{", "}", "^", ";", "\"", "'", "`", "|", "*", "#", "+", "?", "!", "&", "@", "\\", "," };
            //City.initCities();
           // string path = @"C:\Users\nastia\source\posting\{0}FINAL.gz";
            ////string filePath = @"C:\Users\nastia\source\{0}.csv";
            //Dictionary<string, int> tfs = new Dictionary<string, int>();
            //char c = 'a';
            //for (int i = 0; i<=26; i++)
            //{
            //    StringBuilder posting = PostingsSet.Unzip(File.ReadAllBytes(string.Format(path,c)));
            //    string[] splited = posting.ToString().Split('\n');
            //    for(int j = 0; j< splited.Length; j++)
            //    {

            //    }
            //    c++;
            //}
           //other!
            
           

           
            

        }
    }
}
