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
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using RedstoneShell.ApproxD;
using Microsoft.Win32.SafeHandles;

namespace RedstoneShell.ApproxD.Collectors {
    public class Object2ParamField<A, B> {
        public A v1;
        public B v2;

        public void SetValue(A v1, B v2) {
            if (!(v1 is A)||!(v2 is B)) throw new Exception("Object2ParamField: One of params type not equals to type of arg");
            this.v1 = v1;
            this.v2 = v2;
        }
    }

    public class Material
    {
        public double Ambient = 0.15;
        public double Diffuse = 1.0;
        public double Specular = 0.5;
        public double Shininess = 32;
        public double Alpha = 1.0;
    }

    public class Entity {
        public Vec3 Position = new Vec3(0, 0, 0);
        public Vec3 Rotation = new Vec3(0, 0, 0);
        public Vec3 Scale = new Vec3(1, 1, 1);
        public EntityLayer Layer = EntityLayer.Default;
        public Vec3[] Mesh;
        public Material Material = new Material();
        public bool IsPendingKill = false;
        public uint Color = 0xFFFFFFFF;

        public Entity(Vec3[] mesh, uint color) {
            Mesh = mesh;
            Color = color;
        }
    }

    public enum EntityLayer
    {
        Default,   
        Static,    
        Projectile,
        UI,        
        Skybox     
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vec3
    {
        public double x, y, z;

        public Vec3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vec3 Normalize()
        {
            double len = Math.Sqrt(x * x + y * y + z * z);
            return len == 0 ? new Vec3(0, 0, 0) : new Vec3(x / len, y / len, z / len);
        }

        public double Length() { return Math.Sqrt(x * x + y * y + z * z); }

        public static double Dot(Vec3 a, Vec3 b) { return a.x * b.x + a.y * b.y + a.z * b.z; }
        public static Vec3 Cross(Vec3 a, Vec3 b) { return new Vec3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x); }
        public static Vec3 operator -(Vec3 a, Vec3 b) { return new Vec3(a.x - b.x, a.y - b.y, a.z - b.z); }
        public static Vec3 operator +(Vec3 a, Vec3 b) { return new Vec3(a.x + b.x, a.y + b.y, a.z + b.z); }
        public static Vec3 operator *(Vec3 a, double b) { return new Vec3(a.x * b, a.y * b, a.z * b); }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vec4
    {
        public double x, y, z, w;

        public Vec4(double x, double y, double z, double w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
    }

    public struct Mat4
    {
        public double M00, M01, M02, M03;
        public double M10, M11, M12, M13;
        public double M20, M21, M22, M23;
        public double M30, M31, M32, M33;

        public static Mat4 Identity()
        {
            Mat4 mat = new Mat4();
            mat.M00 = 1; mat.M11 = 1; mat.M22 = 1; mat.M33 = 1;
            return mat;
        }

        public static void Multiply(Mat4 a, Mat4 b, out Mat4 res)
        {
            res = new Mat4();
            res.M00 = a.M00 * b.M00 + a.M01 * b.M10 + a.M02 * b.M20 + a.M03 * b.M30;
            res.M01 = a.M00 * b.M01 + a.M01 * b.M11 + a.M02 * b.M21 + a.M03 * b.M31;
            res.M02 = a.M00 * b.M02 + a.M01 * b.M12 + a.M02 * b.M22 + a.M03 * b.M32;
            res.M03 = a.M00 * b.M03 + a.M01 * b.M13 + a.M02 * b.M23 + a.M03 * b.M33;

            res.M10 = a.M10 * b.M00 + a.M11 * b.M10 + a.M12 * b.M20 + a.M13 * b.M30;
            res.M11 = a.M10 * b.M01 + a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31;
            res.M12 = a.M10 * b.M02 + a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32;
            res.M13 = a.M10 * b.M03 + a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33;

            res.M20 = a.M20 * b.M00 + a.M21 * b.M10 + a.M22 * b.M20 + a.M23 * b.M30;
            res.M21 = a.M20 * b.M01 + a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31;
            res.M22 = a.M20 * b.M02 + a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32;
            res.M23 = a.M20 * b.M03 + a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33;

            res.M30 = a.M30 * b.M00 + a.M31 * b.M10 + a.M32 * b.M20 + a.M33 * b.M30;
            res.M31 = a.M30 * b.M01 + a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31;
            res.M32 = a.M30 * b.M02 + a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32;
            res.M33 = a.M30 * b.M03 + a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33;
        }

        public static Mat4 View(Vec3 pos, Vec3 rot)
        {
            double p = -rot.x;
            double y = -rot.y;

            double cp = Math.Cos(p), sp = Math.Sin(p);
            double cy = Math.Cos(y), sy = Math.Sin(y);

            Mat4 res = Identity();
            res.M00 = cy;
            res.M02 = -sy;
            res.M10 = sy * sp;
            res.M11 = cp;
            res.M12 = cy * sp;
            res.M20 = sy * cp;
            res.M21 = -sp;
            res.M22 = cy * cp;

            res.M30 = -(pos.x * res.M00 + pos.y * res.M10 + pos.z * res.M20);
            res.M31 = -(pos.x * res.M01 + pos.y * res.M11 + pos.z * res.M21);
            res.M32 = -(pos.x * res.M02 + pos.y * res.M12 + pos.z * res.M22);

            return res;
        }

        public static Vec3 Multiply(Mat4 mat, Vec3 v)
        {
            double x = v.x * mat.M00 + v.y * mat.M10 + v.z * mat.M20 + mat.M30;
            double y = v.x * mat.M01 + v.y * mat.M11 + v.z * mat.M21 + mat.M31;
            double z = v.x * mat.M02 + v.y * mat.M12 + v.z * mat.M22 + mat.M32;
            double w = v.x * mat.M03 + v.y * mat.M13 + v.z * mat.M23 + mat.M33;

            if (w != 0 && w != 1.0) { x /= w; y /= w; z /= w; }
            return new Vec3(x, y, z);
        }

        public static Mat4 Projection(float fov, float aspect, float near, float far)
        {
            float fovRad = 1.0f / (float)Math.Tan(fov * 0.5f / 180.0f * Math.PI);
            Mat4 mat = new Mat4();
            mat.M00 = fovRad / aspect;
            mat.M11 = fovRad;
            mat.M22 = far / (far - near);
            mat.M32 = (-far * near) / (far - near);
            mat.M23 = 1.0;
            mat.M33 = 0.0;
            return mat;
        }
    }
    /// <summary>
    /// High-speed C# 3.5 analog of System MemoryMappedFile, but use raw pointers and P/Invoke to fast write/read
    /// </summary>
    public unsafe class FastMemoryMappedFile : IDisposable
    {
        private const uint OPEN_ALWAYS = 4;
        private const uint PAGE_EXECUTE_READWRITE = 0x40;
        private const uint FILE_MAP_EXECUTE = 0x0020;
        private const uint FILE_MAP_ALL_ACCESS = 0x001f | FILE_MAP_EXECUTE;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr CreateFileMapping(IntPtr hFile, IntPtr lpAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, IntPtr dwNumberOfBytesToMap);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);

        private IntPtr _hFile = IntPtr.Zero;
        private IntPtr _hMap = IntPtr.Zero;
        private IntPtr _baseAddress = IntPtr.Zero;
        private long _size;

        // Pub API for Dev's and Engine  - RedstoneShell 13:47 15.02.2026
        public byte* Pointer { get { return (byte*)_baseAddress; } }

        /// <summary>
        /// Make a new file with name 'fileName' and size in VRAM as 'size'
        /// </summary>
        public FastMemoryMappedFile(string fileName, long size) {
            _size = size;
            const uint GENERIC_READ = 0x80000000;
            const uint GENERIC_WRITE = 0x40000000;
            const uint GENERIC_EXECUTE = 0x20000000;
            const uint FILE_SHARE_READ = 1;
            _hFile = CreateFile(fileName, 
                GENERIC_READ | GENERIC_WRITE | GENERIC_EXECUTE, 
                FILE_SHARE_READ, 
                IntPtr.Zero, 
                OPEN_ALWAYS, 
                0, 
                IntPtr.Zero);
            if (_hFile == new IntPtr(-1)) throw new Exception("Failed to open file. Error: " + Marshal.GetLastWin32Error());
            uint high = (uint)(size >> 32);
            uint low = (uint)(size & 0xFFFFFFFF);
            _hMap = CreateFileMapping(_hFile, IntPtr.Zero, PAGE_EXECUTE_READWRITE, high, low, null);
            if (_hMap == IntPtr.Zero) throw new Exception("Failed to create mapping. Error: " + Marshal.GetLastWin32Error());
            _baseAddress = MapViewOfFile(_hMap, FILE_MAP_ALL_ACCESS, 0, 0, new IntPtr(size));
            if (_baseAddress == IntPtr.Zero) {
                int error = Marshal.GetLastWin32Error();
                throw new Exception("Failed to map view of file. Win32 Error: " + error);
            }
        }

        public void Prefetch() {
            byte dummy = 0;
            for (long i = 0; i < _size; i += 4096) dummy ^= *(Pointer + i);
            Debug_.Log("Allocated \""+dummy+"\" in RAM by ApproxD for \""+this+"\"");
        }

        public void Dispose() {
            if (_baseAddress != IntPtr.Zero) UnmapViewOfFile(_baseAddress);
            if (_hMap != IntPtr.Zero) CloseHandle(_hMap);
            if (_hFile != IntPtr.Zero) CloseHandle(_hFile);
        }
    }
}