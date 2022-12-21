using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Diagnostics;

namespace AvaloniaFormsConverter.App
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void Navigate(object? sender, RoutedEventArgs e)
        {
            if (sender is Control control && control.Tag is string link)
            {
                ProcessStartInfo info = null;
                if (link.Contains("$SELF$")
                    && System.Diagnostics.Process.GetCurrentProcess().ProcessName is string self
                    && System.Reflection.Assembly.GetExecutingAssembly().Location is string loc
                    && System.IO.Path.GetDirectoryName(loc) is string folder)
                {
                    info = new ProcessStartInfo()
                    {
                        UseShellExecute = true,
                        FileName = System.IO.Path.Combine(folder, self + (System.Environment.OSVersion.Platform == System.PlatformID.Win32NT ? ".exe" : "")),
                        Arguments = link.Replace("$SELF$ ", ""),
                    };
                }
                else
                {
                    info = new ProcessStartInfo()
                    {
                        UseShellExecute = true,
                        FileName = link
                    };
                }
                System.Diagnostics.Process.Start(info);
            }
        }
    }
}