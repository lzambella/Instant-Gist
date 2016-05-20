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
        private bool anonymous = false;
        private GitHubClient client = new GitHubClient(new ProductHeaderValue("Instant-Gist"));
        private Credentials login;
        //private string ID = "";
        //private string OAuthUrl = "https://github.com/login/oauth/authorize?client_id=" + ID + "&scopes=gist"; // Ask user for permission to read and write gists

        /// <summary>
        /// Command ID.
        /// </summary>
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
            {
                throw new ArgumentNullException("package");
            }

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
            GithubLogin auth = new GithubLogin();
            auth.ShowModal();
        }

        private async void On_PublicGistUpload_Clicked(object sender, EventArgs e)
        {
            const string tokenFile = "token.txt";

            if (login == null)
            {
                StreamReader reader = new StreamReader(tokenFile);
                if (File.Exists(tokenFile))
                {
                    var token = reader.ReadLine();
                    login = new Credentials(token);
                    client.Credentials = login;
                }
                else
                    anonymous = true;
            }

            if (anonymous == false)
            {
                //OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
                try
                {
                    var editor = this.ServiceProvider.GetService(typeof (SDTE)) as DTE;
                    EnvDTE.TextSelection selection = editor.ActiveDocument.Selection as EnvDTE.TextSelection;
                    if (selection != null)
                    {
                        var text = selection.Text;
                        NewGist gist = new NewGist
                        {
                            Description = "Instant Gist automatic description.",
                            Public = true
                        };
                        gist.Files.Add(DateTime.Now.ToString("F"), text);
                        Gist toUpload = await client.Gist.Create(gist);
                        Clipboard.SetText(toUpload.HtmlUrl);

                        VsShellUtilities.ShowMessageBox(
                        this.ServiceProvider,
                        "Gist has been successfully uploaded and the link has been copied to the clipboard.\n(This will be replaced by a non-intrusive tooltip.)",
                        "Success",
                        OLEMSGICON.OLEMSGICON_INFO,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    }
                }
                catch (Exception exception)
                {
                    VsShellUtilities.ShowMessageBox(
                        this.ServiceProvider,
                        "No text might have been selected \nStack Trace\n" + exception.ToString(),
                        "Error",
                        OLEMSGICON.OLEMSGICON_INFO,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                }
            }
            else //anonymous == true
            {
                VsShellUtilities.ShowMessageBox(
                this.ServiceProvider,
                "Anonymous uploads not yet implemented",
                "Error",
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }

        }
        /// <summary>
        /// Copy pasted function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void On_PrivateGistUpload_Clicked(object sender, EventArgs e)
        {
            const string tokenFile = "token.txt";

            if (login == null)
            {
                StreamReader reader = new StreamReader(tokenFile);
                if (File.Exists(tokenFile))
                {
                    var token = reader.ReadLine();
                    login = new Credentials(token);
                    client.Credentials = login;
                }
                else
                    anonymous = true;
            }

            if (anonymous == false)
            {
                //OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
                try
                {
                    var editor = this.ServiceProvider.GetService(typeof(SDTE)) as DTE;
                    EnvDTE.TextSelection selection = editor.ActiveDocument.Selection as EnvDTE.TextSelection;
                    if (selection != null)
                    {
                        var text = selection.Text;
                        NewGist gist = new NewGist
                        {
                            Description = "Instant Gist automatic description.",
                            Public = false
                        };
                        gist.Files.Add(DateTime.Now.ToString("F"), text);
                        Gist toUpload = await client.Gist.Create(gist);
                        Clipboard.SetText(toUpload.HtmlUrl);

                        VsShellUtilities.ShowMessageBox(
                        this.ServiceProvider,
                        "Gist has been successfully uploaded and the link has been copied to the clipboard.\n(This will be replaced by a non-intrusive tooltip.)",
                        "Success",
                        OLEMSGICON.OLEMSGICON_INFO,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    }
                }
                catch (Exception exception)
                {
                    VsShellUtilities.ShowMessageBox(
                        this.ServiceProvider,
                        "No text might have been selected \nStack Trace\n" + exception.ToString(),
                        "Error",
                        OLEMSGICON.OLEMSGICON_INFO,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                }
            }
            else //anonymous == true
            {
                VsShellUtilities.ShowMessageBox(
                this.ServiceProvider,
                "Anonymous uploads not yet implemented",
                "Error",
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }
        //TODO: create a generic upload function and have anonymous as one parameter

    }

}
