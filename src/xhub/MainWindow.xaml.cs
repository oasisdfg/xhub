using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using xhub.ViewModels;

namespace xhub;

public partial class MainWindow : Window
{
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        EnableDarkTitleBar();
    }

    private void EnableDarkTitleBar()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero) return;

        int useDarkMode = 1;
        DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE,
            ref useDarkMode, Marshal.SizeOf(typeof(int)));
    }
}
