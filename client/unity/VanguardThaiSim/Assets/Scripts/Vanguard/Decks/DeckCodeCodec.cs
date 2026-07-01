using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace VanguardThaiSim.Decks
{
    public static class DeckCodeCodec
    {
        public const string Prefix = "VGTH1.";

        public static string Export(VanguardDeck deck)
        {
            if (deck == null)
            {
                throw new ArgumentNullException(nameof(deck));
            }

            byte[] jsonBytes = Encoding.UTF8.GetBytes(deck.ToJson(false));
            byte[] compressed = Compress(jsonBytes);
            return Prefix + ToBase64Url(compressed);
        }

        public static VanguardDeck Import(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || !code.StartsWith(Prefix, StringComparison.Ordinal))
            {
                throw new FormatException("Deck code must start with " + Prefix);
            }

            string payload = code.Substring(Prefix.Length);
            byte[] compressed = FromBase64Url(payload);
            byte[] jsonBytes = Decompress(compressed);
            string json = Encoding.UTF8.GetString(jsonBytes);
            return VanguardDeck.FromJson(json);
        }

        private static byte[] Compress(byte[] input)
        {
            using (MemoryStream output = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(output, CompressionMode.Compress))
                {
                    gzip.Write(input, 0, input.Length);
                }

                return output.ToArray();
            }
        }

        private static byte[] Decompress(byte[] input)
        {
            using (MemoryStream source = new MemoryStream(input))
            using (GZipStream gzip = new GZipStream(source, CompressionMode.Decompress))
            using (MemoryStream output = new MemoryStream())
            {
                gzip.CopyTo(output);
                return output.ToArray();
            }
        }

        private static string ToBase64Url(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static byte[] FromBase64Url(string value)
        {
            string base64 = value.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2:
                    base64 += "==";
                    break;
                case 3:
                    base64 += "=";
                    break;
            }

            return Convert.FromBase64String(base64);
        }
    }
}
