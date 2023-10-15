using System;
using System.Windows;

namespace DragonAttacker;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    public delegate void ApplicationEvent();

    public static event ApplicationEvent? OnApplicationStartup;
    public static event ApplicationEvent? OnApplicationExit;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        OnApplicationStartup?.Invoke();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        OnApplicationExit?.Invoke();
    }
}