/*
 * ApproxD 3D Engine. Simple 3D engine without Graphics Libraries, only WinAPI and C# 3.5
 * Copyright (C) 2026  RedstoneShell
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using System;
using System.Text;
using System.Management;
using System.Reflection;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Linq;
using Microsoft.Win32;

using RedstoneShell.ApproxD.Collectors;
using RedstoneShell.ApproxD.GraphicsNat;

namespace RedstoneShell.ApproxD
{
    [StructLayout(
        LayoutKind.Sequential,
        CharSet = CharSet.Auto
    )]
    public struct WNDCLASS
    {
        public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string lpszMenuName;
        public string lpszClassName;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG {
        public IntPtr hwnd;
        public uint   message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint   time;
        public System.Drawing.Point  pt;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFO {
        public BITMAPINFOHEADER bmiHeader;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=256)]
        public uint[] bmiColors;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFOHEADER {
        public uint biSize;
        public int  biWidth;
        public int  biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int  biXPelsPerMeter;
        public int  biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT { public int X; public int Y; }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    public static class WinAccessor {
        // Window Messages Segment
        public static readonly uint WM_DESTROY     = 0x0002;
        public static readonly uint WM_RESIZE      = 0x0006;
        public static readonly uint WM_CLOSE       = 0x0010;
        public static readonly uint WM_KEYDOWN     = 0x0100;
        public static readonly uint WM_KEYUP       = 0x0101;
        public static readonly uint WM_MOUSEMOVE   = 0x0200;
        public static readonly uint WM_LBUTTONDOWN = 0x0201;
        public static readonly uint WM_LBUTTONUP   = 0x0202;

        // Window display modes
        public static readonly int SW_HIDE        = 0;
        public static readonly int SW_NORMAL      = 1;
        public static readonly int SW_SHOWMINIMIZE= 2;
        public static readonly int SW_MAXIMIZE    = 3;
        public static readonly int SW_NOACTIVE    = 4;
        public static readonly int SW_SHOW        = 5;
        public static readonly int SW_MINIMIZE    = 6;
        public static readonly int SW_FORCEMIN    = 11;

        // Bitmap Info
        public static readonly uint BI_RGB = 0;

        // External API
        public static bool IsKeyDown(int code) {
            return (GetAsyncKeyState(code)&0x8000)!=0;
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CreateWindowEx(
            int exStyle, 
            IntPtr className,
            string windowName,
            int style, int x, int y, int width, int height,
            IntPtr parent, IntPtr menu, IntPtr instance, IntPtr param);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetMessage(out MSG msg, IntPtr hWnd, uint min, uint max);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int PeekMessage(out MSG msg, IntPtr hWnd, uint min, uint max, uint ahh);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr DispatchMessage(ref MSG msg);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool TranslateMessage(ref MSG msg);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr malloc(IntPtr size);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void free(IntPtr ptr);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr calloc(IntPtr num, IntPtr size);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern ushort RegisterClass(ref WNDCLASS lpWndClass);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr LoadCursor(IntPtr hInstance, IntPtr lpCursorName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void PostQuitMessage(int nExitCode);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);



        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern int SetDIBitsToDevice(
            IntPtr hdc,
            int xDest,
            int yDest,
            int w,
            int h,
            int xSrc,
            int ySrc,
            uint startScan,
            uint cLines,
            IntPtr lpvBits,
            ref BITMAPINFO lpbmi,
            uint colorUse
        );

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateFont(
            int nHeight, int nWidth, int nEscapement, int nOrientation, 
            int fnWeight, uint fdwItalic, uint fdwUnderline, uint fdwStrikeOut, 
            uint fdwCharSet, uint fdwOutputPrecision, uint fdwClipPrecision, 
            uint fdwQuality, uint fdwPitchAndFamily, string lpszFace);

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        public static extern bool TextOut(IntPtr hdc, int x, int y, string lpString, int c);

        [DllImport("gdi32.dll")]
        public static extern int SetBkMode(IntPtr hdc, int mode);

        [DllImport("gdi32.dll")]
        public static extern uint SetTextColor(IntPtr hdc, uint color);



        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentThread();

        [DllImport("kernel32.dll")]
        public static extern IntPtr SetThreadAffinityMask(IntPtr hThread, IntPtr dwThreadAffinityMask);

        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(IntPtr proc, IntPtr min, IntPtr max);


        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static unsafe extern void* MemSet(void* dest, int c, long count);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr memset(IntPtr dest, int value, IntPtr count);
    }

    public struct VirtualKeyCodes {
        public static readonly int VK_W      = 0x57;
        public static readonly int VK_S      = 0x53;
        public static readonly int VK_A      = 0x41;
        public static readonly int VK_D      = 0x44;
        public static readonly int VK_SPACE  = 0x20;
        public static readonly int VK_LSHIFT = 0xA1;
        public static readonly int VK_ESC    = 0x1B;

        // For debug
        public static readonly int VK_F3     = 0x72;
        public static readonly int VK_RSHIFT = 0xA0;
    }

    /// <summary>
    /// Render Buffer to render in GDI32 window
    /// </summary>
    public unsafe class RenderFrame {
        private static EEBF8x8Font font8x8 = EEBF8x8Font.Load(AppDomain.CurrentDomain.BaseDirectory + "fontdb/keyboard.eebf");

        public static uint* bufferPtr = null;
        public static float* zBuffer = null;

        private static IntPtr nativeBuffer = IntPtr.Zero;
        private static IntPtr nativeZBuffer = IntPtr.Zero;

        public static Object2ParamField<int, int> viewport;
        private static readonly object _sync = new object();

        public static void InitFramebuffer(int w, int h) {
            if (w <= 0 || h <= 0) return;

            lock (_sync) {
                Shutdown();

                viewport = new Object2ParamField<int, int>();
                viewport.SetValue(w, h);

                int pixelCount = w * h;
                nativeBuffer = WinAccessor.calloc((IntPtr)pixelCount, (IntPtr)sizeof(uint));
                nativeZBuffer = WinAccessor.malloc((IntPtr)(pixelCount * sizeof(float)));

                bufferPtr = (uint*)nativeBuffer;
                zBuffer = (float*)nativeZBuffer;

                Debug_.Log("Rendering System: Initialized "+w+"x"+h+" videobuffer.");
            }
        }

        public static void Shutdown() {
            if (nativeBuffer != IntPtr.Zero) { WinAccessor.free(nativeBuffer); nativeBuffer = IntPtr.Zero; }
            if (nativeZBuffer != IntPtr.Zero) { WinAccessor.free(nativeZBuffer); nativeZBuffer = IntPtr.Zero; }
        }

        public static void ChangeFont(EEBF8x8Font font)
        {
            font8x8 = font;
        }

        public static void DrawString(int x, int y, string text, uint color) {
            int startX = x;
            foreach (char c in text) {
                if (c == '\n') { y += 8; x = startX; continue; }
                DrawChar(x, y, c, color);
                x += 8;
            }
        }
    
        public static void DrawChar(int x, int y, char c, uint color) {
            if (c >= 256 || bufferPtr == null) return;
            int w = viewport.v1;
            for (int row = 0; row < 8; row++) {
                byte line = font8x8.Glyphs[c, row];
                if (line == 0) continue;
                int py = y + row;
                if (py < 0 || py >= viewport.v2) continue;
                int baseIdx = py * w;
                for (int col = 0; col < 8; col++) {
                    if ((line & (1 << col)) != 0) {
                        int px = x + col;
                        if (px >= 0 && px < w) bufferPtr[baseIdx + px] = color;
                    }
                }
            }
        }

        public static void ApplyFXAA() {
            if (bufferPtr == null) return;
            int w = viewport.v1;
            int h = viewport.v2;

            for (int y = 1; y < h - 1; y++) {
                uint* prevRow = bufferPtr + (y - 1) * w;
                uint* currRow = bufferPtr + y * w;
                uint* nextRow = bufferPtr + (y + 1) * w;

                for (int x = 1; x < w - 1; x++) {
                    uint c = currRow[x];
                    uint cx = currRow[x + 1];
                    uint cy = nextRow[x];

                    int r = (int)(c & 255), g = (int)((c >> 8) & 255), b = (int)((c >> 16) & 255);
                    int rx = (int)(cx & 255), gx = (int)((cx >> 8) & 255), bx = (int)((cx >> 16) & 255);
                    int ry = (int)(cy & 255), gy = (int)((cy >> 8) & 255), by = (int)((cy >> 16) & 255);

                    if (Math.Abs(r - rx) + Math.Abs(g - gx) + Math.Abs(b - bx) > 40) {
                        currRow[x] = (255u << 24) | ((uint)((r + rx + ry) / 3) & 0xFF)
                                                 | (((uint)((g + gx + gy) / 3) & 0xFF) << 8)
                                                 | (((uint)((b + bx + by) / 3) & 0xFF) << 16);
                    }
                }
            }
        }

        public static void Resize(int w, int h) { InitFramebuffer(w, h); }

        public unsafe static void BeginFrame() {
            Monitor.Enter(_sync);
        }

        public unsafe static void ClearBuffers() {
            int pixelCount = viewport.v1 * viewport.v2;
            uint color = (uint)ApproxD.SkyboxStdColor;
            ulong doubleColor = ((ulong)color << 32) | color;
            float maxZ = float.MaxValue;
            uint zBits = *(uint*)&maxZ;
            ulong doubleZ = ((ulong)zBits << 32) | zBits;
            ulong* bPtr64 = (ulong*)bufferPtr;
            ulong* zPtr64 = (ulong*)zBuffer;
            int i = 0;
            int count64 = pixelCount / 2;
            for (; i < count64; i++) {
                bPtr64[i] = doubleColor;
                zPtr64[i] = doubleZ;
            }

            if (pixelCount % 2 != 0) {
                bufferPtr[pixelCount - 1] = color;
                zBuffer[pixelCount - 1] = maxZ;
            }
        }

        public static void PutPixel(int x, int y, double z, uint src) {
            if ((uint)x >= (uint)viewport.v1 || (uint)y >= (uint)viewport.v2) return;
            int idx = x + y * viewport.v1;
            float fz = (float)z;
            if (fz >= zBuffer[idx]) return;

            uint sa = (src >> 24);
            if (sa == 0) return;
            if (sa == 255) { zBuffer[idx] = fz; bufferPtr[idx] = src; return; }

            uint dst = bufferPtr[idx];
            uint rb = (dst & 0xFF00FF) + ((((src & 0xFF00FF) - (dst & 0xFF00FF)) * sa) >> 8) & 0xFF00FF;
            uint g = (dst & 0x00FF00) + ((((src & 0x00FF00) - (dst & 0x00FF00)) * sa) >> 8) & 0x00FF00;

            zBuffer[idx] = fz;
            bufferPtr[idx] = (255u << 24) | rb | g;
        }

        public static void EndFrame() {
            Monitor.Exit(_sync);
        }

        public static void DrawLine(Vec3 a, Vec3 b, uint color) {
            int x0=(int)a.x, y0=(int)a.y, x1=(int)b.x, y1=(int)b.y;
            int dx=Math.Abs(x1-x0), dy=Math.Abs(y1-y0);
            int sx = x0<x1 ? 1:-1, sy = y0<y1 ? 1:-1;
            int err = dx-dy;

            while(true)
            {
                PutPixel(x0,y0,(a.z+b.z)*0.5d,color);
                if(x0==x1 && y0==y1) break;
                int e2 = err<<1;
                if(e2>-dy){err-=dy;x0+=sx;}
                if(e2<dx){err+=dx;y0+=sy;}
            }
        }

        public static void DrawTriangle(Vec3 v1, Vec3 v2, Vec3 v3, uint color) {
            if (v1.y > v2.y) { var t = v1; v1 = v2; v2 = t; }
            if (v1.y > v3.y) { var t = v1; v1 = v3; v3 = t; }
            if (v2.y > v3.y) { var t = v2; v2 = v3; v3 = t; }

            int total_height = (int)(v3.y - v1.y);
            if (total_height == 0) return;

            for (int i = 0; i < total_height; i++) {
                bool second_half = i > v2.y - v1.y || v2.y == v1.y;
                int segment_height = second_half ? (int)(v3.y - v2.y) : (int)(v2.y - v1.y);

                float alpha = (float)i / total_height;
                float beta  = (float)(i - (second_half ? (v2.y - v1.y) : 0)) / segment_height;

                Vec3 A = v1 + (v3 - v1) * alpha;
                Vec3 B = second_half ? v2 + (v3 - v2) * beta : v1 + (v2 - v1) * beta;

                if (A.x > B.x) { var t = A; A = B; B = t; }
                for (int j = (int)A.x; j <= (int)B.x; j++) {
                    double phi = B.x == A.x ? 1.0 : (double)(j - A.x) / (B.x - A.x);
                    double current_z = A.z + (B.z - A.z) * phi;

                    PutPixel(j, (int)(v1.y + i), current_z, color);
                }
            }
        }

        public unsafe static void RawPutPixel(int idx, uint color) {
            bufferPtr[idx] = color;
        }
    }

    /// <summary>
    /// Internal ApproxD Font System to lightweight font loading, to work used EEBF (Eight-Eight Byte Font) system, for creating use this table
    /// Offset    Size    Desciption
    /// 0         4       Signature "EEBF"
    /// 4         1       Version (1)
    /// 5         1       Width (8)
    /// 6         1       Height (8)
    /// 7         1       Reserved (0)
    /// 8         32      Font name (ASCII, padded with 0)
    /// 40        2048    Glyph data (256 chars x 8 bytes)
    /// or use EEBF8x8Font.CompileFrom(byte[,] data);
    /// </summary>
    public sealed class EEBF8x8Font
    {
        public string Name { get; private set; }
        public byte[,] Glyphs { get; private set; }
        public const int GlyphWidth = 8;
        public const int GlyphHeight = 8;
        public const int GlyphCount = 256;
        public const int HeaderSize = 40;
        public const int ExpectedSize = HeaderSize + (GlyphCount * GlyphHeight);

        private EEBF8x8Font()
        {
            Glyphs = new byte[256, 8];
        }

        public static EEBF8x8Font Load(string path)
        {
            byte[] data;
            if (path== null || !File.Exists(path)) {
                Debug_.Error("EEBF Loader: Attempt to load not exists font! For not engine crash we replace to \"chk_notdef.eebf\", chk own program fonts if u lost copy font to path");
                data = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + "fontdb/chk_notdef.eebf");
            } else data = File.ReadAllBytes(path);

            if (data.Length != ExpectedSize)
                throw new Exception("Invalid EEBF file size.");

            if (data[0] != 'E' || data[1] != 'E' ||
                data[2] != 'B' || data[3] != 'F')
                throw new Exception("Invalid EEBF signature.");

            if (data[4] != 1)
                throw new Exception("Unsupported EEBF version.");

            if (data[5] != 8 || data[6] != 8)
                throw new Exception("Unsupported font size.");

            var font = new EEBF8x8Font();
            font.Name = Encoding.ASCII.GetString(data, 8, 32).TrimEnd('\0');

            int glyphOffset = 40;

            for (int c = 0; c < 256; c++)
            {
                for (int row = 0; row < 8; row++)
                {
                    font.Glyphs[c, row] = data[glyphOffset++];
                }
            }

            return font;
        }

        public static EEBF8x8Font CompileFrom(byte[,] glyphData, string name)
        {
            if (name == null) name = "Unnamed";
            if (glyphData == null)
                throw new ArgumentNullException("glyphData");
            if (glyphData.GetLength(0) != GlyphCount ||
                glyphData.GetLength(1) != GlyphHeight)
                throw new Exception("Glyph table must be [256,8].");
            var font = new EEBF8x8Font();
            font.Name = name;
            for (int c = 0; c < GlyphCount; c++)
            {
                for (int row = 0; row < GlyphHeight; row++)
                {
                    font.Glyphs[c, row] = glyphData[c, row];
                }
            }
    
            return font;
        }

        public void Save(string path)
        {
            byte[] data = new byte[ExpectedSize];
            data[0] = (byte)'E';
            data[1] = (byte)'E';
            data[2] = (byte)'B';
            data[3] = (byte)'F';
            data[4] = 1;
            data[5] = GlyphWidth;
            data[6] = GlyphHeight;
            data[7] = 0;
            byte[] nameBytes = Encoding.ASCII.GetBytes(Name ?? "Unnamed");
            Array.Copy(nameBytes, 0, data, 8,
                Math.Min(32, nameBytes.Length));
            int offset = HeaderSize;
            for (int c = 0; c < GlyphCount; c++)
            {
                for (int row = 0; row < GlyphHeight; row++)
                {
                    data[offset++] = Glyphs[c, row];
                }
            }
    
            File.WriteAllBytes(path, data);
        }
    }

    public class CrashFactory {
        public static void CreateLog(string code) {
            string reason = "Unknown";
            switch (code)
            {
                case "AEE00001":reason = "Window creating error"; break;
                case "AEE00002":reason = "GPU Session open failed";break;
                default:reason = "Unknown engine error";break;
            }
            Debug_.Fatal("In ApproxD run procces, occured fatal error and engine stop working to safety.");
            Debug_.Raw("");
            Debug_.Raw("*************************** CRASH REPORT ***************************");
            Debug_.Raw("Engine: ApproxD");
            Debug_.Raw("Code:   " + code);
            Debug_.Raw("Reason: " + reason);
            int winErr = Marshal.GetLastWin32Error();
            Debug_.Raw("Win32 Error: " + winErr);
            var ex = new Win32Exception(winErr);
            if (ex != null)
            {
                Debug_.Raw("");
                Debug_.Raw("Exception:");
                Debug_.Raw(ex.GetType().FullName);
                Debug_.Raw(ex.Message);
                Debug_.Raw(ex.StackTrace);
            }
            Debug_.Raw("********************************************************************");
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Internal Window information store, to optimize calling in window
    /// </summary>
    public class ApproxD_WndInfo {
        /// <summary>
        /// The Window with all data, you can use ApproxD.CurrentOpenedWindow
        /// </summary>
        public ApplicationManager Window;
        /// <summary>
        /// Rendering graphics to GDI32 render by HDC
        /// </summary>
        public Graphics GDI_Rect;
        /// <summary>
        /// Only for GDI32, NOT CHANGE IN WORK
        /// </summary>
        public IntPtr HDC;
        /// <summary>
        /// Only for Optimization and GDI32, NOT CHANGE IN WORK
        /// </summary>
        public BITMAPINFO Bmi;
        /// <sumary>
        /// Render switch CPU/GPU
        /// </summary>
        public bool gpuRenderEnabled = false;
    }

    /// <summary>   
    /// Use only in own GPU drawing or internal/external
    /// </summary>
    public class InternalDriverData
    {
        /// <summary> 
        /// Driver class, need to extends the GPUDrawingDummy class
        /// </summary>
        public GPUDrawingDummy drvClass;
        /// <summary> 
        /// Unique driver ID to receive commands from ApproxD Engine
        /// </summary>
        public GMUID           drvUID;
    }

    public class ApproxD {
        public static readonly string EngineID               = "1";
        public static EEBF8x8Font[] FontDatabase;
        public static bool IsStockGraphicsUnitEnabled        = true;
        public static object[] pc_info                       = new object[2];
        public static uint SkyboxStdColor                    = 0xFF000000;
        public static bool isClientWork                      = false;
        public static int WindowsRegistered                  = 0;
        public static volatile float dT                      = 0;
        public static ulong mpm                              = 0;
        public static bool IsDebugEnabled                    = true;
        public static bool IsDebugMenuVis =false, WireframeE = false;
        public static string ApplicationClassName            = "ApproxD_Window_";
        public static ApplicationManager CurrentOpenedWindow = null;
        public static List<ApproxD_WndInfo> InternalRAID     = new List<ApproxD_WndInfo>();
        public static List<InternalDriverData> DriversList   = new List<InternalDriverData>();
        public static WndProc ReservedDelegate;

        private static bool IsInstalled(StoreName name, string certName) {
                    X509Store store = new X509Store(name, StoreLocation.LocalMachine);
                    store.Open(OpenFlags.ReadOnly);
                    X509Certificate2Collection found = store.Certificates.Find(X509FindType.FindBySubjectName, certName, false);
                    store.Close();
                    return found.Count > 0;
                }

        private static void InstallCertificate(string sysPath)
        {
            try
            {
                X509Certificate2 cert = new X509Certificate2(sysPath);
                X509Store store = new X509Store(StoreName.TrustedPublisher, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite);
                if (!store.Certificates.Contains(cert))
                {
                    store.Add(cert);
                    Console.WriteLine("Certificate added to Trusted Publishers.");
                }
                store.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cert Install Error: " + ex.Message);
            }
        }

        public static void RunClient() {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Exception ex = (Exception)e.ExceptionObject;
                Debug_.LogException(ex);
            };
            try
            {
                Debug_.Log("For-dev technical information: ");
                ulong maxRam = 0;
                string selectedGpu = "Unknown";
                var searcher = new ManagementObjectSearcher(
                    "select Name, AdapterRAM from Win32_VideoController");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string name = obj["Name"] != null 
                        ? obj["Name"].ToString() 
                        : "Unknown GPU";
                    object ramObj = obj["AdapterRAM"];
                    ulong ram = 0;
                    if (ramObj != null)
                    {
                        try
                        {
                            ram = Convert.ToUInt64(ramObj);
                        }
                        catch
                        {
                            Debug_.Warn("Failed to convert AdapterRAM for " + name);
                        }
                    }

                    Debug_.Info("GPU detected: " + name);
                    Debug_.Info("VRAM detected: " + (ram / 1024 / 1024) + " MB");
                    if (ram > maxRam)
                    {
                        maxRam = ram;
                        selectedGpu = name;
                    }
                }
                pc_info[0] = selectedGpu;
                pc_info[1] = maxRam;
                Debug_.Info("Selected GPU: " + selectedGpu);
                Debug_.Info("Selected VRAM: " + (maxRam / 1024 / 1024) + " MB");
                string certName = "RedstoneShell and ApproxD Inc";
                string certPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, 
                    "ApproxD.cer");

                bool inRoot = IsInstalled(StoreName.Root, certName);
                bool inPublisher = IsInstalled(StoreName.TrustedPublisher, certName);

                if (inRoot && inPublisher)
                {
                    Debug_.Info("ApproxD CRTRS already registered. Skipping...");
                }
                else
                {
                    if (File.Exists(certPath))
                    {
                        Debug_.Info("ApproxD cert not registered or incomplete, registering...");

                        ProcessStartInfo certInfo = new ProcessStartInfo(
                            "certutil.exe",
                            "-addstore -f \"Root\" \"" + certPath + "\"");

                        certInfo.Verb = "runas";
                        certInfo.WindowStyle = ProcessWindowStyle.Hidden;

                        Process.Start(certInfo).WaitForExit();

                        certInfo.Arguments =
                            "-addstore -f \"TrustedPublisher\" \"" + certPath + "\"";

                        Process.Start(certInfo).WaitForExit();

                        InstallCertificate(certPath);

                        Debug_.Info("Registration finished.");
                    }
                    else
                    {
                        Debug_.Error("ApproxD.cer not found! Cannot establish trust.");
                    }
                    string exe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApproxDEEBFviewer.exe");
                    using (var key = Registry.ClassesRoot.CreateSubKey(".eebf"))
                    {
                        key.SetValue("", "EEBFFile");
                        key.SetValue("Content Type", "application/x-font-eebf");
                    }
                    using (var key = Registry.ClassesRoot.CreateSubKey("EEBFFile"))
                    {
                        key.SetValue("", "Eight-Eight Byte Font");
                        key.SetValue("FriendlyTypeName", "Eight-Eight Byte Font");
                    }
                    using (var key = Registry.ClassesRoot.CreateSubKey(@"EEBFFile\shell\open\command"))
                    {
                        key.SetValue("", "\""+exe+"\" \"%1\"");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug_.Warn("ApproxD check/register cert failed: " + ex.Message);
            }

            GarbageCollector.InvokePrelaunchClean();

            string dllPath = Assembly.GetExecutingAssembly().Location;
            Debug_.Log("ApproxD Engine registered in AppDomain by path " + dllPath);
            Assembly.LoadFrom(dllPath);
            isClientWork = true;
            Debug_.Log("ApproxD Ver " + EngineID + ", launching...");
            ApplicationClassName = "ApproxD_Window_0";

            DriversList.Clear();

            // RedstoneShell: Registering all extends GPUDrawingDummy class drivers in ECL (Engine Code List)
            Assembly[] dll = AppDomain.CurrentDomain.GetAssemblies();
            Debug_.Info("D3DKMT Driver System: Start registering GPU rendering drivers. Wait some time...");
            foreach (var asm in dll)
            {
                if (string.IsNullOrEmpty(asm.Location)) continue;
                Type[] types;
                try { types = asm.GetTypes(); } catch { continue; }
                foreach (var t in types)
                {
                    if (t.IsClass && !t.IsAbstract && typeof(GPUDrawingDummy).IsAssignableFrom(t)) {
                        try {
                            var i = (GPUDrawingDummy)Activator.CreateInstance(t);
                            var dat = new InternalDriverData();
                            dat.drvClass = i;
                            dat.drvUID   = GMUID.GetRandom();
                            Debug_.Info("D3DKMT Driver System: Registering in stack new driver: \"" + dat.drvClass + "\", GMUID: \"" + dat.drvUID.ToStr() +"\"");
                            DriversList.Add(dat);
                        } catch (Exception e) {
                            Debug_.Warn("D3DKMT Driver System: Registering of \""+t.FullName+"\" failed by: " + e.Message);
                        }
                    }
                }
            }

            // RedstoneShell: Start updating cycle if drivers count >1
            if (DriversList.Count <= 0) Debug_.Warn("D3DKMT Driver System: In DST (Driver Store Stack) not added anyone driver(s). Possibly is API or internal error, switch to 'GPU Render' disbaled!");

            new Thread(()=> {
                // RedstoneShell FOR DEV:
                // Specially algorithm to call external/internal GPU RAW drawing system
                // First init all drives as send to init method Unique ID, Max VRAM alloc size and TimeStamp
                while (CurrentOpenedWindow == null) Thread.Sleep(1);
                DrawingSystemAccessor.OpenSession(CurrentOpenedWindow.WindowHandler);
                IntPtr hdc = Graphics.FromHwnd(CurrentOpenedWindow.WindowHandler).GetHdc();
                var luid = DrawingSystemAccessor.GetAdapterLuidFromHdc(hdc);
                if (luid.HighPart == 0 && luid.LowPart == 0) Debug_.Error("GPU: Invalid Adapter LUID (0:0)");
                ulong ttl = DrawingSystemAccessor.GetTotalVRAM(luid);
                if (pc_info[0] != null && pc_info[0].ToString().IndexOf("Intel", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Debug_.Log("GPU: Using Intel Bay Trail Celeron(R) graphics.");
                    ttl=Convert.ToUInt64(pc_info[1]);
                } else if (ttl==0) {Debug_.Warn("GPU: VRAM returned 0. Using fallback 512MB."); ttl = 512ul * 1024 * 1024;}
                ulong max = (ttl * 65) / 100;
                mpm = DriversList.Count > 0
                    ? max / (ulong)DriversList.Count
                    : 0;
                Debug_.Log("Allocated VRAM at modules MB/KB/B: "+mpm/1024/1024+"/"+mpm/1024+"/"+mpm);
                foreach (InternalDriverData drv in DriversList) {
                    string time = Debug_.TimeStamp();
                    drv.drvClass.OnModuleEnable(mpm, drv.drvUID, time);
                }

                Stopwatch sw = new Stopwatch();
                sw.Start();
                float lastTime = 0, fpsTimer = 0;
                int frameCount = 0;
                string displayFPS = "0", displayFT = "0";
                while (isClientWork) {
                    float currentTime = (float)sw.Elapsed.TotalSeconds;
                    float delta = currentTime - lastTime, avgFPS=0;
                    if (delta <= 0) continue;
                    dT = currentTime - lastTime;
                    lastTime = currentTime;
                    fpsTimer += delta;
                    frameCount++;
                    if (fpsTimer >= 0.5f) {
                       avgFPS = frameCount / fpsTimer;
                       double avgFT = (fpsTimer / frameCount) * 1000;
                       displayFPS = avgFPS.ToString("F0");
                       displayFT = avgFT.ToString("F5");
                       fpsTimer = 0;
                       frameCount = 0;
                    }
                    foreach (InternalDriverData drv in DriversList) {
                        if (!FindWnd(CurrentOpenedWindow).gpuRenderEnabled) break;
                        drv.drvClass.OnModuleUpdate(dT);
                    }
                    if (!isClientWork) break;
                    Thread.SpinWait(1000);
                }

                Debug_.Log("D3DKMT Driver System: Disabling drivers and shutdown engine...");
                foreach (InternalDriverData drv in DriversList) {
                    string time = Debug_.TimeStamp();
                    drv.drvClass.OnModuleDisable(drv.drvUID, time);
                    Debug_.Info("D3DKMT Driver System: Driver \""+drv.drvClass+"\" shutdown. Watch driver log to correct shutdown!");
                }
                RenderFrame.Shutdown();
            }).Start();
        }

        static ApproxD_WndInfo FindWnd(ApplicationManager wnd)
        {
            return InternalRAID.Find(x => x.Window == wnd);
        }

        public static bool RunExternalCmd(string executable, string args_list, bool outEn) {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = executable;
            psi.Verb = "runas";
            psi.Arguments = args_list;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.UseShellExecute = false;      
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true; 
            psi.CreateNoWindow = true;
            try {
                Process p = Process.Start(psi);
                if (outEn) {
                    p.OutputDataReceived += (sender, e) => {
                        if (!string.IsNullOrEmpty(e.Data)) {
                            Console.WriteLine(e.Data);
                        }
                    };
                    p.BeginOutputReadLine();
                }
                p.WaitForExit();
                return true;
            } catch (Exception ex) {
                Debug_.Fatal("Error at run external process: " + ex.Message);
                return false;
            }
        }

        public static void WindowAdd(ApplicationManager Wnd) {
            Console.WriteLine("[ApproxD/CLIENT]: Registered new window, ClassName: 'ApproxD_Window_"+WindowsRegistered+"'.");
            WindowsRegistered = WindowsRegistered+1;
            ApplicationClassName = "ApproxD_Window_"+WindowsRegistered;
            CurrentOpenedWindow = Wnd;
            var info = new ApproxD_WndInfo();
            info.Window = Wnd;
            info.GDI_Rect = Graphics.FromHwnd(Wnd.WindowHandler);
            info.HDC = info.GDI_Rect.GetHdc();
            info.Bmi = new BITMAPINFO();
            info.Bmi.bmiHeader.biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER));
            info.Bmi.bmiHeader.biPlanes = 1;
            info.Bmi.bmiHeader.biBitCount = 32;
            info.Bmi.bmiHeader.biCompression = WinAccessor.BI_RGB;
            info.Bmi.bmiColors = new uint[256];
            InternalRAID.Add(info);
        }

        public static void OnWindowUpdate(MSG data, IntPtr wid) {
            DrawBBToWindow(wid);
        }

        public static IntPtr Reserved(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam) {
            if (hWnd==null) return new IntPtr(0);
            switch ((int)msg) {
                case 0x0005: // WM_SIZE
                    int nw = (int)(lParam.ToInt32() & 0xFFFF);
                    int nh = (int)((lParam.ToInt32() >> 16) & 0xFFFF);
                    CurrentOpenedWindow.Width  = nw;
                    CurrentOpenedWindow.Height = nh;
                    RenderFrame.Resize(nw, nh);
                    return IntPtr.Zero;
                case 0x000F: // WM_PAINT
                    DrawBBToWindow(hWnd);
                    return IntPtr.Zero;
                case 0x0002: // WM_DESTROY
                    WinAccessor.PostQuitMessage(0);
                    var data = FindWnd(CurrentOpenedWindow);
                    data.GDI_Rect.ReleaseHdc(data.HDC);
                    data.GDI_Rect.Dispose();
                    isClientWork = false;
                    return IntPtr.Zero;
            }
            return WinAccessor.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        public static unsafe void DrawBBToWindow(IntPtr hWnd) {
            if (FindWnd(CurrentOpenedWindow) == null || RenderFrame.bufferPtr == null) { 
                Debug_.Warn("GDI32: Cannot render Scene on empty window! If you watch this message in first time, not react on this, is defend from crash of empty/null RenderFrame."); 
                return; 
            }
            var data = FindWnd(CurrentOpenedWindow);
            IntPtr hdc = data.HDC;
            data.Bmi.bmiHeader.biWidth = RenderFrame.viewport.v1;
            data.Bmi.bmiHeader.biHeight = -RenderFrame.viewport.v2;

            WinAccessor.SetDIBitsToDevice(
                hdc,
                0, 0,
                RenderFrame.viewport.v1,
                RenderFrame.viewport.v2,
                0, 0,
                0,
                (uint)RenderFrame.viewport.v2,
                (IntPtr)RenderFrame.bufferPtr,
                ref data.Bmi,
                0
            );
        }
    }

    public static class GarbageCollector
    {
        private static bool il=false;
        private static DateTime lastClean = DateTime.Now;

        public static void InvokePrelaunchClean()
        {
            if(il==true) return;
            System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.LowLatency;
            il=true;
        }

        public static void FlushMemory() {
            long memUsage = GC.GetTotalMemory(false) / 1024 / 1024;
            if (memUsage > 20 && (DateTime.Now - lastClean).TotalSeconds > 30) {
               GC.Collect(2, GCCollectionMode.Forced);
               GC.WaitForPendingFinalizers();
               Debug_.Log("ApproxD GC: Critical memory flush. Usage was: "+memUsage+"MB");
               lastClean = DateTime.Now;
            }
        }

        public static void CleanScene(List<Entity> sceneObjects, Vec3 cameraPos)
        {
            int removedCount = sceneObjects.RemoveAll(obj => obj.IsPendingKill);
            const double maxDistance = 5000.0;
            int farCount = sceneObjects.RemoveAll(obj => 
                (obj.Layer == EntityLayer.Default || obj.Layer == EntityLayer.Projectile) && 
                (obj.Position - cameraPos).Length() > maxDistance);
            if (removedCount > 0 || farCount > 0)
            {
                Debug_.Log("ApproxD GC: Scene cleaned. Removed "+removedCount+" dead, "+farCount+" far objects.");
            }
        }

        public static void Deduplicate(List<Entity> objects)
        {
            var unique = objects.Distinct().ToList();
            if(unique.Count != objects.Count)
            {
                objects.Clear();
                objects.AddRange(unique);
                Debug_.Log("ApproxD GC: Duplicated entities removed from scene.");
            }
        }
    }

    public class ApplicationManager 
    {
        public int              Width, Height;
        public string           WindowName;
        public IntPtr           WindowHandler;
        public Action           OnUpdateExternal;
        public bool IsDebugMenuVis =false, shiftHandled=false, f3Handled=false;
        public Camera           MainCamera   = new Camera();
        public List<Entity>     SceneObjects = new List<Entity>();
        public List<PointLight> SceneLights  = new List<PointLight>();
        public long             VTC          = 0L;
        private Entity[]        _renderQueue;
        private int             _renderCount;
        private StringBuilder   debugFolder = new StringBuilder(1024);
        private long            lastFrameMemory = 0;
        private long            gcDelta = 0;

        public ApplicationManager(int w, int h, string name) {
            Width = w;
            Height = h;
            WindowName = name;
            ApproxD.ReservedDelegate = ApproxD.Reserved;
            string currentClass = ApproxD.ApplicationClassName;
            IntPtr hInst = WinAccessor.GetModuleHandle(null);

            WNDCLASS wc = new WNDCLASS();
            wc.style = 3;
            wc.lpfnWndProc = Marshal.GetFunctionPointerForDelegate(ApproxD.ReservedDelegate);
            wc.hInstance = hInst;
            wc.lpszClassName = currentClass;
            wc.hCursor = WinAccessor.LoadCursor(IntPtr.Zero, (IntPtr)32512);
            
            // RedstoneShell: Its works, butt only at some op's
            ushort atom = WinAccessor.RegisterClass(ref wc);
            IntPtr classRef;
            if (atom != 0) {
                classRef = (IntPtr)((uint)atom & 0xFFFF);
            } else {
                classRef = Marshal.StringToHGlobalAuto("ApproxD_Win7_Class");
            }

            WindowHandler = WinAccessor.CreateWindowEx(
                0,
                classRef,
                WindowName,
                0x00CF0000, // WS_OVERLAPPEDWINDOW
                100, 100, w, h,
                IntPtr.Zero, IntPtr.Zero, hInst, IntPtr.Zero
            );

            if (WindowHandler == IntPtr.Zero) {
                int realErr = Marshal.GetLastWin32Error();
                Debug_.Fatal("CreateWindow dead. Error: " + realErr);
            } else {
                WinAccessor.ShowWindow(WindowHandler, 5);
                Debug_.Log("New Scene created in ApproxD, HandleID: " + WindowHandler);
                ApproxD.WindowAdd(this);
            }
        }

        public void AddObject(Entity obj) {
            SceneObjects.Add(obj);
        }

        public void AddLamp(PointLight obj) {
            SceneLights.Add(obj);
        }

        private static readonly object _syncLock = new object();
        public void StartLifecycle()
        {
            MSG msg;
            RenderFrame.InitFramebuffer(Width, Height);
            _renderQueue = new Entity[1024];
            Debug_.Log("Initialized Rendering System. Framebuffer calculating...");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            float lastTime = 0, fpsTimer = 0, dT=0, avgFPS=0;
            int frameCount = 0;
            string displayFPS = "0", displayFT = "0";
            int lastGen0Count = GC.CollectionCount(0);
            string displayGCUS = "0";
            while (WinAccessor.GetMessage(out msg, IntPtr.Zero, 0, 0)>0)
            {
                OnUpdateExternal.Invoke();
                WinAccessor.TranslateMessage(ref msg);
                WinAccessor.DispatchMessage(ref msg);
                ApproxD.OnWindowUpdate(msg, WindowHandler);
                long currentMemory = GC.GetTotalMemory(false);
                gcDelta = currentMemory - lastFrameMemory;
                float aspect = (float)Width / (float)Height;
                Mat4 projMatrix = Mat4.Projection(MainCamera.Fov, aspect, 0.1f, 1000.0f);
                RenderFrame.BeginFrame();
                RenderFrame.ClearBuffers();
                RenderScene(MainCamera.GetViewMatrix(), projMatrix);
                if (IsDebugMenuVis)
                {
                    // FOR DEV INFO:
                    // GCM  - Garbage Collector Memory in MB
                    // GCOC - Garbage Collector Objects Count, gen0 - fast objects (changbl vars, fast-freq changable value, and other), gen1 - buffer zone (variables with same value at long time, other long-time), gen2 - old memory (static classes, loaded resources, buffers)
                    // GCUS - GC Updates per Second
                    // ΔGC  - GC Allocations on last frame
                    // TC   - Threads Count
                    uint fpsColor = avgFPS > 100 ? 0xFF00FF00 : (avgFPS > 50 ? 0xFFFFFF00 : 0xFFFF0000);

                    RenderFrame.DrawString(5, 5,  (debugFolder.Length = 0) == 0 ? debugFolder.Append("ApproxD Engine [").Append(ApproxD.EngineID).Append("] RAW GPU/CPU").ToString() : "", 0xFF00FF00);
                    RenderFrame.DrawString(5, 17, (debugFolder.Length = 0) == 0 ? debugFolder.Append("Wireframe: ").Append(ApproxD.WireframeE ? "ON" : "OFF").ToString() : "", 0xFF00FF00);
                    RenderFrame.DrawString(5, 29, (debugFolder.Length = 0) == 0 ? debugFolder.Append("Triangles: ").Append(VTC).ToString() : "", 0xFF00FF00);
                    RenderFrame.DrawString(5, 41, (debugFolder.Length = 0) == 0 ? debugFolder.Append("VRAM per driver: ").Append(ApproxD.mpm / 1048576).Append(" MB").ToString() : "", 0xFF00FF00);

                    debugFolder.Length = 0;
                    debugFolder.Append("Camera Y/P: ").Append(MainCamera.Rotation.y.ToString("F2")).Append(" / ").Append(MainCamera.Rotation.x.ToString("F2"))
                               .Append(", Pos: ").Append(MainCamera.Position.x.ToString("F1")).Append("/").Append(MainCamera.Position.y.ToString("F1")).Append("/").Append(MainCamera.Position.z.ToString("F1"));
                    RenderFrame.DrawString(5, 53, debugFolder.ToString(), 0xFF00FF00);

                    RenderFrame.DrawString(5, 65, (debugFolder.Length = 0) == 0 ? debugFolder.Append("Frame Time: ").Append(displayFT).Append(" ms").ToString() : "", 0xFF00FF00);

                    debugFolder.Length = 0;
                    debugFolder.Append("GCM: ").Append(GC.GetTotalMemory(false) / 1048576).Append(" MB, GCOC: ").Append(GC.CollectionCount(0)).Append("/").Append(GC.CollectionCount(1)).Append("/").Append(GC.CollectionCount(2)).Append(", GCUS: ").Append(displayGCUS).Append("/s, ΔGC: ").Append(gcDelta);
                    RenderFrame.DrawString(5, 77, debugFolder.ToString(), 0xFF00FF00);

                    RenderFrame.DrawString(5, 89, (debugFolder.Length = 0) == 0 ? debugFolder.Append("FPS: ").Append(displayFPS).Append(", TC: ").Append(Process.GetCurrentProcess().Threads.Count).ToString() : "", fpsColor);
                }
                RenderFrame.EndFrame();
                RenderFrame.ApplyFXAA();
                bool isShiftPressed = WinAccessor.IsKeyDown(VirtualKeyCodes.VK_LSHIFT);
                if (isShiftPressed && !shiftHandled && ApproxD.IsDebugEnabled) {
                    IsDebugMenuVis = !IsDebugMenuVis;
                    Debug_.Log("Debug: Debug menu "+(IsDebugMenuVis==true?"opened":"closed")+".");
                    shiftHandled = true;
                } else if (!isShiftPressed) {
                    shiftHandled = false;
                }
                bool isF3Pressed = WinAccessor.IsKeyDown(VirtualKeyCodes.VK_F3);
                if (isF3Pressed && !f3Handled&&ApproxD.IsDebugEnabled) {
                    ApproxD.WireframeE = !ApproxD.WireframeE;
                    f3Handled = true;
                    Debug_.Log("Debug: Wireframe: "+(ApproxD.WireframeE ? "ON" : "OFF"));
                } else if (!isF3Pressed) {
                    f3Handled = false;
                }
                ApproxD.DrawBBToWindow(WindowHandler);
                float currentTime = (float)sw.Elapsed.TotalSeconds;
                float delta = currentTime - lastTime;
                if (delta <= 0) continue;
                dT = currentTime - lastTime;
                lastTime = currentTime;
                fpsTimer += delta;
                frameCount++;
                if (fpsTimer >= 0.5f) {
                    avgFPS = frameCount / fpsTimer;
                    double avgFT = (fpsTimer / frameCount) * 1000;

                    int currentGen0 = GC.CollectionCount(0);
                    displayGCUS = ((currentGen0 - lastGen0Count) / fpsTimer).ToString("F1");
                    lastGen0Count = currentGen0;

                    displayFPS = avgFPS.ToString("F0");
                    displayFT = avgFT.ToString("F5");

                    fpsTimer = 0;
                    frameCount = 0;
                }
                lastFrameMemory = currentMemory;
            }
        }

        public void RenderScene(Mat4 view, Mat4 proj)
        {
            VTC =0L;
            _renderCount =0;
            for (int i = 0; i < SceneObjects.Count; i++) {
                RenderObject(SceneObjects[i], view, proj);
            }
        }

        private unsafe void RenderObject(Entity obj, Mat4 view, Mat4 proj) {
            Vec3 lightDir = new Vec3(0.5, 1.0, -1.0).Normalize();
            if (obj == null || obj.Mesh == null)
                return;

            for (int i = 0; i < obj.Mesh.Length; i += 3) {
                Vec3* worldVerts = stackalloc Vec3[3];

                for (int j = 0; j < 3; j++) {
                    Vec3 v = obj.Mesh[i + j];
                    v = new Vec3(v.x * obj.Scale.x, v.y * obj.Scale.y, v.z * obj.Scale.z);
                    v = RotatePoint(v, obj.Rotation) + obj.Position;
                    worldVerts[j] = v;
                }

                Vec3 line1 = worldVerts[1] - worldVerts[0];
                Vec3 line2 = worldVerts[2] - worldVerts[0];
                Vec3 normal = Vec3.Cross(line1, line2).Normalize();
                Vec3 viewDir = (MainCamera.Position - worldVerts[0]).Normalize();

                double ambient = obj.Material.Ambient;
                double diffuse = 0;
                double specular = 0;

                foreach (var light in SceneLights) {
                    Vec3 L = (light.Position - worldVerts[0]);
                    double dist = L.Length();
                    if (dist > light.Range) continue;
                    L = L.Normalize();
                    double NdotL = Math.Max(0, Vec3.Dot(normal, L));
                    Vec3 H = (L + viewDir).Normalize();
                    double NdotH = Math.Max(0, Vec3.Dot(normal, H));
                    double spec = Math.Pow(NdotH, obj.Material.Shininess);
                    double atten = 1.0 / (1.0 + dist * dist * 0.05);
                    diffuse += NdotL * atten * light.Intensity;
                    specular += spec * atten * light.Intensity;
                }

                double intensity = ambient + diffuse * obj.Material.Diffuse + specular * obj.Material.Specular;
                intensity = Math.Min(1.0, intensity);

                Vec3* ndc = stackalloc Vec3[3];
                bool visible = true;
                bool inside = false;

                for (int j = 0; j < 3; j++) {
                    Vec3 vView = Mat4.Multiply(view, worldVerts[j]);

                    if (vView.z <= 0.01) {
                        visible = false;
                        break;
                    }

                    Vec3 p = Mat4.Multiply(proj, vView);
                    ndc[j] = p;

                    if (p.x >= -1 && p.x <= 1 && p.y >= -1 && p.y <= 1 && p.z >= 0 && p.z <= 1) {
                        inside = true;
                    }
                }

                if (!visible || !inside) continue;

                Vec3* screenVerts = stackalloc Vec3[3];
                for (int j = 0; j < 3; j++) {
                    screenVerts[j] = new Vec3(
                        (ndc[j].x + 1) * 0.5 * Width,
                        (1 - ndc[j].y) * 0.5 * Height,
                        ndc[j].z
                    );
                }

                if(!MeshHelper.IsFaceVisible(screenVerts[0], screenVerts[1], screenVerts[2], MainCamera.Position)) 
                    continue;
                uint shadedColor = ApplyIntensity(obj.Color, intensity, obj.Material.Alpha);
                if (ApproxD.WireframeE) 
                {
                    RenderFrame.DrawLine(screenVerts[0], screenVerts[1], shadedColor);
                    RenderFrame.DrawLine(screenVerts[1], screenVerts[2], shadedColor);
                    RenderFrame.DrawLine(screenVerts[2], screenVerts[0], shadedColor);
                }
                else RenderFrame.DrawTriangle(screenVerts[0], screenVerts[1], screenVerts[2], shadedColor);
                VTC=VTC+1L;
            }
        }

        public static uint ApplyIntensity(uint color, double intensity, double alpha)
        {
            byte a = (byte)(alpha * 255);

            byte r = (byte)(((color >> 16) & 255) * intensity);
            byte g = (byte)(((color >> 8) & 255) * intensity);
            byte b = (byte)((color & 255) * intensity);

            return (uint)(
                (a << 24) |
                (r << 16) |
                (g << 8) |
                b
            );
        }

        private Vec3 RotatePoint(Vec3 v, Vec3 rot) {
            double x = v.x, y = v.y, z = v.z;
            double tx, ty, tz;
            ty = y * Math.Cos(rot.x) - z * Math.Sin(rot.x);
            tz = y * Math.Sin(rot.x) + z * Math.Cos(rot.x);
            y = ty; z = tz;
            tx = x * Math.Cos(rot.y) + z * Math.Sin(rot.y);
            tz = -x * Math.Sin(rot.y) + z * Math.Cos(rot.y);
            x = tx; z = tz;
            tx = x * Math.Cos(rot.z) - y * Math.Sin(rot.z);
            ty = x * Math.Sin(rot.z) + y * Math.Cos(rot.z);
            x = tx; y = ty;
            return new Vec3(x, y, z);
        }
    }

    public class Camera {
        public Vec3 Position = new Vec3(0, 0, -5);
        public Vec3 Rotation = new Vec3(0, 0, 0);
        public float Fov = 90.0f;

        public Mat4 GetViewMatrix() {
            return Mat4.View(Position, Rotation);
        }
    }

    public static class MeshHelper {
        public static bool IsFaceVisible(Vec3 v0, Vec3 v1, Vec3 v2, Vec3 cameraPos) {
            Vec3 edge1 = v1 - v0;
            Vec3 edge2 = v2 - v0;
            Vec3 normal = Vec3.Cross(edge1, edge2).Normalize();
            Vec3 viewDir = (v0 - cameraPos).Normalize();
            double dot = Vec3.Dot(normal, viewDir);
            return dot > 0; 
        }

        public static Vec3[] GetTesseractMesh() {
            Vec4[] verts4D = GetTesseractVertices();
            Vec3[] verts3D = new Vec3[16];
            for(int i=0;i<16;i++) verts3D[i] = Project4Dto3D(verts4D[i], 3.0);

            List<Vec3> mesh = new List<Vec3>();

            int[][] edges = new int[][] {
                new int[]{0,1}, new int[]{0,2}, new int[]{0,4}, new int[]{0,8},
                new int[]{1,3}, new int[]{1,5}, new int[]{1,9},
                new int[]{2,3}, new int[]{2,6}, new int[]{2,10},
                new int[]{3,7}, new int[]{3,11},
                new int[]{4,5}, new int[]{4,6}, new int[]{4,12},
                new int[]{5,7}, new int[]{5,13},
                new int[]{6,7}, new int[]{6,14},
                new int[]{7,15},
                new int[]{8,9}, new int[]{8,10}, new int[]{8,12},
                new int[]{9,11}, new int[]{9,13},
                new int[]{10,11}, new int[]{10,14},
                new int[]{11,15},
                new int[]{12,13}, new int[]{12,14},
                new int[]{13,15},
                new int[]{14,15}
            };

            foreach(var e in edges) {
                int a = e[0];
                int b = e[1];
                Vec3 v0 = verts3D[a];
                Vec3 v1 = verts3D[b];
                Vec3 v2 = (v0+v1)*0.5; 
                mesh.Add(v0); mesh.Add(v1); mesh.Add(v2);
            }

            return mesh.ToArray();
        }

        public static Vec3 Project4Dto3D(Vec4 v, double distance) {
            double factor = distance / (distance - v.w); // w-perspective
            return new Vec3(v.x * factor, v.y * factor, v.z * factor);
        }

        public static Vec4[] GetTesseractVertices() {
            Vec4[] verts = new Vec4[16];
            int idx = 0;
            for (int i=0;i<16;i++) {
                verts[idx++] = new Vec4(
                    ((i & 1) == 0 ? -1 : 1),
                    ((i & 2) == 0 ? -1 : 1),
                    ((i & 4) == 0 ? -1 : 1),
                    ((i & 8) == 0 ? -1 : 1)
                );
            }
            return verts;
        }

        public static Vec3[] GetCubeMesh() {
            return new Vec3[] {
                new Vec3(-1, -1, -1), new Vec3(-1,  1, -1), new Vec3( 1,  1, -1),
                new Vec3(-1, -1, -1), new Vec3( 1,  1, -1), new Vec3( 1, -1, -1),
                new Vec3( 1, -1, -1), new Vec3( 1,  1, -1), new Vec3( 1,  1,  1),
                new Vec3( 1, -1, -1), new Vec3( 1,  1,  1), new Vec3( 1, -1,  1),
                new Vec3( 1, -1,  1), new Vec3( 1,  1,  1), new Vec3(-1,  1,  1),
                new Vec3( 1, -1,  1), new Vec3(-1,  1,  1), new Vec3(-1, -1,  1),
                new Vec3(-1, -1,  1), new Vec3(-1,  1,  1), new Vec3(-1,  1, -1),
                new Vec3(-1, -1,  1), new Vec3(-1,  1, -1), new Vec3(-1, -1, -1),
                new Vec3(-1,  1, -1), new Vec3(-1,  1,  1), new Vec3( 1,  1,  1),
                new Vec3(-1,  1, -1), new Vec3( 1,  1,  1), new Vec3( 1,  1, -1),
                new Vec3( 1, -1,  1), new Vec3(-1, -1,  1), new Vec3(-1, -1, -1),
                new Vec3( 1, -1,  1), new Vec3(-1, -1, -1), new Vec3( 1, -1, -1)
            };
        }

        public static Vec3[] GetPyramidMesh(
            double size,
            double height
        )
        {
            double s = size;

            Vec3 top = new Vec3(0, height, 0);

            Vec3 a = new Vec3(-s, 0, -s);
            Vec3 b = new Vec3( s, 0, -s);
            Vec3 c = new Vec3( s, 0,  s);
            Vec3 d = new Vec3(-s, 0,  s);

            return new Vec3[]
            {
                top, a, b,
                top, b, c,
                top, c, d,
                top, d, a,

                a, d, c,
                a, c, b
            };
        }

        public static Vec3[] GetSphereMesh(
            int stacks,
            int slices,
            double radius
        )
        {
            List<Vec3> verts = new List<Vec3>();

            for (int i = 0; i < stacks; i++)
            {
                double lat0 = Math.PI * (-0.5 + (double)i / stacks);
                double lat1 = Math.PI * (-0.5 + (double)(i + 1) / stacks);

                double y0 = Math.Sin(lat0);
                double y1 = Math.Sin(lat1);

                double r0 = Math.Cos(lat0);
                double r1 = Math.Cos(lat1);

                for (int j = 0; j < slices; j++)
                {
                    double lon0 = 2 * Math.PI * j / slices;
                    double lon1 = 2 * Math.PI * (j + 1) / slices;

                    double x0 = Math.Cos(lon0);
                    double z0 = Math.Sin(lon0);

                    double x1 = Math.Cos(lon1);
                    double z1 = Math.Sin(lon1);

                    Vec3 v1 = new Vec3(x0 * r0, y0, z0 * r0) * radius;
                    Vec3 v2 = new Vec3(x0 * r1, y1, z0 * r1) * radius;
                    Vec3 v3 = new Vec3(x1 * r1, y1, z1 * r1) * radius;
                    Vec3 v4 = new Vec3(x1 * r0, y0, z1 * r0) * radius;

                    verts.Add(v1);
                    verts.Add(v2);
                    verts.Add(v3);

                    verts.Add(v1);
                    verts.Add(v3);
                    verts.Add(v4);
                }
            }

            return verts.ToArray();
        }
    }

    public static class Debug_
    {
        private static readonly object _lock = new object();
        private static readonly Stopwatch sw = Stopwatch.StartNew();
        private static readonly string LogFile =
            "Logs/ApproxD_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log";

        static Debug_()
        {
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory+"/Logs")) Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory+"/Logs");
            WriteHeader();
        }

        private static void WriteHeader()
        {
            lock (_lock)
            {
                File.AppendAllText(
                    LogFile,

                    "===== ApproxD Engine Log =====\r\n" +
                    "Start: " + DateTime.Now + "\r\n" +
                    "================================\r\n\r\n"
                );
            }
        }

        public static string TimeStamp()
        {
            DateTime now = DateTime.Now;

            long nano =
                sw.ElapsedTicks * 1000000000L / Stopwatch.Frequency;

            return
                now.ToString("yyyy-MM-dd HH:mm:ss") + "." +
                now.Millisecond.ToString("D3") + "_" +
                nano.ToString("D9");
        }

        private static void Write(string level, string text)
        {
            string line =
                "|" + TimeStamp() + "| [ApproxD/" + level + "] " + text;

            lock (_lock)
            {
                if (level != "RAW-Format") {
                    Console.WriteLine(line);
                    File.AppendAllText(LogFile, line + "\r\n");
                } else {
                    Console.WriteLine(text);
                    File.AppendAllText(LogFile, text + "\r\n");
                }
            }
        }

        internal static void Log(string text)
        {
            Write("INTERNAL", text);
        }

        public static void Info(string text)
        {
            Write("INFO", text);
        }

        public static void Warn(string text)
        {
            Write("WARN", text);
        }

        public static void Error(string text)
        {
            Write("ERROR", text);
        }

        public static void Fatal(string text)
        {
            Write("FATAL", text);
        }

        public static void Raw(string txt) {
            Write("RAW-Format", txt);
        }

        public static void LogException(Exception ex) {
            Write("Exception", ex.GetType()+"("+ex.InnerException+"): "+ex.Message+"\n"+ex.StackTrace+"\n\n");
        }
    }


    #region Objects
    public class PointLight
    {
        public Vec3 Position;
        public double Intensity = 1;
        public double Range = 10;
        public uint Color;
    }

    #endregion
}