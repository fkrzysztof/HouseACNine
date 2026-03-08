using System.Security.Cryptography;

namespace HouseNet9.Helpers
{
    public static class ReservationNumberGenerator
    {
        private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        public static string Generate()
        {
            var bytes = new byte[6];
            RandomNumberGenerator.Fill(bytes);

            var result = new char[6];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Chars[bytes[i] % Chars.Length];
            }

            var date = DateTime.Now.ToString("yyMMdd");

            return $"RH-{date}-{new string(result)}";
        }
    }
}
