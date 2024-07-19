using MilitaryGradeAPI_Client.Encryption;
using MilitaryGradeAPI_Client.Utilities;

namespace MilitaryGradeAPI_Client
{
    public class ApiConnection
    {
        #region Member vars
        MilitaryGradeAPI_Client.Form1 callingClass;
        AesEncryption aesEncryption = new AesEncryption();
        private static string apiBaseUrl = "https://localhost:44350/DataAccessor/"; //update with your actual API URL
        
        //could do monkey patching on these, but I prefer assignment at constructor level
        public string apiKey = "";
        public string apiIV = "";

        //todo, remove default val here - was just for debugging
        //public string apiServerAsymmKeyBase64 = "MIIBCgKCAQEA1UxitFfpJjJuC4ReLDJ7/KzKoVBRPLbGmKpdgE8F0jdeFezZMfKN+Edx7d54tosybv5gIZL3an4+2j3U09+Qsda8OVkOC7gjDeyJY+ugURoycD/NiCEVqdkKBA1H17JMyxBndnijkyARsA+AO06h7xKUS4lGhDQGbiP1tvbu2blTXOTsqqk5iPUHRQk3rwi7xE+A6aL1DoP7XweGM1tACHNHBqjTAGIkbCB1fLUgB37IzIKjksy1bYmxJi8AdsPs8d5tZxD67s7ae4mFGKYdII45Ty2g7Y0VVfhTFBskcCWmjTY0yUVFhBf3DeJ523Uj8gZUtdZq8YRYapcW58GBHQIDAQAB";
        public string apiServerAsymmKeyBase64 = "";
        
        public string username = "CyberFinn";
        //replace pword text here with your actual credentials but keep rest as-is (DO NOT SEND PASSWORD-TEXT, ONLY SEND A HASH!)
        public static string passwordHash => SHA256Generator.ComputeSHA256Hash("Password123"); 
        //value is defined in: this.GetApiRandomToken(). Will be 128 bytes (a keystretched sha512 token, which will also always change every time the app is started)
        public static string apiRandomToken = ""; //this isn't really required, but I like to have an extra token on top of the hash because it adds security for the user, whose data we're meant to protect
        #endregion Member vars

        #region connect to the remote API
        /// <summary>
        /// Connecting to a service in IIS on localhost
        /// </summary>
        /// <returns>The data from the endpoint as a string</returns>
        private static async Task<object> ConnectToServer(string APITarget)
        {
            using HttpClient httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"{apiBaseUrl}{APITarget}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                return null;
            }
        }
        private static async Task<object> SendMessageToServer(string APITarget, string input)
        {
            using HttpClient httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"{apiBaseUrl}{APITarget}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            else
            {
                return null;
            }
        }
        #endregion connect to the remote API

        #region Load Encryption details on constructor creation

        private void GetApiRandomToken()
        {
            // result will be 128 bytes (result will be a keystretched sha512 hash token, which will also always change every time the app is started [rolling and secure pword for this session)
            apiRandomToken = passwordHash + SHA256Generator.ComputeSHA256Hash(EncodingUtils.ByteArrayToString(AesEncryption.GetRandomAesKey()));
        }
        public void GetEncryptionKey()
        {
            apiKey = EncodingUtils.ByteArrayToString(AesEncryption.GetRandomAesKey());
        }
        public void GetEncryptionIV()
        {
            apiIV = EncodingUtils.ByteArrayToString(AesEncryption.GetRandomAesIV());
        }
        public async void GetServerEncryptionKey()
        {
            apiServerAsymmKeyBase64 = (string) await ConnectToServer(ApiEndpointURLs.GetServerPK);
        }

        public async void SendMessageToServer_Handshake(string input)
        {
            string response = (string)await SendMessageToServer(ApiEndpointURLs.AuthenticatePhaseOne, input);
        }
        public async void SendMessageToServer_Auth(string input)
        {
            string response = (string)await SendMessageToServer(ApiEndpointURLs.Authenticate, input);
        }
        public async Task<string> SendMessageToServer(string input)
        {
            string response = (string)await SendMessageToServer(ApiEndpointURLs.GetSomeResponseFromServer, input);
            return response;
        }

        #endregion Load Encryption details on constructor creation

