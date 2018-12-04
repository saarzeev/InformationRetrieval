
using System.IO;
using System.Text;

namespace Model2
{
    class Class1
    {
        static void Main(string[] args)
        {
            // Parse parse = new Parse();
            //Parse.Instance().FromFilesToDocs(@"C:\MiniMiniCorpus", @"C:\MiniMiniCorpusOutput", @"C:\Users\Saar\source\repos\InformationRetrieval\stopwords.txt", false);
            ////parse.FromFilesToDocs(@"C:\Users\nastia\source\repos\saarzeev\corpus", @"C:\Users\nastia\source\repos\saarzeev\InformationRetrieval\stopwords.txt", false);
            ////string[] delimiters = { " - ", " ", "(", ")", "<", ">", "[", "]", "{", "}", "^", ";", "\"", "'", "`", "|", "*", "#", "+", "?", "!", "&", "@", "\\", "," };
            //City.initCities();
            string path = @"C:\Users\nastia\source\posting\{0}FINAL.gz";
            string filePath = @"C:\Users\nastia\source\{0}.csv";
            char c = 'a';
            for (int i = 0; i<=36; i++)
            {
                StringBuilder posting = PostingsSet.Unzip(File.ReadAllBytes(string.Format(path,c)));
                File.WriteAllText(filePath, posting.ToString());
                c++;
            }
           
            //need 3 more + other
           

           
            

        }
    }
}
