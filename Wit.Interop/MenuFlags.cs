using System;

namespace Wit.Interop
{
    // XXX: I'm not sure if this is right.  This may need to be
    //      split into different enums.
	public enum	MenuFlags : uint
	{
		Insert =        0x00000000,
		Change =        0x00000080,
		Append =        0x00000100,
		Delete =        0x00000200,
		Remove =        0x00001000,
		ByCommand =     0x00000000,
		ByPosition =    0x00000400,
		Separator =     0x00000800,
		Enabled =       0x00000000,
		Grayed =        0x00000001,
		Disabled =      0x00000002,
		Unchecked =     0x00000000,
		Checked =       0x00000008,
		UseCheckBitmaps=0x00000200,
		String =        0x00000000,
		Bitmap =        0x00000004,
		OwnerDraw =     0x00000100,
		Popup =         0x00000010,
		MenubarBreak =  0x00000020,
		MenuBreak =     0x00000040,
		Unhilite =      0x00000000,
		Hilite =        0x00000080,
		Default =       0x00001000,
		SysMenu =       0x00002000,
		Help =          0x00004000,
		RightJustify =  0x00004000,
		MouseSelect =   0x00008000
	}
}
