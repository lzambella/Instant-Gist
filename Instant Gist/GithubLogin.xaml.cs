using System.Globalization;
using System.Windows;
using System.Windows.Controls;
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
        public GithubLogin()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            if (this.textBox.Text.Length != 40)
            {
                textBox.Text = "Enter a valid token.";
            }
            else
            {
                var tokenFile = "Token.txt";
                if (!File.Exists(tokenFile))
                    File.Create(tokenFile);
                var fileWriter = new StreamWriter(tokenFile);
                string ID = this.textBox.Text;
                fileWriter.WriteLine(ID);
                fileWriter.Close();
                this.Close();
            }
        }
    }
}
