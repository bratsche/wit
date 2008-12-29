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
    public class RegistryManager
    {
        public RegistryManager(Type type)
        {
            this.type = type;
        }

        public void Register()
        {
            RegisterDesktopProcess();

            RegistryKey rk = GetCLSKey();
            rk.SetValue(null, "wit");
            rk.Close();

            RegisterType();
        }

        public void Unregister()
        {
            UnregisterApproved();
            UnregisterType();
        }

#region private properties
        private string GUID
        {
            get
            {
                GuidAttribute guid_attr = (GuidAttribute)Attribute.GetCustomAttribute(type, typeof(GuidAttribute));
                string guid = guid_attr.Value;

                return string.Format("{{{0}}}", guid);
            }
        }

        private RegistryKey ApprovedShellExtensionsKey
        {
            get
            {
                return Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved", true);
            }
        }

        private RegistryKey GetCLSKey()
        {
            return Registry.ClassesRoot.CreateSubKey(String.Format(CultureInfo.InvariantCulture, @"CLSID\{0}", GUID));
        }
#endregion

#region private data
        private Type type;
#endregion

#region private methods
        private void RegisterDesktopProcess()
        {
            RegistryKey root;
            RegistryKey rk;

            root = Registry.CurrentUser;
            rk = root.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer", true);
            rk.SetValue("DesktopProcess", 1);
            rk.Close();
            root.Close();
        }

        private void RegisterType()
        {
            RegisteredByAttribute[] attributes = (RegisteredByAttribute[])Attribute.GetCustomAttributes(type, typeof(RegisteredByAttribute));

            RegistryKey root = Registry.ClassesRoot;
            RegistryKey rk = root.CreateSubKey("wit");
            rk.SetValue("", "wit");
            rk.Close();

            foreach (RegisteredByAttribute a in attributes)
            {
                root = a.Root;
                rk = root.CreateSubKey(a.RegistryPath);
                rk.SetValue(a.Name != String.Empty ? a.Name : "", GUID);
                rk.Close();
                root.Close();
            }
        }

        private void UnregisterType()
        {
            RegisteredByAttribute[] attributes = (RegisteredByAttribute[])Attribute.GetCustomAttributes(type, typeof(RegisteredByAttribute));

            foreach (RegisteredByAttribute a in attributes)
            {
                RegistryKey root = a.Root;
                if (a.Name == "" || a.Name == String.Empty)
                {
                    root.DeleteSubKey(a.RegistryPath);
                }
                else
                {
                    RegistryKey rk = root.OpenSubKey(a.RegistryPath, true);
                    if (rk != null)
                    {
                        rk.DeleteValue(a.Name, false);
                    }
                }
            }
        }

        private void RegisterApproved()
        {
            RegistryKey rk = ApprovedShellExtensionsKey;
            rk.SetValue(GUID, "wit");
            rk.Close();
        }

        private void UnregisterApproved()
        {
            RegistryKey rk = ApprovedShellExtensionsKey;
            rk.DeleteValue(GUID, false);
            rk.Close();
        }
#endregion
    }
}