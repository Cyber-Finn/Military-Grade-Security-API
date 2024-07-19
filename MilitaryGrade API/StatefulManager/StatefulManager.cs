using MilitaryGrade_API.DataModels;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using MilitaryGrade_API.Utilities;

namespace MilitaryGrade_API.StatefulManager;

/// <summary>
/// Manages statefulness for our API by persisting user connections, and keeping track of tokens, usernames and encryption keys
/// </summary>
/// <remarks>In a proper system, we would use other methods of handling statefulness, like saving to a connections db, etc., but for simplicity-sake, we are going to be using a text file</remarks>
public class StatefulnessManager
{
    public UserData _objUserData = new UserData();
    private string _stateFiles_FilePath => ServerCertificatePaths.StateFiles_FolderLocation + ServerCertificatePaths.StateFiles_Filename;

    public StatefulnessManager()
    {
        CheckDirExists();
        CheckFileExists();
    }

    /// <summary>
    /// Called at initialization. Ensures that the folder exists.
    /// </summary>
    private void CheckDirExists()
    {
        if (!Directory.Exists(ServerCertificatePaths.StateFiles_FolderLocation))
        {
            Directory.CreateDirectory(ServerCertificatePaths.StateFiles_FolderLocation);
        }
    }

    /// <summary>
    /// Called at initialization. Ensures that the file exists.
    /// </summary>
    private void CheckFileExists()
    {
        if (!File.Exists(ServerCertificatePaths.StateFiles_Filename))
        {
            File.Create(ServerCertificatePaths.StateFiles_Filename);
        }
    }

    #region basic file manipulation
    private void WriteMessageToFile(string message)
    {
        // this method also locks the file
        // note that we're only doing it like this for simplicity-sake
        //      in reality - with multiple concurrent connections to the API - we'd have deadlocks or dropped writes from this approach
        using (var fileStream = File.Open(_stateFiles_FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            using (var writer = new StreamWriter(fileStream))
            {
                // write to the file
                writer.WriteLine(message);
            }
        }
    }
    private string[] ReadFileContents()
    {
        return File.ReadAllLines(_stateFiles_FilePath);
    }

    public void ReplaceFileContents(ref List<string> newContents)
    {
        int newContentsLength = newContents.Count;
        //replace the file with an empty one
        File.WriteAllText(_stateFiles_FilePath, "");

        // lock the file
        using (var fileStream = File.Open(_stateFiles_FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            using (var writer = new StreamWriter(fileStream))
            {
                for (int i = 0; i < newContentsLength; i++)
                {
                    writer.WriteLine(newContents[i]);
                }
            }
        }
    }
    #endregion basic file manipulation

    #region JSON 
    /// <summary>
    /// Converts the current user data object into a json string to save to the file
    /// </summary>
    /// <returns></returns>
    private string GetJSONString()
    {
        return JsonSerializer.Serialize(_objUserData);
    }

    /// <summary>
    /// converts a line of text (in JSON format) into a user data object
    /// </summary>
    /// <param name="jsonLine"></param>
    /// <returns></returns>
    private static UserData? GetUserData(string jsonLine)
    {
        return JsonSerializer.Deserialize<UserData>(jsonLine);
    }
    #endregion JSON

    #region Complex File Manipulation

    /// <summary>
    /// Gets and replaces the users data in the sessions file. Allows us to update user details
    /// </summary>
    private void ReplaceUserDataWithUpdatedData()
    {
        //get the current file contents
        string[] oldContents = ReadFileContents();
        //replace the specified section
        List<string> newContents = ReplaceOldVersionOfUserData(ref oldContents);
        //rewrite the file with new contents
        ReplaceFileContents(ref newContents);
    }
    private List<string> ReplaceOldVersionOfUserData(ref string[] oldContents)
    {
        List<string> newContents = new List<string>();

        int lengthofOldContents = oldContents.Length;
        for (int i = 0; i < lengthofOldContents; i++)
        {
            UserData? oldUserData = GetUserData(oldContents[i]);
            if (oldUserData == null)
            {
                continue;
            }

            if (checkMatch_Username(ref oldUserData)) //we need to replace it because it matches
            {
                newContents.Add(GetJSONString());
            }
            else //it's not the one we're looking for, so add as-is
            {
                newContents.Add(oldContents[i]);
            }
        }
        return newContents;
    }

    /// <summary>
    /// allows us to terminate the API user's session after a certain time -> there are a few ways we could manage this
    /// For simplicity, we'll assume the API client sends a "session closing" message
    /// </summary>
    /// <param name="oldContents"></param>
    /// <returns></returns>
    private void RemoveUserData()
    {
        //get the current file contents
        string[] oldContents = ReadFileContents();

        //remove our existing user data
        List<string> newContents = new List<string>();

        int lengthofOldContents = oldContents.Length;
        for (int i = 0; i < lengthofOldContents; i++)
        {
            UserData? oldUserData = GetUserData(oldContents[i]);
            if (oldUserData == null)
            {
                continue;
            }

            if (checkMatch_Username(ref oldUserData)) //we need to replace it because it matches
            {
                //do nothing
            }
            else //it's not the one we're looking for, so add as-is
            {
                newContents.Add(oldContents[i]);
            }
        }

        //rewrite the file, without our data
        ReplaceFileContents(ref newContents);
    }
    #endregion Complex File Manipulation

    #region Checks to see if old object matches current object
    private bool checkMatch_Username(ref UserData oldUserData)
    {
        return (oldUserData.Username.Equals(_objUserData.Username)) ? true : false;
    }
    private bool checkMatch_Token(ref UserData oldUserData)
    {
        return (oldUserData.SessionToken.Equals(_objUserData.SessionToken)) ? true : false;
    }
    #endregion Checks to see if old object matches current object

    /// <summary>
    /// Saves the user's data for future connections (During this session only), allowing us to implement Perfect Forward Secrecy (PFS)
    /// </summary>
    /// <param name="decrypted">plaintext string which contains: Key, IV and username</param>
    public void HandleAuthPhaseOne([StringLength(78)] string decrypted) //min&max must be 78 chars only
    {
        //load up their current connection object
        this._objUserData.EncryptionKey = decrypted.Substring(0, 32); //key is first 32
        this._objUserData.EncryptionInitializationVector = decrypted.Substring(64, 16); //IV is only 16
        this._objUserData.Username = decrypted.Substring(64, 30); //username can be up to 30

        //save the current object to the stateful manager file, so that we can persist their session
        WriteMessageToFile(GetJSONString());
    }
    public void HandleAuth([StringLength(100)] string decryptedString) //min&max must be 100 chars only
    {
        //user has now successfully done a "handshake" with us, so we'll now talk to them in future, using this session token instead of a password
        //  this increases the users' protection against having their password stolen by on-path attackers or sniffers
        //      stolen sessionTokens will only be compromised for a single session - and attackers would need to repeat the attack next session
        this._objUserData.SessionToken = decryptedString.Substring(94, 128);

        ReplaceUserDataWithUpdatedData();
    }
}
