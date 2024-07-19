namespace MilitaryGrade_API.Database;
public class DatabaseConnection
{
    #region members
    private List<string> _lsListOfPreparedStatements = new List<string>();

    //DB connection stuff
    private string User = string.Empty;
    private string Pass = string.Empty;
    private string Server = string.Empty;
    private string ConnectionString = string.Empty;
    private string Database = string.Empty;
    private string Port = string.Empty;
    #endregion members

    public bool authLogin(string username, string password)
    {
        //for simplicity-sake, we're not going to connect to a db and auth a user, we're just going to return true (assumed success)
        //  for real-life applications, change this as needed
        return true;
    }
}
