using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simulator
{
    public static class StringUtil
    {
        public static List<int> StringToList(string str, char separator = ',')
        {
            var list = new List<int>();
            if (string.IsNullOrEmpty(str))
                return list;

            var split = str.Split(separator);
            foreach (var s in split)
            {
                if (string.IsNullOrEmpty(s))
                    continue;

                list.Add(int.Parse(s));
            }
            return list;
            //return new List<int>(str.Split(separator).Select(s => int.Parse(s)).ToArray());
        }

        public static Dictionary<int, int> StringToDictionary(string str, char separator = ',')
        {
            var dict = new Dictionary<int, int>();
            if (string.IsNullOrEmpty(str))
                return dict;

            str = str.Trim();

            var arr = str.Split(separator).Select(s => int.Parse(s)).ToArray();
            for (int i = 0; i < arr.Length; i += 2)
            {
                int key = arr[i];
                int value = arr[i + 1];
                dict.Add(key, value);
            }

            return dict;
        }

        public static Dictionary<string, int> StringToStringDictionary(string str, char separator = ',')
        {
            var dict = new Dictionary<string, int>();
            if (string.IsNullOrEmpty(str))
                return dict;

            var arr = str.Split(separator);
            for (int i = 0; i < arr.Length; i += 2)
            {
                string key = arr[i];
                int value = int.Parse(arr[i + 1]);
                if (dict.ContainsKey(key))
                    dict[key] += value;
                else
                    dict.Add(key, value);
            }

            return dict;
        }

        public static string DictionaryToString(Dictionary<int, int> dict, char separator = ',')
        {
            if (dict.Count == 0)
                return "";

            StringBuilder sb = new StringBuilder();
            foreach (var pair in dict)
            {
                sb.AppendFormat("{0}{1}{2}{3}", pair.Key, separator, pair.Value, separator);
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        // https://stackoverflow.com/questions/11454004/calculate-a-md5-hash-from-a-string
        public static string CreateMD5(string input)
        {
            byte[] encodedInput = new UTF8Encoding().GetBytes(input);
            // need MD5 to calculate the hash
            byte[] hash = ((System.Security.Cryptography.HashAlgorithm)System.Security.Cryptography.CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedInput);

            // string representation (similar to UNIX format)
            string encoded = System.BitConverter.ToString(hash)
               // without dashes
               .Replace("-", string.Empty)
               // make lowercase
               .ToLower();
            return encoded;
        }
    }
}
