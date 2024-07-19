using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace MilitaryGradeAPI_Client
{
    public static class ApiEndpointURLs
    {
        public static string GetServerPK = "DebugGetServerAsymmetricPublicKey";

        public static string AuthenticatePhaseOne = "AuthenticatePhaseOne";
        public static string Authenticate = "Authenticate";

        public static string GetSomeResponseFromServer = "GetSomeResponseFromServer";

        public static string CloseSessionWithServer = "CloseSessionWithServer";

    }
}
