using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model2
{
    class Doc
    {
        public string _path { get; }
        public string _text { get; }
        private long _pos { get; }
   
    public Doc(string path, string text, long pos)
        {
            this._path = path;
            this._text = text;
            this._pos = pos; 
        }
    }
}
