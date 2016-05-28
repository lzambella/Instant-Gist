using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Windows;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Octokit;

namespace Instant_Gist
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class GistMenu
    {
        //Database database = new Database("history", ".");
        private const string TokenFile = "token.txt";
        private readonly GitHubClient _client = new GitHubClient(new ProductHeaderValue("Instant-Gist"));
        private Credentials _login;

        public const int CommandId = 0x0100;

        public const int PublicGist = 0x0200;

        public const int PrivateGist = 0x0300;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("6a36d1ed-4044-4316-a10d-e6bd826c944c");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="GistMenu"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private GistMenu(Package package)
        {
            if (package == null)
                throw new ArgumentNullException("package");
            this.package = package;
            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var publicGistUpload = new CommandID(CommandSet, PublicGist);
                var privateGistUpload = new CommandID(CommandSet, PrivateGist);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                var publicGistUploadItem = new MenuCommand(this.On_PublicGistUpload_Clicked, publicGistUpload);
                var privateGistUploadItem = new MenuCommand(this.On_PrivateGistUpload_Clicked, privateGistUpload);
                commandService.AddCommand(menuItem);
                commandService.AddCommand(publicGistUploadItem);
                commandService.AddCommand(privateGistUploadItem);
            }
        }


        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static GistMenu Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider => this.package;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new GistMenu(package);
        }

        /// <summary>
        /// Opens a menu where you enter your personal auth token
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            var auth = new GithubLogin();
            auth.ShowModal();
        }
        /// <summary>
        /// Handler for button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void On_PublicGistUpload_Clicked(object sender, EventArgs e)
        {
            var loggedIn = TryLogin();
            UploadGist(true, loggedIn);
        }
        /// <summary>
        /// Copy pasted function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void On_PrivateGistUpload_Clicked(object sender, EventArgs e)
        {
            var loggedIn = TryLogin();
            UploadGist(false, loggedIn); // loggedIn is always true here
        }
        /// <summary>
        /// Checks whether the program can log in with the selected token and logs in.
        /// </summary>
        /// <returns></returns>
        private bool TryLogin()
        {
            try
            {
                var reader = new StreamReader(TokenFile);
                if (!File.Exists(TokenFile)) return false;
                var token = reader.ReadLine();
                _login = new Credentials(token);
                _client.Credentials = _login;
                return true;
            }
            catch (Exception)
            {
                // If there's an error with the login
                return false;
            }
        }
        /// <summary>
        /// Gets the selected text of the most recently selected editor window.
        /// </summary>
        /// <returns></returns>
        private string GetSelectedText()
        {
            try
            {
                var editor = ServiceProvider.GetService(typeof(SDTE)) as DTE;
                var selection = editor?.ActiveDocument.Selection as TextSelection;
                return selection == null ? "" : selection.Text;
            }
            catch (Exception)
            {
                return "";
            }
        }
        /// <summary>
        /// Gets the extension of the file in the currently active editor.
        /// </summary>
        /// <returns>Extension of file as string (no '.').</returns>
        private string GetExtension()
        {
            var editor = ServiceProvider.GetService(typeof(SDTE)) as DTE;
            if (editor == null) return "";
            var fileName = editor.ActiveDocument.FullName;
            var arr = fileName.Split('\\');
            arr = arr[arr.Length - 1].Split('.');
            var extension = arr[arr.Length - 1];
            return extension;
        }

        /// <summary>
        /// Upload a gist to GitHub using the selected text
        /// </summary>
        /// <param name="_public">Whether or not the gist is public</param>
        /// <param name="loggedIn">Whether or not the gist in loggedIn</param>
        public async void UploadGist(bool _public, bool loggedIn)
        {
            try
            {
                var confirmation = 0;
                // Get the selected text
                var text = GetSelectedText();
                // If there is no text throw an error
                if (text.Equals("")) throw new Exception();
                // Create a new gist object that represents the gist to upload
                var gist = new NewGist
                {
                    Description = "Instant Gist upload.",
                    Public = true
                };
                // Get the file extension
                var extension = GetExtension();
                // Create a name for the upload
                var fileName = DateTime.Now.ToString("F");
                gist.Files.Add(fileName + "." + extension, text);

                if (!loggedIn)
                {
                    confirmation = VsShellUtilities.ShowMessageBox(
                        ServiceProvider,
                        "Anonymous uploads can not easily be deleted. Really upload one?",
                        "Warning.",
                        OLEMSGICON.OLEMSGICON_CRITICAL,
                        OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                }
                if (confirmation == 7) return;
                var upload = await _client.Gist.Create(gist);
                Clipboard.SetText(upload.HtmlUrl);
                var editor = ServiceProvider.GetService(typeof(SDTE)) as DTE;
                var file = editor.ActiveDocument.FullName;
                var arr = file.Split('\\');
                //database.AddHistory(DateTime.Now, upload.HtmlUrl, arr[arr.Length - 1]);
                QuickMessage("Done.", "Gist successfully uploaded and the address was sent to the clipboard.");
            }
            catch (Exception)
            {
                QuickMessage("Error.", " Exception occured.\nNo text might have been selected.");
                throw;
            }
        }
        /// <summary>
        /// Simplified messagebox dialog
        /// </summary>
        /// <param name="header">header text</param>
        /// <param name="text">body text</param>
        public void QuickMessage(string header, string text)
        {
            VsShellUtilities.ShowMessageBox(
                        this.ServiceProvider,
                        text,
                        header,
                        OLEMSGICON.OLEMSGICON_INFO,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }

}
