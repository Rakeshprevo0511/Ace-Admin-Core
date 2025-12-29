
using System.Security.Cryptography;

namespace Ace_Admin.Models
{
    public class RsaKeyGenerator
    {
        public static void GenerateKeys(string privateKeyPath, string publicKeyPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(privateKeyPath)!);

            // ✅ Skip if already exist
            if (File.Exists(privateKeyPath) && File.Exists(publicKeyPath))
                return;

            using (var rsa = RSA.Create(2048))
            {
                var privateKeyPem = rsa.ExportRSAPrivateKeyPem();
                File.WriteAllText(privateKeyPath, privateKeyPem);

                var publicKeyPem = rsa.ExportRSAPublicKeyPem();
                File.WriteAllText(publicKeyPath, publicKeyPem);
            }

            Console.WriteLine("✅ RSA keys generated successfully!");
        }
    }
}
