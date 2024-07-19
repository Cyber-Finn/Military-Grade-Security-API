namespace MilitaryGrade_API.Database;
public class DatabaseConnection
{
    #region members
    //DB connection stuff
    private readonly string User = string.Empty;
    private readonly string Pass = string.Empty;
    private readonly string Server = string.Empty;
    private readonly string ConnectionString = string.Empty;
    private readonly string Database = string.Empty;
    private readonly string Port = string.Empty;
    #endregion members

    public bool AuthLoginOnDB(string username, string password)
    {
        //for simplicity-sake, we're not going to connect to a db and auth a user, we're just going to return true (assumed success)
        //  for real-life applications, change this as needed
        return true;
    }
}
