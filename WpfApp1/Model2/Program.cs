using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model2
{
    class Program
    {
        static void Main(string[] args)
        {
            FileReader fr = new FileReader("C:\\corpus");
            while (fr.HasNext()) { }
                //Console.WriteLine( fr.ReadNextDoc());
        }
    }
}
