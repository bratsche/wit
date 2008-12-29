using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using Microsoft.Win32;

namespace Wit.Interop
{
    public enum IconOverlayFlags : uint
    {
        IconFile  = 1,
        IconIndex = 2
    }

    [ComImport, GuidAttribute("0C6C4200-C589-11D0-999A-00C04FD655E1")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IShellIconOverlayIdentifier
    {
        [PreserveSig]
        int IsMemberOf([MarshalAs(UnmanagedType.LPWStr)] string iconFileBuffer,
                       [MarshalAs(UnmanagedType.U4)] int attributes);

        [PreserveSig]
        int GetOverlayInfo([MarshalAs(UnmanagedType.LPWStr)] out string iconFileBuffer,
                           int iconFileBufferSize,
                           out int iconIndex,
                           [MarshalAs(UnmanagedType.U4)] out uint flags);

        [PreserveSig]
        int GetPriority(out int priority);
    }
}