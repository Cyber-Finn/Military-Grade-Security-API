namespace MilitaryGrade_API.Utilities;
public static class ServerCertificatePaths
{
    //general config
    public static string ServerConfigFolderPath = @"C:\CyberFinn\MilitaryGradeAPI\";

    //config for certs
    public static string CertificateFileName = "OurServerCert.pfx";
    public static string CertificatePath => ServerConfigFolderPath + CertificateFileName;
    public static string CertificatePassword = "password123"; //please use an actual strong password

    //config for State files
    public static string StateFiles_FolderLocation = ServerConfigFolderPath;
    public static string StateFiles_Filename = "ConnectedUsers.txt"; //arb filename
}
