using Newtonsoft;
namespace Model2
{
    
   public class SimpleTerm
    {
       [Newtonsoft.Json.JsonIgnore]
        public string _term { get; }
        public bool _isLowerCase;
        public string _postingPath;
        public int _df;
        public int _tf;

        public SimpleTerm(string term, string path, bool isLower, int tf = 0)
        {
            //this._term = term;
            this._isLowerCase = isLower;
            this._postingPath = path;
            this.df = 1;
            this.tf = tf;
        }

        public SimpleTerm(string allTerm)
        {
            string[] splited = allTerm.Split(',');
          //  _term = splited[0];
            int.TryParse(splited[1], out int df);
            df = df;
            int.TryParse(splited[2], out int tf);
            tf = tf;
            _postingPath = splited[3];
            _isLowerCase = splited[4] == "1" ? true : false;
        }

        public void addTf(int tf)
        {
            this.tf += tf;
        }

     //   public string GetTerm { get => _term; set => _term = value; }
        public string path { get => _postingPath; set => _postingPath = value; }
        public bool lower { get => _isLowerCase; set => _isLowerCase = value; }
        public int df { get => _df; set => _df = value; }
        public int tf { get => _tf; set => _tf = value; }

        override
       // public int GetHashCode()
       // {
       ////     return this._term.GetHashCode();
       // }

        //override
        public string ToString()
        {
            string isLower = _isLowerCase ? "1" : "0";
            System.Text.StringBuilder str = new System.Text.StringBuilder();
            string toString = str.AppendFormat("{0},{1},{2},{3},{4}", _term, df, tf, _postingPath, isLower).ToString();
            str = null;
            return toString;
        }
        //public bool Equals(string term)
        //{
        //    return this._term.Equals(term);
        //}

        //public bool Equals(Term term)
        //{
        //    return this._term.Equals(term.GetTerm);
        //}

        //public int CompareTo(Term term)
        //{
        //    return this._term.CompareTo(term.GetTerm);
        //}
        //public int CompareTo(string term)
        //{
        //    return this._term.CompareTo(term);
        //}
    }
}

