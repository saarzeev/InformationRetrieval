using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controllers
{
    public class MainController
    {
        Model2.Parse parser;
        Model2.Indexer indexer;
        public string[] init(string sourcePath, string destination, bool stemming)
        {
            DateTime start = DateTime.Now;
            parser = Model2.Parse.Instance();
            indexer = Model2.Indexer.Instance(destination, stemming);
            parser.FromFilesToDocs(sourcePath, destination, sourcePath + "\\stopwords.txt", stemming);
            string totalTime = (DateTime.Now - start).TotalSeconds.ToString();
            string[] values = { totalTime, Model2.Indexer.docsIndexer.Count().ToString(), Model2.Indexer.fullDictionary.Count().ToString() };
            Model2.Parse.DestructParse();
            return values;
        }

        public void LoadDictionary( string destination, bool stemming)
        {
            indexer = Model2.Indexer.Instance(destination, stemming);
            indexer.LoadDictionery();
        }

        public void getDictionary(string destination, bool stemming)
        {
             indexer.getDictionary();
        }

        public void reset(string path)
        {
            if (indexer == null)
            {
                indexer = Model2.Indexer.Instance(path, true);
            }
            indexer.reset();
            Model2.Parse.DestructParse();
            Model2.Indexer.DestructIndexer();
            indexer = null;
            parser = null;
        }

        public Dictionary<string,string> getLaguages()
        {
            Dictionary < string,string> languages = indexer.getLaguages();
            Model2.Indexer.DestructIndexer();
            return languages;
        }
    }
}
