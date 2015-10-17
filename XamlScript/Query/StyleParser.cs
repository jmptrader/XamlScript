using System.Collections.Generic;
using System.Linq;

namespace XamlQuery
{
    internal class StyleParser
    {
        public static KeyValuePair<string, string> Parse(string src)
        {
            var arr = src.Split(':').Select(i => i.Trim()).ToList();
            return new KeyValuePair<string, string>(arr[0], arr[1]);
        }
    }
}