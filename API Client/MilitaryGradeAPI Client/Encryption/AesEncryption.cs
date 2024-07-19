using MilitaryGradeAPI_Client.Utilities;
using System.Security.Cryptography;
using System.Text;

namespace MilitaryGradeAPI_Client.Encryption;

public class AesEncryption
{
    #region Handler Methods
    /// <summary>
    /// A "publicly" accessible method that allows other parts of our app to just pass a string and get the encrypted version back
    /// </summary>
    /// <param name="s"></param>
    /// <param name="serverKey"></param>
    /// <returns>Base64 encoded Payload encrypted with the server's public asymmetric encryption key</returns>
    public string Handler_EncryptionWithServerKey(string s, string serverKey)
    {
        return (EncryptWithRSA(s, EncodingUtils.Base64StringToByteArray(serverKey)));
    }
    
    /// <summary>
    /// A "publicly" accessible method that allows other parts of our app to just pass a string and get the encrypted version back
    /// </summary>
    /// <param name="s"></param>
    /// <param name="statemanager"></param>
    /// <returns>Payload encrypted with the given symmetric key and initialization vector</returns>
    public byte[] Handler_Encryption(string s, string key, string IV)
    {
        byte[] bytesOfKey = Encoding.ASCII.GetBytes(key, 0, 32);
        byte[] bytesOfIV = Encoding.ASCII.GetBytes(IV, 0, 16);
        return (Encrypt(s, bytesOfKey, bytesOfIV));
    }

    /// <summary>
    /// A "publicly" accessible method that allows other parts of our app to just pass a string and get the decrypted version back
    /// </summary>
    /// <param name="s"></param>
    /// <param name="statemanager"></param>
    /// <returns>Payload decrypted with the given symmetric key and initialization vector</returns>
    public string Handler_Decryption(byte[] s, string key, string IV)
    {
        //int sizeofString = s.Length;

        //byte[] bytesOfString = Encoding.ASCII.GetBytes(s, 0, sizeofString);
        byte[] bytesOfKey = Encoding.ASCII.GetBytes(key, 0, 32);
        byte[] bytesOfIV = Encoding.ASCII.GetBytes(IV, 0, 16);

        return Decrypt(s, bytesOfKey, bytesOfIV);
    }
    #endregion Handler Methods

    #region actual encryption and decryption methods
    private static byte[] Encrypt(string plainText, byte[] key, byte[] iv)
    {
        using Aes aesAlg = Aes.Create();
        aesAlg.Key = key;
        aesAlg.IV = iv;
        aesAlg.Mode = CipherMode.CBC; // Use CBC mode for security
        aesAlg.Padding = PaddingMode.PKCS7;

        ICryptoTransform encryptor = aesAlg.CreateEncryptor();

        using MemoryStream msEncrypt = new MemoryStream();
        using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);

        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(plainText);
        }

        return msEncrypt.ToArray();
    }

    private static string Decrypt(byte[] cipherText, byte[] key, byte[] iv)
    {
        using Aes aesAlg = Aes.Create();
        aesAlg.Mode = CipherMode.CBC;
        aesAlg.Padding = PaddingMode.PKCS7;
        aesAlg.Key = key;
        aesAlg.IV = iv;

        ICryptoTransform decryptor = aesAlg.CreateDecryptor();

        using MemoryStream msDecrypt = new MemoryStream(cipherText);
        using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);

        using StreamReader srDecrypt = new StreamReader(csDecrypt);
        return srDecrypt.ReadToEnd();
    }

    //takes a 256-bit key (32 bytes/chars), random 16-bit IV (4 bytes/chars)
    public string test(byte[] key, byte[] iv, string input)
    {
        // Your plaintext data
        string originalText = input;

        // Encrypt the data
        byte[] encryptedBytes = Encrypt(originalText, key, iv);

        // Decrypt the data
        string decryptedText = Decrypt(encryptedBytes, key, iv);

        return $"Original: {originalText} .\r\nEncrypted: {Convert.ToBase64String(encryptedBytes)} .\r\nDecrypted: {decryptedText}";
    }

    /// <summary>
    /// A method that allows other parts of our app to just pass a string and get the encrypted version back
    /// </summary>
    /// <param name="input"></param>
    /// <param name="base64ServerPublicKey"></param>
    /// <returns>Base64 encoded Payload encrypted with the server's public asymmetric encryption key</returns>
    private static string EncryptWithRSA(string input, byte[] base64ServerPublicKey)
    {
        var rsa = new RSACryptoServiceProvider();
        rsa.ImportRSAPublicKey(base64ServerPublicKey, out _);

        var dataToEncrypt = EncodingUtils.StringToByteArray(input);
        var encryptedByteArray = rsa.Encrypt(dataToEncrypt, false);

        // Convert encrypted bytes to a base64-encoded string
        var encryptedText = EncodingUtils.ByteArrayToBase64String(encryptedByteArray);
        return encryptedText;
    }

    #endregion actual encryption and decryption methods

    #region Key helpers
    ///<summary>
    /// Generates a random 256bit/32byte cryptographic key
    /// </summary>
    /// <returns></returns>
    public static byte[] GetRandomAesKey()
    {
        byte[] bytes = new byte[32]; //32 bytes or 256 bits

        using (Aes aesAlgorithm = Aes.Create())
        {
            //using aes256
            aesAlgorithm.KeySize = 256;
            aesAlgorithm.GenerateKey();
            bytes = aesAlgorithm.Key;
        }
        return bytes;
    }
    ///<summary>
    /// Generates a random 128bit/16byte cryptographic Initialization Vector
    /// </summary>
    /// <returns></returns>
    public static byte[] GetRandomAesIV()
    {
        byte[] bytes = new byte[16]; //16 bytes or 128 bits

        using (Aes aesAlgorithm = Aes.Create())
        {
            //using aes256
            aesAlgorithm.GenerateIV();
            bytes = aesAlgorithm.IV;
        }
        return bytes;
    }
    ///<summary>
    /// Generates a random 128bit/16byte cryptographic random token
    /// </summary>
    /// <returns></returns>
    public static byte[] GetRandomToken()
    {
        byte[] bytes = new byte[16]; //16 bytes or 128 bits

        using (Aes aesAlgorithm = Aes.Create())
        {
            //using aes256
            aesAlgorithm.GenerateIV();
            bytes = aesAlgorithm.IV;
        }
        return bytes;
    }
    #endregion Key helpers
}
