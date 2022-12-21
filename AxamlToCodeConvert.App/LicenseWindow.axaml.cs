using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaFormsConverter.App
{
    public partial class LicenseWindow : Window
    {
        public LicenseWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}