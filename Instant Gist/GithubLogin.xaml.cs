using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Instant_Gist
{
    /// <summary>
    /// Interaction logic for GithubLogin.xaml.
    /// </summary>
    [ProvideToolboxControl("Instant_Gist.GithubLogin", true)]
    public partial class GithubLogin : UserControl
    {
        public GithubLogin()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(string.Format(CultureInfo.CurrentUICulture, "We are inside {0}.Button1_Click()", this.ToString()));
        }
    }
}
