/*
 * Copyright (C) 2022 haltroy
 *
 * Use of this source code is governed by MIT License that can be found in
 * https://github.com/haltroy/AxamlToCodeConvert/blob/main/LICENSE
 *
 */

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.IO;
using System.Reflection;

namespace AxamlToCodeConvert
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

            if (Content is DockPanel dock)
            {
                dock.FindControl<TextBox>("License").Text = ReadResource("AxamlToCodeConvert.LICENSE");
            }
        }

        public string ReadResource(string name)
        {
            try
            {
                // Determine path
                var assembly = Assembly.GetExecutingAssembly();
                string resourcePath = name;
                // Format: "{Namespace}.{Folder}.{filename}.{Extension}"

                using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (System.Exception ex)
            {
                return "Error while reading the license file: " + ex.ToString();
            }
        }
    }
}