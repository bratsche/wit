using System;

namespace wit
{
    [Flags]
    public enum CommandInfoMaskFlags
    {
        None = 0x00000000,
        Icon = 0x00000010,
        HotKey = 0x00000020,
        FlagNoUI = 0x00000400,
        NoConsole = 0x00008000,
        AsyncOk = 0x00100000
    }

    public struct CommandInfo
    {
        internal uint cbSize;
        internal CommandInfoMaskFlags fMask;
        internal IntPtr hwnd;
        internal IntPtr lpVerb;
        internal string lpParameters;
        internal string lpDirectory;
        internal int nShow;
        internal uint dwHotKey;
        internal IntPtr hIcon;
    }
}