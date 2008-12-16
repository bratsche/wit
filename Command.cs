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

[assembly:AssemblyKeyFile(@"..\..\wit.snk")]

namespace wit
{
    [Guid("66ABF5B3-9113-4010-92C4-B66CD2D86121")]
	public class WitShellExtension : IShellExtInit, IContextMenu
	{
        const string guid = "{66ABF5B3-9113-4010-92C4-B66CD2D86121}";

#region Protected Members
		protected IDataObject m_dataObject = null;
		uint m_hDrop = 0;
        List<MenuItem> menu_items;
#endregion

#region IContextMenu
		int	IContextMenu.QueryContextMenu(uint hMenu, uint iMenu, int idCmdFirst, int idCmdLast, uint uFlags)
		{
			int id = 1;
            uint pos = 0;
			if ((uFlags & 0xf) == 0 || (uFlags & (uint)CMF.CMF_EXPLORE) != 0)
			{
				uint nselected = Helpers.DragQueryFile(m_hDrop, 0xffffffff, null, 0);
				if (nselected == 1)
				{
                    /*
                    uint hmnuPopup = Helpers.CreatePopupMenu();
                    id = PopulateMenu(hmnuPopup, idCmdFirst + id);

                    MenuItemInfo mii = new MenuItemInfo();
                    mii.cbSize = 48;
                    mii.fMask = (uint)MIIM.Type | (uint)MIIM.State | (uint)MIIM.SubMenu;
                    mii.hSubMenu = (int)hmnuPopup;
                    mii.fType = (uint)MF.STRING;
                    mii.dwTypeData = "ACTIONS";
                    mii.fState = (uint)MF.ENABLED;
                    Helpers.InsertMenuItem(hMenu, (uint)iMenu, 1, ref mii);
                    */

                    foreach (MenuItem i in menu_items)
                    {
                        AddMenuItem(hMenu, i.Text, id++, pos++);
                    }
				}
			}
			return id;
		}

        void AddMenuItem(uint hMenu, string text, int id, uint position)
        {
            MenuItemInfo mii = new MenuItemInfo();
            mii.cbSize = 48;
            mii.fMask = (uint)MIIM.ID | (uint)MIIM.Type | (uint)MIIM.State;
            mii.wID = id;
            mii.fType = (uint)MF.STRING;
            mii.dwTypeData = text;
            mii.fState = (uint)MF.ENABLED;
            Helpers.InsertMenuItem(hMenu, position, 1, ref mii);
        }

        private int PopulateMenu(uint hMenu, int id)
        {
            int status = RunProcess(@"C:\Program Files\Git\bin\git", "rev-parse --cdup");

            if (status < 0)
            {
                AddMenuItem(hMenu, "Cannot find git", id++, 0);
            }

            if (status > 0)
            {
                AddMenuItem(hMenu, "Initialize a git repo", id++, 0);
            }
            else if (status == 0)
            {
                AddMenuItem(hMenu, "Git Actions", id++, 0);
            }

            //AddMenuItem(hMenu, "Clone Git Repository", id++, 0);
            //AddMenuItem(hMenu, "Create Git Repository", id++, 1);

            return id;
        }

        private int RunProcess(string command, string args)
        {
            var proc = new Process();
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.FileName = command;
            if (args != String.Empty)
            {
                proc.StartInfo.Arguments = args;
            }

            try
            {
                proc.Start();
                proc.WaitForExit();
            }
            catch (Win32Exception e)
            {
                return -1;
            }

            return proc.ExitCode;
        }
		
		void IContextMenu.GetCommandString(int idCmd, uint uFlags, int pwReserved, StringBuilder commandString, int cchMax)
		{
			switch(uFlags)
			{
			case (uint)GCS.VERB:
				commandString = new StringBuilder(menu_items[idCmd - 1].Command.Substring(1, cchMax-1));
				break;
			case (uint)GCS.HELPTEXT:
				commandString = new StringBuilder(menu_items[idCmd - 1].HelpText.Substring(1, cchMax));
				break;
			case (uint)GCS.VALIDATE:
				break;
			}
		}
		
