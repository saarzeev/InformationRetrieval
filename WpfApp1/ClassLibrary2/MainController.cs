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
        public void init(string sourcePath, string destination, bool stemming)
        {
            parser.FromFilesToDocs(sourcePath, destination, sourcePath + "\\stopwords.txt", stemming);
        }

    }
}
