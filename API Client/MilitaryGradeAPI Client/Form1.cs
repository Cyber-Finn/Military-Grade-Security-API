namespace MilitaryGradeAPI_Client
{
    public partial class Form1 : Form
    {
        private string userInput = "";
        private string systemOutput = "";
        //we do this globally/member-level because we want to use the same keys for this session (until the app is closed and restarted), not on every message sent (this approach is inefficient)
        private ApiConnection apiConnection;

        public Form1()
        {
            InitializeComponent();
            apiConnection = new ApiConnection(this);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            Controller();
        }
        private void Controller()
        {
            LoadUpUserInput();

            apiConnection.Controller(userInput);
        }
        private void LoadUpUserInput()
        {
            userInput = txtInput.Text;
        }

        public void DisplayOutput(string output)
        {
            txtOutput.Text = output;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //close the session with the API
            apiConnection.CloseSessionWithServer();
        }
    }
}
