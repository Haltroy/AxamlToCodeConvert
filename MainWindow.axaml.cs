/*
 * Copyright (C) 2022 haltroy
 *
 * Use of this source code is governed by MIT License that can be found in
 * https://github.com/haltroy/AxamlToCodeConvert/blob/main/LICENSE
 *
 */

using Avalonia;
using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using System.IO;

namespace AxamlToCodeConvert
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

        private AvaloniaEdit.TextEditor? input;
        private AvaloniaEdit.TextEditor? output;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            if (Content is Grid grid)
            {
                input = grid.FindControl<AvaloniaEdit.TextEditor>("AXAML");
                output = grid.FindControl<AvaloniaEdit.TextEditor>("Code");

                //Here we initialize RegistryOptions with the theme we want to use.
                var _registryOptions = new TextMateSharp.Grammars.RegistryOptions(TextMateSharp.Grammars.ThemeName.DarkPlus);

                //Initial setup of TextMate.
                var _textMateInstallation = input.InstallTextMate(_registryOptions);
                var _textMateInstallation2 = output.InstallTextMate(_registryOptions);

                _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(_registryOptions.GetLanguageByExtension(".xml").Id));
                _textMateInstallation2.SetGrammar(_registryOptions.GetScopeByLanguageId(_registryOptions.GetLanguageByExtension(".cs").Id));

                input.PropertyChanged += (sender, e) =>
                {
                    if (e.Property == TextBlock.TextProperty)
                    {
                        output.Text = Converter.Convert(input.Text);
                    }
                };
            }
        }

        private async void OpenFile(object? sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Open a file...",
                AllowMultiple = false,
                Filters = new System.Collections.Generic.List<FileDialogFilter>() { new FileDialogFilter() { Name = "Avalonia XAML file", Extensions = { "axaml" } } }
            };

            var str = await dialog.ShowAsync(this);
            if (input != null && str != null && !string.IsNullOrWhiteSpace(str[0]) && File.Exists(str[0]))
            {
                using (var reader = new StreamReader(new FileStream(str[0], FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    input.Text = reader.ReadToEnd();
                }
                output.Text = Converter.Convert(input.Text);
            }
        }

        private void About(object? sender, RoutedEventArgs e) => new AboutWindow().ShowDialog(this);
    }
}