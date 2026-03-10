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
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using RedstoneShell.ApproxD;
using RedstoneShell.ApproxD.Collectors;

namespace RedstoneShell.ApproxD.GraphicsNat
{
    [StructLayout(LayoutKind.Sequential, Pack=8)]
    public unsafe struct D3DKMT_CREATEALLOCATION
    {
        public IntPtr hDevice;                   // [in] Хендл пристрою
        public IntPtr hResource;                 // [in/out] Хендл ресурсу
        public IntPtr hGlobalShare;              // [out] Глобальний хендл
        public IntPtr pPrivateRuntimeData;       // [in]
        public uint PrivateRuntimeDataSize;      // [in]
        public IntPtr pStandardAllocation;       // [in]
        public IntPtr pPrivateDriverData;        // [in]
        public uint PrivateDriverDataSize;       // [in/out]
        public uint NumAllocations;              // [in]
        public IntPtr pAllocationInfo;           // [in]
        public uint Flags;                       // [in]
        public ulong hPrivateRuntimeResourceHandle; // [in]
    }

    [StructLayout(LayoutKind.Sequential, Pack=8)]
    public struct D3DDDI_ALLOCATIONINFO
    {
        public IntPtr hAllocation;           // [in/out] хендл
        public IntPtr pSystemMem;           
        public IntPtr pPrivateDriverData;    
        public uint PrivateDriverDataSize;   
        public uint VidPnSourceId;           
        public uint Flags;                    
    }

