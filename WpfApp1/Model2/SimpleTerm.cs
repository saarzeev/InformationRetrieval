using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model2
{
    class SimpleTerm
    {
        private string _term;
        private bool _isLowerCase;
        private string _postingPath;

        public SimpleTerm(string term, string path, bool isLower)
        {
            this._term = term;
            this._isLowerCase = isLower;
            this._postingPath = path;
        }
    
        public string GetTerm { get => _term; set => _term = value; }
        public string PostingPath { get => _postingPath; }
        public bool IsLowerCase { get => _isLowerCase; set => _isLowerCase = value; }

        override
        public int GetHashCode()
        {
            return this._term.GetHashCode();
        }

        public bool Equals(string term)
        {
            return this._term.Equals(term);
        }

        public bool Equals(Term term)
        {
            return this._term.Equals(term.GetTerm);
        }

        public int CompareTo(Term term)
        {
            return this._term.CompareTo(term.GetTerm);
        }
        public int CompareTo(string term)
        {
            return this._term.CompareTo(term);
        }
    }
}

