using System;
using System.Collections.Generic;

namespace DragonAttacker.Utils;

public static class Extensions
{
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (var item in enumerable)
        {
            action(item);
        }
    }
    
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int> action)
    {
        var i = 0;
        foreach (var item in enumerable)
        {
            action(item, i++);
        }
    }
}