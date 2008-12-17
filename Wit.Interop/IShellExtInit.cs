﻿using System;
using System.Runtime.InteropServices;

namespace Wit.Interop
{
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), GuidAttribute("000214e8-0000-0000-c000-000000000046")]
    public interface IShellExtInit
    {
        [PreserveSig]
        int Initialize(IntPtr pidlFolder, IntPtr lpdobj, IntPtr hKeyProgID);
    }
}