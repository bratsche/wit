using System;
using Microsoft.Win32;

namespace Wit.Interop
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
    public sealed class RegisteredByAttribute : Attribute
    {
        public RegistryKey Root { get { return root; } }
        public string RegistryPath { get { return path; } }
        public string Name { get { return name; } }
        
        public RegisteredByAttribute(RegistryLocation location, string path)
            : this(location, path, String.Empty)
        {
        }

        public RegisteredByAttribute(RegistryLocation location, string path, string name)
        {
            this.path = path;
            this.name = name;

            switch (location)
            {
                case RegistryLocation.ClassesRoot:
                    root = Registry.ClassesRoot;
                    break;
                case RegistryLocation.CurrentConfig:
                    root = Registry.CurrentConfig;
                    break;
                case RegistryLocation.CurrentUser:
                    root = Registry.CurrentUser;
                    break;
                case RegistryLocation.DynData:
                    root = Registry.DynData;
                    break;
                case RegistryLocation.LocalMachine:
                    root = Registry.LocalMachine;
                    break;
                case RegistryLocation.PerformanceData:
                    root = Registry.PerformanceData;
                    break;
                case RegistryLocation.Users:
                    root = Registry.Users;
                    break;
            }
        }

        private RegistryKey root;
        private string path = String.Empty;
        private string name = String.Empty;
    }
}