        /// <summary>
        /// When constructor is created, the system automatically generates symmetric Encryption keys and initialization vectors
        /// <br></br>
        /// This class automatically handles sending messages to the target API and the encryption/decryption thereof
        /// </summary>
        public ApiConnection(Form1 callingClass)
        {
            //could do monkey patching on these, but prefer this method
            GetEncryptionKey();
            GetEncryptionIV();
            GetApiRandomToken();
            this.callingClass = callingClass;
            GetServerEncryptionKey();
        }
        #region Encryption/Decryption handlers
        public string Handle_EncryptionWithServerKey(string input)
        {
            byte[] encrypted = aesEncryption.Handler_Encryption(input, apiKey, apiIV);

            return EncodingUtils.ByteArrayToBase64String(encrypted);
        }

        public string Handle_Encryption(string input)
        {
            byte[] encrypted = aesEncryption.Handler_Encryption(input, apiKey, apiIV);

            return EncodingUtils.ByteArrayToBase64String(encrypted);
        }
        public string Handle_Decryption(string input)
        {
            byte[] base64DecodedByteArr = EncodingUtils.Base64StringToByteArray(input);
            return aesEncryption.Handler_Decryption(base64DecodedByteArr, apiKey, apiIV);
        }
        #endregion Encryption/Decryption handlers

        /// <summary>
        /// will move this to the test files later on, using for debugging now
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string TestEncryptionDecryption(string input)
        {
            string encrypted = Handle_Encryption(input);
            string decrypted = Handle_Decryption(encrypted);

            return $"Input: {input} \r\nEncrypted: {encrypted} \r\nDecrypted: {decrypted}";
        }

        public void Controller(string input)
        {
            SendHandshakeToServer(apiKey.PadRight(32) + apiIV.PadRight(16) + username.PadRight(30));
            AuthWithServer(username.PadRight(30) + passwordHash + apiRandomToken); //30 + 64 + (128) bytes
            GetResponseFromServer(username.PadRight(30) + apiRandomToken + input); //30 + 128 + variable length
        }

        #region Server Comms handlers

        /// <summary>
        /// Establishes the initial handshake with the server. All future comms will use the encryption details passed in here
        /// </summary>
        /// <param name="input">API will take: username (30chars), initialization vector (16chars) and encryption key (32 chars) for this session</param>
        private void SendHandshakeToServer(string input)
        {
            //get the encrypted (with server public asymmetric key) payload in Base64. String wont necessarily be 80chars anymore, as it's base64 encoded now
            string encryptedPayload = aesEncryption.Handler_EncryptionWithServerKey(input, apiServerAsymmKeyBase64);
            //send the message to the server
            SendMessageToServer_Handshake(encryptedPayload);
        }

        /// <summary>
        /// Second phase of handshake. 
        /// <br></br>
        /// Here, we pass our username, password hash (for first-time login) and security token (for all future auth). These are encrypted with our AES256 symmetric encryption keys
        /// </summary>
        /// <param name="input">API will take: username (30), pword hash (64) and security token (128) [API'll use token, instead of pword hash, for future comms and extra security]</param>
        private void AuthWithServer(string input)
        {
            //encrypt string with our ephemeral symmetric encryption key and ephemeral initialization vector, then encode to base64 and send to API
            string encryptedPayload = EncodingUtils.ByteArrayToBase64String(aesEncryption.Handler_Encryption(input, apiKey, apiIV));
            SendMessageToServer_Auth(encryptedPayload);
        }
        /// <summary>
        /// First phase of actual data transmission (non login stuff).
        /// <br></br>
        /// Here, we'll actually send some info to the server (in some apps this would be account transaction information, a text message, an image, etc.)
        /// </summary>
        /// <param name="input">API will take: username (30), token (128) and input data(variable length)</param>
        private async void GetResponseFromServer(string input)
        {
            //encrypt string with our ephemeral symmetric encryption key and ephemeral initialization vector, then encode to base64 and send to API
            string encryptedPayload = EncodingUtils.ByteArrayToBase64String(aesEncryption.Handler_Encryption(input, apiKey, apiIV));
            string response = (string) await SendMessageToServer(encryptedPayload);

            while (response == null) //wait for the response (there are other/better ways of doing this, but keeping this simple)
            {
            }
            //display the result to the user
            callingClass.DisplayOutput(response);
        }

        /// <summary>
        /// Will be called on formclosing event to terminate our session. 
        /// <br></br>
        /// API will take: username (30), token (128)
        /// </summary>
        public async void CloseSessionWithServer()
        {
            string input = username.PadRight(30) + apiRandomToken;
            //encrypt string with our ephemeral symmetric encryption key and ephemeral initialization vector, then encode to base64 and send to API
            await SendMessageToServer(ApiEndpointURLs.CloseSessionWithServer, input);
        }
        #endregion Server Comms handlers
    }
}
