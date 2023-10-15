using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace DragonAttacker.Utils;

public static class DragonMouseController
{
    #region MouseEvent

    [StructLayout(LayoutKind.Sequential)]
    struct POINT
    {
        public Int32 X;
        public Int32 Y;
    }

    [Flags]
    public enum MouseEventFlags : uint
    {
        LEFTDOWN = 0x00000002,
        LEFTUP = 0x00000004,
        MIDDLEDOWN = 0x00000020,
        MIDDLEUP = 0x00000040,
        MOVE = 0x00000001,
        ABSOLUTE = 0x00008000,
        RIGHTDOWN = 0x00000008,
        RIGHTUP = 0x00000010,
        WHEEL = 0x00000800,
        XDOWN = 0x00000080,
        XUP = 0x00000100
    }


    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetCursorPos(out POINT point);

    [DllImport("user32.dll")]
    static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData,
                                   UIntPtr dwExtraInfo);

    #endregion

    #region SendInput

    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardInput
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MouseInput
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HardwareInput
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InputUnion
    {
        [FieldOffset(0)] public MouseInput mi;
        [FieldOffset(0)] public KeyboardInput ki;
        [FieldOffset(0)] public HardwareInput hi;
    }

    public struct Input
    {
        public int type;
        public InputUnion u;
    }

    [Flags]
    public enum InputType
    {
        Mouse = 0,
        Keyboard = 1,
        Hardware = 2
    }

    [DllImport("user32.dll")]
    static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] Input[] pInputs, int cbSize);

    [Flags]
    public enum MouseEventF
    {
        Absolute = 0x8000,
        HWheel = 0x01000,
        Move = 0x0001,
        MoveNoCoalesce = 0x2000,
        LeftDown = 0x0002,
        LeftUp = 0x0004,
        RightDown = 0x0008,
        RightUp = 0x0010,
        MiddleDown = 0x0020,
        MiddleUp = 0x0040,
        VirtualDesk = 0x4000,
        Wheel = 0x0800,
        XDown = 0x0080,
        XUp = 0x0100
    }

    #endregion


    private static readonly Thread Thread = new(Worker)
    {
        IsBackground = true,
    };

    private static bool _isWorking;

    private static readonly BlockingCollection<DragonMouseActionState> BlockingQueue = new();

    public struct DragonMouseActionState
    {
        public bool AutoClick;
        public int MovePixelsPerTime;
        public int MouseDownTime;
        public int RotationTime;

        public override string ToString()
        {
            return "AutoClick: " + AutoClick + ", MovePixelsPerTime: " + MovePixelsPerTime +
                   ", MouseDownTime: " + MouseDownTime + ", RotationTime: " + RotationTime;
        }
    }

    private static readonly TaskFactory Factory = new();
    private static CancellationTokenSource _source = new();
    private static CancellationToken _cancellationToken = _source.Token;

    private static Timer _rotationTimer = new()
    {
        Interval = 1,
        AutoReset = true,
    };

    private static void Cancel()
    {
        Console.WriteLine("Canceled action");
        _source.Cancel();
        _source = new CancellationTokenSource();
        _cancellationToken = _source.Token;

        lock (_rotationTimer)
        {
            _rotationTimer.Stop();
            _rotationTimer.Close();
            _isWorking = false;
        }
    }

    private static Input AllocNewMouseInput()
    {
        return new Input
        {
            type = (int)InputType.Mouse,
            u = new InputUnion
            {
                mi = new MouseInput()
            }
        };
    }

    [DoesNotReturn]
    private static void Worker()
    {
        while (true)
        {
            var actionState = BlockingQueue.Take();
            Factory.StartNew(async () =>
            {
                _isWorking = true;

                POINT p;

                if (actionState.AutoClick)
                {
                    GetCursorPos(out p);
                    mouse_event((uint)MouseEventFlags.LEFTDOWN, p.X, p.Y, 0, 0);
                    await Task.Delay(actionState.MouseDownTime, _cancellationToken);
                    GetCursorPos(out p);
                    mouse_event((uint)MouseEventFlags.LEFTUP, p.X, p.Y, 0, 0);
                }

                lock (_rotationTimer)
                {
                    _rotationTimer = new Timer
                    {
                        Interval = 1,
                        AutoReset = true
                    };
                }

                _rotationTimer.Elapsed += (_, _) =>
                {
                    var input = new[]
                    {
                        AllocNewMouseInput()
                    };
                    input[0].u.mi.dx = actionState.MovePixelsPerTime;
                    input[0].u.mi.dwFlags = (uint)MouseEventF.Move;
                    
                    SendInput(1, input, Marshal.SizeOf(input[0]));
                };

                _rotationTimer.Start();
                await Task.Delay(actionState.RotationTime, _cancellationToken);

                lock (_rotationTimer)
                {
                    _rotationTimer.Stop();
                    _rotationTimer.Close();
                }

                _isWorking = false;
                Console.WriteLine("Finished action");
            }, _cancellationToken).Wait();
        }
    }

    static DragonMouseController()
    {
        Thread.Start();
    }

    public static void PushAction(DragonMouseActionState state)
    {
        if (_isWorking)
        {
            Cancel();
            return;
        }

        BlockingQueue.Add(state);
    }
}