using System.Security.Cryptography;
using System.Text;
using MilitaryGrade_API.StatefulManager;

namespace MilitaryGrade_API.Encryption;
public class AesEncryption
{
    #region Handler Methods

    /// <summary>
    /// A "publicly" accessible method that allows other parts of our app to just pass a string and get the encrypted version back
    /// </summary>
    /// <param name="s"></param>
    /// <param name="statemanager"></param>
    /// <returns></returns>
    public string Handler_Encryption(string s, StatefulnessManager statemanager)
    {
        //load up our db keys and IVs
        string key = statemanager._objUserData.EncryptionKey;
        string IV = statemanager._objUserData.EncryptionInitializationVector;

        byte[] bytesOfKey = Encoding.ASCII.GetBytes(key, 0, 32);
        byte[] bytesOfIV = Encoding.ASCII.GetBytes(IV, 0, 16);
        return "";//EncryptStringToBytes_Aes(s, bytesOfKey, bytesOfIV);
    }

    /// <summary>
    /// A "publicly" accessible method that allows other parts of our app to just pass a string and get the decrypted version back
    /// </summary>
    /// <param name="s"></param>
    /// <param name="statemanager"></param>
    /// <returns></returns>
    public string Handler_Decryption(string s, StatefulnessManager statemanager)
    {
        //load up our db keys and IVs
        string key = statemanager._objUserData.EncryptionKey;
        string IV = statemanager._objUserData.EncryptionInitializationVector;

        string base64InputStr = Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
        int sizeofString = base64InputStr.Length;

        byte[] bytesOfString = Encoding.ASCII.GetBytes(base64InputStr, 0, sizeofString);
        byte[] bytesOfKey = Encoding.ASCII.GetBytes(key, 0, 32);
        byte[] bytesOfIV = Encoding.ASCII.GetBytes(IV, 0, 16);

        return Decrypt(bytesOfKey, bytesOfIV, bytesOfString);
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
        aesAlg.Key = key;
        aesAlg.IV = iv;
        aesAlg.Mode = CipherMode.CBC;
        aesAlg.Padding = PaddingMode.PKCS7;

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

        return $"Original: {originalText}" + $" . Encrypted: {Convert.ToBase64String(encryptedBytes)}" + $" . Decrypted: {decryptedText}";
    }

    #endregion actual encryption and decryption methods
}
