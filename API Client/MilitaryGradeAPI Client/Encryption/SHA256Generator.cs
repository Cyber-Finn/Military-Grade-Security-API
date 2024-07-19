using System.Text;
using System.Security.Cryptography;

namespace MilitaryGradeAPI_Client.Encryption;
internal class SHA256Generator
{

    /// <summary>
    /// SHA256 is the global standard for military and governmental organizations. 
    /// <br></br>
    /// ALWAYS pass HASHES of passwords, NOT passwords themselves.
    /// <br></br>
    /// SHA256 and other hashing algs are used for password protection, because hashes are (near) impossible to reverse-engineer (I have created a reverse-engineering tool here: https://github.com/Cyber-Finn/Brute-Force-Password-Hash-Cracker)
    /// <br></br>
    /// Only the user should ever know what the password is, we only store the hash of the pword on the server, and use only the hash in transmission. We compare their given hash to the hash we have on the server to verify that it's them.
    /// </summary>
    /// <param name="input"></param>
    /// <returns>string of 64 characters representing the hashed password</returns>
    public static string ComputeSHA256Hash(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = sha256.ComputeHash(inputBytes);

            //Convert the hash bytes to a hexadecimal string
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2")); // Format as two hex characters
            }
            return sb.ToString();
        }
    }
}
