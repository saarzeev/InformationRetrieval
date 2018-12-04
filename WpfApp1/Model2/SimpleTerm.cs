

namespace Model2
{
   public class SimpleTerm
    {
        private string _term;
        private bool _isLowerCase;
        private string _postingPath;
        private int _df;
        private int _tf;

        public SimpleTerm(string term, string path, bool isLower, int tf = 0)
        {
            this._term = term;
            this._isLowerCase = isLower;
            this._postingPath = path;
            this.Df = 1;
            this.Tf = tf;
        }

        public SimpleTerm(string allTerm)
        {
            string[] splited = allTerm.Split(',');
            _term = splited[0];
            int df = (int)Parse.QuickDoubleParse(splited[1]);
           // int.TryParse(splited[1], out df);
            Df = df;
            int tf = (int)Parse.QuickDoubleParse(splited[2]);
            Tf = tf;
            _postingPath = splited[3];
            _isLowerCase = splited[4] == "1" ? true : false;
        }

        public void addTf(int tf)
        {
            this.Tf += tf;
        }

        public string GetTerm { get => _term; set => _term = value; }
        public string PostingPath { get => _postingPath; set => _postingPath = value; }
        public bool IsLowerCase { get => _isLowerCase; set => _isLowerCase = value; }
        public int Df { get => _df; set => _df = value; }
        public int Tf { get => _tf; set => _tf = value; }

        override
        public int GetHashCode()
        {
            return this._term.GetHashCode();
        }

        override
        public string ToString()
        {
            string isLower = _isLowerCase ? "1" : "0";
            return _term + "," + Df + "," + Tf +"," + _postingPath + "," + isLower;
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