        //void IContextMenu.InvokeCommand(IntPtr pici)
		void IContextMenu.InvokeCommand ([In] ref CommandInfo pici)
		{
            try
            {
                int index = (int)pici.lpVerb;
                Console.WriteLine(index);
            }
            catch (Exception) { }
		}
#endregion

#region IShellExtInit
        //int IShellExtInit.Initialize([In] ref ITEMIDLIST pidlFolder, IntPtr lpdobj, uint hKeyProgID)
		int IShellExtInit.Initialize (IntPtr pidlFolder, IntPtr lpdobj, uint hKeyProgID)
		{
			try
			{
				m_dataObject = null;
				if (lpdobj != (IntPtr)0)
				{
					// Get info about the directory
					m_dataObject = (IDataObject)Marshal.GetObjectForIUnknown(lpdobj);
					FORMATETC fmt = new FORMATETC();
					fmt.cfFormat = CLIPFORMAT.CF_HDROP;
					fmt.ptd		 = 0;
					fmt.dwAspect = DVASPECT.DVASPECT_CONTENT;
					fmt.lindex	 = -1;
					fmt.tymed	 = TYMED.TYMED_HGLOBAL;
					STGMEDIUM medium = new STGMEDIUM();
					m_dataObject.GetData(ref fmt, ref medium);
					m_hDrop = medium.hGlobal;

                    // Get the location that was clicked
                    StringBuilder sb = new StringBuilder(1024);
                    Helpers.DragQueryFile(m_hDrop, 0, sb, sb.Capacity + 1);
                    string filename = sb.ToString();

                    // Change the pwd
                    Directory.SetCurrentDirectory(filename);

                    if (menu_items == null)
                        menu_items = new List<MenuItem>();

                    // Setup our menu items
                    int status = RunProcess(@"C:\Program Files\Git\bin\git", "rev-parse --cdup");
                    if (status < 0)
                        menu_items.Add(new MenuItem("Cannot find git"));
                    else if (status > 0)
                        menu_items.Add(new MenuItem("Init Git Repo"));
                    else if (status == 0)
                        menu_items.Add(new MenuItem("Git Actions"));

				}
			}
			catch(Exception)
			{
			}

			return 0;
		}
#endregion

#region Registration
        private static string[] keys = {
            @"*\shellex\ContextMenuHandlers\wit",
            @"Directory\Background\shellex\ContextMenuHandlers\wit",
            @"Directory\shellex\ContextMenuHandlers\wit",
            @"Folder\shellex\ContextMenuHandlers\wit"
        };

		[ComRegisterFunctionAttribute]
		static void RegisterServer(Type type)
		{
			try
			{
				RegistryKey root;
				RegistryKey rk;
				root = Registry.CurrentUser;
				rk = root.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer", true);
				rk.SetValue("DesktopProcess", 1);
				rk.Close();


                root = Registry.LocalMachine;
				rk = root.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Approved", true);
				rk.SetValue(guid.ToString(), "wit shell extension");
				rk.Close();

                foreach (string s in keys)
                {
                    root = Registry.ClassesRoot;
                    rk = root.CreateSubKey(s);
                    rk.SetValue("", guid.ToString());
                    rk.Close();
                }
			}
			catch(Exception e)
			{
				System.Console.WriteLine(e.ToString());
			}
		}

		[ComUnregisterFunctionAttribute]
		static void UnregisterServer(Type type)
		{
			try
			{
				RegistryKey root;
				RegistryKey rk;

				root = Registry.LocalMachine;
				rk = root.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Approved", true);
				rk.DeleteValue(guid);
				rk.Close();

                foreach (string s in keys)
                {
                    root = Registry.ClassesRoot;
                    root.DeleteSubKey(s);
                }
			}
			catch(Exception e)
			{
				System.Console.WriteLine(e.ToString());
			}
		}
#endregion
	}
}
