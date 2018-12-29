
using Newtonsoft.Json;
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
            //Parse.Instance().FromFilesToDocs(@"C:\minicorpus",@"c:\minicorpusoutput", @"C:\minicorpus\stopwords.txt", false);
            ////Console.WriteLine(DateTime.Now - startingTime);
            //Searcher searcher = new Searcher(@"c:\minicorpusoutput", false);
            //List<string> nu = new List<string>();
            //nu.AddRange(new string[] { "fellow", "founder-states", "potala", "szymanski", "szymanski", "szymanski", "szymanski" });
            //searcher.GetTermsPosting(nu);
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
            //string _currentFile = @"C:\Users\nastia\Documents\ppdb-2.0-m-lexical";
            //using (FileStream fs = new FileStream(_currentFile, FileMode.Open, FileAccess.Read))
            //{
            //    using (var fileStream = File.OpenRead(_currentFile))
            //    {
            //        using (var streamReader = new StreamReader(_currentFile, Encoding.ASCII))
            //        {
            //            Dictionary<string, int> numOfSim = new Dictionary<string, int>();
            //            Dictionary<string, List<KeyValuePair<string, double>>> simDict = new Dictionary<string, List<KeyValuePair<string, double>>>();
            //            string line;
            //            while (!streamReader.EndOfStream)
            //            {
            //                while ((line = streamReader.ReadLine()) != null)
            //                {
            //                    string[] curr = line.Split(new string[] { " ||| " }, StringSplitOptions.None);
            //                    string word = curr[1];
            //                    string otherWord = curr[2];
            //                    string scoreStr = curr[3].Split(' ')[0].Split('=')[1];
            //                    Double.TryParse(scoreStr, out double score);

            //                    if (!numOfSim.ContainsKey(word))
            //                    {
            //                        numOfSim[word] = 0;
            //                        simDict[word] = new List<KeyValuePair<string, double>>();
            //                    }
            //                    if (numOfSim[word] < 3 && score >= 3.7)
            //                    {
            //                        bool alreadyHas = false;
            //                        foreach(KeyValuePair<string, double> pair in simDict[word])
            //                        {
            //                            if(otherWord == pair.Key || otherWord == word)
            //                            {
            //                                alreadyHas = true;
            //                                break;
            //                            }
            //                        }

            //                        if (!alreadyHas)
            //                        {
            //                            simDict[word].Add(new KeyValuePair<string, double>(otherWord, score));
            //                            numOfSim[word] += 1;
            //                        }
            //                    }
            //                }
            //            }
            //            string json = JsonConvert.SerializeObject(simDict, Formatting.Indented);
            //            File.WriteAllText(@"C:\Users\nastia\Documents\semantics.json", json);
            //        }
            //    }
            //}

            /*
             *   
                         if numOfSim[word] < 3:
                             tup = (otherWord, float(score))
                             similarityDict[word].append(tup)
                             numOfSim[word] = numOfSim[word] + 1
                     with open("simDictS.json", "w+") as tmpFileDict: #change to your favorite type of file
                         json.dump(similarityDict, tmpFileDict)
                         tmpFileDict.close()
                     print(similarityDict["car"])
                     print(similarityDict["link"])
                     print(similarityDict["iran"])
             */




            //                    }
            //    }
        }
    }
}
