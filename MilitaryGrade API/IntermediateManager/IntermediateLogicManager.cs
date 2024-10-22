﻿using MilitaryGrade_API.Database;
using MilitaryGrade_API.Encryption;
using MilitaryGrade_API.StatefulManager;

namespace MilitaryGrade_API.IntermediateManager;
public class IntermediateLogicManager
{
    #region Members
    //this instance will only be accessible to a single user -> using static here creates an instance that may allow users to access each other's data - which we don't want
    private DatabaseConnection dbConn = new DatabaseConnection();
    private AesEncryption aesEncryption = new AesEncryption();
    private ServerEncryption serverEncryption = new ServerEncryption();
    public StatefulnessManager statefulManager = new StatefulnessManager();
    #endregion Members

    #region Auth handshake Managers

    /// <summary>
    /// Why do we have this Method/App?:
    /// <br></br>
    /// Because there's a limit to how much info you can securely send to any remote API using their RSA keys. 
    /// The keyspace is always limited for RSA.
    /// <br></br>
    /// A 2048-bit RSA key has a keyspace of (2^{2048}), so we can basically (depending on padding) only encrypt between 214~245 bytes/chars. 
    /// This makes sending large volumes of data, securely, completely impossible
    /// <br></br>
    /// Most websites/APIs combat this by having massive (and expensive) keys to allow users to encrypt more data,but this is still not enough for big data (text, audio, video, etc.). 
    /// <br></br>
    /// This API (and the system/logic I've created here) allows us to use a standard 2048bit public asymmetric key, to circumvent/bypass the massive limitation on how much data we can send, 
    /// while providing max security to the users/system by ensuring that symmetric keys are exchanged securely, with ephemeral session keys in use to prevent against on-path attacks.
    /// </summary>
    /// <param name="encryptedString">A base64Decoded string (which is still encrypted with the server's RSA public asymmetric encryption key</param>

    //Initial handshake between us and client
    //  happens using the PKI public RSA asymmetric encryption key (API-client will encrypt a small amount of data using our public key and send it here)
    public void ManageLogin_PhaseOne(string encryptedString, string IpAddress)
    {
        try
        {
            //Here, we'll be using our server's asymmetric private key to decrypt the user's (Base64Decoded now) input string, before we can start substringing data and getting the symmetric keys;
            string decryptedString = this.serverEncryption.ManageDecryptionOfInitialHandshakeData(encrypted: encryptedString);

            if (decryptedString.Length == 78) //has to match exactly what we expect, because there's limited keyspace for the RSA keys
            {
                this.statefulManager.HandleAuthPhaseOne(decrypted: decryptedString, IpAddress: IpAddress);
                //user data has now been stored and they can move onto the next phase of login
            }
        }
        catch
        {
        }
    }

    //first step in the chain after auth handshake (This happens using AES, not RSA)
    public void ManageLogin(string encryptedString)
    {
        try
        {
            //todo: we need to find the user in statefulmanager somehow, figure out what their key is, then decrypt
            //using IPs here to figure out who the request is

            string decryptedString = this.aesEncryption.Handler_Decryption(encryptedString, statefulManager);

            if (!string.IsNullOrEmpty(decryptedString))
            {
                if (decryptedString.Length == 222) //30 + 64 +128
                {
                    //once decryption is done, we have the plaintext, so we need to substr the vars out
                    string username = decryptedString.Substring(0, 30).Replace("'", "");
                    string password = decryptedString.Substring(30, 64).Replace("'", "");

                    //once our vals have been loaded up, we auth the user
                    bool userCouldLogIn = this.dbConn.AuthLoginOnDB(username, password);

                    //only load up the token for this session, if the user could log in (They're an authorized user)
                    if (userCouldLogIn)
                    {
                        ///<remarks>
                        ///user has now successfully done a "handshake" with us, so we'll now talk to them in future, using this session token instead of a password hash (protects them against rainbow table attacks, etc.)
                        ///this increases the users' protection against having their password stolen by attackers
                        ///stolen sessionTokens will only be compromised for a single session - and attackers would need to repeat the attack next session (This is if they're somehow able to magically steal the encryption keys)
                        /// </remarks>
                        this.statefulManager.HandleUpdateAuthToken(username, decryptedString);

                        //need to:
                        //get the user's call, get the user details, check the connectedUsers log, find them, then replace their info with the updated info
                    }
                }
            }
        }
        catch
        {
        }
    }
    #endregion Auth handshake Managers

    #region Connect and Get Data from server
    //Here, the user has authed and sent us all required info, so now we'll be using their info to decrypt/encrypt messages
    public string GetResponseFromServer(string strEncryptedInitialCall)
    {
        try
        {
            //Here, we'll be using our session's symmetric cryptographic key to decrypt the user's call, to get our data
            string decrypted = this.aesEncryption.Handler_Decryption(strEncryptedInitialCall, statefulManager);

            if (!string.IsNullOrEmpty(decrypted))
            {
                //now that input was decrypted using session's symmetric cryptographic key, we can load up the data
                string username = decrypted.Substring(0, 30).Replace("'", "");
                string token = decrypted.Substring(30, 128).Replace("'", "");

                string JsonString = "";
                //todo: load rest of the message and do something
                //todo: need to rework this, considering concurrent connections to the API, we'd need to first check the state manager

                //encrypt the data we return back to client for bidirectional safety, using the current session's symmetric cryptographic key
                string base64EncodedEncryptedJsonString = this.aesEncryption.Handler_Encryption(JsonString, statefulManager);
                return base64EncodedEncryptedJsonString;
            }
        }
        catch
        {
        }
        return "";
    }
    #endregion Connect and Get Data from server

    #region Close connection with server
    //todo: add close logic
    #endregion Close connection with server
}
