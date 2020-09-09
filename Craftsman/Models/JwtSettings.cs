namespace Craftsman.Models
{
    using System;
    using System.Security.Cryptography;

    public class JwtSettings
    {
        public JwtSettings()
        {
            Key ??= GenerateRandomKey();
        }

        public string Key { get; set; }
        public string Issuer { get; set; } = "CoreIdentity";
        public string Audience { get; set; } = "CoreIdentityUser";
        public int DurationInMinutes { get; set; } = 60;

        private string GenerateRandomKey()
        {
            var key = new byte[32];
            RNGCryptoServiceProvider.Create().GetBytes(key);
            var base64Secret = Convert.ToBase64String(key);
            // make safe for url
            var urlEncoded = base64Secret.TrimEnd('=').Replace('+', '-').Replace('/', '_');

            return urlEncoded;
        }
    }
}
