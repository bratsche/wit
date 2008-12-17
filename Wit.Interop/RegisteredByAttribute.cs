using System;

namespace Wit.Interop
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
    public sealed class RegisteredByAttribute : Attribute
    {
        public string RegistryKey { get { return key; } }

        public RegisteredByAttribute(string registry_key)
        {
            key = registry_key;
        }

        private string key = String.Empty;
    }
}