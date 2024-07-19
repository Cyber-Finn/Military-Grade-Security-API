using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace MilitaryGrade_API.Encryption; 
using MilitaryGrade_API.Utilities;

/// <summary>
/// Custom class to automatically generate an X509 format Certificate file with Public and Private RSA asymmetric encryption/decryption keys
/// </summary>
public class X509CertGenerator
{
    public static X509Certificate2? lifetimeX509Cert = null;
    public static X509Certificate2 CreateRSACertificate()
    {
        // Generate RSA key pair
        using (var rsa = RSA.Create(2048))
        {
            var request = new CertificateRequest("CN=CyberFinn", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            // Set certificate validity period (adjust as needed)
            //request.NotBefore = DateTime.UtcNow;
            //request.NotAfter = request.NotBefore.AddYears(1);

            // Create self-signed certificate with expiry (Usually 1 year by default for most sites)
            var certificate = request.CreateSelfSigned(DateTime.UtcNow, DateTime.UtcNow.AddYears(1));

            lifetimeX509Cert = new X509Certificate2(certificate.Export(X509ContentType.Pfx));

            //ensure that our folder exists
            if (!Directory.Exists(ServerCertificatePaths.ServerConfigFolderPath))
            {
                Directory.CreateDirectory(ServerCertificatePaths.ServerConfigFolderPath);
            }

            //actually export the cert to a location on our server/pc
            File.WriteAllBytes(ServerCertificatePaths.CertificatePath, certificate.Export(X509ContentType.Pfx, ServerCertificatePaths.CertificatePassword));

            return lifetimeX509Cert;
        }
    }

    public static string Test()
    {
        var certificate = CreateRSACertificate();
        // Display public key
        string message = $"Public Key: {Convert.ToBase64String(certificate.GetPublicKey())}" + $" . Private Key: {Convert.ToBase64String(certificate.PrivateKey.ExportPkcs8PrivateKey())}"
            + $" . Certificate Thumbprint: {certificate.Thumbprint}";

        return message;
    }

    public X509Certificate2 GetCertificate()
    {
        return (lifetimeX509Cert != null) ? lifetimeX509Cert : null;
    }
    /// <summary>
    /// Automatically generates an X509 format Certificate file with Public and Private RSA asymmetric encryption/decryption keys
    /// </summary>
    public X509CertGenerator()
    {
        //auto generate a cert for us when we create an instance object of this class
        CreateRSACertificate();
    }

    public static string GetServerPublicKey()
    {
        return EncodingUtils.ByteArrayToBase64String(lifetimeX509Cert.GetPublicKey());
    }

    /// <summary>
    /// the key is stored as an RSA object, which we'll use to decrypt the input on server-side
    /// </summary>
    /// <returns></returns>
    public static RSA GetServerPrivateKey()
    {
        return lifetimeX509Cert.GetRSAPrivateKey();
        ;
    }
}