    [StructLayout(LayoutKind.Sequential, Pack=8)]
    public struct D3DKMT_DESTROYALLOCATION
    {
        public IntPtr hDevice;
        public IntPtr hResource;
        public IntPtr phAllocationList;
        public uint AllocationCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack=8)]
    public struct D3DKMT_CREATEDEVICE
    {
        public IntPtr hAdapter;             
        public uint Flags;                
        public IntPtr hDevice;              
        public IntPtr pPrivateDriverData; 
        public uint PrivateDriverDataSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack=8)]
    public struct D3DKMT_OPENADAPTERFROMHDC
    {
        public IntPtr hDc;
        public IntPtr hAdapter;
        public LUID AdapterLuid;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LUID
    {
        public uint LowPart;
        public int HighPart;
    }

    [StructLayout(LayoutKind.Sequential, Pack=8)]
    public struct D3DKMT_QUERYSTATISTICS
    {
        public D3DKMT_QUERYSTATISTICS_TYPE Type;
        public LUID AdapterLuid;
        public IntPtr hProcess;
        public D3DKMT_QUERYSTATISTICS_RESULT QueryResult;
        public D3DKMT_QUERYSTATISTICS_UNION Data;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DKMT_QUERYSTATISTICS_RESULT
    {
        public int Status;
    }

    [StructLayout(LayoutKind.Explicit, Pack=8)]
    public struct D3DKMT_QUERYSTATISTICS_UNION
    {
        [FieldOffset(0)]
        public D3DKMT_QUERYSTATISTICS_QUERY_SEGMENT QuerySegment;
        [FieldOffset(0)]
        public D3DKMT_QUERYSTATISTICS_QUERY_ADAPTER2 QueryAdapter2;
    }

    [StructLayout(LayoutKind.Sequential, Pack=8)]
    public struct D3DKMT_QUERYSTATISTICS_QUERY_SEGMENT
    {
        public ulong CommitLimit;
        public ulong BytesCommitted;
    }

    [StructLayout(LayoutKind.Sequential, Pack=8)]
    public struct D3DKMT_QUERYSTATISTICS_QUERY_ADAPTER2
    {
        public ulong DedicatedVideoMemory;
        public ulong DedicatedSystemMemory;
        public ulong SharedSystemMemory;
    }

    [StructLayout(LayoutKind.Sequential, Pack=8)]
    public unsafe struct D3DKMT_QUERYADAPTERINFO
    {
        public IntPtr hAdapter;
        public KMTQUERYADAPTERINFOTYPE Type;
        public void* pPrivateDriverData;
        public uint PrivateDriverDataSize;
    }

    public enum D3DKMT_QUERYSTATISTICS_TYPE : int { Adapter = 0, Segment = 1, Process = 2, Node = 3, VidPnSource = 4, }

    public enum KMTQUERYADAPTERINFOTYPE {
        KMTQAITYPE_UMDRIVERPRIVATE,
        KMTQAITYPE_UMDRIVERNAME,
        KMTQAITYPE_UMOPENGLINFO,
        KMTQAITYPE_GETSEGMENTSIZE,
        KMTQAITYPE_ADAPTERGUID,
        KMTQAITYPE_FLIPQUEUEINFO,
        KMTQAITYPE_ADAPTERADDRESS,
        KMTQAITYPE_SETWORKINGSETINFO,
        KMTQAITYPE_ADAPTERREGISTRYINFO,
        KMTQAITYPE_CURRENTDISPLAYMODE,
        KMTQAITYPE_MODELIST,
        KMTQAITYPE_CHECKDRIVERUPDATESTATUS,
        KMTQAITYPE_VIRTUALADDRESSINFO,
        KMTQAITYPE_DRIVERVERSION,
        KMTQAITYPE_ADAPTERTYPE,
        KMTQAITYPE_OUTPUTDUPLCONTEXTSCOUNT,
        KMTQAITYPE_WDDM_1_2_CAPS,
        KMTQAITYPE_UMD_DRIVER_VERSION,
        KMTQAITYPE_DIRECTFLIP_SUPPORT,
        KMTQAITYPE_MULTIPLANEOVERLAY_SUPPORT,
        KMTQAITYPE_DLIST_DRIVER_NAME,
        KMTQAITYPE_WDDM_1_3_CAPS,
        KMTQAITYPE_MULTIPLANEOVERLAY_HUD_SUPPORT,
        KMTQAITYPE_WDDM_2_0_CAPS,
        KMTQAITYPE_NODEMETADATA,
        KMTQAITYPE_CPDRIVERNAME,
        KMTQAITYPE_XBOX,
        KMTQAITYPE_INDEPENDENTFLIP_SUPPORT,
        KMTQAITYPE_MIRACASTCOMPANIONDRIVERNAME,
        KMTQAITYPE_PHYSICALADAPTERCOUNT,
        KMTQAITYPE_PHYSICALADAPTERDEVICEIDS,
        KMTQAITYPE_DRIVERCAPS_EXT,
        KMTQAITYPE_QUERY_MIRACAST_DRIVER_TYPE,
        KMTQAITYPE_QUERY_GPUMMU_CAPS,
        KMTQAITYPE_QUERY_MULTIPLANEOVERLAY_DECODE_SUPPORT,
        KMTQAITYPE_QUERY_HW_PROTECTION_TEARDOWN_COUNT,
        KMTQAITYPE_QUERY_ISBADDRIVERFORHWPROTECTIONDISABLED,
        KMTQAITYPE_MULTIPLANEOVERLAY_SECONDARY_SUPPORT,
        KMTQAITYPE_INDEPENDENTFLIP_SECONDARY_SUPPORT,
        KMTQAITYPE_PANELFITTER_SUPPORT,
        KMTQAITYPE_PHYSICALADAPTERPNPKEY,
        KMTQAITYPE_GETSEGMENTGROUPSIZE,
        KMTQAITYPE_MPO3DDI_SUPPORT,
        KMTQAITYPE_HWDRM_SUPPORT,
        KMTQAITYPE_MPOKERNELCAPS_SUPPORT,
        KMTQAITYPE_MULTIPLANEOVERLAY_STRETCH_SUPPORT,
        KMTQAITYPE_GET_DEVICE_VIDPN_OWNERSHIP_INFO,
        KMTQAITYPE_QUERYREGISTRY,
        KMTQAITYPE_KMD_DRIVER_VERSION,
        KMTQAITYPE_BLOCKLIST_KERNEL,
        KMTQAITYPE_BLOCKLIST_RUNTIME,
        KMTQAITYPE_ADAPTERGUID_RENDER,
        KMTQAITYPE_ADAPTERADDRESS_RENDER,
        KMTQAITYPE_ADAPTERREGISTRYINFO_RENDER,
        KMTQAITYPE_CHECKDRIVERUPDATESTATUS_RENDER,
        KMTQAITYPE_DRIVERVERSION_RENDER,
        KMTQAITYPE_ADAPTERTYPE_RENDER,
        KMTQAITYPE_WDDM_1_2_CAPS_RENDER,
        KMTQAITYPE_WDDM_1_3_CAPS_RENDER,
        KMTQAITYPE_QUERY_ADAPTER_UNIQUE_GUID,
        KMTQAITYPE_NODEPERFDATA,
        KMTQAITYPE_ADAPTERPERFDATA,
        KMTQAITYPE_ADAPTERPERFDATA_CAPS,
        KMTQUITYPE_GPUVERSION,
        KMTQAITYPE_DRIVER_DESCRIPTION,
        KMTQAITYPE_DRIVER_DESCRIPTION_RENDER,
        KMTQAITYPE_SCANOUT_CAPS,
        KMTQAITYPE_DISPLAY_UMDRIVERNAME,
        KMTQAITYPE_PARAVIRTUALIZATION_RENDER,
        KMTQAITYPE_SERVICENAME,
        KMTQAITYPE_WDDM_2_7_CAPS,
        KMTQAITYPE_TRACKEDWORKLOAD_SUPPORT,
        KMTQAITYPE_HYBRID_DLIST_DLL_SUPPORT,
        KMTQAITYPE_DISPLAY_CAPS,
        KMTQAITYPE_WDDM_2_9_CAPS,
        KMTQAITYPE_CROSSADAPTERRESOURCE_SUPPORT,
        KMTQAITYPE_WDDM_3_0_CAPS,
        KMTQAITYPE_WSAUMDIMAGENAME,
        KMTQAITYPE_VGPUINTERFACEID,
        KMTQAITYPE_WDDM_3_1_CAPS,
        KMTQAITYPE_HYBRID_DLIST_DLL_MUX_SUPPORT
    }

    public enum NtStatus : uint
    {
        STATUS_SUCCESS = 0x00000000,
        STATUS_PENDING = 0x00000103,

        STATUS_INVALID_PARAMETER = 0xC000000D,
        STATUS_INVALID_HANDLE = 0xC0000008,
        STATUS_NO_MEMORY = 0xC0000017,
        STATUS_ACCESS_DENIED = 0xC0000022,

        STATUS_GRAPHICS_NO_VIDEO_MEMORY = 0xC00002F0,
        STATUS_GRAPHICS_INVALID_VIDEO_MEMORY_ACCESS = 0xC00002F1,
        STATUS_GRAPHICS_CANT_LOCK_MEMORY = 0xC00002F2,
        STATUS_GRAPHICS_ALLOCATION_BUSY = 0xC00002F3,
        STATUS_GRAPHICS_TOO_MANY_REFERENCES = 0xC00002F4,
        STATUS_GRAPHICS_TRY_AGAIN_LATER = 0xC00002F5,
        STATUS_GRAPHICS_ALLOCATION_INVALID = 0xC00002F6,
        STATUS_GRAPHICS_UNSWIZZLING_APERTURE_UNAVAILABLE = 0xC00002F7,

        STATUS_GRAPHICS_DRIVER_INTERNAL_ERROR = 0xC00002FD,
        STATUS_GRAPHICS_DEVICE_HUNG = 0xC00002FE,
        STATUS_GRAPHICS_DEVICE_REMOVED = 0xC00002FF,
    }

    internal class GPUNativeAccess {
        [DllImport("gdi32.dll")]
        internal static extern int D3DKMTCreateAllocation(ref D3DKMT_CREATEALLOCATION pData);

        [DllImport("gdi32.dll")]
        public static extern int D3DKMTOpenAdapterFromHdc(ref D3DKMT_OPENADAPTERFROMHDC pData);
    
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("gdi32.dll")]
        public static extern int D3DKMTCreateDevice(ref D3DKMT_CREATEDEVICE pData);

        [DllImport("gdi32.dll")]
        public static extern int D3DKMTDestroyAllocation(ref D3DKMT_DESTROYALLOCATION pData);

        [DllImport("gdi32.dll")]
        public static extern int D3DKMTQueryStatistics(ref D3DKMT_QUERYSTATISTICS pData);

        [DllImport("gdi32.dll")]
        public static extern int D3DKMTQueryAdapterInfo(ref D3DKMT_QUERYADAPTERINFO pData);
    }

    /// <summary>
    /// GPU Raw C/C++ access from dxgkrnl.sys driver
    /// </summary>
    public static class DrawingSystemAccessor
    {
        public static D3DKMT_CREATEDEVICE? RenderingInfo = null;
        public static D3DKMT_OPENADAPTERFROMHDC Reserved0;
        /// <summary>
        /// Start session of drawing (calling on engine start)
        /// </summary>
        public static void OpenSession(IntPtr hWnd)
        {
            IntPtr hdc = GPUNativeAccess.GetDC(hWnd);
            if (hdc == IntPtr.Zero)
            {
                Debug_.Fatal("Graphics System: GetDC failed");
                return;
            }
            var openData = new D3DKMT_OPENADAPTERFROMHDC();
            openData.hDc = hdc;
            int status = GPUNativeAccess.D3DKMTOpenAdapterFromHdc(ref openData);
            GPUNativeAccess.ReleaseDC(hWnd, hdc);
            if ((NtStatus)status != NtStatus.STATUS_SUCCESS)
            {
                Debug_.Fatal("OpenAdapterFromHdc failed: 0x" + status.ToString("X"));
                return;
            }
            Reserved0 = openData;
            Debug_.Info(
                "Graphics System: Session opened\n" +
                "  Handle: " + openData.hAdapter +
                "\n  LUID: " + openData.AdapterLuid.HighPart + ":" + openData.AdapterLuid.LowPart
            );
            var dev = new D3DKMT_CREATEDEVICE();
            dev.hAdapter = openData.hAdapter;
            dev.Flags = 0;
            status = GPUNativeAccess.D3DKMTCreateDevice(ref dev);
            if ((NtStatus)status != NtStatus.STATUS_SUCCESS)
            {
                Debug_.Fatal("CreateDevice failed: 0x" + status.ToString("X"));
                return;
            }
            RenderingInfo = dev;
            Debug_.Info("Graphics System: Device created: " + dev.hDevice);
        }

        /// <summary>
        /// Allocate memory (VRAM) in GPU to videowriting
        /// </summary>
        public unsafe static void AllocateGPUWriteSegment(uint size)
        {
            if (RenderingInfo==null) {
                Debug_.Error("Graphics System: Rendering device to set! Call: DrawingSystemAccessor.OpenSession() before this method to set Std Render Device.");
                return;
            }

            var allocData = new D3DDDI_ALLOCATIONINFO();
            allocData.pSystemMem = IntPtr.Zero;
            IntPtr pAllocInfo = Marshal.AllocHGlobal(Marshal.SizeOf(allocData));
            Marshal.StructureToPtr(allocData, pAllocInfo, false);

            var alloc     = new D3DKMT_CREATEALLOCATION();
            alloc.hDevice        = RenderingInfo.Value.hDevice;
            alloc.NumAllocations = 1;
            alloc.pAllocationInfo= pAllocInfo;
            alloc.Flags = 0;
            alloc.hResource = IntPtr.Zero;
            alloc.hGlobalShare = IntPtr.Zero;
            alloc.pPrivateRuntimeData = IntPtr.Zero;
            alloc.PrivateRuntimeDataSize = 0;
            alloc.pStandardAllocation = IntPtr.Zero;
            alloc.pPrivateDriverData = IntPtr.Zero;
            alloc.PrivateDriverDataSize = 0;
            alloc.hPrivateRuntimeResourceHandle = 0;

            int status = GPUNativeAccess.D3DKMTCreateAllocation(ref alloc);
            if ((NtStatus)status == NtStatus.STATUS_SUCCESS) {
                allocData = (D3DDDI_ALLOCATIONINFO)Marshal.PtrToStructure(pAllocInfo, typeof(D3DDDI_ALLOCATIONINFO));
                Debug_.Info("Graphics System: Memory allocated. ForDev info:\n  Handle: "+allocData.hAllocation);
            } else Debug_.Fatal("Graphics System: Mem allocation error. NTSTATUS (after 0x convert to HEX): 0x"+status);
        
            Marshal.FreeHGlobal(pAllocInfo);
        }

        public static LUID GetAdapterLuidFromHdc(IntPtr hdc)
        {
            var open = new D3DKMT_OPENADAPTERFROMHDC();
            open.hDc = hdc;

            if (GPUNativeAccess.D3DKMTOpenAdapterFromHdc(ref open) != 0)
                return new LUID { HighPart=0, LowPart=0 };

            return open.AdapterLuid;
        }

        /// <summary>
        /// Get max VRAM size
        /// </summary>
        public unsafe static ulong GetTotalVRAM(LUID adapterLuid)
        {
            var queryInfo = new D3DKMT_QUERYADAPTERINFO();
            queryInfo.hAdapter = (IntPtr)DrawingSystemAccessor.Reserved0.hAdapter;
            queryInfo.Type = KMTQUERYADAPTERINFOTYPE.KMTQAITYPE_GETSEGMENTSIZE;
            ulong* segmentData = stackalloc ulong[2];
            segmentData[0] = 0;
            segmentData[1] = 0;
            queryInfo.pPrivateDriverData = segmentData;
            queryInfo.PrivateDriverDataSize = sizeof(ulong) * 2;
            int status = GPUNativeAccess.D3DKMTQueryAdapterInfo(ref queryInfo);
            if ((NtStatus)status != NtStatus.STATUS_SUCCESS)
            {
                Debug_.Error("GetTotalVRAM: QueryAdapterInfo failed: 0x" + status.ToString("X"));
                return 0ul;
            }
            ulong dedicated = segmentData[0];
            ulong shared = segmentData[1];
            Debug_.Log(
                "GPU Memory Info:\n" +
                "  Dedicated: " + (dedicated / 1024 / 1024) + " MB\n" +
                "  Shared: " + (shared / 1024 / 1024) + " MB"
            );
            if (dedicated > 0) return dedicated;
            return shared;
        }
    }

    public struct GMUID
    {
        public uint LowPart;
        public int HighPart;

        private static HashSet<ulong> usedIds = new HashSet<ulong>();
        private static Random rnd = new Random();

        public static GMUID GetRandom()
        {
            ulong id;
            do
            {
                id = ((ulong)rnd.Next() << 32) | (uint)rnd.Next();
            } while (!usedIds.Add(id));

            return new GMUID
            {
                LowPart = (uint)(id & 0xFFFFFFFF),
                HighPart = (int)(id >> 32)
            };
        }

        public string ToStr() {
            return HighPart+":"+LowPart;
        }
    }

    public abstract class GPUDrawingDummy
    {
        public abstract void OnModuleEnable(ulong maxVRAMAlloc, GMUID moduleId, string timeStamp);
        public abstract void OnModuleDisable(GMUID moduleId, string timeStamp);
        public abstract void OnWin32Error(NtStatus code, string timeStamp);
        public abstract void OnObjectDrawing(Entity object_, RenderFrame renderBuffer);
        public abstract void OnViewportResize(RenderFrame newBuffer);
        public abstract void OnModuleUpdate(double dT);
        public abstract void OnPreDraw(RenderFrame buffer);
        public abstract void OnPostDraw(RenderFrame buffer);
        public abstract void OnGPUFree(IntPtr handle, GMUID moduleId);
        public abstract void OnInputWnd(string text, Vec3 postion, Vec3 rotation, Vec3 scale, Camera cam);
        public abstract bool IntermoduleDataTransfer(GMUID moduleId, object[] data);
        public abstract void WindowInfo(bool invisible, IntPtr id, ushort atom, IntPtr lpfnWndProc, uint width, uint height, bool onCloseProc, IntPtr inputMsg);
        public abstract GMUID GetModuleUID();
    }

    public class GPUExpress : GPUDrawingDummy
    {
        private GMUID moduleID;
        private ulong maxAllocMem;
        private ulong[,] vramInternal;
        private bool moduleReadyForUse = false;

        public override unsafe void OnModuleEnable(ulong maxVRAMAlloc, GMUID moduleId, string timeStamp)
        {
            this.moduleID = moduleId;
            Debug_.Log("GPU: Internal GPU Module \"GPUExpress\" activated.");
            var queryInfo = new D3DKMT_QUERYADAPTERINFO();
            queryInfo.hAdapter = (IntPtr)DrawingSystemAccessor.Reserved0.hAdapter;
            queryInfo.Type = KMTQUERYADAPTERINFOTYPE.KMTQAITYPE_GETSEGMENTSIZE;
            ulong* segmentData = stackalloc ulong[2]; 
            queryInfo.pPrivateDriverData = (void*)segmentData;
            queryInfo.PrivateDriverDataSize = sizeof(ulong) * 2;
            Debug_.Log("GPUExpress Debug: Internal handler value \""+queryInfo.hAdapter+"\"");
            int status = GPUNativeAccess.D3DKMTQueryAdapterInfo(ref queryInfo);
            if (status == 0) {
                ulong dedicated = segmentData[0];
                ulong shared = segmentData[1];
                Debug_.Info("GPUExpress Internal ApproxD GPU driver: Adapter Segments Queried for "+moduleId.ToStr());
                Debug_.Log("Driver Diagnostics: Received from dxgkrnl.sys limits on GPUExpress module: Dedicated VRAM: "+dedicated/1024/1024+" MB, Shared Memory: "+shared/1024/1024+" MB");
                ulong effectiveLimit = (dedicated > 0) ? dedicated : shared;
                ulong finalBudget = Math.Min(maxVRAMAlloc, (ulong)(effectiveLimit * 0.8));
                Debug_.Log("Driver Diagnostics: Set for GPUExpress maximal VRAM range as: "+finalBudget/1024/1024+" MB");
                maxAllocMem = finalBudget;
                vramInternal = new ulong[1, 3] { { dedicated, shared, (dedicated>0)?1UL:0UL } };
                moduleReadyForUse = true;
            }
        }

        public override void OnModuleDisable(GMUID moduleId, string timeStamp) {}
        public override void OnWin32Error(NtStatus code, string timeStamp) {}
        public override void OnObjectDrawing(Entity object_, RenderFrame renderBuffer) {}
        public override void OnViewportResize(RenderFrame newBuffer) {}
        public override void OnModuleUpdate(double dT) {}
        public override void OnPreDraw(RenderFrame buffer) {}
        public override void OnPostDraw(RenderFrame buffer) {}
        public override void OnGPUFree(IntPtr handle, GMUID moduleId) {}
        public override void OnInputWnd(string text, Vec3 position, Vec3 roatation, Vec3 scale, Camera cam) {}
        public override bool IntermoduleDataTransfer(GMUID moduleId, object[] data) {return true;}
        public override void WindowInfo(bool invisible, IntPtr id, ushort atom, IntPtr lpfnWndProc, uint width, uint height, bool onCloseProc, IntPtr inputMsg) {}
        public override GMUID GetModuleUID() {return moduleID;}
    }

    /// <summary>
    /// Super-fast 0-alloc hash generator
    /// </summary>
    public struct FNV_1a
    {
        /// <summary>
        /// Generate hash from string
        /// </summary>
        public static uint GetHashCode(string str) {
            if (str == null) return 0;
            uint hash = 2166136261;
            for (int i = 0; i < str.Length; i++) {
                hash = (hash ^ str[i]) * 16777619;
            }
            return hash;
        }
    }

    public static unsafe class TextureManager {
        public struct TextureAddress {
            public uint AssetHash;
            public long Offset;
            public int Width;
            public int Height;
            public int MipCount;
            public unsafe fixed long MipOffsets[10];
        }

        private static FastMemoryMappedFile _tsf;
        private static TextureAddress[] _indexTable;
        private static int _textureCount;

        public static void Init(string ddfPath, string tsfPath) {
            if (File.Exists(ddfPath)) {
                byte[] ddfData = File.ReadAllBytes(ddfPath);
                int structSize = sizeof(TextureAddress);
                _textureCount = ddfData.Length / structSize;
                _indexTable = new TextureAddress[_textureCount];

                fixed (byte* pData = ddfData) {
                    TextureAddress* pAddr = (TextureAddress*)pData;
                    for (int i = 0; i < _textureCount; i++) _indexTable[i] = pAddr[i];
                }
            }

            long fileSize = new FileInfo(tsfPath).Length;
            _tsf = new FastMemoryMappedFile(tsfPath, fileSize);
            _tsf.Prefetch();
        }

        /// <summary>
        /// Find texture ID in engine RAM-hash (call in model load, not in render)
        /// </summary>
        public static int GetTextureId(string name) {
            uint searchHash = FNV_1a.GetHashCode(name);
            for (int i = 0; i < _textureCount; i++) {
                if (_indexTable[i].AssetHash == searchHash) return i;
            }
            return -1;
        }

        /// <summary>
        /// Pixel selection by Mip-map
        /// </summary>
        public static uint GetPixelMip(int textureId, float u, float v, int mipLevel) {
            if (textureId < 0 || textureId >= _textureCount) return 0xFFFF00FF;

            fixed (TextureAddress* addr = &_indexTable[textureId]) {
                int level = (mipLevel < 0) ? 0 : (mipLevel >= addr->MipCount ? addr->MipCount - 1 : mipLevel);
                int mipW = Math.Max(1, addr->Width >> level);
                int mipH = addr->Height >> level;
                if (mipW < 1) mipW = 1; if (mipH < 1) mipH = 1;
                int tx = (int)(u * (mipW - 1)) % mipW;
                int ty = (int)(v * (mipH - 1)) % mipH;
                if (tx < 0) tx += mipW; if (ty < 0) ty += mipH;
                uint* pTexData = (uint*)(_tsf.Pointer + addr->MipOffsets[level]);
                return pTexData[ty * mipW + tx];
            }
        }



        private static void WriteBitmapToStream(Bitmap bmp, Stream stream) {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            byte* ptr = (byte*)data.Scan0;
            int size = data.Width * data.Height * 4;
            byte[] buffer = new byte[size];
            Marshal.Copy(data.Scan0, buffer, 0, size);
            stream.Write(buffer, 0, size);
            bmp.UnlockBits(data);
        }

        private static void WriteIndexFile(string path, List<TextureAddress> index) {
            using (FileStream fs = new FileStream(path, FileMode.Create)) {
                int structSize = sizeof(TextureAddress);
                byte[] buffer = new byte[structSize];
                foreach (var addr in index) {
                    fixed (byte* p = buffer) {
                        *(TextureAddress*)p = addr;
                    }
                    fs.Write(buffer, 0, structSize);
                }
            }
        }

        public static void CompileAssets(string sourceDir, string ddfPath, string tsfPath) {
            var files = Directory.GetFiles(sourceDir, "*.png");
            List<TextureAddress> index = new List<TextureAddress>();

            using (FileStream tsf = new FileStream(tsfPath, FileMode.Create)) {
                foreach (var file in files) {
                    Bitmap bmp = new Bitmap(file);
                    TextureAddress addr = new TextureAddress();
                    addr.AssetHash = FNV_1a.GetHashCode(Path.GetFileNameWithoutExtension(file));
                    addr.Width = bmp.Width;
                    addr.Height = bmp.Height;
                    addr.Offset = tsf.Position;
                    int mips = (int)Math.Log(Math.Max(bmp.Width, bmp.Height), 2) + 1;
                    addr.MipCount = Math.Min(mips, 10);
                    Bitmap currentBmp = bmp;
                    for (int i = 0; i < addr.MipCount; i++) {
                        addr.MipOffsets[i] = tsf.Position;
                        WriteBitmapToStream(currentBmp, tsf);
                        if (i < addr.MipCount - 1) {
                            currentBmp = new Bitmap(currentBmp, currentBmp.Width / 2, currentBmp.Height / 2);
                        }
                    }
                    index.Add(addr);
                }
            }
            WriteIndexFile(ddfPath, index);
        }
    }
}