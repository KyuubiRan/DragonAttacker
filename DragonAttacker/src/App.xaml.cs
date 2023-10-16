using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using DragonAttacker.Utils;

namespace DragonAttacker;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    public delegate void ApplicationEvent();

    // public static event ApplicationEvent? OnApplicationStartup;
    public static event ApplicationEvent? OnApplicationExit;

    private static void CheckSingleInstance()
    {
        Current.Dispatcher.Invoke(() =>
        {
            var processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            if (processes.Length > 1)
                Current.Shutdown();
        });
    }
    
    protected override void OnStartup(StartupEventArgs e)
    {
        CheckSingleInstance();

        base.OnStartup(e);
        // OnApplicationStartup?.Invoke();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        OnApplicationExit?.Invoke();
    }
}