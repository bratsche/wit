using System;
using System.Runtime.InteropServices;

namespace Wit
{
    public enum MIIM : uint
    {
        State       = 0x00000001,
        ID          = 0x00000002,
        SubMenu     = 0x00000004,
        CheckMarks  = 0x00000008,
        Type        = 0x00000010,
        Data        = 0x00000020,
        String      = 0x00000040,
        Bitmap      = 0x00000080,
        FType       = 0x00000100
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MenuItemInfo
    {
        public uint cbSize;
        public uint fMask;
        public uint fType;
        public uint fState;
        public int wID;
        public int	/*HMENU*/	  hSubMenu;
        public int	/*HBITMAP*/   hbmpChecked;
        public int	/*HBITMAP*/	  hbmpUnchecked;
        public int	/*ULONG_PTR*/ dwItemData;
        public String dwTypeData;
        public uint cch;
        public int /*HBITMAP*/ hbmpItem;
    }
}