using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace MilitaryGrade_API.Encryption; 
using MilitaryGrade_API.Utilities;

/// <summary>
/// Custom class to automatically generate an X509 format Certificate file with Public and Private RSA asymmetric encryption/decryption keys
/// </summary>
public class X509CertGenerator
{
    #region Members
    public static X509Certificate2? lifetimeX509Cert = null;
    #endregion Members

    #region Generate RSA Cert
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
    #endregion Generate RSA Cert

    #region Constructor
    /// <summary>
    /// Automatically generates an X509 format Certificate file with Public and Private RSA asymmetric encryption/decryption keys
    /// </summary>
    public X509CertGenerator()
    {
        //auto generate a cert for us when we create an instance object of this class
        CreateRSACertificate();
    }
    #endregion Constructor

    #region Get Methods
    /// <summary>
    /// Gets the public asymmetric encryption key of the server in a Base64 Encoded format
    /// </summary>
    /// <returns>Base64 Encoded Public key of server</returns>
    public static string GetServerPublicKey()
    {
        return EncodingUtils.ByteArrayToBase64String(lifetimeX509Cert.GetPublicKey());
    }

    /// <summary>
    /// Gets the newly generated X509.2 certificate file
    /// </summary>
    /// <returns>The system generated - self-signed - X509.2 certificate file</returns>
    public X509Certificate2 GetCertificate()
    {
        return (lifetimeX509Cert != null) ? lifetimeX509Cert : null;
    }

    /// <summary>
    /// Gets the server's private decryption key for decryption of Asymmetrically encrypted data
    /// </summary>
    /// <remarks>the key is stored as an RSA object</remarks>
    /// <returns></returns>
    public static RSA GetServerPrivateKey()
    {
        return lifetimeX509Cert.GetRSAPrivateKey();
    }
    #endregion Get Methods
}
