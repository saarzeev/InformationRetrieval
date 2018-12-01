

namespace Model2
{
   public class SimpleTerm
    {
        private string _term;
        private bool _isLowerCase;
        private string _postingPath;
        private int _df;

        public SimpleTerm(string term, string path, bool isLower)
        {
            this._term = term;
            this._isLowerCase = isLower;
            this._postingPath = path;
            this.Df = 1;
        }

        public SimpleTerm(string allTerm)
        {
            string[] splited = allTerm.Split(',');
            _term = splited[0];
            int df;
            int.TryParse(splited[1], out df);
            Df = df;
            _postingPath = splited[2];
            _isLowerCase = splited[3] == "1" ? true : false;
        }

        public string GetTerm { get => _term; set => _term = value; }
        public string PostingPath { get => _postingPath; set => _postingPath = value; }
        public bool IsLowerCase { get => _isLowerCase; set => _isLowerCase = value; }
        public int Df { get => _df; set => _df = value; }

        override
        public int GetHashCode()
        {
            return this._term.GetHashCode();
        }

        override
        public string ToString()
        {
            string isLower = _isLowerCase ? "1" : "0";
            return _term + "," + Df + "," + _postingPath + "," + isLower;
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

