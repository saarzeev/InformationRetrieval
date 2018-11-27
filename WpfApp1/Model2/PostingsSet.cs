using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Model2
{
    class PostingsSet
    {
        private static int counter = 0;
        private Mutex mutex = new Mutex();
        private Dictionary<string, List<StringBuilder>> _termsDictionary = new Dictionary<string, List<StringBuilder>>(); //terms, postings
        private int id;
        private static PostingsSet ps;



        public PostingsSet()
        {
            id = counter;
            counter++;
        }
        
        public void Add(string term, StringBuilder posting)
        {
            mutex.WaitOne();
            if (_termsDictionary.ContainsKey(term))
            {
                List<>
            }
            mutex.ReleaseMutex();
        }
    }
}
