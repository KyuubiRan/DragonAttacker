using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using DragonAttacker.Structures;

namespace DragonAttacker.Utils;

public static class HotkeyManager
{
    private static readonly HashSet<Hotkey> Hotkeys = new();

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;

    private static IntPtr _hookId = IntPtr.Zero;

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    private static void Hook()
    {
        using var proc = Process.GetCurrentProcess();
        _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, LowLevelKbdProc, proc.MainModule!.BaseAddress, 0);
        Console.WriteLine("Installed kbdLowLevelHook, id=" + _hookId);
    }

    private static void Unhook()
    {
        UnhookWindowsHookEx(_hookId);
        Console.WriteLine("Uninstalled kbdLowLevelHook");
    }

    private static IntPtr LowLevelKbdProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode < 0)
            return CallNextHookEx(_hookId, nCode, wParam, lParam);

        var vkCode = Marshal.ReadInt16(lParam);
#if DEBUG
       // Console.WriteLine($"LLPROC: nCode={nCode}, wParam={wParam}, vkCode={vkCode}");
#endif

        switch (wParam)
        {
            case WM_KEYUP:
                Hotkeys.Where(htk => htk.VkCode == vkCode).ForEach(htk => htk.KeyUp(vkCode));
                break;
            case WM_KEYDOWN:
                Hotkeys.Where(htk => htk.VkCode == vkCode).ForEach(htk => htk.KeyDown(vkCode));
                break;
        }

        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    static HotkeyManager()
    {
        Hook();
        App.OnApplicationExit += Unhook;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    public static void Unregister(Hotkey hotkey)
    {
        Console.WriteLine($"Unregister hotkey: {hotkey.KeyString}({hotkey.KeyCode})");
        Hotkeys.Remove(hotkey);
    }

    public static void Register(Hotkey hotkey)
    {
        Console.WriteLine($"Register hotkey: {hotkey.KeyString}({hotkey.KeyCode})");
        Hotkeys.Add(hotkey);
    }
}