using Avalonia;
using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace AvaloniaFormsConverter.App
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private TextBox? input;
        private TextBox? output;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            if (Content is Grid grid)
            {
                input = grid.FindControl<TextBox>("AXAML");
                output = grid.FindControl<TextBox>("Code");

                input.PropertyChanged += (sender, e) =>
                {
                    if (e.Property == TextBlock.TextProperty)
                    {
                        output.Text = AvaloniaFormsConvert.Base.AvaloniaFormsConverter.Convert(input.Text);
                    }
                };
            }
        }

        private void OpenFile(object? sender, RoutedEventArgs e)
        {
        }

        private void About(object? sender, RoutedEventArgs e) => new AboutWindow().ShowDialog(this);
    }
}