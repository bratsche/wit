using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Microsoft.Win32;

[assembly:AssemblyKeyFile(@"..\..\wit.snk")]

namespace wit
{
    [Guid("66ABF5B3-9113-4010-92C4-B66CD2D86121")]
	public class CommandShellExtension : IShellExtInit, IContextMenu
	{
        const string guid = "{66ABF5B3-9113-4010-92C4-B66CD2D86121}";

#region Protected Members
		protected IDataObject m_dataObject = null;
		uint m_hDrop = 0;
		MenuItem[] m_items;
#endregion

#region IContextMenu
		int	IContextMenu.QueryContextMenu(uint hMenu, uint iMenu, int idCmdFirst, int idCmdLast, uint uFlags)
		{
			int id = 1;
			if ((uFlags & 0xf) == 0 || (uFlags & (uint)CMF.CMF_EXPLORE) != 0)
			{
				uint nselected = Helpers.DragQueryFile(m_hDrop, 0xffffffff, null, 0);
				if (nselected == 1)
				{
                    uint hmnuPopup = Helpers.CreatePopupMenu();
                    id = PopulateMenu(hmnuPopup, idCmdFirst + id);

                    MENUITEMINFO mii = new MENUITEMINFO();
                    mii.cbSize = 48;
                    mii.fMask = (uint)MIIM.TYPE | (uint)MIIM.STATE | (uint)MIIM.SUBMENU;
                    mii.hSubMenu = (int)hmnuPopup;
                    mii.fType = (uint)MF.STRING;
                    mii.dwTypeData = "ACTIONS";
                    mii.fState = (uint)MF.ENABLED;
                    Helpers.InsertMenuItem(hMenu, (uint)iMenu, 1, ref mii);
				}
			}
			return id;
		}

        void AddMenuItem(uint hMenu, string text, int id, uint position)
        {
            MENUITEMINFO mii = new MENUITEMINFO();
            mii.cbSize = 48;
            mii.fMask = (uint)MIIM.ID | (uint)MIIM.TYPE | (uint)MIIM.STATE;
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
				commandString = new StringBuilder(m_items[idCmd - 1].Command.Substring(1, cchMax-1));
				break;
			case (uint)GCS.HELPTEXT:
				commandString = new StringBuilder(m_items[idCmd - 1].HelpText.Substring(1, cchMax));
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
            /*
             *
			try
			{
				Type typINVOKECOMMANDINFO = Type.GetType("ShellExt.INVOKECOMMANDINFO");
				INVOKECOMMANDINFO ici = (INVOKECOMMANDINFO)Marshal.PtrToStructure(pici, typINVOKECOMMANDINFO);
				if (ici.verb - 1 <= m_items.Length)
					ExecuteCommand(m_items[ici.verb - 1].Command);
			}
			catch(Exception)
			{
			}
            */
		}
#endregion

#region IShellExtInit
		int	IShellExtInit.Initialize (IntPtr pidlFolder, IntPtr lpdobj, uint hKeyProgID)
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

					// Now retrieve the menu information from the registry
					RegistryKey sc = Registry.LocalMachine;
					sc = sc.OpenSubKey("Software\\Microsoft\\wit", true);
					if (sc.SubKeyCount > 0)
					{
						m_items = new MenuItem[sc.SubKeyCount];
						int	i=0;
						foreach(string name in sc.GetSubKeyNames())
						{
							try
							{
								RegistryKey item = sc.OpenSubKey(name, true);
								string command = (string)item.GetValue("");
								MenuItem m = new MenuItem();
                                m.Text = name;
                                m.Command = command;
								m_items[i] = m;
								++i;
							}
							catch(Exception)
							{
							}
						}
					}
					else
					{
						m_items = new MenuItem[0];
					}
				}
			}
			catch(Exception)
			{
			}

			return 0;
		}
#endregion

#region Registration
		[System.Runtime.InteropServices.ComRegisterFunctionAttribute()]
		static void RegisterServer(String str1)
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


				root = Registry.ClassesRoot;
				rk = root.CreateSubKey("Folder\\shellex\\ContextMenuHandlers\\wit");
				rk.SetValue("", guid.ToString());
				rk.Close();
			}
			catch(Exception e)
			{
				System.Console.WriteLine(e.ToString());
			}
		}

		[System.Runtime.InteropServices.ComUnregisterFunctionAttribute()]
		static void UnregisterServer(String str1)
		{
			try
			{
				RegistryKey root;
				RegistryKey rk;

				root = Registry.LocalMachine;
				rk = root.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Approved", true);
				rk.DeleteValue(guid);
				rk.Close();


                root = Registry.ClassesRoot;
				root.DeleteSubKey("Folder\\shellex\\ContextMenuHandlers\\wit");
			}
			catch(Exception e)
			{
				System.Console.WriteLine(e.ToString());
			}
		}
#endregion
	}
}
