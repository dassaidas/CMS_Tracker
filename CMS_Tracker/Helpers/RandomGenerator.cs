using System.Security.Cryptography;
using System.Text;

namespace CMS_Tracker.Helpers
{
    public class RandomGenerator
    {
        public static string GeneratePassword(int length = 12)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%&*";
            var result = new StringBuilder();
            var buffer = new byte[1];

            while (result.Length < length)
            {
                RandomNumberGenerator.Fill(buffer);
                var num = buffer[0] % validChars.Length;
                result.Append(validChars[num]);
            }

            return result.ToString();
        }
    }
}
