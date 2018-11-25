using System.Collections.Generic;

namespace Model2
{
     class Term
    {
        private string _term;
        private int _tf;
        private bool _isIn100;
        private HashSet<int> positons;
      //??  private string type; //term - number - date - time - money ???
      //TODO all for posting

        public Term(string term, int firstPos)
        {
            this._term = term;
            _tf = 1;
            this._isIn100 = firstPos <= 100 ? true : false;
            positons = new HashSet<int>();
            positons.Add(firstPos);
        }

        public bool IsIn100 { get => _isIn100; }
        public int Tf { get => _tf; }
        public string GetTerm { get => _term; }
        public void addPosition(int pos)
        {
            if (!this.IsIn100 && pos <= 100 )
            {
                this._isIn100 = true;
            }
            this._tf++;
            this.positons.Add(pos);
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