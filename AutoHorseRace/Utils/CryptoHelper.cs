using System.Security.Cryptography;
using System.Text;

namespace AutoHorseRace.Utils
{
    public static class CryptoHelper
    {
        private static string Sha1(string input)
        {
            using var sha1 = SHA1.Create();
            var bytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        public static string EncryptPassword(string password, string username, string token, string captcha)
        {
            string s1 = Sha1(password);
            string s2 = Sha1("voodoo_people_" + username + s1);
            return Sha1(token + captcha + s2);
        }

        public static string EncryptPin(string username, string pin, string r1, string r2)
        {
            string s1 = Sha1("pin_" + username + pin);
            return Sha1(r1 + r2 + s1);
        }
    }
}
