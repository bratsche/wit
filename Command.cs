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
    [ComVisible(true), Guid("FF8B02E1-C721-41c6-97D8-5CDC8E441CCD")]
	public class WitShellExtension : IShellExtInit, IContextMenu
	{
        const string guid = "{FF8B02E1-C721-41c6-97D8-5CDC8E441CCD}";

#region private members
		private IDataObject m_dataObject = null;
		private uint m_hDrop = 0;
        private Dictionary<uint, int> id_hash = new Dictionary<uint, int>();
        private Git git = new Git();

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
        private Dictionary<int, MenuItem> actions_hash = new Dictionary<int, MenuItem>();

        private GitState state;

#endregion

#region IContextMenu
		int	IContextMenu.QueryContextMenu(uint hMenu, uint iMenu, int idCmdFirst, int idCmdLast, uint uFlags)
		{
            id_hash[hMenu] = 1;
			if ((uFlags & 0xf) == 0 || (uFlags & (uint)CMF.CMF_EXPLORE) != 0)
			{
				uint nselected = Helpers.DragQueryFile(m_hDrop, 0xffffffff, null, 0);
				if (nselected == 1)
				{
                    PopulateMenus(hMenu, actions_popup_items);
				}
			}

			return id_hash[hMenu];
		}

        private void PopulateMenus(uint hMenu, MenuItem[] items)
        {
            uint pos = 0;
            foreach (MenuItem item in items)
            {
                InsertMenuItem(hMenu, item, pos++);
            }
        }

        private void InsertMenuItem(uint hMenu, MenuItem item, uint pos)
        {
            if ((state & item.Requisites) == item.Requisites)
            {
                if (item is PopupItem)
                {
                    PopupItem popup = item as PopupItem;
                    uint popup_id = AddPopupItem(hMenu, popup, pos);
                    uint popup_pos = 0;
                    foreach (MenuItem child in popup)
                    {
                        InsertMenuItem(popup_id, child, popup_pos++);
                    }
                }
                else
                {
                    AddMenuItem(hMenu, item, pos);
                }
            }
        }

        uint AddPopupItem(uint hMenu, MenuItem item, uint position)
        {
            uint popup = Helpers.CreatePopupMenu();
            id_hash[popup] = 1;

            MenuItemInfo mii = new MenuItemInfo();
            mii.cbSize = 48;
            mii.fMask = (uint)MIIM.Type | (uint)MIIM.State | (uint)MIIM.SubMenu;
            mii.hSubMenu = (int)popup;
            mii.fType = (uint)MF.STRING;
            mii.dwTypeData = item.Text;
            mii.fState = (uint)MF.ENABLED;
            Helpers.InsertMenuItem(hMenu, position, 1, ref mii);

            return popup;
        }

        void AddMenuItem(uint hMenu, MenuItem item, uint position)
        {
            int id = id_hash[hMenu];

            MenuItemInfo mii = new MenuItemInfo();
            mii.cbSize = 48;
            mii.fMask = (uint)MIIM.ID | (uint)MIIM.Type | (uint)MIIM.State;
            mii.wID = id;
            mii.fType = (uint)MF.STRING;
            mii.dwTypeData = item.Text;
            mii.fState = (uint)MF.ENABLED;
            Helpers.InsertMenuItem(hMenu, position, 1, ref mii);

            actions_hash[id] = item;
            id_hash[hMenu] = ++id;
        }
		
		void IContextMenu.GetCommandString(int idCmd, uint uFlags, int pwReserved, StringBuilder commandString, int cchMax)
		{
			switch(uFlags)
			{
			case (uint)GCS.VERB:
				commandString = new StringBuilder(actions_hash[idCmd].Command.Substring(1, cchMax-1));
				break;
			case (uint)GCS.HELPTEXT:
				commandString = new StringBuilder(actions_hash[idCmd].HelpText.Substring(1, cchMax));
				break;
			case (uint)GCS.VALIDATE:
				break;
			}
		}
		
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

                    Console.WriteLine(git.UserName);
				}
			}
			catch(Exception e)
			{
			}

			return 0;
		}
#endregion

#region Registration
        private static string[] keys = {
            @"*.*\shellex\ContextMenuHandlers\wit",
            @"Directory\shellex\ContextMenuHandlers\wit",
            @"Drive\shellex\ContextMenuHandlers\wit",
        };

        static RegistryKey ApprovedShellExtensionsKey
        {
            get
            {
                return Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved", true);
            }
        }

        static RegistryKey CLSKey
        {
            get
            {
                return Registry.ClassesRoot.CreateSubKey(String.Format(CultureInfo.InvariantCulture, @"CLSID\{0}", guid));
            }
        }

		[ComRegisterFunctionAttribute]
		static void RegisterServer(Type type)
		{
			try
			{
				RegistryKey root;
				RegistryKey rk;

                root = Registry.CurrentUser;
				rk = root.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer", true);
				rk.SetValue("DesktopProcess", 1);
				rk.Close();

                rk = CLSKey;
                rk.SetValue(null, "wit");

                root = Registry.ClassesRoot;
                rk = root.CreateSubKey("wit");
                rk.SetValue("", "wit");
                rk.Close();
                foreach (string s in keys)
                {
                    rk = root.CreateSubKey(s);
                    rk.SetValue("", guid);
                    rk.Close();
                }

                rk = ApprovedShellExtensionsKey;
                rk.SetValue(guid, "wit");
                rk.Close();
                root.Close();
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

                rk = ApprovedShellExtensionsKey;
				rk.DeleteValue(guid);
				rk.Close();

                root = Registry.ClassesRoot;
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
