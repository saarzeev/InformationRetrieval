using Model2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controllers
{
    public class MainController
    {
        Model2.Parse parser;
        Model2.Indexer indexer;
        public Dictionary<string, string> laguagesD;

        public string[] init(string sourcePath, string destination, bool stemming)
        {
            DateTime start = DateTime.Now;
            parser = Model2.Parse.Instance();
            laguagesD = parser.laguagesD;
            indexer = Model2.Indexer.Instance(destination, stemming);
            parser.FromFilesToDocs(sourcePath, destination, sourcePath + "\\stopwords.txt", stemming);
            string totalTime = (DateTime.Now - start).TotalSeconds.ToString();
            string[] values = { totalTime, indexer.docsCount.ToString(), indexer.termCount.ToString() };
            return values;
        }

        public void DestructSingletons()
        {
            Model2.Parse.DestructParse();
            Model2.Indexer.DestructIndexer();
        }

        public void LoadDictionary( string destination, bool stemming)
        {
            indexer = Model2.Indexer.Instance(destination, stemming);
            indexer.LoadDictionary();
        }

        public void getDictionary()
        {
             indexer.GetDictionary();
        }

        public void reset(string path)
        {
            if (indexer == null)
            {
                indexer = Model2.Indexer.Instance(path, true);
            }
            indexer.Reset();
            Model2.Parse.DestructParse();
            Model2.Indexer.DestructIndexer();
            indexer = null;
            parser = null;
        }

        public Dictionary<string, List<CityPosting>> getCities(string path, bool isStemming)
        {
          
            if (indexer != null)
            {
                if(indexer.currenPostingSet != null)
                {
                    if (indexer.currenPostingSet.CitiesDictionary != null && indexer.currenPostingSet.CitiesDictionary.Count > 0)
                    {
                        return indexer.currenPostingSet.CitiesDictionary;
                    }
                }

                indexer.LoadCitiesDictionary();
                return indexer.currenPostingSet.CitiesDictionary;
            }
            indexer = Indexer.Instance(path, isStemming);
            indexer.LoadCitiesDictionary();
            return indexer.currenPostingSet.CitiesDictionary;
        }

        public Dictionary<string,string> getLaguages()
        {
            Dictionary < string,string> languages = indexer.getLaguages();
            Model2.Indexer.DestructIndexer();
            return languages;
        }
    }
}
