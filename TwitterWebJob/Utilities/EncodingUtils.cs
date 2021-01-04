using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterWebJob
{
    public class TwitterBotEncoding
    {
        public static string PercentEncode3986(string input)
        {
            // List containing all byte values that don't need to be escaped.
            // Source: https://dev.twitter.com/docs/auth/percent-encoding-parameters
            byte[] nonEscaped =
            {
                // Digits
                0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39,

                // Uppercase Letters
                0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D,
                0x4E, 0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A,

                // Lowercase Letters
                0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A, 0x6B, 0x6C, 0x6D,
                0x6E, 0x6F, 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A,

                // Reserved Characters
                0x2D, 0x2E, 0x5F, 0x7E
            };

            byte[] bytes = Encoding.UTF8.GetBytes(input);
            List<byte> output = new List<byte>();

            foreach (byte b in bytes)
            {
                if (nonEscaped.Contains(b))
                {
                    output.Add(b);
                }
                else
                {
                    // add percent char
                    output.Add(0x25);

                    // encode first and last half of byte
                    int first = b & 0x0F;
                    int last = b >> 4;
                    output.Add(Convert.ToByte(last.ToString("X")[0]));
                    output.Add(Convert.ToByte(first.ToString("X")[0]));
                }
            }

            return Encoding.UTF8.GetString(output.ToArray());
        }

        static Uri ForceDoubleEncodeSafeChars(string url)
        {
            var encodingReplaceMap = new Dictionary<string, string>
            {
                { "%2A", "%252A" },
                { "%21", "!" },
                { "%29", "%2529" },
                { "%28", "%2528" },
                { "%2E", "%252E" },
                { "%5F", "%255F" },
                { "%2D", "%252D" },
            };

            var uri = new Uri(url);
            if (uri.AbsoluteUri != uri.OriginalString)
            {
                foreach (KeyValuePair<string, string> entry in encodingReplaceMap)
                {
                    url = url.Replace(entry.Key, entry.Value);
                }
                return new Uri(url);
            }
            return uri;
        }
    }
}
