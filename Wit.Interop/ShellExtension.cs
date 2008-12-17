﻿using System;
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

namespace Wit
{
    public abstract class ShellExtension : IShellExtInit, IContextMenu
    {
        public abstract MenuItem[] MenuItems { get; }

        public GitState State { get { return state; } }
        public Git Git { get { return git; } } // <-- this is my favorite line of code ever.

#region IShellExtInit implementation
        int IShellExtInit.Initialize(IntPtr pidlFolder, IntPtr lpdobj, IntPtr hKeyProgID)
        {
            try
            {
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
            catch (Exception e)
            {
            }

            return 0;
        }
#endregion

#region IContextMenu implementation
        int IContextMenu.QueryContextMenu(uint hMenu, uint iMenu, int idCmdFirst, int idCmdLast, uint uFlags)
        {
            id_hash[hMenu] = 1;
            if ((uFlags & 0xf) == 0 || (uFlags & (uint)ContextMenuFlags.Explore) != 0)
            {
                uint nselected = Helpers.DragQueryFile(m_hDrop, 0xffffffff, null, 0);
                if (nselected == 1)
                {
                    PopulateMenus(hMenu, MenuItems);
                }
            }

            return id_hash[hMenu];
        }

        void IContextMenu.GetCommandString(int idCmd, GetCommandStringFlags uFlags, int pwReserved, StringBuilder commandString, int cchMax)
        {
            switch (uFlags)
            {
                case GetCommandStringFlags.Verb:
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
                int index = (int)pici.lpVerb;
                Console.WriteLine(index);
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
            mii.fType = (uint)MenuFlags.String;
            mii.dwTypeData = item.Text;
            mii.fState = (uint)MenuFlags.Enabled;
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
            mii.fType = (uint)MenuFlags.String;
            mii.dwTypeData = item.Text;
            mii.fState = (uint)MenuFlags.Enabled;
            Helpers.InsertMenuItem(hMenu, position, 1, ref mii);

            actions_hash[id] = item;
            id_hash[hMenu] = ++id;
        }
#endregion

#region COM registration
        static RegistryKey ApprovedShellExtensionsKey
        {
            get
            {
                return Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved", true);
            }
        }

        private static string GetGuid(Type type)
        {
            GuidAttribute guid_attr = (GuidAttribute)Attribute.GetCustomAttribute(type, typeof(GuidAttribute));
            string guid = guid_attr.Value;

            return string.Format("{{{0}}}", guid);
        }

        private static RegistryKey GetCLSKey(Type type)
        {
            string guid = GetGuid(type);
            return Registry.ClassesRoot.CreateSubKey(String.Format(CultureInfo.InvariantCulture, @"CLSID\{0}", guid));
        }

        static string[] GetKeysFromType(Type type)
        {
            RegisteredByAttribute[] attributes = (RegisteredByAttribute[])Attribute.GetCustomAttributes(type, typeof(RegisteredByAttribute));
            List<string> keys_list = new List<string>();
            foreach (RegisteredByAttribute a in attributes)
            {
                keys_list.Add(a.RegistryKey);
            }

            return keys_list.ToArray();
        }

        [ComRegisterFunctionAttribute]
        static void RegisterServer(Type type)
        {
            try
            {
                string[] keys = GetKeysFromType(type);

                string guid = GetGuid(type);

                RegistryKey root;
                RegistryKey rk;

                root = Registry.CurrentUser;
                rk = root.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer", true);
                rk.SetValue("DesktopProcess", 1);
                rk.Close();

                rk = GetCLSKey(type);
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
            catch (Exception e)
            {
                System.Console.WriteLine(e.ToString());
            }
        }

        [ComUnregisterFunctionAttribute]
        static void UnregisterServer(Type type)
        {
            try
            {
                string[] keys = GetKeysFromType(type);
                string guid = GetGuid(type);

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
            catch (Exception e)
            {
                System.Console.WriteLine(e.ToString());
            }
        }
#endregion

#region private data
        private IDataObject m_dataObject = null;
        private uint m_hDrop = 0;
        private GitState state;
        private Git git = new Git();
        private Dictionary<uint, int> id_hash = new Dictionary<uint, int>();
        private Dictionary<int, MenuItem> actions_hash = new Dictionary<int, MenuItem>();
#endregion
    }
}