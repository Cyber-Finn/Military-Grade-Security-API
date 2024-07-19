using System.Text;

namespace MilitaryGradeAPI_Client.Utilities
{
    public class EncodingUtils
    {

        /// Note:
        /// We want the API-client to send data in base64 format, because base64 ensures compatibility with systems and removes text transport issues, URL safety, etc. <summary>
        /// Note:
        /// we also want to use ASCII over UTF8 here, because UTF8 is variable length, where ASCII encoding uses a full 8bits to represent a character.
        ///     ensures transport safety and quality

        public static string Base64Encode(byte[] input)
        {
            return System.Convert.ToBase64String(input);
        }

        public static byte[] Base64Decode(string input)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(input);
            return base64EncodedBytes;
        }

        public static string ByteArrayToString(byte[] bytes)
        {
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        public static byte[] StringToByteArray(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        public static string ByteArrayToBase64String(byte[] bytes)
        {
            //return Base64Encode(ByteArrayToString(bytes));
            return Base64Encode(bytes);
        }
        public static byte[] Base64StringToByteArray(string str)
        {
            return Base64Decode(str);
        }

        public static string Base64StringToRegularString(string str)
        {
            byte[] data = Base64StringToByteArray(str);
            return ByteArrayToString(data);
        }

    }
}
