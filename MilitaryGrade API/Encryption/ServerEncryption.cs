using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using MilitaryGrade_API.Utilities;

namespace MilitaryGrade_API.Encryption;

/// <summary>
/// Custom class to handle decryption of data using the server's Private RSA asymmetric decryption key
/// </summary>
/// <remarks>Takes a limited input due to keyspace constraints</remarks>
public class ServerEncryption
{
    /// <summary>
    /// Handles decryption of data using the server's Private RSA asymmetric decryption key
    /// <br></br>
    /// Should only be called at the start of the session
    /// </summary>
    /// <param name="encrypted">Base64Decoded ciphertext, which we'll decrypt using the server private key</param>
    /// <returns>The decrypted plaintext of the given ciphertext (encrypted data)</returns>
    public string ManageDecryptionOfInitialHandshakeData(string encrypted)
    {
        try
        {
            //the string is still encrypted here, so we need to convert it to a byte[], then decrypt it, then convert it back to a string and do stuff with it
            return DecryptDataWithServerPrivateKey(EncodingUtils.StringToByteArray(encrypted));
        }
        catch (Exception)
        {
            return "";
        }
    }
    /// <summary>
    /// Decrypts the given data with the server's Private RSA asymmetric decryption key
    /// </summary>
    /// <param name="strInput"></param>
    /// <returns>The decrypted plaintext of the given ciphertext (encrypted data)</returns>
    private static string DecryptDataWithServerPrivateKey(byte[] strInput)
    {
        try
        {
            RSA? privateKey = GetPrivateKeyFromServer();

            if (privateKey != null)
            {
                //we now decrypt the user's input txt, and convert the result from byte[] back to a regular string
                //so this is now the decrypted version of their initial call
                return EncodingUtils.ByteArrayToString(privateKey.DecryptValue(strInput));
            }
        }
        catch
        {
        }
        return "";
    }

    /// <summary>
    /// Custom method to get the RSA X509 format certificate file which contains both the server Public and Private keys.
    /// <br></br>
    /// Generates an X509.2 format Certificate file with Public and Private RSA asymmetric encryption/decryption keys, if it doesn't exist
    /// </summary>
    /// <returns>
    /// Private key of server - either from the given location, or a new cert created at the given location
    /// </returns>
    /// <remarks>Please note that this method will replace any existing certs at the given location</remarks>
    private static RSA? GetPrivateKeyFromServer()
    {
        try
        {
            X509Certificate2 certificate = new X509Certificate2(ServerCertificatePaths.CertificatePath, ServerCertificatePaths.CertificatePassword);
            RSA? privateKey = certificate.GetRSAPrivateKey();
            if (privateKey != null)
            {
                return privateKey; //we actually have some kind of certificate and pk, so use that
            }

            //private key doesnt exist, so we need to gen an x509 and get the pk
            /// Very important Note: dont use this irl (it'll replace your existing cert if you point the ServerCertificatePaths.certificatePath to the same folder as the existing cert), find another workaround,
            /// I'm using it in mine because I'm localhosting and don't have an existing cert
            return X509CertGenerator.lifetimeX509Cert.GetRSAPrivateKey();
        }
        catch
        {
            return null;
        }
    }
}
