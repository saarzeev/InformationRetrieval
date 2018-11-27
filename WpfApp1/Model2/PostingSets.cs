using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Model2
{
    class PostingSets
    {
        class PostingsSet
        {

            private static int counter = 0;
            private Mutex mutex = new Mutex();
            private Dictionary<string, List<Posting>> _termsDictionary = new Dictionary<string, List<Posting>>(); //terms, postings
            private int id;
            private static PostingsSet ps;
            private int capacity = 5000;
            private string _path = "";



            public PostingsSet(string destPath)
            {
                id = counter;
                _path = destPath;
                counter++;
            }

            public void Add(string term, Posting posting)
            {
                mutex.WaitOne();
                if (hasCapacity())
                {
                    if (_termsDictionary.ContainsKey(term))
                    {
                        _termsDictionary[term].Add(posting);
                    }
                    else
                    {
                        List<Posting> newList = new List<Posting>();
                        newList.Add(posting);
                        _termsDictionary.Add(term, newList);
                        capacity--;
                    }
                }
                mutex.ReleaseMutex();
            }

            public bool hasCapacity()
            {
                return capacity > 0;
            }
        }
    }
}
