using System;
using System.Runtime.InteropServices;

namespace Simrun.Engine.Rendering.OpenGl;

internal sealed class Win32Window : IDisposable
{
    private RenderSurface _surface;
    private IntPtr _hInstance;
    private IntPtr _hwnd;
    private IntPtr _hdc;
    private IntPtr _hglrc;
    private readonly WndProcDelegate _wndProcDelegate = WndProc;
    private const int WsOverlappedWindow = 0x00CF0000;
    private const uint PmRemove = 0x0001;

    public Win32Window(RenderSurface surface)
    {
        _surface = surface;
        CreateWindowAndContext();
    }

    public void MakeCurrent() => Wgl.wglMakeCurrent(_hdc, _hglrc);

    public void Swap() => SwapBuffers(_hdc);

    public void Resize(RenderSurface surface)
    {
        _surface = surface;
        MoveWindow(_hwnd, 100, 100, surface.Width, surface.Height, true);
    }

    public void PumpMessages()
    {
        while (PeekMessage(out var msg, IntPtr.Zero, 0, 0, PmRemove))
        {
            TranslateMessage(ref msg);
            DispatchMessage(ref msg);
        }
    }

    private void CreateWindowAndContext()
    {
        _hInstance = GetModuleHandle(IntPtr.Zero);

        var wndClass = new WNDCLASS
        {
            style = 0,
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
            hInstance = _hInstance,
            lpszClassName = "SimrunGLWindow"
        };

        RegisterClass(ref wndClass);

        _hwnd = CreateWindowEx(
            0,
            wndClass.lpszClassName,
            _surface.Title,
            WsOverlappedWindow,
            100, 100,
            _surface.Width,
            _surface.Height,
            IntPtr.Zero,
            IntPtr.Zero,
            _hInstance,
            IntPtr.Zero);

        _hdc = GetDC(_hwnd);

        var pfd = new PIXELFORMATDESCRIPTOR
        {
            nSize = (ushort)Marshal.SizeOf<PIXELFORMATDESCRIPTOR>(),
            nVersion = 1,
            dwFlags = 0x00000004 | 0x00000020 | 0x00000001, // PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER
            iPixelType = 0, // PFD_TYPE_RGBA
            cColorBits = 32,
            cDepthBits = 24,
            cStencilBits = 8,
            iLayerType = 0 // PFD_MAIN_PLANE
        };

        var pixelFormat = ChoosePixelFormat(_hdc, ref pfd);
        SetPixelFormat(_hdc, pixelFormat, ref pfd);

        _hglrc = Wgl.wglCreateContext(_hdc);
        Wgl.wglMakeCurrent(_hdc, _hglrc);

        ShowWindow(_hwnd, 1);
        UpdateWindow(_hwnd);
    }

    public void Dispose()
    {
        Wgl.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
        if (_hglrc != IntPtr.Zero)
        {
            Wgl.wglDeleteContext(_hglrc);
            _hglrc = IntPtr.Zero;
        }

        if (_hwnd != IntPtr.Zero)
        {
            if (_hdc != IntPtr.Zero)
            {
                ReleaseDC(_hwnd, _hdc);
                _hdc = IntPtr.Zero;
            }
            DestroyWindow(_hwnd);
            _hwnd = IntPtr.Zero;
        }
    }

    private static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == 0x0010) // WM_CLOSE
        {
            PostQuitMessage(0);
        }

        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    #region Win32 Interop
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WNDCLASS
    {
        public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszMenuName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszClassName;
    }

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct PIXELFORMATDESCRIPTOR
    {
        public ushort nSize;
        public ushort nVersion;
        public uint dwFlags;
        public byte iPixelType;
        public byte cColorBits;
        public byte cRedBits;
        public byte cRedShift;
        public byte cGreenBits;
        public byte cGreenShift;
        public byte cBlueBits;
        public byte cBlueShift;
        public byte cAlphaBits;
        public byte cAlphaShift;
        public byte cAccumBits;
        public byte cAccumRedBits;
        public byte cAccumGreenBits;
        public byte cAccumBlueBits;
        public byte cAccumAlphaBits;
        public byte cDepthBits;
        public byte cStencilBits;
        public byte cAuxBuffers;
        public sbyte iLayerType;
        public byte bReserved;
        public uint dwLayerMask;
        public uint dwVisibleMask;
        public uint dwDamageMask;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public POINT pt;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern ushort RegisterClass([In] ref WNDCLASS lpWndClass);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateWindowEx(
        int dwExStyle,
        [MarshalAs(UnmanagedType.LPWStr)] string lpClassName,
        [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName,
        int dwStyle,
        int x,
        int y,
        int nWidth,
        int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool UpdateWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("gdi32.dll")]
    private static extern int ChoosePixelFormat(IntPtr hdc, [In] ref PIXELFORMATDESCRIPTOR ppfd);

    [DllImport("gdi32.dll")]
    private static extern bool SetPixelFormat(IntPtr hdc, int iPixelFormat, [In] ref PIXELFORMATDESCRIPTOR ppfd);

    [DllImport("gdi32.dll")]
    private static extern bool SwapBuffers(IntPtr hdc);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr GetModuleHandle(IntPtr lpModuleName);

    [DllImport("user32.dll")]
    private static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

    [DllImport("user32.dll")]
    private static extern bool TranslateMessage([In] ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

    [DllImport("user32.dll")]
    private static extern void PostQuitMessage(int nExitCode);
    #endregion
}
