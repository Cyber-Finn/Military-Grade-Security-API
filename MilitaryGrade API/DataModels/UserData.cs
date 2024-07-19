namespace MilitaryGrade_API.DataModels;
/// <summary>
/// represents the user's session details
/// </summary>
public class UserData
{
    public string EncryptionKey { get; set; } = String.Empty;
    public string EncryptionInitializationVector { get; set; } = String.Empty;
    public string Username { get; set; } = String.Empty;
    public string SessionToken { get; set; } = String.Empty;
}
