using System;
using System.Runtime.InteropServices;

namespace Wit.Interop
{
    [Flags]
    public enum CommandInfoMaskFlags : uint
    {
        None = 0x00000000,
        Icon = 0x00000010,
        HotKey = 0x00000020,
        FlagNoUI = 0x00000400,
        NoConsole = 0x00008000,
        AsyncOk = 0x00100000
    }

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    public struct CommandInfo
    {
        public uint cbSize;
        public uint /*CommandInfoMaskFlags*/ fMask;
        public uint hwnd;
        public int Verb;
        [MarshalAs(UnmanagedType.LPStr)] public string lpParameters;
        [MarshalAs(UnmanagedType.LPStr)] public string lpDirectory;
        public int nShow;
        public uint dwHotKey;
        public uint hIcon;
    }
}