using System.Collections.Generic;

namespace Model2
{
     public class Term
    {
        private string _term;
        private int _tf;
        private bool _isIn100;
        private bool _isLowerCase;
        private HashSet<int> _positons;

        public Term(string term, int firstPos, char firstChar)
        {
            this._term = term;
            _tf = 1;
            this._isIn100 = firstPos <= 100 ? true : false;
            _positons = new HashSet<int>();
            _positons.Add(firstPos);
            _isLowerCase = char.IsLower(firstChar);
        }
        public Term(string term, int firstPos)
        {
            this._term = term;
            _tf = 1;
            this._isIn100 = firstPos <= 100 ? true : false;
            _positons = new HashSet<int>();
            _positons.Add(firstPos);
            _isLowerCase = true;
        }

        public bool IsIn100 { get => _isIn100; }
        public int Tf { get => _tf; }
        public string GetTerm { get => _term; }
        public bool IsLowerCase { get => _isLowerCase; }
        public HashSet<int> Positons { get => _positons;}

        public void addPosition(int pos, char firstChar)
        {
            if (!this.IsIn100 && pos <= 100 )
            {
                this._isIn100 = true;
            }
            this._tf++;
            this._positons.Add(pos);
            if (!this._isLowerCase)
            {
                _isLowerCase = char.IsLower(firstChar);
            }
        }
        public void addPosition(int pos)
        {
            if (!this.IsIn100 && pos <= 100)
            {
                this._isIn100 = true;
            }
            this._tf++;
            this._positons.Add(pos);
        }
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