using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Runtime.InteropServices;
using System.Text;

public class usbcamera
{
    [DllImport("uvcdotnet.dll", EntryPoint = "uvsGetAvailableDevice", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern int uvsGetAvailableDevice();
    [DllImport("uvcdotnet.dll", EntryPoint = "uvsOpenDevice", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern IntPtr uvsOpenDevice(int index);
    [DllImport("uvcdotnet.dll", EntryPoint = "uvsCloseDevice", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern int uvsCloseDevice(IntPtr h);

    [DllImport("uvcdotnet.dll", EntryPoint = "uvsGetDeviceName", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int uvsGetDeviceName(IntPtr h, StringBuilder pszName, int len);
    [DllImport("uvcdotnet.dll", EntryPoint = "uvsGetDeviceSN", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int uvsGetDeviceSN(IntPtr h, StringBuilder pszSN, int len);
    [DllImport("uvcdotnet.dll", EntryPoint = "uvsFeatureVideoProcGetRange", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern int uvsFeatureVideoProcGetRange(IntPtr hDev, int bFeatureId, ref long min, ref long max, ref long step, ref long def, ref int flags);

    [DllImport("uvcdotnet.dll", EntryPoint = "uvsFeatureVideoProcGet", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern int uvsFeatureVideoProcGet(IntPtr hDev, int bFeatureId, ref long val, ref int flags);

    [DllImport("uvcdotnet.dll", EntryPoint = "uvsFeatureVideoProcSet", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern int uvsFeatureVideoProcSet(IntPtr hDev, int bFeatureId, long val, int flags);

    [DllImport("uvcdotnet.dll", EntryPoint = "uvsCreateWindow", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern IntPtr CreateWindow(int index, int width, int height, IntPtr hParent);
    [DllImport("uvcdotnet.dll", EntryPoint = "uvsStartCapture", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern int StartCapture(IntPtr h);
    [DllImport("uvcdotnet.dll", EntryPoint = "uvsStopCapture", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern int StopCapture(IntPtr h);
    [DllImport("uvcdotnet.dll", EntryPoint = "uvsGetJpegFile", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern int GetJpegFile(IntPtr h, int width, int height, String str);
    [DllImport("uvcdotnet.dll", EntryPoint = "uvsStartRecord", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern int StartRecord(IntPtr h, String str);
    [DllImport("uvcdotnet.dll", EntryPoint = "uvsStopRecord", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern int StopRecord(IntPtr h);
    [DllImport("uvcdotnet.dll", EntryPoint = "uvsDestroyWindow", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern int DestroyWindow(IntPtr h);
}
