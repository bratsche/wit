using System;

namespace Wit.Interop
{
    public enum ClipFormat : uint
    {
        Text = 1,
        Bitmap = 2,
        MetafilePict = 3,
        SYLK = 4,
        DIF = 5,
        TIFF = 6,
        OEMText = 7,
        DIB = 8,
        Palette = 9,
        PenData = 10,
        RIFF = 11,
        Wave = 12,
        UnicodeText = 13,
        EnhMetafile = 14,
        HDrop = 15,
        Locale = 16,
        Max = 17,

        OwnerDisplay = 0x0080,
        DspText = 0x0081,
        DispBitmap = 0x0082,
        DspMetafilePict = 0x0083,
        DspEnhMetafile = 0x008E,

        PrivateFirst = 0x0200,
        PrivateLast = 0x02FF,

        GdiObjFirst = 0x0300,
        GdiObjLast = 0x03FF
    }
}