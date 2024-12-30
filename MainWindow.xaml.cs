using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.Storage;
using System.Reflection;
using Microsoft.UI;
using System.Runtime.InteropServices;

using GlobalStructures;
using static GlobalStructures.GlobalTools;
using Direct2D;
using static Direct2D.D2DTools;
using DXGI;
using static DXGI.DXGITools;
using WIC;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;
using Windows.Devices.Enumeration;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices.Marshalling;

// Reference :
// https://github.com/microsoft/Windows-classic-samples/blob/main/Samples/D2DPrintingFromDesktopApps/cpp/D2DPrintingFromDesktopApps.cpp
// E:\Sources\Windows-classic-samples\Samples\D2DPrintingFromDesktopApps\cpp

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUI3_Direct2D_Print
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern uint GetDpiForWindow(IntPtr hwnd);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class PRINTDLG
        {
            public int lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hDevMode;
            public IntPtr hDevNames;
            public IntPtr hDC;
            public int Flags;
            public short nFromPage;
            public short nToPage;
            public short nMinPage;
            public short nMaxPage;
            public short nCopies;
            public IntPtr hInstance;
            public IntPtr lCustData;
            public IntPtr lpfnPrintHook;
            public IntPtr lpfnSetupHook;
            public string lpPrintTemplateName;
            public string lpSetupTemplateName;
            public IntPtr hPrintTemplate;
            public IntPtr hSetupTemplate;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        public class PRINTDLGX86
        {
            public int lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hDevMode;
            public IntPtr hDevNames;
            public IntPtr hDC;
            public int Flags;
            public short nFromPage;
            public short nToPage;
            public short nMinPage;
            public short nMaxPage;
            public short nCopies;
            public IntPtr hInstance;
            public IntPtr lCustData;
            public IntPtr lpfnPrintHook;
            public IntPtr lpfnSetupHook;
            public string lpPrintTemplateName;
            public string lpSetupTemplateName;
            public IntPtr hPrintTemplate;
            public IntPtr hSetupTemplate;
        }

        [DllImport("Comdlg32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool PrintDlg([In, Out] PRINTDLG lppd);

        [DllImport("Comdlg32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool PrintDlg([In, Out] PRINTDLGX86 lppd);

        public delegate IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class PRINTDLGEX
        {
            public uint lStructSize;

            public IntPtr hwndOwner;
            public IntPtr hDevMode;
            public IntPtr hDevNames;
            public IntPtr hDC;

            public uint Flags;
            public uint Flags2;

            public uint ExclusionFlags;

            public uint nPageRanges;
            public uint nMaxPageRanges;

            public IntPtr pageRanges;

            public uint nMinPage;
            public uint nMaxPage;
            public uint nCopies;

            public IntPtr hInstance;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpPrintTemplateName;

            public WndProc lpCallback = null;
            public uint nPropertyPages;
            public IntPtr lphPropertyPages;
            public uint nStartPage;
            public uint dwResultAction;
        }

        public const uint START_PAGE_GENERAL = 0xffffffff;

        [DllImport("Comdlg32.dll", SetLastError = true, CharSet = CharSet.Unicode)]       
        public static extern HRESULT PrintDlgEx([In, Out] PRINTDLGEX lppdex);

        public const int PD_ALLPAGES = 0x00000000;
        public const int PD_SELECTION = 0x00000001;
        public const int PD_PAGENUMS = 0x00000002;
        public const int PD_NOSELECTION = 0x00000004;
        public const int PD_NOPAGENUMS = 0x00000008;
        public const int PD_COLLATE = 0x00000010;
        public const int PD_PRINTTOFILE = 0x00000020;
        public const int PD_PRINTSETUP = 0x00000040;
        public const int PD_NOWARNING = 0x00000080;
        public const int PD_RETURNDC = 0x00000100;
        public const int PD_RETURNIC = 0x00000200;
        public const int PD_RETURNDEFAULT = 0x00000400;
        public const int PD_SHOWHELP = 0x00000800;
        public const int PD_ENABLEPRINTHOOK = 0x00001000;
        public const int PD_ENABLESETUPHOOK = 0x00002000;
        public const int PD_ENABLEPRINTTEMPLATE = 0x00004000;
        public const int PD_ENABLESETUPTEMPLATE = 0x00008000;
        public const int PD_ENABLEPRINTTEMPLATEHANDLE = 0x00010000;
        public const int PD_ENABLESETUPTEMPLATEHANDLE = 0x00020000;
        public const int PD_USEDEVMODECOPIES = 0x00040000;
        public const int PD_USEDEVMODECOPIESANDCOLLATE = 0x00040000;
        public const int PD_DISABLEPRINTTOFILE = 0x00080000;
        public const int PD_HIDEPRINTTOFILE = 0x00100000;
        public const int PD_NONETWORKBUTTON = 0x00200000;
        public const int PD_CURRENTPAGE = 0x00400000;
        public const int PD_NOCURRENTPAGE = 0x00800000;
        public const int PD_EXCLUSIONFLAGS = 0x01000000;
        public const int PD_USELARGETEMPLATE = 0x10000000;
        //  Exclusion flags for PrintDlgEx.
        public const int PD_EXCL_COPIESANDCOLLATE = ((int)(DevModeFields.DM_COPIES | DevModeFields.DM_COLLATE));

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

        public const int PW_CLIENTONLY = 0x1;
        public const int PW_RENDERFULLCONTENT = 0x2;

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateDC(string pwszDriver, string pwszDevice, string pszPort, IntPtr pdm);

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool DeleteDC(IntPtr hdc);

        public const int SRCCOPY = 0x00CC0020; /* dest = source                   */
        public const int SRCPAINT = 0x00EE0086; /* dest = source OR dest           */
        public const int SRCAND = 0x008800C6; /* dest = source AND dest          */
        public const int SRCINVERT = 0x00660046; /* dest = source XOR dest          */
        public const int SRCERASE = 0x00440328; /* dest = source AND (NOT dest )   */
        public const int NOTSRCCOPY = 0x00330008; /* dest = (NOT source)             */
        public const int NOTSRCERASE = 0x001100A6; /* dest = (NOT src) AND (NOT dest) */
        public const int MERGECOPY = 0x00C000CA; /* dest = (source AND pattern)     */
        public const int MERGEPAINT = 0x00BB0226; /* dest = (NOT source) OR dest     */
        public const int PATCOPY = 0x00F00021; /* dest = pattern                  */
        public const int PATPAINT = 0x00FB0A09; /* dest = DPSnoo                   */
        public const int PATINVERT = 0x005A0049; /* dest = pattern XOR dest         */
        public const int DSTINVERT = 0x00550009; /* dest = (NOT dest)               */
        public const int BLACKNESS = 0x00000042; /* dest = BLACK                    */
        public const int WHITENESS = 0x00FF0062; /* dest = WHITE                    */
        public const int NOMIRRORBITMAP = unchecked((int)0x80000000); /* Do not Mirror the bitmap in this call */
        public const int CAPTUREBLT = 0x40000000; /* Include layered windows */

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DEVNAMES
        {
            public short wDriverOffset;
            public short wDeviceOffset;
            public short wOutputOffset;
            public short wDefault;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DEVMODE
        {
            private const int CCHDEVICENAME = 32;
            private const int CCHFORMNAME = 32;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public DevModeFields dmFields;

            //public int dmPositionX;
            //public int dmPositionY;
            //public int dmDisplayOrientation;
            //public int dmDisplayFixedOutput;

            private DEVMODE_U1 Union;

            [StructLayout(LayoutKind.Explicit)]
            private struct DEVMODE_U1
            {
                [FieldOffset(0)]
                public short dmOrientation;

                [FieldOffset(2)]
                public short dmPaperSize;

                [FieldOffset(4)]
                public short dmPaperLength;

                [FieldOffset(6)]
                public short dmPaperWidth;

                [FieldOffset(8)]
                public short dmScale;

                [FieldOffset(10)]
                public short dmCopies;

                [FieldOffset(12)]
                public short dmDefaultSource;

                [FieldOffset(14)]
                public short dmPrintQuality;

                [FieldOffset(0)]
                public POINT dmPosition;

                [FieldOffset(8)]
                public uint dmDisplayOrientation;

                [FieldOffset(12)]
                public uint dmDisplayFixedOutput;
            }

            public short dmOrientation { get => Union.dmOrientation; set => Union.dmOrientation = value; }
            public short dmPaperSize { get => Union.dmPaperSize; set => Union.dmPaperSize = value; }
            public short dmPaperLength { get => Union.dmPaperLength; set => Union.dmPaperLength = value; }
            public short dmPaperWidth { get => Union.dmPaperWidth; set => Union.dmPaperWidth = value; }
            public short dmScale { get => Union.dmScale; set => Union.dmScale = value; }
            public short dmCopies { get => Union.dmCopies; set => Union.dmCopies = value; }
            public short dmDefaultSource { get => Union.dmDefaultSource; set => Union.dmDefaultSource = value; }
            public short dmPrintQuality { get => Union.dmPrintQuality; set => Union.dmPrintQuality = value; }
            public POINT dmPosition { get => Union.dmPosition; set => Union.dmPosition = value; }
            public uint dmDisplayOrientation { get => Union.dmDisplayOrientation; set => Union.dmDisplayOrientation = value; }
            public uint dmDisplayFixedOutput { get => Union.dmDisplayFixedOutput; set => Union.dmDisplayFixedOutput = value; }

            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        } 

        [Flags]
        public enum DevModeFields : uint
        {
            DM_ORIENTATION = (uint)0x00000001,
            DM_PAPERSIZE = (uint)0x00000002,
            DM_PAPERLENGTH = (uint)0x00000004,
            DM_PAPERWIDTH = (uint)0x00000008,
            DM_SCALE = (uint)0x00000010,
            DM_POSITION = (uint)0x00000020,
            DM_NUP = (uint)0x00000040,
            DM_DISPLAYORIENTATION = (uint)0x00000080,
            DM_COPIES = (uint)0x00000100,
            DM_DEFAULTSOURCE = (uint)0x00000200,
            DM_PRINTQUALITY = (uint)0x00000400,
            DM_COLOR = (uint)0x00000800,
            DM_DUPLEX = (uint)0x00001000,
            DM_YRESOLUTION = (uint)0x00002000,
            DM_TTOPTION = (uint)0x00004000,
            DM_COLLATE = (uint)0x00008000,
            DM_FORMNAME = (uint)0x00010000,
            DM_LOGPIXELS = (uint)0x00020000,
            DM_BITSPERPEL = (uint)0x00040000,
            DM_PELSWIDTH = (uint)0x00080000,
            DM_PELSHEIGHT = (uint)0x00100000,
            DM_DISPLAYFLAGS = (uint)0x00200000,
            DM_DISPLAYFREQUENCY = (uint)0x00400000,
            DM_ICMMETHOD = (uint)0x00800000,
            DM_ICMINTENT = (uint)0x01000000,
            DM_MEDIATYPE = (uint)0x02000000,
            DM_DITHERTYPE = (uint)0x04000000,
            DM_DISPLAYFIXEDOUTPUT = (uint)0x20000000,
            All = (uint)0xFFFFFFFF
        }

        public const int DMORIENT_PORTRAIT = 1;
        public const int DMORIENT_LANDSCAPE = 2;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GlobalLock(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GlobalFree(IntPtr hMem);

        [DllImport("Prntvpt.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern HRESULT PTConvertDevModeToPrintTicket(IntPtr hProvider, uint cbDevmode, IntPtr pDevmode, EPrintTicketScope scope,
            System.Runtime.InteropServices.ComTypes.IStream pPrintTicket);

        [DllImport("Prntvpt.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern HRESULT PTConvertDevModeToPrintTicket(IntPtr hProvider, uint cbDevmode, ref DEVMODE pDevmode, EPrintTicketScope scope,
           System.Runtime.InteropServices.ComTypes.IStream pPrintTicket);

        public enum EPrintTicketScope
        {
            kPTPageScope,
            kPTDocumentScope,
            kPTJobScope
        }

        [DllImport("Prntvpt.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern HRESULT PTOpenProvider(string pszPrinterName, uint dwVersion, out IntPtr phProvider);

        [DllImport("Prntvpt.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern HRESULT PTCloseProvider(IntPtr hProvider);

        [DllImport("Ole32.dll", SetLastError = true)]
        static extern HRESULT CreateStreamOnHGlobal(IntPtr hGlobal, bool fDeleteOnRelease, out System.Runtime.InteropServices.ComTypes.IStream ppstm);

        public const int GMEM_FIXED = 0x0000;
        public const int GMEM_ZEROINIT = 0x0040;

        [ComImport]
        [Guid("d2959bf7-b31b-4a3d-9600-712eb1335ba4")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPrintDocumentPackageTargetFactory
        {
           HRESULT CreateDocumentPackageTargetForPrintJob(string printerName,string  jobName,
               System.Runtime.InteropServices.ComTypes.IStream jobOutputStream, System.Runtime.InteropServices.ComTypes.IStream jobPrintTicketStream,
               out IPrintDocumentPackageTarget docPackageTarget);
        }

        [ComImport]
        [Guid("1b8efec4-3019-4c27-964e-367202156906")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPrintDocumentPackageTarget
        {
            HRESULT GetPackageTargetTypes(out uint targetCount, out /*GUID ***/ IntPtr targetTypes);
            HRESULT GetPackageTarget(ref Guid guidTargetType, ref Guid riid, out IntPtr ppvTarget);
            HRESULT Cancel();
        }

        [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern HRESULT SHCreateStreamOnFile(string pszFile, int grfMode, out System.Runtime.InteropServices.ComTypes.IStream ppstm);

        public const long STGM_READ = 0x00000000L;
        public const long STGM_WRITE = 0x00000001L;
        public const long GENERIC_READ = (0x80000000L);
        public const long GENERIC_WRITE = (0x40000000L);
        public const int CREATE_NEW = 1;
        public const int CREATE_ALWAYS = 2;
        public const int OPEN_EXISTING = 3;

        [DllImport("msoert2.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern HRESULT WriteStreamToFile(System.Runtime.InteropServices.ComTypes.IStream pstm,
            string lpszFile, uint dwCreationDistribution, uint dwAccess);

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        public const int HORZSIZE = 4; /* Horizontal size in millimeters */
        public const int VERTSIZE = 6; /* Vertical size in millimeters */
        public const int HORZRES = 8; /* Horizontal width in pixels */
        public const int VERTRES = 10; /* Vertical height in pixels */
        public const int PHYSICALWIDTH = 110; /* Physical Width in device units */
        public const int PHYSICALHEIGHT = 111; /* Physical Height in device units */
        public const int PHYSICALOFFSETX = 112; /* Physical Printable Area x margin */
        public const int PHYSICALOFFSETY = 113; /* Physical Printable Area y margin */
        public const int LOGPIXELSX = 88;   /* Logical pixels/inch in X */
        public const int LOGPIXELSY = 90;   /* Logical pixels/inch in Y */

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFOHEADER
        {
            [MarshalAs(UnmanagedType.I4)]
            public int biSize;
            [MarshalAs(UnmanagedType.I4)]
            public int biWidth;
            [MarshalAs(UnmanagedType.I4)]
            public int biHeight;
            [MarshalAs(UnmanagedType.I2)]
            public short biPlanes;
            [MarshalAs(UnmanagedType.I2)]
            public short biBitCount;
            [MarshalAs(UnmanagedType.I4)]
            public int biCompression;
            [MarshalAs(UnmanagedType.I4)]
            public int biSizeImage;
            [MarshalAs(UnmanagedType.I4)]
            public int biXPelsPerMeter;
            [MarshalAs(UnmanagedType.I4)]
            public int biYPelsPerMeter;
            [MarshalAs(UnmanagedType.I4)]
            public int biClrUsed;
            [MarshalAs(UnmanagedType.I4)]
            public int biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFO
        {
            [MarshalAs(UnmanagedType.Struct, SizeConst = 40)]
            public BITMAPINFOHEADER bmiHeader;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
            public int[] bmiColors;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPV5HEADER
        {
            public int bV5Size;
            public int bV5Width;
            public int bV5Height;
            public short bV5Planes;
            public short bV5BitCount;
            public int bV5Compression;
            public int bV5SizeImage;
            public int bV5XPelsPerMeter;
            public int bV5YPelsPerMeter;
            public int bV5ClrUsed;
            public int bV5ClrImportant;
            public int bV5RedMask;
            public int bV5GreenMask;
            public int bV5BlueMask;
            public int bV5AlphaMask;
            public int bV5CSType;
            public CIEXYZTRIPLE bV5Endpoints;
            public int bV5GammaRed;
            public int bV5GammaGreen;
            public int bV5GammaBlue;
            public int bV5Intent;
            public int bV5ProfileData;
            public int bV5ProfileSize;
            public int bV5Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CIEXYZTRIPLE
        {
            public CIEXYZ ciexyzRed;
            public CIEXYZ ciexyzGreen;
            public CIEXYZ ciexyzBlue;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CIEXYZ
        {
            public int ciexyzX;
            public int ciexyzY;
            public int ciexyzZ;
        }

        public const int BI_RGB = 0;
        public const int BI_RLE8 = 1;
        public const int BI_RLE4 = 2;
        public const int BI_BITFIELDS = 3;
        public const int BI_JPEG = 4;
        public const int BI_PNG = 5;

        public const int DIB_RGB_COLORS = 0;
        public const int DIB_PAL_COLORS = 1;

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFO pbmi, uint usage, ref IntPtr ppvBits, IntPtr hSection, int offset);

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPV5HEADER pbmi, uint usage, ref IntPtr ppvBits, IntPtr hSection, int offset);

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("WinSpool.drv", SetLastError = true, CharSet = CharSet.Unicode)]      
        public static extern int DeviceCapabilities(string pDevice, [Optional] string pPort, short fwCapability, IntPtr pOutput, IntPtr pDevMode);

        public const int DC_PAPERS = 2;
        public const int DC_PAPERSIZE = 3;
        public const int DC_MINEXTENT = 4;
        public const int DC_MAXEXTENT = 5;
        public const int DC_BINS = 6;
        public const int DC_DUPLEX = 7;
        public const int DC_SIZE = 8;
        public const int DC_EXTRA = 9;
        public const int DC_VERSION = 10;
        public const int DC_DRIVER = 11;
        public const int DC_BINNAMES = 12;
        public const int DC_ENUMRESOLUTIONS = 13;
        public const int DC_FILEDEPENDENCIES = 14;
        public const int DC_TRUETYPE = 15;
        public const int DC_PAPERNAMES = 16;
        public const int DC_ORIENTATION = 17;
        public const int DC_COPIES = 18;
        public const int DC_BINADJUST = 19;
        public const int DC_EMF_COMPLIANT = 20;
        public const int DC_DATATYPE_PRODUCED = 21;
        public const int DC_COLLATE = 22;
        public const int DC_MANUFACTURER = 23;
        public const int DC_MODEL = 24;
        public const int DC_PERSONALITY = 25;
        public const int DC_PRINTRATE = 26;
        public const int DC_PRINTRATEUNIT = 27;
        public const int PRINTRATEUNIT_PPM = 1;
        public const int PRINTRATEUNIT_CPS = 2;
        public const int PRINTRATEUNIT_LPM = 3;
        public const int PRINTRATEUNIT_IPM = 4;
        public const int DC_PRINTERMEM = 28;
        public const int DC_MEDIAREADY = 29;
        public const int DC_STAPLE = 30;
        public const int DC_PRINTRATEPPM = 31;
        public const int DC_COLORDEVICE = 32;
        public const int DC_NUP = 33;
        public const int DC_MEDIATYPENAMES = 34;
        public const int DC_MEDIATYPES = 35;

        [DllImport("WinSpool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool GetPrinterDriver(IntPtr hPrinter, string pEnvironment, uint Level, IntPtr pDriverInfo, uint cbBuf,  out uint pcbNeeded);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DRIVER_INFO_6
        {
            public uint cVersion;
            public string pName;
            public string pEnvironment;
            public string pDriverPath;
            public string pDataFile;
            public string pConfigFile;
            public string pHelpFile;
            public IntPtr pDependentFiles;
            public string pMonitorName;
            public string pDefaultDataType;
            public string pszzPreviousNames;
            public FILETIME ftDriverDate;
            public UInt64 dwlDriverVersion;
            public string pszMfgName;
            public string pszOEMUrl;
            public string pszHardwareID;
            public string pszProvider;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct FILETIME
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        }

        [DllImport("Winspool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int DocumentProperties(IntPtr hwnd, IntPtr hPrinter, string pDeviceName, IntPtr /*DEVMODE*/ pDevModeOutput, IntPtr /*DEVMODE*/ pDevModeInput, int fMode);

        [DllImport("Winspool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int DocumentProperties(IntPtr hWnd, IntPtr hPrinter, string pDeviceName, ref DEVMODE pDevModeOutput, ref DEVMODE pDevModeInput, int fMode);

        public const int DM_UPDATE = 1;
        public const int DM_COPY = 2;
        public const int DM_PROMPT = 4;
        public const int DM_MODIFY = 8;

        public const int DM_IN_BUFFER = DM_MODIFY;
        public const int DM_IN_PROMPT = DM_PROMPT;
        public const int DM_OUT_BUFFER = DM_COPY;
        public const int DM_OUT_DEFAULT = DM_UPDATE;

        [DllImport("WinSpool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, ref PRINTER_DEFAULTS pDefault);

        [DllImport("WinSpool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

        [Flags]
        public enum PRINTER_ACCESS_MASK : uint
        {
            PRINTER_ACCESS_ADMINISTER = 0x00000004,
            PRINTER_ACCESS_USE = 0x00000008,
            PRINTER_ACCESS_MANAGE_LIMITED = 0x00000040,
            PRINTER_ALL_ACCESS = 0x000F000C,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct PRINTER_DEFAULTS
        {
            public string pDatatype;

            private IntPtr pDevMode;

            public PRINTER_ACCESS_MASK DesiredPrinterAccess;
        }

        [DllImport("WinSpool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOZORDER = 0x0004;
        public const int SWP_NOREDRAW = 0x0008;
        public const int SWP_NOACTIVATE = 0x0010;
        public const int SWP_FRAMECHANGED = 0x0020;  /* The frame changed: send WM_NCCALCSIZE */
        public const int SWP_SHOWWINDOW = 0x0040;
        public const int SWP_HIDEWINDOW = 0x0080;
        public const int SWP_NOCOPYBITS = 0x0100;
        public const int SWP_NOOWNERZORDER = 0x0200;  /* Don't do owner Z ordering */
        public const int SWP_NOSENDCHANGING = 0x0400;  /* Don't send WM_WINDOWPOSCHANGING */
        public const int SWP_DRAWFRAME = SWP_FRAMECHANGED;
        public const int SWP_NOREPOSITION = SWP_NOOWNERZORDER;
        public const int SWP_DEFERERASE = 0x2000;
        public const int SWP_ASYNCWINDOWPOS = 0x4000;

        public delegate int SUBCLASSPROC(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, uint dwRefData);

        [DllImport("Comctl32.dll", SetLastError = true)]
        public static extern bool SetWindowSubclass(IntPtr hWnd, SUBCLASSPROC pfnSubclass, uint uIdSubclass, uint dwRefData);

        [DllImport("Comctl32.dll", SetLastError = true)]
        public static extern int DefSubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);       

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        public const int WM_USER = 0x0400;
        public const int WM_CLOSE = 0x0010;

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern uint SetTimer(IntPtr hWnd, uint nIDEvent, uint uElapse, IntPtr lpTimerFunc);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool KillTimer(IntPtr hWnd, uint uIDEvent);

        public const int WM_TIMER = 0x0113;


        public const float PAGE_WIDTH_IN_DIPS = 8.5f * 96.0f;     // 8.5 inches
        public const float PAGE_HEIGHT_IN_DIPS = 11.0f * 96.0f;   // 11 inches

        public float m_pageWidth = 0.0f;
        public float m_pageHeight = 0.0f;

        ID2D1Factory m_pD2DFactory = null;
        ID2D1Factory1 m_pD2DFactory1 = null;
        IWICImagingFactory m_pWICImagingFactory = null;
        IWICImagingFactory2 m_pWICImagingFactory2 = null;

        IntPtr m_pD3D11DevicePtr = IntPtr.Zero;
        ID3D11DeviceContext m_pD3D11DeviceContext = null; // Released in Clean : not used
        IDXGIDevice1 m_pDXGIDevice = null;
        ID2D1Device m_pD2DDevice = null;

        ID2D1DeviceContext m_pD2DDeviceContext = null;

        IDXGISwapChain1 m_pDXGISwapChain1 = null;
        ID2D1Bitmap1 m_pD2DTargetBitmap = null;

        IntPtr hWndMain;
        private Microsoft.UI.Windowing.AppWindow _apw;

        public D2D1_COLOR_F m_BackColor = null;
        public System.Collections.ObjectModel.ObservableCollection<String> Printers = new System.Collections.ObjectModel.ObservableCollection<String>();
        public Windows.Media.Playback.MediaPlayer m_mp = new Windows.Media.Playback.MediaPlayer();

        public double m_nXPos, m_nYPos, m_nWidth, m_nHeight = 0;

        //private SUBCLASSPROC SubClassDelegate;

        public MainWindow()
        {
            this.InitializeComponent();
            hWndMain = WinRT.Interop.WindowNative.GetWindowHandle(this);
            Microsoft.UI.WindowId myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWndMain);
            _apw = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(myWndId);
            this.Title = "WinUI 3 - Test ID2D1PrintControl";

            Application.Current.Resources["ComboBoxBackgroundPointerOver"] = new SolidColorBrush(Microsoft.UI.Colors.RoyalBlue);
            Application.Current.Resources["ComboBoxItemBackgroundSelected"] = new SolidColorBrush(Microsoft.UI.Colors.RoyalBlue);
            Application.Current.Resources["ComboBoxItemBackgroundPointerOver"] = new SolidColorBrush(Microsoft.UI.Colors.BlueViolet);

            App.Current.Resources["ButtonBackgroundPressed"] = App.Current.Resources["SystemAccentColor"];
            App.Current.Resources["TextControlBackgroundFocused"] = new SolidColorBrush(Colors.White);
            App.Current.Resources["TextControlForegroundFocused"] = new SolidColorBrush(Colors.Black);
            App.Current.Resources["TextControlBackgroundPointerOver"] = new SolidColorBrush(Colors.White);
            App.Current.Resources["TextControlForegroundPointerOver"] = new SolidColorBrush(Colors.Black);

            double nDisplayWidth = Microsoft.UI.Windowing.DisplayArea.Primary.WorkArea.Width;
            double nDisplayHeight = Microsoft.UI.Windowing.DisplayArea.Primary.WorkArea.Height;
            m_nWidth = 1500;
            m_nHeight = 1000;
            m_nXPos = (nDisplayWidth - m_nWidth) / 2;
            m_nYPos = (nDisplayHeight - m_nHeight) / 2;
            this.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32((int)(m_nXPos), (int)(m_nYPos), (int)(m_nWidth), (int)(m_nHeight)));

            reb1.Loaded += Reb1_Loaded;

            //bug https://github.com/microsoft/microsoft-ui-xaml/issues/10071

            //reb1.AddHandler(RichEditBox.PointerPressedEvent, new PointerEventHandler(Reb1_PointerPressed), true);
            reb1.SelectionChanged += (sender, args) =>
            {
                var reb = (RichEditBox)sender;
                var childElement = FindChildElementByName(reb, "ContentElement");
                if (childElement != null)
                {
                    var sw = (ScrollViewer)childElement;
                    var h = sw.HorizontalOffset;
                    var v = sw.VerticalOffset;
                    sw.ChangeView(h, v + 1, 1.0f, true);
                    sw.ChangeView(h, v, 1.0f, true);                  
                }
            };

            //iv1.Loaded += Iv1_Loaded;
            lv1.Loaded += Lv1_Loaded;
            this.Closed += MainWindow_Closed;

            m_pWICImagingFactory = (IWICImagingFactory)Activator.CreateInstance(Type.GetTypeFromCLSID(WICTools.CLSID_WICImagingFactory));
            m_pWICImagingFactory2 = (IWICImagingFactory2)m_pWICImagingFactory;
            HRESULT hr = CreateD2D1Factory();
            if (hr == HRESULT.S_OK)
            {
                hr = CreateDeviceContext();
                //hr = CreateDeviceResources();
                //hr = CreateSwapChain(IntPtr.Zero);
                //if (hr == HRESULT.S_OK)
                //{
                //    hr = ConfigureSwapChain(hWndMain);
                //    //ISwapChainPanelNative panelNative = WinRT.CastExtensions.As<ISwapChainPanelNative>(scpD2D);
                //    //hr = panelNative.SetSwapChain(m_pDXGISwapChain1);
                //}
                //scpD2D.SizeChanged += scpD2D_SizeChanged;
                //CompositionTarget.Rendering += CompositionTarget_Rendering;
            }
            FillPrinters(cbPrinters);
            LoadMP3("Assets\\PrintingComplete.mp3");
        }

        //private void Reb1_PointerPressed(object sender, PointerRoutedEventArgs e)
        //{
        //    Console.Beep(6000, 10);
        //}

        private async void FillPrinters(ComboBox cb)
        {
            string sPrinterSelector = "System.Devices.InterfaceClassGuid:=\"{0ecef634-6ef0-472a-8085-5ad023ecbccd}\"";
            var deviceInfos = await DeviceInformation.FindAllAsync(sPrinterSelector);          
            string sDefaultPrinterName = null;
            foreach (var deviceInfo in deviceInfos)
            { 
                Printers.Add(deviceInfo.Name);
                if (deviceInfo.IsDefault)
                {
                    sDefaultPrinterName = deviceInfo.Name;
                }
                if (!string.IsNullOrEmpty(sDefaultPrinterName))
                {
                    cb.SelectedItem = Printers.FirstOrDefault(printer => printer == sDefaultPrinterName);
                }
            }
        }

        private async void LoadMP3(string sRelativePath)
        {
            string sAbsolutePath = Path.Combine(AppContext.BaseDirectory, sRelativePath);
            StorageFile sfFile = await StorageFile.GetFileFromPathAsync(sAbsolutePath);
            m_mp.Source = Windows.Media.Core.MediaSource.CreateFromStorageFile(sfFile);
        }

        private void Iv1_Loaded(object sender, RoutedEventArgs e)
        {
            ItemsView iv = sender as ItemsView;
            List<CustomDataObject> tempList = CustomDataObject.GetDataObjects("Assets\\Animals");
            ObservableCollection<CustomDataObject> Items = new ObservableCollection<CustomDataObject>(tempList);
            if (iv != null)
            {
                iv.ItemsSource = Items;
            }
        }

        private void Lv1_Loaded(object sender, RoutedEventArgs e)
        {
            ListView lv = sender as ListView;
            List<CustomDataObject> tempList = CustomDataObject.GetDataObjects("Assets\\Cars");
            ObservableCollection<CustomDataObject> Items = new ObservableCollection<CustomDataObject>(tempList);
            if (lv != null)
            {
                lv.ItemsSource = Items;
            }
        }

        private void Reb1_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRTF(sender as RichEditBox, "Assets\\sample-5.rtf");
            borderRE1.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Red);
            borderIMG1.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray);
            borderLV1.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray);
        }

        public async void LoadRTF(RichEditBox reb, string sRelativePath)
        {
            string sExePath = AppContext.BaseDirectory;
            string sFilePath = System.IO.Path.Combine(sExePath, sRelativePath);
            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(sFilePath);
            //StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/sample-4.rtf"));          
            using (IRandomAccessStream ras = await file.OpenAsync(FileAccessMode.Read))
            {
                reb.Document.LoadFromStream(Microsoft.UI.Text.TextSetOptions.FormatRtf, ras);
            }
        }

        private IntPtr m_pBits = IntPtr.Zero;
        private RenderTargetBitmap m_rtb = new RenderTargetBitmap();

        //private double m_nWidthTemp = 0;
        //private double m_nHeightTemp = 0;
        //private IntPtr m_hWndTemp = IntPtr.Zero;
        //private Window m_WndTemp = null;

        private async void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!tsScale.IsOn && (double.IsNaN(nbScale.Value) || nbScale.Value == 0))
            {
                Windows.UI.Popups.MessageDialog md = new Windows.UI.Popups.MessageDialog("Scale must not be = 0", "Error");
                WinRT.Interop.InitializeWithWindow.Initialize(md, hWndMain);
                _ = await md.ShowAsync();
            }
            else
            {  
                string sSelectedItem = (cmbControls.SelectedItem as ComboBoxItem)?.Content.ToString();
                if (sSelectedItem == "RichEditBox")
                {
                    // ScrollViewer 
                    var childElement = FindChildElementByName(reb1, "ContentElement");
                    if (childElement != null)
                    {
                        var sw = (ScrollViewer)childElement;

                        // Temporary RichEditBox to copy the content of reb1 into rtb
                        RichEditBox reb2 = new RichEditBox();
                        reb1.Document.GetText(Microsoft.UI.Text.TextGetOptions.UseLf, out string sText);
                        var range = reb1.Document.GetRange(0, sText.Length);
                        range.GetText(Microsoft.UI.Text.TextGetOptions.FormatRtf, out string sFinalText);
                        reb2.Document.SetText(Microsoft.UI.Text.TextSetOptions.FormatRtf, sFinalText);
                        // Hide the temporary RichEditBox                        
                        reb2.Margin = new Thickness(-5000, -5000, 0, 0);
                        reb2.Width = reb1.ActualWidth;
                        //reb2.Height = reb1.ActualHeight + sw.ExtentHeight;
                        reb2.Height = sw.ExtentHeight;
                        reb2.Visibility = Visibility.Visible;
                        //((Grid)this.Content).Children.Add(reb2);
                        sp0.Children.Add(reb2);

                        await m_rtb.RenderAsync((UIElement)reb2);
                        sp0.Children.Remove(reb2);

                        SolidColorBrush backgroundBrush = reb1.Background as SolidColorBrush;
                        if (backgroundBrush != null)
                        {
                            Windows.UI.Color backgroundColor = backgroundBrush.Color;
                            m_BackColor = new ColorF(backgroundColor.R, backgroundColor.G, backgroundColor.B, 1.0f);
                        }
                        else
                            m_BackColor = new ColorF(ColorF.Enum.Black, 1.0f);

                        Print(false);
                    }
                    else
                    {
                        Windows.UI.Popups.MessageDialog md = new Windows.UI.Popups.MessageDialog("Could not find ContentElement in RichEditBox", "Error");
                        WinRT.Interop.InitializeWithWindow.Initialize(md, hWndMain);
                        _ = await md.ShowAsync();
                        return;
                    }
                }
                else if (sSelectedItem == "Image")
                {   
                    await m_rtb.RenderAsync((UIElement)img1);
                    m_BackColor = new ColorF(ColorF.Enum.Black, 1.0f);
                    Print(false);                   
                }
                //else if (sSelectedItem == "ItemsView")
                //{
                //    // ScrollView
                //    //  Grid
                //    //   ScrollPresenter
                //    //    ItemsRepeater
                //    //     ItemContainer
                //    //     ItemContainer
                //    //     
                //    {
                //        var childElement = FindChildElement<ItemsRepeater>(iv1);
                //        if (childElement != null)
                //        {
                //            var ir = (ItemsRepeater)childElement;

                //            double nHeightItemsRepeater = childElement.ActualHeight;
                //            double nOldWidth = iv1.ActualWidth;
                //            double nOldHeight = iv1.ActualHeight;
                //            iv1.Height = nHeightItemsRepeater;

                //            //iv1.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                //            //iv1.Arrange(new Rect(0, 0, childElement.ActualWidth, childElement.ActualHeight));
                //            //iv1.UpdateLayout();

                //            var nTotalSize = _apw.Size;
                //            var nContentWidth = ((FrameworkElement)this.Content).ActualWidth;
                //            var nContentHeight = ((FrameworkElement)this.Content).ActualHeight;
                //            double nBorderWidth = (nTotalSize.Width - nContentWidth) / 2;
                //            double nTitleBarHeight = nTotalSize.Height - nContentHeight - nBorderWidth * 2;
                //            //m_nWidthTemp = (int)childElement.ActualWidth + (int)nBorderWidth * 2;
                //            m_nHeightTemp = (int)childElement.ActualHeight + (int)nTitleBarHeight + (int)nBorderWidth * 2;
                //            m_nHeightTemp += mainGrid.RowDefinitions[0].ActualHeight;
                //            m_nWidthTemp = nTotalSize.Width;
                //            SetWindowPos(hWndMain, IntPtr.Zero, 0, 0, (int)m_nWidthTemp, (int)m_nHeightTemp, SWP_NOMOVE | SWP_NOZORDER | SWP_SHOWWINDOW | SWP_FRAMECHANGED | SWP_NOACTIVATE | SWP_NOSENDCHANGING);
                //            iv1.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                //            iv1.Arrange(new Rect(0, 0, childElement.ActualWidth, childElement.ActualHeight));
                //            iv1.UpdateLayout();

                //            DispatcherTimer timer = new DispatcherTimer();
                //            timer.Interval = TimeSpan.FromSeconds(2);
                //            timer.Tick += async (sender, args) =>
                //            {
                //                timer.Stop();     

                //                //Print(false);
                //                //iv1.Height = nOldHeight;
                //                //iv1.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                //                //iv1.Arrange(new Rect(0, 0, childElement.ActualWidth, childElement.ActualHeight));
                //                //iv1.UpdateLayout();

                //                //GetBitmapFromWindow(hWndMain, m_nWidthTemp, m_nHeightTemp);
                //                //await m_rtb.RenderAsync((UIElement)ir);

                //                await m_rtb.RenderAsync((UIElement)iv1);
                //                Print(false);
                //                _apw.Resize(new Windows.Graphics.SizeInt32(nTotalSize.Width, nTotalSize.Height));
                //                iv1.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                //                iv1.Arrange(new Rect(0, 0, nOldWidth, nOldHeight));
                //                iv1.UpdateLayout();
                //            };                               
                //            timer.Start();                           
                //        }
                //        else
                //        {
                //            Windows.UI.Popups.MessageDialog md = new Windows.UI.Popups.MessageDialog("Could not find ItemsRepeater in ItemsView", "Error");
                //            WinRT.Interop.InitializeWithWindow.Initialize(md, hWndMain);
                //            _ = await md.ShowAsync();
                //            return;
                //        }
                //        return;
                //    }

                //    if (1 == 0)
                //    {
                //        //var childElement = FindChildElementByName(iv1, "PART_ItemsRepeater");
                //        var childElement = FindChildElement<ItemsRepeater>(iv1);
                //        if (childElement != null)
                //        {
                //            var ir = (ItemsRepeater)childElement;
                //        }
                //        else
                //        {
                //            Windows.UI.Popups.MessageDialog md = new Windows.UI.Popups.MessageDialog("Could not find ItemsRepeater in ItemsView", "Error");
                //            WinRT.Interop.InitializeWithWindow.Initialize(md, hWndMain);
                //            _ = await md.ShowAsync();
                //            return;
                //        }

                //        //double nLineWidth = iv1.ActualWidth;
                //        var originalLayout = iv1.Layout as LinedFlowLayout;
                //        var newItemsView = new ItemsView();
                //        newItemsView.Background = iv1.Background;
                //        newItemsView.ItemsSource = iv1.ItemsSource;
                //        newItemsView.ItemTemplate = iv1.ItemTemplate;
                //        if (originalLayout != null)
                //        {
                //            var newLayout = new LinedFlowLayout
                //            {
                //                LineHeight = originalLayout.LineHeight,
                //                LineSpacing = originalLayout.LineSpacing,
                //                ItemsStretch = originalLayout.ItemsStretch,
                //                MinItemSpacing = originalLayout.MinItemSpacing
                //            };
                //            newItemsView.Layout = newLayout;
                //        }

                //        m_WndTemp = new Window()
                //        {
                //            Content = newItemsView
                //        };

                //        Windows.Foundation.TypedEventHandler<object, Microsoft.UI.Xaml.WindowEventArgs> closedHandler = null;
                //        closedHandler = (sender, e) =>
                //        {
                //            m_WndTemp.Closed -= closedHandler;
                //            //newItemsView.ItemsSource = null;                       
                //            //newItemsView.Layout = null;                        
                //            m_WndTemp = null;
                //        };
                //        m_WndTemp.Closed += closedHandler;

                //        m_hWndTemp = WinRT.Interop.WindowNative.GetWindowHandle(m_WndTemp);
                //        var nTotalSize = _apw.Size;
                //        var nContentWidth = ((FrameworkElement)this.Content).ActualWidth;
                //        var nContentHeight = ((FrameworkElement)this.Content).ActualHeight;
                //        double nBorderWidth = (nTotalSize.Width - nContentWidth) / 2;
                //        double nTitleBarHeight = nTotalSize.Height - nContentHeight - nBorderWidth * 2;
                //        m_nWidthTemp = (int)childElement.ActualWidth + (int)nBorderWidth * 2;
                //        m_nHeightTemp = (int)childElement.ActualHeight + (int)nTitleBarHeight + (int)nBorderWidth * 2;
                //        //wnd1.Activated += async (s, e) =>
                //        //{
                //        //    GetBitmap();
                //        //};

                //        //SubClassDelegate = new SUBCLASSPROC(WindowSubClass);
                //        //bool bRet = SetWindowSubclass(hWndTemp, SubClassDelegate, 0, 0);
                //        //if (bRet)
                //        {
                //            //m_WndTemp.Activate();
                //            //m_WndTemp.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(100, 100, (int)childElement.ActualWidth + (int)borderWidth*2, (int)childElement.ActualHeight));
                //            SetWindowPos(m_hWndTemp, IntPtr.Zero, -2000, 0, (int)m_nWidthTemp, (int)m_nHeightTemp, SWP_NOZORDER | SWP_SHOWWINDOW | SWP_FRAMECHANGED | SWP_NOACTIVATE | SWP_NOSENDCHANGING);
                //            newItemsView.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                //            newItemsView.Arrange(new Rect(0, 0, childElement.ActualWidth, childElement.ActualHeight));
                //            newItemsView.UpdateLayout();
                //            //PostMessage(hWndTemp, WM_USER + 100, IntPtr.Zero, IntPtr.Zero);
                //            //SendMessage(hWndTemp, WM_USER + 100, IntPtr.Zero, IntPtr.Zero);

                //            DispatcherTimer timer = new DispatcherTimer();
                //            timer.Interval = TimeSpan.FromSeconds(2);
                //            timer.Tick += (sender, args) =>
                //            {
                //                //System.Diagnostics.Debug.WriteLine("Executed after 2 seconds.");
                //                timer.Stop();
                //                GetBitmapFromWindow(m_hWndTemp, m_nWidthTemp, m_nHeightTemp);
                //                //PostMessage(m_hWndTemp, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);                           
                //            };
                //            timer.Start();

                //            //await Task.Run(async () =>
                //            //{
                //            //    await Task.Delay(2000); // Wait for 2 seconds                                                   
                //            //    GetBitmapFromWindow(hWndTemp, nWidthTemp, nHeightTemp);
                //            //    PostMessage(hWndTemp, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                //            //});
                //        }
                //        //else
                //        //{
                //        //    Windows.UI.Popups.MessageDialog md = new Windows.UI.Popups.MessageDialog("Could not subclass temporary window", "Error");
                //        //    WinRT.Interop.InitializeWithWindow.Initialize(md, hWndMain);
                //        //    _ = await md.ShowAsync();
                //        //    return;
                //        //}
                //    }
                //}
                else if (sSelectedItem == "ListView")
                {
                    //var childElement = FindChildElementByName(lv1, "ScrollViewer");
                    var childElement = FindChildElement<ItemsPresenter>(lv1);
                    if (childElement != null)
                    {
                        var ip = (ItemsPresenter)childElement;
                        //await rtb.RenderAsync((UIElement)ip, (int)ip.ActualWidth, (int)ip.ActualHeight);
                        await m_rtb.RenderAsync((UIElement)ip);

                        SolidColorBrush backgroundBrush = lv1.Background as SolidColorBrush;
                        if (backgroundBrush != null)
                        {
                            Windows.UI.Color backgroundColor = backgroundBrush.Color;
                            m_BackColor = new ColorF(backgroundColor.R, backgroundColor.G, backgroundColor.B, 1.0f);
                        }
                        else
                            m_BackColor = new ColorF(ColorF.Enum.Black, 1.0f);

                        Print(false);
                    }
                    else
                    {
                        Windows.UI.Popups.MessageDialog md = new Windows.UI.Popups.MessageDialog("Could not find ItemsPresenter in ListView", "Error");
                        WinRT.Interop.InitializeWithWindow.Initialize(md, hWndMain);
                        _ = await md.ShowAsync();
                        return;
                    }
                }
                else if (sSelectedItem == "Window")
                {
                    Windows.Graphics.SizeInt32 windowSize = _apw.Size;
                    BITMAPV5HEADER bi = new BITMAPV5HEADER();
                    bi.bV5Size = Marshal.SizeOf(typeof(BITMAPV5HEADER));
                    bi.bV5Width = windowSize.Width;
                    bi.bV5Height = -windowSize.Height;
                    bi.bV5Planes = 1;
                    bi.bV5BitCount = 32;
                    bi.bV5Compression = BI_BITFIELDS;
                    bi.bV5AlphaMask = unchecked((int)0xFF000000);
                    bi.bV5RedMask = 0x00FF0000;
                    bi.bV5GreenMask = 0x0000FF00;
                    bi.bV5BlueMask = 0x000000FF;
                    IntPtr hDC = GetDC(IntPtr.Zero);
                    IntPtr hDCMem = CreateCompatibleDC(hDC);
                    IntPtr hBitmapDIBSection = CreateDIBSection(hDC, ref bi, DIB_RGB_COLORS, ref m_pBits, IntPtr.Zero, 0);
                    if (hBitmapDIBSection != IntPtr.Zero)
                    {
                        IntPtr hBitmapOld = SelectObject(hDCMem, hBitmapDIBSection);
                        PrintWindow(hWndMain, hDCMem, PW_RENDERFULLCONTENT);
                        SelectObject(hDC, hBitmapOld);
                        DeleteObject(hBitmapDIBSection);

                        //IntPtr hDCScreen = GetDC(IntPtr.Zero);
                        //BitBlt(hDCScreen, 0, 0, windowSize.Width, windowSize.Height, hDCMem, 0, 0, SRCCOPY);
                        //ReleaseDC(hDCScreen, IntPtr.Zero);                       

                        Print(true, windowSize.Width, windowSize.Height);
                    }
                    DeleteObject(hDCMem);
                    ReleaseDC(IntPtr.Zero, hDC);
                }                
            }
        }

        private void GetBitmapFromWindow(IntPtr hWnd, double nWidth, double nHeight)
        {
            Windows.Graphics.SizeInt32 windowSize = _apw.Size;
            BITMAPV5HEADER bi = new BITMAPV5HEADER();
            bi.bV5Size = Marshal.SizeOf(typeof(BITMAPV5HEADER));
            bi.bV5Width = (int)nWidth;
            bi.bV5Height = (int)-nHeight;
            bi.bV5Planes = 1;
            bi.bV5BitCount = 32;
            bi.bV5Compression = BI_BITFIELDS;
            bi.bV5AlphaMask = unchecked((int)0xFF000000);
            bi.bV5RedMask = 0x00FF0000;
            bi.bV5GreenMask = 0x0000FF00;
            bi.bV5BlueMask = 0x000000FF;
            IntPtr hDC = GetDC(IntPtr.Zero);
            IntPtr hDCMem = CreateCompatibleDC(hDC);
            IntPtr hBitmapDIBSection = CreateDIBSection(hDC, ref bi, DIB_RGB_COLORS, ref m_pBits, IntPtr.Zero, 0);
            if (hBitmapDIBSection != IntPtr.Zero)
            {
                IntPtr hBitmapOld = SelectObject(hDCMem, hBitmapDIBSection);
                PrintWindow(hWnd, hDCMem, PW_RENDERFULLCONTENT | PW_CLIENTONLY);
                SelectObject(hDC, hBitmapOld);
                DeleteObject(hBitmapDIBSection);

                //IntPtr hDCScreen = GetDC(IntPtr.Zero);
                //BitBlt(hDCScreen, 0, 0, nWidth, nHeight, hDCMem, 0, 0, SRCCOPY);
                //ReleaseDC(hDCScreen, IntPtr.Zero);
                
                Print(true, (int)nWidth, (int)nHeight);
            }
            DeleteObject(hDCMem);
            ReleaseDC(IntPtr.Zero, hDC);
        }

        //private int WindowSubClass(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, uint dwRefData)
        //{
        //    //System.Diagnostics.Debug.WriteLine("Message : 0x{0:X4}", uMsg);
        //    switch (uMsg)
        //    {               
        //        case WM_USER + 100:
        //            {                        
        //                SetTimer(hWnd, 1, 2000, IntPtr.Zero);
        //            }
        //            break;
        //        case WM_TIMER:
        //            {
        //                if (wParam == (IntPtr)1)
        //                {
        //                    KillTimer(hWnd, 1);
        //                    Console.Beep(6000, 10);
        //                    GetBitmapFromWindow(hWndTemp, nWidthTemp, nHeightTemp);
        //                    PostMessage(hWndTemp, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        //                }
        //            }
        //            break;
        //    }
        //    return DefSubclassProc(hWnd, uMsg, wParam, lParam);
        //}
 
        async void Print(bool bPrintWindow, int nWindowWidth = 0, int nWindowHeight = 0)
        {  
            IntPtr pDevNames = IntPtr.Zero;
            IntPtr pDevMode = IntPtr.Zero;
            IntPtr hPrinterDC = IntPtr.Zero;
            string sPrinterName = null;
            string sDriverName = null;

            bool bDialog = tsSilent.IsOn ? false : true;
            if (bDialog)
                hPrinterDC = GetPrinterDC(bDialog, ref pDevNames, ref pDevMode);
            else
            {
                sPrinterName = cbPrinters.SelectedItem.ToString();
                hPrinterDC = GetPrinterDCFromPrinter(sPrinterName, out pDevMode);
            }

            if (hPrinterDC != IntPtr.Zero)
            {
                // Not used
                // 4961
                int nWidthPage = GetDeviceCaps(hPrinterDC, HORZRES);
                // 7016
                int nHeightPage = GetDeviceCaps(hPrinterDC, VERTRES);
                //
                int nPhysWidth = GetDeviceCaps(hPrinterDC, PHYSICALWIDTH);
                int nPhysHeight = GetDeviceCaps(hPrinterDC, PHYSICALHEIGHT);

                // Not used atm...
                int nPhysOffsetX = GetDeviceCaps(hPrinterDC, PHYSICALOFFSETX);
                int nPhysOffsetY = GetDeviceCaps(hPrinterDC, PHYSICALOFFSETY);

                // Not used, scale too small from screen/printer...
                IntPtr hDCScreen = GetDC(IntPtr.Zero);
                float nLogPixelsXScreen = GetDeviceCaps(hDCScreen, LOGPIXELSX);
                float nLogPixelsYScreen = GetDeviceCaps(hDCScreen, LOGPIXELSY);
                ReleaseDC(hDCScreen, IntPtr.Zero);
                float nLogPixelsXPrinter = GetDeviceCaps(hPrinterDC, LOGPIXELSX);
                float nLogPixelsYPrinter = GetDeviceCaps(hPrinterDC, LOGPIXELSY);
                float nScaleX = Math.Max(nLogPixelsXScreen, nLogPixelsXPrinter) / Math.Min(nLogPixelsXScreen, nLogPixelsXPrinter);
                float nScaleY = Math.Max(nLogPixelsYScreen, nLogPixelsYPrinter) / Math.Min(nLogPixelsYScreen, nLogPixelsYPrinter);

                // Read user value from NumberBox instead
                nScaleX = (double.IsNaN(nbScale.Value)) ? 0 : (float)nbScale.Value;
                nScaleY = (double.IsNaN(nbScale.Value)) ? 0 : (float)nbScale.Value;

                if (pDevNames != IntPtr.Zero)
                {
                    DEVNAMES dn = new DEVNAMES();
                    IntPtr pDevNamesLock = GlobalLock(pDevNames);
                    dn = (DEVNAMES)Marshal.PtrToStructure(pDevNamesLock, typeof(DEVNAMES));

                    // sPrinterName = "Microsoft XPS Document Writer"
                    int nOffsetPrinterName = dn.wDeviceOffset * Marshal.SystemDefaultCharSize;
                    //int nOffset = checked(Marshal.SystemDefaultCharSize * Marshal.ReadInt16((IntPtr)(checked((long)pDevNames + 1 * 2))));
                    sPrinterName = Marshal.PtrToStringUni((IntPtr)(checked((long)pDevNamesLock + nOffsetPrinterName)));

                    // sDriverName = "winspool"
                    //int nOffsetDriver = dn.wDriverOffset * Marshal.SystemDefaultCharSize;
                    //string sDriverName = Marshal.PtrToStringUni((IntPtr)(checked((long)pDevNamesLock + nOffsetDriver)));

                    // sOutput = "PORTPROMPT:"
                    //int nOffsetOutput = dn.wOutputOffset * Marshal.SystemDefaultCharSize;
                    //string sOutput = Marshal.PtrToStringUni((IntPtr)(checked((long)pDevNamesLock + nOffsetOutput)));

                    GlobalUnlock(pDevNamesLock);
                    GlobalFree(pDevNamesLock);
                }

                if (pDevMode != IntPtr.Zero)
                {
                    DEVMODE dm = new DEVMODE();
                    IntPtr pDevModeLock = GlobalLock(pDevMode);
                    dm = (DEVMODE)Marshal.PtrToStructure(pDevModeLock, typeof(DEVMODE));
                    //dmPaperLength   2970    short
                    //dmPaperSize 9   short
                    //dmPaperWidth    2100    short

                    if (tsOrientation.Visibility == Visibility.Visible)
                    {
                        dm.dmFields |= DevModeFields.DM_ORIENTATION;
                        dm.dmOrientation = (short)(tsOrientation.IsOn ? DMORIENT_PORTRAIT : DMORIENT_LANDSCAPE);
                    }

                    if (dm.dmFields.HasFlag(DevModeFields.DM_PAPERLENGTH) &&
                       dm.dmFields.HasFlag(DevModeFields.DM_PAPERWIDTH))
                    {
                        // Convert 1/10 of a millimeter DEVMODE unit to 1/96 of inch D2D unit
                        m_pageHeight = dm.dmPaperLength / 254.0f * 96.0f;
                        m_pageWidth = dm.dmPaperWidth / 254.0f * 96.0f;
                    }
                    else
                    {
                        // Use default values if the user does not specify page size.
                        m_pageHeight = PAGE_HEIGHT_IN_DIPS;
                        m_pageWidth = PAGE_WIDTH_IN_DIPS;
                    }

                    if (dm.dmFields.HasFlag(DevModeFields.DM_ORIENTATION))
                    {
                        if (dm.dmOrientation == DMORIENT_LANDSCAPE)
                        {
                            float nOldPageWidth = m_pageWidth;
                            m_pageWidth = m_pageHeight;
                            m_pageHeight = nOldPageWidth;
                        }
                    }

                    int nWidth = 0, nHeight = 0;
                    if (!bPrintWindow)
                    {
                        nWidth = m_rtb.PixelWidth;
                        nHeight = m_rtb.PixelHeight;

                    }
                    else
                    {                        
                        nWidth = nWindowWidth;
                        nHeight = nWindowHeight;
                    }
                    double nRatio = 0;
                    var nHeightDest = 0;
                    nRatio = (double)m_pageWidth / (double)nWidth;
                    nHeightDest = (int)(nHeight * nRatio);                   

                    Marshal.StructureToPtr(dm, pDevModeLock, false);

                    bool bPDF = false;
                    bool bXPS = false;
                    var pd = new PRINTER_DEFAULTS
                    {
                        DesiredPrinterAccess = PRINTER_ACCESS_MASK.PRINTER_ACCESS_USE
                    };
                    IntPtr hPrinter = IntPtr.Zero;
                    if (OpenPrinter(sPrinterName, out hPrinter, ref pd))
                    {
                        uint nBufferNeeded = 0;
                        bool bRet = GetPrinterDriver(hPrinter, null, 6, IntPtr.Zero, 0, out nBufferNeeded);
                        IntPtr pDriverInfo = Marshal.AllocHGlobal((int)nBufferNeeded);
                        bRet = GetPrinterDriver(hPrinter, null, 6, pDriverInfo, nBufferNeeded, out nBufferNeeded);
                        if (bRet)
                        {
                            var di6 = (DRIVER_INFO_6)Marshal.PtrToStructure(pDriverInfo, typeof(DRIVER_INFO_6));
                            // Useless
                            //var pDependentFiles = ReadMultiSz(di6.pDependentFiles).ToList();
                            sDriverName = di6.pName;
                            if (sDriverName.ToUpper().Contains("PDF"))
                                bPDF = true;
                            if (sDriverName.ToUpper().Contains("XPS"))
                                bXPS = true;
                        }
                        Marshal.FreeHGlobal(pDriverInfo);
                        ClosePrinter(hPrinter);
                    }
                    else
                    {
                        Windows.UI.Popups.MessageDialog md = new Windows.UI.Popups.MessageDialog("Could not open printer :" + sPrinterName, "Error");
                        WinRT.Interop.InitializeWithWindow.Initialize(md, hWndMain);
                        _ = await md.ShowAsync();
                        GlobalUnlock(pDevModeLock);
                        GlobalFree(pDevModeLock);
                        DeleteDC(hPrinterDC);
                        return;
                    }

                    System.Runtime.InteropServices.ComTypes.IStream pPrintTicketStream = null;
                    HRESULT hr = CreateStreamOnHGlobal(IntPtr.Zero, true, out pPrintTicketStream);
                    if (hr == HRESULT.S_OK)
                    {
                        IntPtr pProvider = IntPtr.Zero;
                        hr = PTOpenProvider(sPrinterName, 1, out pProvider);
                        if (hr == HRESULT.S_OK)
                        {
                            hr = PTConvertDevModeToPrintTicket(pProvider, (uint)(dm.dmSize + dm.dmDriverExtra), pDevModeLock, EPrintTicketScope.kPTJobScope, pPrintTicketStream);
                            //hr = PTConvertDevModeToPrintTicket(pProvider, (uint)(dm.dmSize + dm.dmDriverExtra), ref dm, EPrintTicketScope.kPTJobScope, pPrintTicketStream);

                            if (hr == HRESULT.S_OK)
                            {
                                //pPrintTicketStream.Seek(0, 0, IntPtr.Zero);

                                Guid CLSID_PrintDocumentPackageTargetFactory = new Guid("348ef17d-6c81-4982-92b4-ee188a43867a");
                                Type PrintDocumentPackageTargetFactoryType = Type.GetTypeFromCLSID(CLSID_PrintDocumentPackageTargetFactory, true);
                                object DocumentPackageTargetFactory = Activator.CreateInstance(PrintDocumentPackageTargetFactoryType);
                                IPrintDocumentPackageTargetFactory pDocumentDocumentPackageTargetFactory = (IPrintDocumentPackageTargetFactory)DocumentPackageTargetFactory;
                                if (pDocumentDocumentPackageTargetFactory != null)
                                {
                                    bool bWriteStream = false;
                                    System.Runtime.InteropServices.ComTypes.IStream pstm = null;
                                    // Otherwise Bullzip create .EPS file
                                    if (sDriverName == "Bullzip PDF Printer")
                                    {
                                        pstm = null;
                                    }
                                    // Dialog Box if XPS, PDF
                                    else if (bPDF || bXPS)
                                    {
                                        hr = CreateStreamOnHGlobal(IntPtr.Zero, true, out pstm);
                                        bWriteStream = true;
                                    }
                                    IPrintDocumentPackageTarget pPrintDocumentPackageTarget = null;
                                    hr = pDocumentDocumentPackageTargetFactory.CreateDocumentPackageTargetForPrintJob(
                                        sPrinterName,
                                        "Test Direct2D printing",

                                        pstm, // job output stream; when nullptr, send to printer

                                        pPrintTicketStream, // job print ticket
                                        out pPrintDocumentPackageTarget
                                     );
                                    if (hr == HRESULT.S_OK)
                                    {
                                        IntPtr pUnknown = Marshal.GetIUnknownForObject(pPrintDocumentPackageTarget);
                                        ID2D1PrintControl pD2DPrintControl = null;
                                        D2D1_PRINT_CONTROL_PROPERTIES pcp = new D2D1_PRINT_CONTROL_PROPERTIES();
                                        pcp.fontSubset = D2D1_PRINT_FONT_SUBSET_MODE.D2D1_PRINT_FONT_SUBSET_MODE_DEFAULT;
                                        pcp.colorSpace = D2D1_COLOR_SPACE.D2D1_COLOR_SPACE_SRGB;
                                        pcp.rasterDPI = 150.0f;
                                        hr = m_pD2DDevice.CreatePrintControl(m_pWICImagingFactory, pUnknown, ref pcp, out pD2DPrintControl);
                                        if (hr == HRESULT.S_OK)
                                        {
                                            ID2D1CommandList pD2DCommandList = null;

                                            IntPtr pPixelArray = IntPtr.Zero;
                                            if (!bPrintWindow)
                                            {
                                                byte[] pixelArray;
                                                var pixelBuffer = await m_rtb.GetPixelsAsync();
                                                pixelArray = pixelBuffer.ToArray();  // BGRA8 format                                                                                              
                                                pPixelArray = Marshal.AllocHGlobal(pixelArray.Length);
                                                Marshal.Copy(pixelArray, 0, pPixelArray, pixelArray.Length);
                                            }
                                            else
                                            {
                                                //int nSize = (32 / 8) * nWidth * nHeight;
                                                //byte[] pixelArray = new byte[nSize];
                                                pPixelArray = m_pBits;
                                            }

                                            ID2D1Bitmap pD2DBitmap = null;
                                            D2D1_BITMAP_PROPERTIES bitmapProperties = new D2D1_BITMAP_PROPERTIES();
                                            //bitmapProperties.pixelFormat = D2DTools.PixelFormat();
                                            bitmapProperties.pixelFormat = D2DTools.PixelFormat(DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM, D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_PREMULTIPLIED);
                                            bitmapProperties.dpiX = 96;
                                            bitmapProperties.dpiY = 96;
                                            m_pD2DDeviceContext.CreateBitmap(new D2D1_SIZE_U((uint)nWidth, (uint)nHeight), pPixelArray, (uint)((32 / 8) * nWidth), bitmapProperties, out pD2DBitmap);

                                            if (!bPrintWindow)
                                                Marshal.FreeHGlobal(pPixelArray);

                                            float nPages = 0;
                                            int nNbPages = 0;
                                            if (tsScale.IsOn)
                                            {
                                                nPages = nHeightDest / m_pageHeight;
                                                nNbPages = (int)Math.Ceiling(nPages);
                                            }
                                            else
                                            {
                                                nPages = (nHeightDest * nScaleY) / m_pageHeight;
                                                nNbPages = (int)Math.Ceiling(nPages);
                                            }

                                            float nSourceX = 0.0f, nSourceY = 0.0f;
                                            float nSourceWidth = m_pageWidth, nSourceHeight = m_pageHeight;

                                            for (int i = 0; i < nNbPages; i++)
                                            {
                                                hr = m_pD2DDeviceContext.CreateCommandList(out pD2DCommandList);
                                                m_pD2DDeviceContext.SetTarget(pD2DCommandList);

                                                m_pD2DDeviceContext.BeginDraw();
                                                //m_pD2DDeviceContext.Clear(new ColorF(ColorF.Enum.Blue, 1.0f));
                                                m_pD2DDeviceContext.Clear(m_BackColor);                                                

                                                D2D1_SIZE_F sizeBmp = pD2DBitmap.GetSize();
                                                //D2D1_RECT_F sourceRect = new D2D1_RECT_F(0.0f, 0.0f, sizeBmp.width, sizeBmp.height);                                                    
                                                nSourceHeight = sizeBmp.height / nPages;
                                                D2D1_RECT_F sourceRect = new D2D1_RECT_F(nSourceX, nSourceY, nSourceX + sizeBmp.width, nSourceY + nSourceHeight);

                                                nSourceY += nSourceHeight;

                                                float nHeightDestFinal = 0.0f;
                                                if (i == nNbPages - 1)
                                                {
                                                    if (tsScale.IsOn)
                                                        nHeightDestFinal = nHeightDest - m_pageHeight * i;
                                                    else
                                                        nHeightDestFinal = nHeightDest * nScaleX - m_pageHeight * i;
                                                    //if (tsScale.IsOn)
                                                    //    nHeightDestFinal = nHeightDest - m_pageHeight * i;
                                                    //else
                                                    //    nHeightDestFinal = nHeight - m_pageHeight * i;
                                                }
                                                else
                                                {
                                                    nHeightDestFinal = m_pageHeight;
                                                    //if (tsScale.IsOn)
                                                    //    nHeightDestFinal = m_pageHeight;
                                                    //else
                                                    //    nHeightDestFinal = nHeight;
                                                }

                                                D2D1_RECT_F destRect;
                                                if (tsScale.IsOn)
                                                    destRect = new D2D1_RECT_F(0.0f, 0.0f, m_pageWidth, nHeightDestFinal);
                                                else
                                                {
                                                    destRect = new D2D1_RECT_F(0.0f, 0.0f, m_pageWidth * nScaleX, nHeightDestFinal);
                                                }

                                                m_pD2DDeviceContext.DrawBitmap(pD2DBitmap, ref destRect, 1.0f, D2D1_BITMAP_INTERPOLATION_MODE.D2D1_BITMAP_INTERPOLATION_MODE_LINEAR, ref sourceRect);

                                                // Test ellipse 
                                                if (1 == 0)
                                                {
                                                    ID2D1SolidColorBrush pBrush1 = null;
                                                    hr = m_pD2DDeviceContext.CreateSolidColorBrush(new ColorF(ColorF.Enum.Red, 1.0f), BrushProperties(), out pBrush1);
                                                    m_pD2DDeviceContext.FillEllipse(Ellipse(new Direct2D.D2D1_POINT_2F(200.0f, 200.0f), 100.0f, 100.0f), pBrush1);
                                                    SafeRelease(ref pBrush1);
                                                }

                                                // Test rounded rectangle with glow effect (Gaussian Blur)
                                                if (1 == 0)
                                                {
                                                    D2D1_ROUNDED_RECT roundedRect = RoundedRect(RectF(20, 20, 400, 100), 20.0f, 20.0f);
                                                    ID2D1SolidColorBrush pBrush = null;
                                                    hr = m_pD2DDeviceContext.CreateSolidColorBrush(new ColorF(ColorF.Enum.Yellow, 1.0f), BrushProperties(), out pBrush);

                                                    ID2D1BitmapRenderTarget pCompatibleRenderTarget = null;
                                                    D2D1_SIZE_U sizeU = SizeU((uint)nWidth, (uint)nHeight);
                                                    D2D1_SIZE_F sizeF = SizeF((uint)nWidth, (uint)nHeight);
                                                    hr = m_pD2DDeviceContext.CreateCompatibleRenderTarget(ref sizeF, ref sizeU, PixelFormat(DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM, D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_PREMULTIPLIED),
                                                        D2D1_COMPATIBLE_RENDER_TARGET_OPTIONS.D2D1_COMPATIBLE_RENDER_TARGET_OPTIONS_NONE, out pCompatibleRenderTarget);
                                                    pCompatibleRenderTarget.BeginDraw();
                                                    pCompatibleRenderTarget.Clear(null);
                                                    pCompatibleRenderTarget.FillRoundedRectangle(ref roundedRect, pBrush);
                                                    hr = pCompatibleRenderTarget.EndDraw(out UInt64 tag1a, out UInt64 tag2a);

                                                    ID2D1Bitmap pCompatibleBitmap = null;
                                                    hr = pCompatibleRenderTarget.GetBitmap(out pCompatibleBitmap);
                                                    ID2D1Effect pEffect = null;
                                                    hr = m_pD2DDeviceContext.CreateEffect(CLSID_D2D1GaussianBlur, out pEffect);
                                                    pEffect.SetInput(0, pCompatibleBitmap);
                                                    SetEffectFloat(pEffect, (uint)D2D1_GAUSSIANBLUR_PROP.D2D1_GAUSSIANBLUR_PROP_STANDARD_DEVIATION, 15.0f);
                                                                                                        
                                                    D2D1_POINT_2F pt = new D2D1_POINT_2F(0, 0);
                                                    D2D1_RECT_F imageRectangle = new D2D1_RECT_F();
                                                    imageRectangle.right = imageRectangle.left + nWidth;
                                                    imageRectangle.bottom = imageRectangle.top + nHeight;
                                                    ID2D1Image pOutputImage = null;
                                                    pEffect.GetOutput(out pOutputImage);

                                                    ID2D1Effect pEffectEmpty = null;
                                                    hr = m_pD2DDeviceContext.CreateEffect(CLSID_D2D1GaussianBlur, out pEffectEmpty);
                                                    pEffectEmpty.SetInput(0, pCompatibleBitmap);
                                                    SetEffectFloat(pEffectEmpty, (uint)D2D1_GAUSSIANBLUR_PROP.D2D1_GAUSSIANBLUR_PROP_STANDARD_DEVIATION, 0.0f);
                                                    ID2D1Image pOutputImageOrig = null;
                                                    pEffectEmpty.GetOutput(out pOutputImageOrig);

                                                    // var s1 = pCompatibleBitmap.GetSize();
                                                    // //ID2D1Bitmap b = (ID2D1Bitmap)pOutputImage;
                                                    // //var s2 = b.GetSize();

                                                    // var D2DBitmapGuid = new Guid("a2296057-ea42-4099-983b-539fb6505426");
                                                    // IntPtr pImage = Marshal.GetIUnknownForObject(pOutputImage);
                                                    // Marshal.QueryInterface(pImage, ref D2DBitmapGuid, out IntPtr pD2DBitmap2);

                                                    m_pD2DDeviceContext.DrawImage(pOutputImage, ref pt, ref destRect, D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_LINEAR, D2D1_COMPOSITE_MODE.D2D1_COMPOSITE_MODE_SOURCE_OVER);
                                                    m_pD2DDeviceContext.DrawImage(pOutputImageOrig, ref pt, ref destRect, D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_LINEAR, D2D1_COMPOSITE_MODE.D2D1_COMPOSITE_MODE_SOURCE_OVER);
                                                                                                        
                                                    //m_pD2DDeviceContext.DrawBitmap(pCompatibleBitmap, ref destRect, 1.0f, D2D1_BITMAP_INTERPOLATION_MODE.D2D1_BITMAP_INTERPOLATION_MODE_LINEAR, ref sourceRect);
                                                    //m_pD2DDeviceContext.FillRoundedRectangle(ref roundedRect, pBrush);                                                  

                                                    SafeRelease(ref pBrush);
                                                    SafeRelease(ref pCompatibleRenderTarget);
                                                    SafeRelease(ref pCompatibleBitmap);
                                                    SafeRelease(ref pOutputImage);
                                                    SafeRelease(ref pEffect);
                                                    SafeRelease(ref pOutputImageOrig);
                                                    SafeRelease(ref pEffectEmpty);
                                                }

                                                hr = m_pD2DDeviceContext.EndDraw(out ulong tag1, out ulong tag2);
                                                pD2DCommandList.Close();
                                                hr = pD2DPrintControl.AddPage(pD2DCommandList, new D2D_SIZE_F(m_pageWidth, m_pageHeight), pPrintTicketStream,
                                                    out ulong tag11, out ulong tag21);
                                                SafeRelease(ref pD2DCommandList);
                                            }
                                            SafeRelease(ref pD2DBitmap);
                                            // AddPage must have been called at least once
                                            hr = pD2DPrintControl.Close();
                                            //m_pD2DDeviceContext.SetTarget(m_pD2DTargetBitmap);
                                            m_pD2DDeviceContext.SetTarget(null);

                                            if (bWriteStream)
                                            {
                                                //StorageFolder documentsFolder = KnownFolders.DocumentsLibrary;
                                                var documentsFolder = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Documents);
                                                var sPath = documentsFolder.SaveFolder.Path;
                                                var sName = "PrinterOutput_" + DateTime.Now.ToString("yyyyMMdd-HHmm-ss");
                                                var sExtension = "";
                                                //if (sDriverName.ToUpper().Contains("FAX"))
                                                //    sExtension = ".tiff";
                                                //else if (sDriverName.ToUpper().Contains("ONENOTE"))
                                                //    sExtension = ".xps";
                                                //else if (sDriverName == "Bullzip PDF Printer")
                                                //    sExtension = ".eps";
                                                //else if (bPDF)
                                                //    sExtension = ".pdf";
                                                //else if (bXPS || sDriverName == "Microsoft Software Printer Driver")
                                                //    sExtension = ".xps";
                                                if (bPDF)
                                                    sExtension = ".pdf";
                                                else if (bXPS)
                                                    sExtension = ".xps";
                                                string sFileName = sPath + "\\" + sName + sExtension;

                                                // To avoid Error 0x80070020 = ERROR_SHARING_VIOLATION
                                                File.Delete(sFileName);
                                                hr = WriteStreamToFile(pstm, sFileName, CREATE_ALWAYS, (uint)GENERIC_WRITE);
                                                if (hr == HRESULT.S_OK)
                                                {
                                                    tbOutputFile.Text = sFileName;
                                                    tbOutputFile.Focus(FocusState.Programmatic);
                                                    tbOutputFile.SelectAll();
                                                }
                                            }
                                            else
                                                tbOutputFile.Text = "";
                                            SafeRelease(ref pD2DPrintControl);
                                        }
                                        SafeRelease(ref pPrintDocumentPackageTarget);
                                    }
                                    else
                                    {
                                        Windows.UI.Popups.MessageDialog md = new Windows.UI.Popups.MessageDialog("Could not create IPrintDocumentPackageTarget for printer :" + sPrinterName, "Error");
                                        WinRT.Interop.InitializeWithWindow.Initialize(md, hWndMain);
                                        _ = await md.ShowAsync();
                                        SafeRelease(ref pPrintTicketStream);
                                        GlobalUnlock(pDevModeLock);
                                        GlobalFree(pDevModeLock);
                                        SafeRelease(ref pstm);
                                        DeleteDC(hPrinterDC);
                                        return;
                                    }
                                    SafeRelease(ref pstm);
                                    SafeRelease(ref pDocumentDocumentPackageTargetFactory);
                                }
                            }
                            else
                            {
                                Windows.UI.Popups.MessageDialog md = new Windows.UI.Popups.MessageDialog("Could not create print ticket for printer :" + sPrinterName, "Error");
                                WinRT.Interop.InitializeWithWindow.Initialize(md, hWndMain);
                                _ = await md.ShowAsync();
                                GlobalUnlock(pDevModeLock);
                                GlobalFree(pDevModeLock);
                                DeleteDC(hPrinterDC);
                                return;
                            }
                            PTCloseProvider(pProvider);
                        }
                        else
                        {
                            Windows.UI.Popups.MessageDialog md = new Windows.UI.Popups.MessageDialog("Could not open print ticket provider for printer : " + sPrinterName, "Error");
                            WinRT.Interop.InitializeWithWindow.Initialize(md, hWndMain);
                            _ = await md.ShowAsync();
                            SafeRelease(ref pPrintTicketStream);
                            GlobalUnlock(pDevModeLock);
                            GlobalFree(pDevModeLock);
                            DeleteDC(hPrinterDC);
                            return;
                        }
                        SafeRelease(ref pPrintTicketStream);
                    }
                    GlobalUnlock(pDevModeLock);
                    GlobalFree(pDevModeLock);
                }               

                DeleteDC(hPrinterDC);
                //if (m_WndTemp != null)
                //{
                //    //PostMessage(m_hWndTemp, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                //    m_WndTemp.Close();
                //    //m_WndTemp = null;
                //    //m_WndTemp = IntPtr.Zero;
                //}
                m_mp.Play();
            }
            else
            {
                Windows.UI.Popups.MessageDialog md = new Windows.UI.Popups.MessageDialog("Could not get printer DC for printer : " + sPrinterName, "Error");
                WinRT.Interop.InitializeWithWindow.Initialize(md, hWndMain);
                _ = await md.ShowAsync();
                return;
            }
        }

        private void SetEffectFloat(ID2D1Effect pEffect, uint nEffect, float fValue)
        {
            float[] aFloatArray = { fValue };
            int nDataSize = aFloatArray.Length * Marshal.SizeOf(typeof(float));
            IntPtr pData = Marshal.AllocHGlobal(nDataSize);
            Marshal.Copy(aFloatArray, 0, pData, aFloatArray.Length);
            HRESULT hr = pEffect.SetValue(nEffect, D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_UNKNOWN, pData, (uint)nDataSize);
            Marshal.FreeHGlobal(pData);
        }

        // Adapted from MS C++ sample
#nullable enable
        private Microsoft.UI.Xaml.DependencyObject? FindChildElementByName(Microsoft.UI.Xaml.DependencyObject? tree, string sName)
        {
            for (int i = 0; i < Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(tree); i++)
            {
                Microsoft.UI.Xaml.DependencyObject child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(tree, i);
                if (child != null && ((Microsoft.UI.Xaml.FrameworkElement)child).Name == sName)
                    return child;
                else
                {
                    Microsoft.UI.Xaml.DependencyObject? childInSubtree = FindChildElementByName(child, sName);
                    if (childInSubtree != null)
                        return childInSubtree;
                }
            }
            return null;
        }

        ChildType? FindChildElement<ChildType>(Microsoft.UI.Xaml.DependencyObject? tree) where ChildType : Microsoft.UI.Xaml.DependencyObject
        {
            for (int i = 0; i < Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(tree); i++)
            {
                Microsoft.UI.Xaml.DependencyObject child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(tree, i);
                if (child != null && child is ChildType)
                {
                    return child as ChildType;
                }
                else
                {
                    ChildType? childInSubtree = FindChildElement<ChildType>(child);
                    if (childInSubtree != null)
                    {
                        return childInSubtree;
                    }
                }
            }
            return null;
        }
#nullable disable

        private void tsSilent_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch ts = sender as ToggleSwitch;
            if (ts.IsOn)
            {
               
            }
            else
            {
               
            }
        }

        private void tsOrientation_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch ts = sender as ToggleSwitch;
            if (ts.IsOn)
            {

            }
            else
            {

            }
        }

        private void cmbControls_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string sSelectedItem = (cmbControls.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (borderRE1 != null && borderIMG1 != null && borderLV1 != null)
            {
                if (sSelectedItem == "RichEditBox")
                {
                    borderRE1.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Red);
                    borderIMG1.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray);
                    borderLV1.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray);
                }
                else if (sSelectedItem == "Image")
                {
                    borderIMG1.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Red);
                    borderRE1.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray);
                    borderLV1.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray);
                }
                else if (sSelectedItem == "ListView")
                {
                    borderLV1.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Red);
                    borderIMG1.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray);
                    borderRE1.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray);
                }
                else if (sSelectedItem == "Window")
                {
                    borderRE1.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray);
                    borderIMG1.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray);
                    borderLV1.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray);
                }
            }
        }

        private void tsScale_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch ts = sender as ToggleSwitch;
            if (ts.IsOn)
            {

            }
            else
            {

            }
        }

        private void cbPrinters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void nbScale_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {

        }

        public IntPtr GetPrinterDC(bool bDialog, ref IntPtr pDevNames, ref IntPtr pDevMode)
        { 
            PRINTDLGEX pDLG = new PRINTDLGEX();
            pDLG.lStructSize = (uint)Marshal.SizeOf(typeof(PRINTDLGEX));
            pDLG.Flags = (uint)((bDialog ? 0 : PD_RETURNDEFAULT) | PD_HIDEPRINTTOFILE | PD_RETURNDC);
            pDLG.Flags |= PD_NOPAGENUMS;
            pDLG.hwndOwner = hWndMain;          
            pDLG.nStartPage = START_PAGE_GENERAL;
            // E_INVALIDARG 0x80070057         
            if (PrintDlgEx(pDLG) == HRESULT.S_OK)
            {
                pDevNames = pDLG.hDevNames;
                pDevMode = pDLG.hDevMode;
                return pDLG.hDC;
            }
            return IntPtr.Zero;
        }

        private IntPtr GetPrinterDCFromPrinter(string sPrinterName, out IntPtr pDevMode)
        {
            IntPtr hPrinter;
            IntPtr hDC = IntPtr.Zero;
            pDevMode = IntPtr.Zero;

            if (!OpenPrinter(sPrinterName, out hPrinter, IntPtr.Zero))           
                return IntPtr.Zero;
            try
            {
                int nDevModeSize = DocumentProperties(IntPtr.Zero, hPrinter, sPrinterName, IntPtr.Zero, IntPtr.Zero, 0);
                if (nDevModeSize > 0)
                {
                    pDevMode = Marshal.AllocHGlobal(nDevModeSize);
                    try
                    {                       
                        int nResult = DocumentProperties(IntPtr.Zero, hPrinter, sPrinterName, pDevMode, pDevMode, DM_OUT_BUFFER);
                        if (nResult == 1) // IDOK
                        {                            
                            hDC = CreateDC(null, sPrinterName, null, IntPtr.Zero);
                            if (hDC == IntPtr.Zero)
                            {                               
                                Marshal.FreeHGlobal(pDevMode);
                                return IntPtr.Zero;
                            }
                            return hDC;
                        }
                        else
                        {  
                            Marshal.FreeHGlobal(pDevMode);
                            return IntPtr.Zero;
                        }
                    }
                    catch
                    {                       
                        Marshal.FreeHGlobal(pDevMode);
                        throw;
                    }
                }
                else
                {                   
                    return IntPtr.Zero;
                }
            }
            finally
            {
                ClosePrinter(hPrinter);
            }
        }

        HRESULT CreateD2D1Factory()
        {
            HRESULT hr = HRESULT.S_OK;
            D2D1_FACTORY_OPTIONS options = new D2D1_FACTORY_OPTIONS();

            // Needs "Enable native code Debugging"
#if DEBUG
            options.debugLevel = D2D1_DEBUG_LEVEL.D2D1_DEBUG_LEVEL_INFORMATION;
#endif

            hr = D2DTools.D2D1CreateFactory(D2D1_FACTORY_TYPE.D2D1_FACTORY_TYPE_SINGLE_THREADED, ref D2DTools.CLSID_D2D1Factory, ref options, out m_pD2DFactory);
            m_pD2DFactory1 = (ID2D1Factory1)m_pD2DFactory;
            return hr;
        }

        public HRESULT CreateDeviceContext()
        {
            HRESULT hr = HRESULT.S_OK;
            uint creationFlags = (uint)D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_BGRA_SUPPORT;

            // Needs "Enable native code Debugging"
#if DEBUG
            creationFlags |= (uint)D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_DEBUG;
#endif

            int[] aD3D_FEATURE_LEVEL = new int[] { (int)D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_11_1, (int)D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_11_0,
                (int)D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_10_1, (int)D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_10_0, (int)D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_9_3,
                (int)D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_9_2, (int)D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_9_1};

            D3D_FEATURE_LEVEL featureLevel;
            hr = D2DTools.D3D11CreateDevice(null,    // specify null to use the default adapter
                D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_HARDWARE,
                IntPtr.Zero,
                creationFlags,              // optionally set debug and Direct2D compatibility flags
                                            //pD3D_FEATURE_LEVEL,              // list of feature levels this app can support
                aD3D_FEATURE_LEVEL,
                //(uint)Marshal.SizeOf(aD3D_FEATURE_LEVEL),   // number of possible feature levels
                (uint)aD3D_FEATURE_LEVEL.Length,
                D2DTools.D3D11_SDK_VERSION,
                out m_pD3D11DevicePtr,                    // returns the Direct3D device created
                out featureLevel,            // returns feature level of device created
                                             //out pD3D11DeviceContextPtr                    // returns the device immediate context
                out m_pD3D11DeviceContext
            );
            if (hr == HRESULT.S_OK)
            {
                //m_pD3D11DeviceContext = Marshal.GetObjectForIUnknown(pD3D11DeviceContextPtr) as ID3D11DeviceContext;             

                m_pDXGIDevice = Marshal.GetObjectForIUnknown(m_pD3D11DevicePtr) as IDXGIDevice1;
                if (m_pD2DFactory1 != null)
                {                   
                    hr = m_pD2DFactory1.CreateDevice(m_pDXGIDevice, out m_pD2DDevice);
                    if (hr == HRESULT.S_OK)
                    {
                        hr = m_pD2DDevice.CreateDeviceContext(D2D1_DEVICE_CONTEXT_OPTIONS.D2D1_DEVICE_CONTEXT_OPTIONS_NONE, out m_pD2DDeviceContext);
                        //SafeRelease(ref pD2DDevice);
                    }
                }
                //Marshal.Release(m_pD3D11DevicePtr);
            }
            return hr;
        }

        HRESULT CreateSwapChain(IntPtr hWnd)
        {
            HRESULT hr = HRESULT.S_OK;
            DXGI_SWAP_CHAIN_DESC1 swapChainDesc = new DXGI_SWAP_CHAIN_DESC1();
            swapChainDesc.Width = 1;
            swapChainDesc.Height = 1;
            swapChainDesc.Format = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM; // this is the most common swapchain format
            swapChainDesc.Stereo = false;
            swapChainDesc.SampleDesc.Count = 1;                // don't use multi-sampling
            swapChainDesc.SampleDesc.Quality = 0;
            swapChainDesc.BufferUsage = D2DTools.DXGI_USAGE_RENDER_TARGET_OUTPUT;
            swapChainDesc.BufferCount = 2;                     // use double buffering to enable flip
            swapChainDesc.Scaling = (hWnd != IntPtr.Zero) ? DXGI_SCALING.DXGI_SCALING_NONE : DXGI_SCALING.DXGI_SCALING_STRETCH;
            swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT.DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL; // all apps must use this SwapEffect       
            swapChainDesc.Flags = 0;

            IDXGIAdapter pDXGIAdapter;
            hr = m_pDXGIDevice.GetAdapter(out pDXGIAdapter);
            if (hr == HRESULT.S_OK)
            {
                IntPtr pDXGIFactory2Ptr;
                hr = pDXGIAdapter.GetParent(typeof(IDXGIFactory2).GUID, out pDXGIFactory2Ptr);
                if (hr == HRESULT.S_OK)
                {
                    IDXGIFactory2 pDXGIFactory2 = Marshal.GetObjectForIUnknown(pDXGIFactory2Ptr) as IDXGIFactory2;
                    if (hWnd != IntPtr.Zero)
                        hr = pDXGIFactory2.CreateSwapChainForHwnd(m_pD3D11DevicePtr, hWnd, ref swapChainDesc, IntPtr.Zero, null, out m_pDXGISwapChain1);
                    else
                        hr = pDXGIFactory2.CreateSwapChainForComposition(m_pD3D11DevicePtr, ref swapChainDesc, null, out m_pDXGISwapChain1);

                    hr = m_pDXGIDevice.SetMaximumFrameLatency(1);
                    SafeRelease(ref pDXGIFactory2);
                    Marshal.Release(pDXGIFactory2Ptr);
                }
                SafeRelease(ref pDXGIAdapter);
            }
            return hr;
        }

        HRESULT ConfigureSwapChain(IntPtr hWnd)
        {
            HRESULT hr = HRESULT.S_OK;

            //IntPtr pD3D11Texture2DPtr = IntPtr.Zero;
            //hr = m_pDXGISwapChain1.GetBuffer(0, typeof(ID3D11Texture2D).GUID, ref pD3D11Texture2DPtr);
            //m_pD3D11Texture2D = Marshal.GetObjectForIUnknown(pD3D11Texture2DPtr) as ID3D11Texture2D;

            D2D1_BITMAP_PROPERTIES1 bitmapProperties = new D2D1_BITMAP_PROPERTIES1();
            bitmapProperties.bitmapOptions = D2D1_BITMAP_OPTIONS.D2D1_BITMAP_OPTIONS_TARGET | D2D1_BITMAP_OPTIONS.D2D1_BITMAP_OPTIONS_CANNOT_DRAW;
            bitmapProperties.pixelFormat = D2DTools.PixelFormat(DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM, D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_IGNORE);
            //float nDpiX, nDpiY = 0.0f;
            //m_pD2DContext.GetDpi(out nDpiX, out nDpiY);
            uint nDPI = GetDpiForWindow(hWnd);
            bitmapProperties.dpiX = nDPI;
            bitmapProperties.dpiY = nDPI;

            IntPtr pDXGISurfacePtr = IntPtr.Zero;
            hr = m_pDXGISwapChain1.GetBuffer(0, typeof(IDXGISurface).GUID, out pDXGISurfacePtr);
            if (hr == HRESULT.S_OK)
            {
                IDXGISurface pDXGISurface = Marshal.GetObjectForIUnknown(pDXGISurfacePtr) as IDXGISurface;
                hr = m_pD2DDeviceContext.CreateBitmapFromDxgiSurface(pDXGISurface, ref bitmapProperties, out m_pD2DTargetBitmap);
                if (hr == HRESULT.S_OK)
                {
                    m_pD2DDeviceContext.SetTarget(m_pD2DTargetBitmap);
                }
                SafeRelease(ref pDXGISurface);
                Marshal.Release(pDXGISurfacePtr);
            }
            return hr;
        }

        void Clean()
        {
            SafeRelease(ref m_pD2DDeviceContext);
            SafeRelease(ref m_pD2DDevice);            
            //SafeRelease(ref m_pD2DDeviceContext3);

            //CleanDeviceResources();

            SafeRelease(ref m_pD2DTargetBitmap);
            SafeRelease(ref m_pDXGISwapChain1);

            SafeRelease(ref m_pDXGIDevice);
            SafeRelease(ref m_pD3D11DeviceContext);
            Marshal.Release(m_pD3D11DevicePtr);

            SafeRelease(ref m_pWICImagingFactory);
            SafeRelease(ref m_pWICImagingFactory2);
            SafeRelease(ref m_pD2DFactory1);
            SafeRelease(ref m_pD2DFactory);
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            Clean();
        } 
    }

    // Adapted from WinUI 3 Gallery
    public class CustomDataObject
    {
        public string Title { get; set; }
        public string ImageLocation { get; set; }
        //public string Views { get; set; }
        //public string Likes { get; set; }
        public string Description { get; set; }

        public CustomDataObject()
        {
        }

        public static List<CustomDataObject> GetDataObjects(string sRelativePath)
        { 
            List<CustomDataObject> objects = new List<CustomDataObject>();
            string sExePath = AppContext.BaseDirectory;
            string sImageDirectory = System.IO.Path.Combine(sExePath, sRelativePath);
            var images = Directory.EnumerateFiles(sImageDirectory, "*.*", SearchOption.AllDirectories);
            foreach (string sCurrentFile in images)
            {
                //string sFileName = currentFile.Substring(sImageDirectory.Length + 1);
                IPropertyStore pPropertyStore = null;
                Guid PropertyStoreGuid = typeof(IPropertyStore).GUID;
                HRESULT hr = SHGetPropertyStoreFromParsingName(sCurrentFile, IntPtr.Zero, GETPROPERTYSTOREFLAGS.GPS_READWRITE, ref PropertyStoreGuid, out pPropertyStore);
                string sTitle = "";
                if (hr == HRESULT.S_OK)
                {                  
                    var pv = new PROPVARIANT();
                    hr = pPropertyStore.GetValue(PKEY_Title, out pv);
                    if (hr == HRESULT.S_OK)
                    {
                        sTitle = Marshal.PtrToStringUni(pv.pwszVal);                       
                    }
                    Marshal.ReleaseComObject(pPropertyStore);
                }
                objects.Add(new CustomDataObject()
                {
                    Title = sTitle,
                    ImageLocation = sCurrentFile
                });
            }
            return objects;
        }

        [DllImport("Shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern HRESULT SHGetPropertyStoreFromParsingName(string pszPath, IntPtr pbc, GETPROPERTYSTOREFLAGS flags, ref Guid iid, [MarshalAs(UnmanagedType.Interface)] out IPropertyStore propertyStore);

        public enum GETPROPERTYSTOREFLAGS
        {
            GPS_DEFAULT = 0,
            GPS_HANDLERPROPERTIESONLY = 0x1,
            GPS_READWRITE = 0x2,
            GPS_TEMPORARY = 0x4,
            GPS_FASTPROPERTIESONLY = 0x8,
            GPS_OPENSLOWITEM = 0x10,
            GPS_DELAYCREATION = 0x20,
            GPS_BESTEFFORT = 0x40,
            GPS_NO_OPLOCK = 0x80,
            GPS_PREFERQUERYPROPERTIES = 0x100,
            GPS_EXTRINSICPROPERTIES = 0x200,
            GPS_EXTRINSICPROPERTIESONLY = 0x400,
            GPS_VOLATILEPROPERTIES = 0x800,
            GPS_VOLATILEPROPERTIESONLY = 0x1000,
            GPS_MASK_VALID = 0x1FFF
        }

        [ComImport, Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPropertyStore
        {
            HRESULT GetCount([Out] out uint propertyCount);
            HRESULT GetAt([In] uint propertyIndex, [Out, MarshalAs(UnmanagedType.Struct)] out PROPERTYKEY key);
            HRESULT GetValue([In, MarshalAs(UnmanagedType.Struct)] ref PROPERTYKEY key, [Out, MarshalAs(UnmanagedType.Struct)] out PROPVARIANT pv);
            HRESULT SetValue([In, MarshalAs(UnmanagedType.Struct)] ref PROPERTYKEY key, [In, MarshalAs(UnmanagedType.Struct)] ref PROPVARIANT pv);
            HRESULT Commit();
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct PROPERTYKEY
        {
            private readonly Guid _fmtid;
            private readonly uint _pid;

            public PROPERTYKEY(Guid fmtid, uint pid)
            {
                _fmtid = fmtid;
                _pid = pid;
            }
        }

        public static readonly PROPERTYKEY PKEY_Title = new PROPERTYKEY(new Guid("F29F85E0-4FF9-1068-AB91-08002B27B3D9"), 2);      
        public static readonly PROPERTYKEY PKEY_Keywords = new PROPERTYKEY(new Guid("F29F85E0-4FF9-1068-AB91-08002B27B3D9"), 5);
        public static readonly PROPERTYKEY PKEY_Comment = new PROPERTYKEY(new Guid("F29F85E0-4FF9-1068-AB91-08002B27B3D9"), 6);

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct PROPARRAY
        {
            public UInt32 cElems;
            public IntPtr pElems;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        public struct PROPVARIANT
        {
            [FieldOffset(0)]
            public ushort varType;
            [FieldOffset(2)]
            public ushort wReserved1;
            [FieldOffset(4)]
            public ushort wReserved2;
            [FieldOffset(6)]
            public ushort wReserved3;

            [FieldOffset(8)]
            public byte bVal;
            [FieldOffset(8)]
            public sbyte cVal;
            [FieldOffset(8)]
            public ushort uiVal;
            [FieldOffset(8)]
            public short iVal;
            [FieldOffset(8)]
            public UInt32 uintVal;
            [FieldOffset(8)]
            public Int32 intVal;
            [FieldOffset(8)]
            public UInt64 ulVal;
            [FieldOffset(8)]
            public Int64 lVal;
            [FieldOffset(8)]
            public float fltVal;
            [FieldOffset(8)]
            public double dblVal;
            [FieldOffset(8)]
            public short boolVal;
            [FieldOffset(8)]
            public IntPtr pclsidVal; // GUID ID pointer  
            [FieldOffset(8)]
            public IntPtr pszVal; // Ansi string pointer  
            [FieldOffset(8)]
            public IntPtr pwszVal; // Unicode string pointer  
            [FieldOffset(8)]
            public IntPtr punkVal; // punkVal (interface pointer)  
            [FieldOffset(8)]
            public PROPARRAY ca;
            [FieldOffset(8)]
            public System.Runtime.InteropServices.ComTypes.FILETIME filetime;
        }

        public enum VARENUM
        {
            VT_EMPTY = 0,
            VT_NULL = 1,
            VT_I2 = 2,
            VT_I4 = 3,
            VT_R4 = 4,
            VT_R8 = 5,
            VT_CY = 6,
            VT_DATE = 7,
            VT_BSTR = 8,
            VT_DISPATCH = 9,
            VT_ERROR = 10,
            VT_BOOL = 11,
            VT_VARIANT = 12,
            VT_UNKNOWN = 13,
            VT_DECIMAL = 14,
            VT_I1 = 16,
            VT_UI1 = 17,
            VT_UI2 = 18,
            VT_UI4 = 19,
            VT_I8 = 20,
            VT_UI8 = 21,
            VT_INT = 22,
            VT_UINT = 23,
            VT_VOID = 24,
            VT_HRESULT = 25,
            VT_PTR = 26,
            VT_SAFEARRAY = 27,
            VT_CARRAY = 28,
            VT_USERDEFINED = 29,
            VT_LPSTR = 30,
            VT_LPWSTR = 31,
            VT_RECORD = 36,
            VT_INT_PTR = 37,
            VT_UINT_PTR = 38,
            VT_FILETIME = 64,
            VT_BLOB = 65,
            VT_STREAM = 66,
            VT_STORAGE = 67,
            VT_STREAMED_OBJECT = 68,
            VT_STORED_OBJECT = 69,
            VT_BLOB_OBJECT = 70,
            VT_CF = 71,
            VT_CLSID = 72,
            VT_VERSIONED_STREAM = 73,
            VT_BSTR_BLOB = 0xfff,
            VT_VECTOR = 0x1000,
            VT_ARRAY = 0x2000,
            VT_BYREF = 0x4000,
            VT_RESERVED = 0x8000,
            VT_ILLEGAL = 0xffff,
            VT_ILLEGALMASKED = 0xfff,
            VT_TYPEMASK = 0xfff
        };
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isOn)
            {               
                return isOn ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
