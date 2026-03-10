# ApproxD Engine 1.1.0.0
The Simple 3D engine without any Graphics Libraries, only GDI32 and WinAPI, and C# 3.5 (.NET Core), Maked by RedstoneShell, cool coding and usage.

## Why GDI32?
Reason: **Speed**, stock System.Drawing or OpenGL have a convertors, checkers, native calls... is slow operations, but using raw GDI32 + SetDIBits we skip all checking and have faster graphics
GDI32 on Win7 is simple to use, only need Bitmap and bitmap.Scan0.

## Why use this???
This is a experimental 3D engine to use only C# core and API, witout C/C++ native core.
But with some modifications this engine have a 0 GCM (See Also: Garbage Collector Memory, LSHIFT debug menu)
But you can experiment with all parts of code and send feedbacks!
Features:
  - Simple coding
  - No hardcore OpenGL or System.Drawing + WinForms
  - Full control with Rendering backbuffer

## Minimal components to run
  - CPU: Intel Celeron N2480
  - GPU: Intel Bay Trail
  - RAM: 2048 MB (2GB)
  - Videocard: Potato...
