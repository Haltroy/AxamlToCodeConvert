/*
 * Copyright (C) 2022 haltroy
 *
 * Use of this source code is governed by MIT License that can be found in
 * https://github.com/haltroy/AxamlToCodeConvert/blob/main/LICENSE
 *
 */

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Diagnostics;

namespace AxamlToCodeConvert
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

        private void License(object? sender, RoutedEventArgs e)
        {
            LicenseWindow license = new LicenseWindow();
            license.Show();
            license.BringIntoView();
        }

        private void Navigate(object? sender, RoutedEventArgs e)
        {
            if (sender is Control control && control.Tag is string link)
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo()
                {
                    UseShellExecute = true,
                    FileName = link
                });
            }
        }
    }
}