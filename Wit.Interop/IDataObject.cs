using System;
using System.Runtime.InteropServices;

namespace Wit.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FormatEtc
    {
        public ClipFormat cfFormat;
        public uint ptd;
        public DVAspect dwAspect;
        public int lindex;
        public Tymed tymed;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct StgMedium
    {
        public uint tymed;
        public uint hGlobal;
        public uint pUnkForRelease;
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), GuidAttribute("0000010e-0000-0000-C000-000000000046")]
    public interface IDataObject
    {
        [PreserveSig]
        int GetData(ref FormatEtc a, ref StgMedium b);

        [PreserveSig]
        void GetDataHere(int a, ref StgMedium b);

        [PreserveSig]
        int QueryGetData(int a);

        [PreserveSig]
        int GetCanonicalFormatEtc(int a, ref int b);

        [PreserveSig]
        int SetData(int a, int b, int c);

        [PreserveSig]
        int EnumFormatEtc(uint a, ref Object b);

        [PreserveSig]
        int DAdvise(int a, uint b, Object c, ref uint d);

        [PreserveSig]
        int DUnadvise(uint a);

        [PreserveSig]
        int EnumDAdvise(ref Object a);
    }
}