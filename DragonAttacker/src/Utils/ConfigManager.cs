using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;

namespace DragonAttacker.Utils;

public static class ConfigManager
{
    private static readonly FileInfo Config = new("./config.txt");

    private static readonly Dictionary<string, string> ConfigMap = new();
    
    static ConfigManager()
    {
        Load();
        App.OnApplicationExit += Save;
    }
    
    private static void Load()
    {
        try
        {
            Console.WriteLine("Begin load config");
            using var ifs = new FileStream(Config.FullName, FileMode.OpenOrCreate);
            if (!ifs.CanRead)
            {
                MessageBox.Show("无法读取配置文件！", "警告", MessageBoxButton.OK);
                return;
            }

            using var reader = new StreamReader(ifs);
            while (reader.ReadLine() is { } s)
            {
                var ret = s.Split(" = ");
                if (ret.Length < 2) continue; // invalid
                var k = ret[0];
                var v = ret[1];
                for (var i = 2; i < ret.Length; ++i) v += ret[i];
                ConfigMap[k] = v;
                Console.WriteLine($"Load config: {k} = {v}");
            }

            Console.WriteLine("Finished load config, size=" + ConfigMap.Count);
        }
        catch (FieldAccessException)
        {
            MessageBox.Show("无法读取配置文件！", "警告", MessageBoxButton.OK);
        }
    }

    private static void Save()
    {
        try
        {
            Console.WriteLine("Begin save config, size=" + ConfigMap.Count);
            using var ofs = new FileStream(Config.FullName, FileMode.OpenOrCreate);
            using var writer = new StreamWriter(ofs);
            foreach (var (k, v) in ConfigMap)
            {
                var s = k + " = " + v;
                writer.WriteLine(s);
            }

            Console.WriteLine("Finished save config");
        }
        catch (FieldAccessException)
        {
            MessageBox.Show("无法写入配置文件！", "警告", MessageBoxButton.OK);
        }
    }

    public static bool? GetBoolOrNull(string k)
    {
        ConfigMap.TryGetValue(k, out var value);
        return bool.TryParse(value, out var b) ? b : null;
    }

    public static bool GetBool(string k, bool defValue = false) => GetBoolOrNull(k) ?? defValue;

    public static int? GetIntOrNull(string k)
    {
        ConfigMap.TryGetValue(k, out var value);
        return int.TryParse(value, out var i) ? i : null;
    }

    public static void PutBool(string k, bool v)
    {
        ConfigMap[k] = v.ToString();
    }

    public static int GetInt(string k, int defValue = 0) => GetIntOrNull(k) ?? defValue;

    public static string? GetStringOrNull(string k)
    {
        ConfigMap.TryGetValue(k, out var value);
        return value;
    }

    public static void PutInt(string k, int v)
    {
        ConfigMap[k] = v.ToString();
    }

    public static string GetString(string k, string defValue = "") => GetStringOrNull(k) ?? defValue;

    public static float? GetFloatOrNull(string k)
    {
        ConfigMap.TryGetValue(k, out var value);
        return float.TryParse(value, out var f) ? f : null;
    }

    public static void PutString(string k, string v)
    {
        ConfigMap[k] = v;
    }

    public static float GetFloat(string k, float defValue = 0.0f) => GetFloatOrNull(k) ?? defValue;

    public static void PutFloat(string k, float v)
    {
        ConfigMap[k] = v.ToString(CultureInfo.InvariantCulture);
    }
}