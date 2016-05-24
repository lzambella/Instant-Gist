using System.Windows;
using System.IO;
using Microsoft.VisualStudio.PlatformUI;

namespace Instant_Gist
{
    /// <summary>
    /// Interaction logic for GithubLogin.xaml.
    /// </summary>
    [ProvideToolboxControl("Instant_Gist.GithubLogin", true)]
    public partial class GithubLogin : DialogWindow
    {
        private const string TokenFile = "Token.txt";
        private bool _cleared = false;
        public GithubLogin()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TextBox.Text.Length != 40)
                    TextBox.Text = "Enter a valid token.";
                else if (TextBox.Text.Equals("Token successfully cleared."))
                    Close();
                else
                {

                    if (!File.Exists(TokenFile))
                        File.Create(TokenFile);
                    var fileWriter = new StreamWriter(TokenFile);
                    var ID = this.TextBox.Text;
                    fileWriter.WriteLine(ID);
                    fileWriter.Close();
                    Close();
                }
            }
            catch (System.Exception exception)
            {
                TextBox.Text = "Error. " + exception;
            }
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            _cleared = false;
            try
            {
                if (File.Exists(TokenFile))
                {
                    File.Delete(TokenFile);
                    TextBox.Text = "Token successfully cleared.";
                    _cleared = true;
                }
                else
                    TextBox.Text = "Nothing to clear.";
            }
            catch (System.Exception exception)
            {

                TextBox.Text = "Error. " + exception.Message;
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
