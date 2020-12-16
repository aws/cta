using System.Security.Cryptography;
using System.Text;

namespace CTA.Rules.Config
{
    public class EncryptionHelper
    {
        private static SHA256 _sha256;
        private static SHA256 SHA256Hash
        {
            get
            {
                _sha256 ??= SHA256.Create();
                
                return _sha256;
            }
        }
        public static string ConvertToSHA256Hex(string toEncrypt)
        {
            // Convert the input string to a byte array and compute the hash.
            var encryptedBytes = SHA256Hash.ComputeHash(Encoding.UTF8.GetBytes(toEncrypt));

            var sBuilder = new StringBuilder();
            foreach (var encryptedByte in encryptedBytes)
            {
                var hexString = encryptedByte.ToString("x2");
                sBuilder.Append(hexString);
            }

            return sBuilder.ToString();
        }
    }
}