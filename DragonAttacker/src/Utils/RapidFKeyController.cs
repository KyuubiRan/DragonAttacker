using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace DragonAttacker.Utils;

public static class RapidFKeyController
{
    [DllImport("user32.dll")]
    static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

    private static readonly Thread WorkerThread = new(Worker)
    {
        IsBackground = true
    };

    public static readonly BlockingCollection<byte> BlockingQueue = new();
    public static bool IsEnabled { get; set; } = false;

    [DoesNotReturn]
    private static void Worker()
    {
        while (true)
        {
            _ = BlockingQueue.Take();
            if (!IsEnabled) continue;

            keybd_event(0x46, 0, 0, 0);
            Thread.SpinWait((int)Random.Shared.NextInt64(50, 100));
            keybd_event(0x46, 0, 2, 0);
            Thread.SpinWait(100);
        }
    }

    static RapidFKeyController()
    {
        WorkerThread.Start();
    }
}