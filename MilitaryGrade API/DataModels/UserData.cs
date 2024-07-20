namespace MilitaryGrade_API.DataModels;
/// <summary>
/// represents the user's session details
/// </summary>
public class UserData
{
    /// <summary>
    /// Symmetric encryption key (AES256)
    /// </summary>
    /// <remarks>Comes from the client app (should always be generated on the client app because of resource costs)</remarks>
    public string EncryptionKey { get; set; } = String.Empty;
    /// <summary>
    /// Symmetric encryption IV -> used with the AES256 key to encrypt/decrypt the data.
    /// </summary>
    /// <remarks>Comes from the client app (should always be generated on the client app because of resource costs)</remarks>
    public string EncryptionInitializationVector { get; set; } = String.Empty;
    public string Username { get; set; } = String.Empty;
    /// <summary>
    /// A 128-byte/char random string token.
    /// </summary>
    /// <remarks>Comes from the client app (should always be generated on the client app)</remarks>
    public string SessionToken { get; set; } = String.Empty;
    /// <summary>
    /// The user's IP Address (IPv6 format)
    /// </summary>
    /// <remarks>
    /// We populate this on server-side when the user has connected to us.
    /// We can use this to do IP Filtering if we wanted, but we'll use it to make lookups in the StateManager easier
    /// </remarks>
    public string IpAddress { get; set; } = String.Empty;
}
