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

namespace Wit.Interop
{
    public abstract class ShellExtension : IShellExtInit, IContextMenu, IShellIconOverlayIdentifier
    {
        public abstract MenuItem[] MenuItems { get; }
        public static Git Git { get { return git; } } // <-- this is my favorite line of code ever.

        int IShellIconOverlayIdentifier.IsMemberOf(string iconFileBuffer, int attributes)
        {
            // Get the path to run git in.  If iconFileBuffer is a directory, use it.
            // If it's a non-directory file, then use its directory name.
            string path = Directory.Exists(iconFileBuffer) ? iconFileBuffer : Path.GetDirectoryName(iconFileBuffer);
            Directory.SetCurrentDirectory(path);

            if (Git.State == GitState.InGitDirectory)
                return 0;

            return 1;
        }

        int IShellIconOverlayIdentifier.GetOverlayInfo(out string iconFileBuffer,
                                                       int iconFileBufferSize,
                                                       out int iconIndex,
                                                       out uint flags)
        {
            iconFileBuffer = String.Empty;
            iconIndex = 0;
            flags = 0;
            return 0;
        }

        int IShellIconOverlayIdentifier.GetPriority(out int priority)
        {
            priority = 0;
            return 0;
        }

#region IShellExtInit implementation
        int IShellExtInit.Initialize(IntPtr pidlFolder, IntPtr lpdobj, IntPtr hKeyProgID)
        {
            try
            {
                Gtk.Application.Init();

                m_dataObject = null;
                if (lpdobj != (IntPtr)0)
                {
                    // Get info about the directory
                    m_dataObject = (IDataObject)Marshal.GetObjectForIUnknown(lpdobj);
                    FormatEtc fmt = new FormatEtc();
                    fmt.cfFormat = ClipFormat.HDrop;
                    fmt.ptd = 0;
                    fmt.dwAspect = DVAspect.Content;
                    fmt.lindex = -1;
                    fmt.tymed = Tymed.HGlobal;
                    StgMedium medium = new StgMedium();
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
            catch (Exception)
            {
            }

            return 0;
        }
#endregion

#region IContextMenu implementation
        int IContextMenu.QueryContextMenu(uint hMenu, uint iMenu, int idCmdFirst, int idCmdLast, uint uFlags)
        {
            current_id = idCmdFirst;
            first_id = idCmdFirst;
            if ((uFlags & 0xf) == 0 || (uFlags & (uint)ContextMenuFlags.Explore) != 0)
            {
                uint nselected = Helpers.DragQueryFile(m_hDrop, 0xffffffff, null, 0);
                if (nselected == 1)
                {
                    PopulateMenus(hMenu, MenuItems);
                }
            }

            return current_id;
        }

        void IContextMenu.GetCommandString(int idCmd, GetCommandStringFlags uFlags, int pwReserved, StringBuilder commandString, int cchMax)
        {
            switch (uFlags)
            {
                case GetCommandStringFlags.Verb:
                case GetCommandStringFlags.VerbW:
                    commandString = new StringBuilder(actions_hash[idCmd].Command.Substring(1, cchMax - 1));
                    break;
                case GetCommandStringFlags.HelpText:
                    commandString = new StringBuilder(actions_hash[idCmd].HelpText.Substring(1, cchMax));
                    break;
                case GetCommandStringFlags.Validate:
                    break;
            }
        }

        void IContextMenu.InvokeCommand([In] ref CommandInfo pici)
        {
            try
            {
                int index = pici.Verb + first_id;
                actions_hash[index].Execute();
            }
            catch (Exception) { }
        }
#endregion

#region menu management
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
            if ((Git.State & item.Requisites) == item.Requisites)
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

            MenuItemInfo mii = new MenuItemInfo();
            mii.cbSize = 48;
            mii.fMask = (uint)MIIM.Type | (uint)MIIM.State | (uint)MIIM.SubMenu;
            mii.hSubMenu = (int)popup;
            mii.fType = (uint)MenuFlags.String;
            mii.dwTypeData = item.Text;
            mii.fState = (uint)MenuFlags.Enabled;
            Helpers.InsertMenuItem(hMenu, position, 1, ref mii);

            return popup;
        }

        void AddMenuItem(uint hMenu, MenuItem item, uint position)
        {
            int id = current_id;

            MenuItemInfo mii = new MenuItemInfo();
            mii.cbSize = 48;
            mii.fMask = (uint)MIIM.ID | (uint)MIIM.Type | (uint)MIIM.State;
            mii.wID = id;
            mii.fType = (uint)MenuFlags.String;
            mii.dwTypeData = item.Text;
            mii.fState = (uint)MenuFlags.Enabled;
            Helpers.InsertMenuItem(hMenu, position, 1, ref mii);

            actions_hash[id] = item;
            current_id = ++id;
        }
#endregion

#region COM registration
        [ComRegisterFunctionAttribute]
        static void RegisterServer(Type type)
        {
            RegistryManager rm = new RegistryManager(type);
            rm.Register();
        }

        [ComUnregisterFunctionAttribute]
        static void UnregisterServer(Type type)
        {
            RegistryManager rm = new RegistryManager(type);
            rm.Unregister();
        }
#endregion

#region private data
        private IDataObject m_dataObject = null;
        private uint m_hDrop = 0;
        private static Git git = new Git();
        private int current_id;
        private Dictionary<int, MenuItem> actions_hash = new Dictionary<int, MenuItem>();
        private int first_id;
#endregion
    }
}