using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Themes.Fluent;

namespace PortableAvaloniaApp;

class Program
{
    [STAThread]
    public static void Main(string[] args) => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .LogToTrace()
        .StartWithClassicDesktopLifetime(args);
}

public class App : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        base.Initialize();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }
        base.OnFrameworkInitializationCompleted();
    }
}

// --- UI AND NATIVE INTERACTION ---
public class MainWindow : Window
{
    // P/Invoke Mapping: C++ DLL functions are correctly mapped here.
    // C++ 'long long' maps perfectly to C# 'long' (64-bit integer).
    [DllImport("NativeCalc.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int AddNumbers(int a, int b);

    [DllImport("NativeCalc.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern double CalculateFactorial(int n);


    private TextBox _inputA;
    private TextBox _inputB;
    private TextBlock _resultText;

    public MainWindow()
    {
        Title = "Avalonia UI + C++ Backend";
        Width = 450;
        Height = 350;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        // UI Control Initialization
        _inputA = new TextBox { PlaceholderText = "Enter Number A", Width = 150 };
        _inputB = new TextBox { PlaceholderText = "Enter Number B", Width = 150 };
        
        var calcButton = new Button { Content = "Calculate via C++", Padding = new Thickness(10, 5) };
        calcButton.Click += OnCalculateClick;

        _resultText = new TextBlock { Text = "Result will appear here...", FontSize = 16, HorizontalAlignment = HorizontalAlignment.Center };

        // Layout Arrangement
        var stackPanel = new StackPanel
        {
            Spacing = 15,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        
        stackPanel.Children.Add(new TextBlock { Text = "C++ Interop Demo", FontSize = 20, HorizontalAlignment = HorizontalAlignment.Center });
        stackPanel.Children.Add(_inputA);
        stackPanel.Children.Add(_inputB);
        stackPanel.Children.Add(calcButton);
        stackPanel.Children.Add(_resultText);

        Content = stackPanel;
    }

    private void OnCalculateClick(object? sender, RoutedEventArgs e)
    {
        // Inputs validation and parsing
        if (int.TryParse(_inputA.Text, out int a) && int.TryParse(_inputB.Text, out int b))
        {
            try
            {
                // Pure Execution: Invoking native C++ code compiled inside the DLL
                int sum = AddNumbers(a, b);
                double factOfA = CalculateFactorial(a);

                // UI Update
                _resultText.Text = $"C++ Outputs:\nSum (A+B) = {sum}\nFactorial of A = {factOfA}";
            }
            catch (DllNotFoundException)
            {
                _resultText.Text = "Error: 'NativeCalc.dll' not found in project path!";
            }
            catch (Exception ex)
            {
                _resultText.Text = $"Error: {ex.Message}";
            }
        }
        else
        {
            _resultText.Text = "Please enter valid integers.";
        }
    }
}
