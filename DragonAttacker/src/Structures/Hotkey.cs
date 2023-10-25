using System;
using System.Windows.Input;

namespace DragonAttacker.Structures;

public class Hotkey
{
    private readonly long _hash = Random.Shared.NextInt64();
    public int KeyCode => (int)Key;
    public int VkCode => KeyInterop.VirtualKeyFromKey(Key);

    private bool IsDown = false;

    public string KeyString => Key.ToString();

    public Key Key { get; set; } = Key.None;

    public void SetVKey(int vKey)
    {
        Key = KeyInterop.KeyFromVirtualKey(vKey);
    }

    public void SetKey(int key)
    {
        Key = (Key)key;
    }

    public delegate void KeyEvent(Hotkey hotkey);

    public event KeyEvent? OnKeyDown;
    public event KeyEvent? OnKeyUp;

    public Hotkey(Key key)
    {
        Key = key;
    }

    public Hotkey(int key, bool vKey = false)
    {
        if (vKey)
            SetVKey(key);
        else
            SetKey(key);
    }

    public Hotkey()
    {
    }

    public void KeyDown(Key key)
    {
        if (key != Key) return;
        if (IsDown)
            return;
        
        IsDown = true;

#if DEBUG
        Console.WriteLine($"KeyDown: {KeyString}({VkCode})");
#endif
        OnKeyDown?.Invoke(this);
    }

    public void KeyDown(int vkCode)
    {
        if (vkCode != VkCode) return;
        if (IsDown)
            return;
        
        IsDown = true;
#if DEBUG
        Console.WriteLine($"KeyDown: {KeyString}({vkCode})");
#endif
        OnKeyDown?.Invoke(this);
    }


    public void KeyUp(Key key)
    {
        if (key != this.Key) return;
        if (!IsDown)
            return;
        
        IsDown = false;
#if DEBUG
        Console.WriteLine($"KeyUp: {KeyString}({VkCode})");
#endif
        OnKeyUp?.Invoke(this);
    }

    public void KeyUp(int vkCode)
    {
        if (vkCode != VkCode) return;
        if (!IsDown)
            return;
        
        IsDown = false;
#if DEBUG
        Console.WriteLine($"KeyUp: {KeyString}({vkCode})");
#endif
        OnKeyUp?.Invoke(this);
    }

    public override bool Equals(object? obj)
    {
        if (obj is Hotkey hotkey)
        {
            return hotkey.Key == Key;
        }

        return false;
    }
    
    public override int GetHashCode()
    {
        return KeyCode + (int)_hash;
    }
}