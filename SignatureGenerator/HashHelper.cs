using System.Security.Cryptography;
using System.Text;

namespace SignatureGenerator
{
    public static class HashHelper
    {
        public static string CalculateSha256(this SHA256 sha256, byte[] input)
        {
            var hash = sha256.ComputeHash(input);
                
            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}