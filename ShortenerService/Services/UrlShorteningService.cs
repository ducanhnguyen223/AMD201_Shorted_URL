using System.Linq; 

namespace ShortenerService.Services
{
    public interface IUrlShorteningService
    {
        string GenerateShortCode(long id);
    }

    public class UrlShorteningService : IUrlShorteningService
    {
        private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly int Base = Alphabet.Length;

        public string GenerateShortCode(long id)
        {
            if (id == 0) return Alphabet[0].ToString();

            var s = string.Empty;
            while (id > 0)
            {
                s += Alphabet[(int)(id % Base)];
                id /= Base;
            }

            // Logic .Reverse() của bạn là đúng
            return string.Join(string.Empty, s.Reverse());
        }
    }
}