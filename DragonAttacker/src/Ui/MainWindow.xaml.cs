using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using DragonAttacker.Structures;
using DragonAttacker.Utils;
using Regexes = DragonAttacker.Utils.Regexes;

namespace DragonAttacker.Ui;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private readonly Hotkey _hotkey = new();

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        RotationSpeedSlider.Value = ConfigManager.GetFloat("RotationSpeed", 2.0f);
        RotationTimeSlider.Value = ConfigManager.GetFloat("RotationTime", 3.0f);
        RotationDirectionCombo.SelectedIndex = ConfigManager.GetInt("RotationDirection");
        AutoClickCheckBox.IsChecked = ConfigManager.GetBool("AutoClick");

        var key = (Key)ConfigManager.GetInt("TriggerKey");
        SetHotkey(key);
        HotkeyManager.Register(_hotkey);
        _hotkey.OnKeyDown += _ =>
        {
            var left = RotationDirectionCombo.SelectedIndex == 1;
            Console.WriteLine($"Left:{left}");
            var infinity = Math.Abs(RotationTimeSlider.Value - RotationTimeSlider.Minimum) < 0.001;
            DragonMouseController.DragonMouseActionState state = new()
            {
                AutoClick = AutoClickCheckBox.IsChecked ?? false,
                RotationTime = infinity ? -1 : (int)(RotationTimeSlider.Value * 1000),
                MovePixelsPerTime = (int)(50 * RotationSpeedSlider.Value) * (left ? -1 : 1),
                MouseDownTime = 300
            };
            Console.WriteLine("Action state: " + state);
            DragonMouseController.PushAction(state);
        };
    }

    private void SaveChanges()
    {
        ConfigManager.PutFloat("RotationTime", (float)RotationTimeSlider.Value);
        ConfigManager.PutFloat("RotationSpeed", (float)RotationSpeedSlider.Value);
        ConfigManager.PutInt("RotationDirection", RotationDirectionCombo.SelectedIndex);
        ConfigManager.PutBool("AutoClick", AutoClickCheckBox.IsChecked ?? false);

        if (_hotkey.Key != Key.None)
            ConfigManager.PutInt("TriggerKey", (int)_hotkey.Key);
    }

    private bool _handleHotkey;

    private void RotationSpeedText_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = Regexes.FloatNumber.IsMatch(e.Text) || string.IsNullOrEmpty(e.Text);
    }

    private void RotationSpeedSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (RotationSpeedText != null)
        {
            RotationSpeedText.Text = $"{e.NewValue:0}x";
        }
    }

    private void TriggerButton_OnClick(object sender, RoutedEventArgs e)
    {
        _handleHotkey = true;
        TriggerButton.Content = "<...>";
        KeyCodeText.Text = "-";
    }

    private void TriggerButton_OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_handleHotkey) return;
        _handleHotkey = false;
        SetKeyText();
    }

    private void SetKeyText()
    {
        TriggerButton.Content = _hotkey.KeyString;
        KeyCodeText.Text = _hotkey.VkCode.ToString();
    }

    private void SetHotkey(Key key)
    {
        _hotkey.SetVKey((short)KeyInterop.VirtualKeyFromKey(key));
        SetKeyText();
    }

    private void TriggerButton_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (!_handleHotkey) return;
        SetHotkey(e.Key);
        _handleHotkey = false;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        HotkeyManager.Unregister(_hotkey);
        SaveChanges();
    }

    private void RotationTimeSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (RotationTimeText != null)
        {
            RotationTimeText.Text =
                Math.Abs(e.NewValue - RotationTimeSlider.Minimum) < 0.001 ? "无限" : $"{e.NewValue:F1}s";
        }
    }
}