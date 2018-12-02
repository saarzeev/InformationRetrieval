using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controllers
{
    public class MainController
    {
       

        Model2.Parse parser =  Model2.Parse.Instance();
        Model2.Indexer indexer;
        public void init(string sourcePath, string destination, bool stemming)
        {
            parser.FromFilesToDocs(sourcePath, destination, sourcePath + "\\stopwords.txt", stemming);
        }

        public void LoadDictionary( string destination, bool stemming)
        {
            indexer = Model2.Indexer.Instance(destination, stemming);
            indexer.LoadDictionery();
        }

        public /*object*/void  getDictionary(string destination, bool stemming)
        {
            if (indexer == null )
            {
                //return null;
            }
            else indexer.getDictionary();
        }

        public void reset(string path)
        {
            if(indexer == null)
            {
                indexer = Model2.Indexer.Instance(path, true);
            }
            indexer.reset();
            indexer = null;
            parser = null;
            GC.Collect();
        }
    }
}
