using System.Security.Cryptography;
using System.Text;

namespace Payoo.Lib
{
    public class PayooCommon
    {
        public PayooCommon() { }
        public string EncryptSHA512(string input)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);
            using (SHA512 hash = System.Security.Cryptography.SHA512.Create())
            {
                byte[] hashedInputBytes = hash.ComputeHash(bytes);

                // Convert to text
                // StringBuilder Capacity is 128, because 512 bits / 8 bits in byte * 2 symbols for byte 
                StringBuilder hashedInputStringBuilder = new System.Text.StringBuilder(128);
                foreach (byte b in hashedInputBytes)
                {
                    hashedInputStringBuilder.Append(b.ToString("X2"));
                }
                return hashedInputStringBuilder.ToString();
            }
        }

    }
}