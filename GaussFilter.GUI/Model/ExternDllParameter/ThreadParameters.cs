using System.Runtime.InteropServices;

namespace GaussFilter.Model.ExternDllParameters
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ThreadParameters
    {
        public int ProcessId;
        public int GaussMaskSize;
        public int CurrentImgOffset;
        public int ImgWidth;
        public int ImgHeight;
        public int IdOfImgPart;
        public int NumOfImgParts;
        public unsafe uint* ImgByteArrayPtr;
        public unsafe uint* TempImgByteArrayPtr;
    }
}
