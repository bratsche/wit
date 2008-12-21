using System;
using System.Reflection;
using System.Runtime.InteropServices;

using Wit.Interop;

using Gtk;

[assembly:AssemblyKeyFile(@"..\..\wit.snk")]

namespace Wit
{
    [ComVisible(true), Guid("FF8B02E1-C721-41c6-97D8-5CDC8E441CCD")]
    [RegisteredBy(@"*\shellex\ContextMenuHandlers\wit")]
    [RegisteredBy(@"Directory\shellex\ContextMenuHandlers\wit")]
    [RegisteredBy(@"Drive\shellex\ContextMenuHandlers\wit")]
    public class WitShellExtension : ShellExtension
    {
        public override MenuItem[] MenuItems
        {
            get { return actions_popup_items; }
        }

        private static void OnInitRepo(object o, EventArgs e)
        {
            ShowMessage("OnInitRepo()");
        }

        private static void OnCloneRepo(object o, EventArgs e)
        {
            ShowMessage("OnCloneRepo()");
        }

        private static void ShowMessage(string msg)
        {
            InfoDialog dialog = new InfoDialog(null, msg, false);
            dialog.Run();
        }

#region private members
        private MenuItem[] actions_popup_items = new MenuItem[] {
            new MenuItem("Git Not Found", GitState.GitNotFound),
            new MenuItem("Init Git Repo", 0, new EventHandler(OnInitRepo)),
            new MenuItem("Clone Git Repo", 0, new EventHandler(OnCloneRepo)),
            new PopupItem("Git", GitState.InGitDirectory,
                new MenuItem[] {
                    new MenuItem("Update", GitState.InGitDirectory),
                    new MenuItem("Branch", GitState.InGitDirectory)
                })
        };
#endregion
	}
}
