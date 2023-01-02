/*
 * Copyright (C) 2022 haltroy
 *
 * Use of this source code is governed by MIT License that can be found in
 * https://github.com/haltroy/AxamlToCodeConvert/blob/main/LICENSE
 *
 */

using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using FluentAvalonia.UI.Controls;
using System.IO;
using System.Linq;

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

        private TabView? Tabs;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            if (Content is Grid panel)
            {
                Tabs = panel.FindControl<TabView>("Tabs");
                Tabs.TabCloseRequested += CloseTab;
                Tabs.AddTabButtonClick += (sender, e) => { OpenFile(Tabs, new RoutedEventArgs()); };
            }
        }

        private void CloseTab(object? sender, TabViewTabCloseRequestedEventArgs e)
        {
            if (Tabs != null && Tabs.TabItems is AvaloniaList<object> list)
            {
                list.Remove(e.Item);
            }
        }

        private async void SaveFile(object? sender, RoutedEventArgs e)
        {
            if (Tabs != null && Tabs.SelectedItem is TabViewItem item && item.Header is string fileName)
            {
                SaveFileDialog dialog = new()
                {
                    Title = "Save to...",
                    Filters = new System.Collections.Generic.List<FileDialogFilter>() { new FileDialogFilter() { Name = "C# Designer Class", Extensions = { "Designer.cs" } } },
                    InitialFileName = fileName
                };
                var str = await dialog.ShowAsync(this);
                if (str != null && item.Content is Grid grid && grid.Children[grid.Children.Count - 1] is TextEditor editor)
                {
                    using var fstr = new FileStream(str, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
                    using var writer = new StreamWriter(fstr);
                    writer.WriteLine(editor.Text);
                }
            }
        }

        private async void SaveAll(object? sender, RoutedEventArgs e)
        {
            if (Tabs != null)
            {
                OpenFolderDialog dialog = new()
                {
                    Title = "Save all to...",
                };
                var str = await dialog.ShowAsync(this);
                if (str != null && Tabs.TabItems is AvaloniaList<object> list)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i] is TabViewItem item && item.Header is string name && item.Content is Grid grid && grid.Children[grid.Children.Count - 1] is TextEditor editor)
                        {
                            string fileName = Path.Combine(str, name + ".Designer.cs");
                            using var fstr = new FileStream(fileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
                            using var writer = new StreamWriter(fstr);
                            writer.WriteLine(editor.Text);
                        }
                    }
                }
            }
        }

        private void Quit(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void OpenFile(object? sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                Title = "Open a file...",
                AllowMultiple = true,
                Filters = new System.Collections.Generic.List<FileDialogFilter>() { new FileDialogFilter() { Name = "Avalonia XAML file", Extensions = { "axaml" } } }
            };

            var str = await dialog.ShowAsync(this);
            if (str != null && Tabs != null && Tabs.TabItems is AvaloniaList<object> list)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(str[i]) && File.Exists(str[i]))
                    {
                        string axaml = "";
                        using (var reader = new StreamReader(new FileStream(str[i], FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                        {
                            axaml = reader.ReadToEnd();
                        }
                        TabViewItem item = new()
                        {
                            Header = Path.GetFileNameWithoutExtension(str[i]),
                            Name = str[i],
                        };
                        list.Add(item);
                        Grid grid = new()
                        {
                            Name = item.Header as string,
                            ColumnDefinitions = new ColumnDefinitions("*,*"),
                            RowDefinitions = new RowDefinitions("20,*")
                        };
                        item.Content = grid;

                        var _registryOptions = new TextMateSharp.Grammars.RegistryOptions(TextMateSharp.Grammars.ThemeName.DarkPlus);

                        TextEditor input = new()
                        {
                            Text = axaml,
                            Name = "AXAML",
                            Background = new SolidColorBrush(Colors.Transparent),
                            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                            ShowLineNumbers = true,
                            FontSize = 14,
                        };
                        TextEditor output = new()
                        {
                            Text = Converter.Convert(input.Text),
                            Name = "code",
                            Background = new SolidColorBrush(Colors.Transparent),
                            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                            ShowLineNumbers = true,
                            FontSize = 14,
                            IsReadOnly = true,
                        };

                        TextBlock axamlTitle = new()
                        {
                            Text = "AXAML:",
                        };

                        TextBlock codeTitle = new()
                        {
                            Text = "Designer.cs:",
                        };

                        grid.Children.Add(axamlTitle);
                        grid.Children.Add(codeTitle);
                        grid.Children.Add(input);
                        grid.Children.Add(output);

                        Grid.SetColumn(axamlTitle, 0);
                        Grid.SetRow(axamlTitle, 0);
                        Grid.SetColumn(codeTitle, 1);
                        Grid.SetRow(codeTitle, 0);

                        Grid.SetColumn(input, 0);
                        Grid.SetRow(input, 1);

                        Grid.SetColumn(output, 1);
                        Grid.SetRow(output, 1);

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
                Tabs.SelectedIndex = list.Count - 1;
            }
        }

        private void About(object? sender, RoutedEventArgs e) => new AboutWindow().ShowDialog(this);
    }
}