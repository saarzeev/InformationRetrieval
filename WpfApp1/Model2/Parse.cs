using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Model2
{

    class Parse
    {
        static private ConcurrentQueue<Doc> _docs = new ConcurrentQueue<Doc>();
        static private SemaphoreSlim _semaphore1 = new SemaphoreSlim(0, 10);
        static private SemaphoreSlim _semaphore2 = new SemaphoreSlim(10, 10);
        static private Mutex mutex = new Mutex();
        static private bool done = false;
        static void Main(string[] args)
        {
            //FileReader fr = new FileReader("C:\\Users\\nastia\\source\\repos\\saarzeev\\corpus");
            //_semaphore.Release(11);
            //while (fr.HasNext()) { }
            //Console.WriteLine( fr.ReadNextDoc());
            Parser("C:\\Users\\nastia\\source\\repos\\saarzeev\\corpus");
        }

        public static void Parser(string path)
        {
            Task task;

            FileReader fr = new FileReader(path);
            task = Task.Run(() => {

                while (fr.HasNext())
                {
                    _semaphore2.Wait();
                    Doc doc = fr.ReadNextDoc();
                    if (doc != null)
                    {
                        _docs.Enqueue(doc);
                        _semaphore1.Release();
                    }
                    else
                    {
                        _semaphore2.Release();
                    }
                }
                done = true;
            });

            while (!done) {
                _semaphore1.Wait();
                Doc currentDoc;
                _docs.TryDequeue(out currentDoc);
                _semaphore2.Release();
            }

            task.Wait();
        }

        private static void ParseNumbers(Doc doc)
        {

        }
    }

  

}
