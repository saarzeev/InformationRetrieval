
namespace Model2
{
    class Class1
    {
        static void Main(string[] args)
        {
            // Parse parse = new Parse();
            Parse.Instance().FromFilesToDocs(@"C:\Corpus", @"C:\CorpusOutput", @"C:\Users\Saar\source\repos\InformationRetrieval\stopwords.txt", true);
            //parse.FromFilesToDocs(@"C:\Users\nastia\source\repos\saarzeev\corpus", @"C:\Users\nastia\source\repos\saarzeev\InformationRetrieval\stopwords.txt", false);
            //string[] delimiters = { " - ", " ", "(", ")", "<", ">", "[", "]", "{", "}", "^", ";", "\"", "'", "`", "|", "*", "#", "+", "?", "!", "&", "@", "\\", "," };
            City.initCities();
        }
    }
}
