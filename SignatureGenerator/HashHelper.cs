using System.Security.Cryptography;
using System.Text;

namespace SignatureGenerator
{
    public static class HashHelper
    {
        public static string CalculateSha256(byte[] input)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(input);
                
                var sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}