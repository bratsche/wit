using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Microsoft.Win32;

using Wit.Interop;

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

#region private members
        private MenuItem[] actions_popup_items = new MenuItem[] {
            new MenuItem("Git Not Found", GitState.GitNotFound),
            new MenuItem("Init Git Repo", 0),
            new MenuItem("Clone Git Repo", 0),
            new PopupItem("Git", GitState.InGitDirectory,
                new MenuItem[] {
                    new MenuItem("Update", GitState.InGitDirectory),
                    new MenuItem("Branch", GitState.InGitDirectory)
                })
        };

#endregion
	}
}
