using System.Collections.Generic;
using System.Text;

namespace Doge
{
    class Casino
    {
        public string Name;
        public string RequestAddress;
        public string RequestText;
        public List<string> Options;
        public string ParseText;

        public Casino(string s)
        {
            Options = new List<string>();
            var split = s.Split(';');
            foreach (var x in split)
            {
                var sb = new StringBuilder(x);
                if (x.Contains("name:"))
                    Name = sb.Replace("name:", "").ToString();
                else if (x.Contains("request:"))
                    RequestAddress = sb.Replace("request:", "").ToString();
                else if (x.Contains("options:"))
                    foreach (var y in sb.Replace("options:", "").ToString().Split(','))
                        Options.Add(y);
                else if (x.Contains("parse:"))
                    ParseText = sb.Replace("parse:", "").ToString();
                else if (x.Contains("format:"))
                    RequestText = sb.Replace("format:", "").ToString();

            }
        }
    }
}
