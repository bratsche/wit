using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Wit.Interop
{
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), GuidAttribute("000214e4-0000-0000-c000-000000000046")]
    public interface IContextMenu
    {
        [PreserveSig]
        int QueryContextMenu(uint hmenu,
                             uint iMenu,
                             int idCmdFirst,
                             int idCmdLast,
                             uint uFlags);

        [PreserveSig]
        void InvokeCommand([In] ref CommandInfo pici);

        [PreserveSig]
        void GetCommandString(int idcmd,
                              GetCommandStringFlags uflags,
                              int reserved,
                              StringBuilder commandstring,
                              int cch);
    }
}