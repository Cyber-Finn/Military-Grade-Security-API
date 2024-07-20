using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using MilitaryGrade_API.IntermediateManager;
using MilitaryGrade_API.Encryption;
using MilitaryGrade_API.Utilities;

namespace MilitaryGrade_API.Controllers;

/// <summary>
/// this will handle which function to call, based on the client input
/// </summary>
[ApiController] // telling system this is api function to make available to client
[Route("DataAccessor")] //name of API
public class DataAccessorController : ControllerBase //inheriting the controller base class here to allow us to use our custom IP address methods below
{
    #region Members
    private IntermediateLogicManager interMan = new IntermediateLogicManager();
    private X509CertGenerator certGenerator = new X509CertGenerator();
    #endregion Members

    #region IP Address Handling
    /// <summary>
    /// Gets the current connection's IP address (We can either do IP filtering with this, or figure out which user to check for in StatefulnessManager, before we decrypt their request
    /// </summary>
    /// <returns>IPV6 IP Address of the connected user. For localhost testing, this would be "::1" (IPV6 equivalent of IPV4s "127.0.0.1")</returns>
    private IActionResult GetClientIpAddress()
    {
        // Retrieve the client's IP address from the current request
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        // Return the IP address as part of the response
        return Ok(ipAddress);
    }
    private string ParseIpAddress()
    {
        var ipAddressResult = GetClientIpAddress();
        if (ipAddressResult is OkObjectResult okResult) //we got the IP
        {
            string? connectedUserIP = okResult.Value.ToString();

            return (!String.IsNullOrEmpty(connectedUserIP)) ? connectedUserIP : String.Empty;
        }
        else //no IP found
        {
        }
        return String.Empty;
    }
    #endregion IP Address Handling

    #region API Endpoints
    //auth handshake between us and client
    /// <summary>
    ///  the data passed into this method IS encrypted with our public asymmetric encryption key. It contains the username, IV and Encryption key
    /// </summary>
    /// <param name="inputString"></param>
    [HttpPost]
    [Route("AuthenticatePhaseOne")]
    public void MilitaryGradeAPI_AuthenticateUser_PhaseOne([StringLength(300)] string inputString) //will take base64 username (30chars), initialization vector (16chars) and encryption key (32 chars) for this session
    {
        try
        {
            if (!String.IsNullOrEmpty(value: inputString))
            {
                string connectedUserIP = ParseIpAddress();
                //this string will still be encrypted with our asymmetric key, so we can't substring just yet
                // we now need to decrypt the string with our asymmetric private key
                this.interMan.ManageLogin_PhaseOne(encryptedString: EncodingUtils.Base64StringToRegularString(inputString), IpAddress: connectedUserIP);
            }
        }
        catch
        {
            //doing HTTP post method and NOT returning here, to limit info sent back to user. The less they know, the better.
            //Error messages can be dangerous in the right hands
        }
    }

    //first step in the chain after auth handshake
    /// <summary>
    ///  the data passed into this method IS NOT encrypted with our public key, it's encrypted (by client) using our shared/symmetric IV and Encryption key
    /// </summary>
    /// <param name="inputString"></param>
    [HttpPost]
    [Route("Authenticate")]
    public void MilitaryGradeAPI_AuthenticateUser([StringLength(300)] string inputString) //takes username (30), pword hash (64) and security token (128) [we'll use token, instead of pword, for future comms, and added security]
    {
        try
        {
            if (!String.IsNullOrEmpty(value: inputString))
            {
                this.interMan.ManageLogin(encryptedString: inputString);
            }
        }
        catch
        {
            //doing HTTP post method and NOT returning here to limit info sent back to user. Less they know, the better. Error messages can be dangerous in the right hands
        }
    }

    //Here, the user has already authed and sent us all required info, so they can now connect and do stuff securely
    [HttpGet]
    [Route("GetSomeResponseFromServer")]
    public string GetResponseFromServer(string inputString) //takes username (30), token (128) and input (variable length)
    {
        try
        {
            if (!String.IsNullOrEmpty(value: inputString))
            {
                return this.interMan.GetResponseFromServer(strEncryptedInitialCall: inputString);
            }
        }
        catch
        {
        }
        return "";
    }

    //Client app has closed session
    [HttpPost]
    [Route("CloseSessionWithServer")]
    public void CloseSessionWithServer(string inputString) //takes username (30), token (128)
    {
        try
        {
        }
        catch
        {
        }
    }

    //Note: irl, this wouldn't be needed because the site would be hosted and the key publicly available to all relevant clients,
    //but since I'm self-hosting locally, I'll need this to get the key of my api from the client-side
    [HttpGet]
    [Route("Debug_GetServerAsymmetricPublicKey")]
    public string Debug_GetServerPublicAsymmetricKey()
    {
        try
        {
            return X509CertGenerator.GetServerPublicKey();
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

    #endregion API Endpoints
}
