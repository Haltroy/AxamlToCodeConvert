/*
 * Copyright (C) 2022 haltroy
 *
 * Use of this source code is governed by MIT License that can be found in
 * https://github.com/haltroy/AxamlToCodeConvert/blob/main/LICENSE
 *
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace AxamlToCodeConvert
{
    public static class Converter
    {
        public static string Convert(string xml)
        {
            if (!AxamlDictionary.IsInitialized)
            {
                AxamlDictionary.Init();
            }
            try
            {
                if (string.IsNullOrWhiteSpace(xml)) { return xml; }
                XmlDocument doc = new();
                doc.LoadXml(xml);
                if (doc.DocumentElement is XmlNode windowNode)
                {
                    string name_space = "MyProgram";
                    string windowName = "MyWindow";

                    if (windowNode.Attributes != null && windowNode.Attributes["x:Class"] is XmlAttribute nsattr)
                    {
                        string ns = nsattr.InnerXml;
                        name_space = ns.Substring(0, ns.LastIndexOf('.'));
                        windowName = ns.Substring(name_space.Length + 1);
                    }

                    string csharpcode = "// Auto-Generated by AxamlToCodeConverter"
                        + Environment.NewLine
                        + "// https://github.com/haltroy/AxamlToCodeConvert"
                        + Environment.NewLine
                        + "// NOTE: This stuff is not perfect, you might have to manually edit it."
                        + Environment.NewLine
                        + "// Also, remove the constructor code from the other CS file. It looks like this:"
                        + Environment.NewLine
                        + $"// public {windowName}()"
                        + Environment.NewLine
                        + $"namespace {name_space}"
                        + Environment.NewLine
                        + "{"
                        + Environment.NewLine
                        + $"    public partial class {windowName}"
                        + Environment.NewLine
                        + "    {"
                        + Environment.NewLine
                        + "        #region Init"
                        + Environment.NewLine
                        + $"        public {windowName}()"
                        + Environment.NewLine
                        + "        {"
                        + Environment.NewLine
                        + "            Init();"
                        + Environment.NewLine
                        + "            #if DEBUG"
                        + Environment.NewLine
                        + "            this.AttachDevTools();"
                        + Environment.NewLine
                        + "            #endif"
                        + Environment.NewLine
                        + "        }"
                        + Environment.NewLine
                        + "        private void Init()"
                        + Environment.NewLine
                        + "        {"
                        + Environment.NewLine;

                    List<(string name, string type)> definedNames = new();

                    csharpcode += NodeAttrToCode(windowNode, "this", false);
                    csharpcode += NodeChildsToCode(windowNode, "this", definedNames);

                    csharpcode += "        }" + Environment.NewLine + Environment.NewLine;

                    for (int i = 0; i < definedNames.Count; i++)
                    {
                        var name = definedNames[i];
                        csharpcode += "        " + name.type + "? " + name.name + ";" + Environment.NewLine;
                    }

                    csharpcode += Environment.NewLine + "#endregion Init" + Environment.NewLine + "    }" + Environment.NewLine + "}";

                    return csharpcode;
                }
                else
                {
                    return "// Cannot convert, somehow the XML root element is not a XML node. ";
                }
            }
            catch (Exception ex)
            {
                return "/* Cannot convert, an error occurred." + Environment.NewLine + ex.ToString() + Environment.NewLine + "*/";
            }
        }

        private static string NodeChildsToCode(XmlNode node, string nodeName, List<(string name, string type)> definedNames)
        {
            if (node.OuterXml.StartsWith("<!--")) { return ""; }
            string childs = "";
            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                var child = node.ChildNodes[i];
                if (child != null)
                {
                    childs += NodeAttrToCode(child, definedNames, out string childName);
                    childs += "            " + nodeName + ".Children.Add(" + childName + ");" + Environment.NewLine + Environment.NewLine;

                    childs += NodeChildsToCode(child, childName, definedNames);
                }
            }
            return childs;
        }

        private static string GenerateNodeName(XmlNode node, List<(string name, string type)> definedNames)
        {
            if (node.Attributes != null && node.Attributes["Name"] is XmlAttribute attr)
            {
                definedNames.Add((attr.InnerXml, node.Name));
                return attr.Value;
            }
            else
            {
                for (int i = 0; i < int.MaxValue; i++)
                {
                    string newName = FirstCharToLowerCaseInvariant(node.Name + i);
                    if (definedNames.FindAll(it => it.name == newName).Count <= 0)
                    {
                        definedNames.Add((newName, node.Name));
                        return newName;
                    }
                }
                return GenerateRandomText();
            }
        }

        private static string FirstCharToLowerCaseInvariant(this string str)
        {
            if (!string.IsNullOrEmpty(str) && char.IsUpper(str[0]))
                return str.Length == 1 ? char.ToLowerInvariant(str[0]).ToString() : char.ToLowerInvariant(str[0]) + str[1..];

            return str;
        }

        private static string GenerateRandomText(int length = 17)
        {
            if (length is 0 or >= int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            if (length < 0) { length *= -1; }
            StringBuilder builder = new();
            Enumerable
               .Range(65, 26)
                .Select(e => ((char)e).ToString())
                .Concat(Enumerable.Range(97, 26).Select(e => ((char)e).ToString()))
                .Concat(Enumerable.Range(0, length - 1).Select(e => e.ToString()))
                .OrderBy(e => Guid.NewGuid())
                .Take(length)
                .ToList().ForEach(e => builder.Append(e));
            return builder.ToString();
        }

        private static string NodeAttrToCode(XmlNode node, List<(string name, string type)> definedNames, out string nodeName, bool generateNewStatement = true)
        {
            if (node.OuterXml.StartsWith("<!--")) { nodeName = ""; return ""; }
            nodeName = GenerateNodeName(node, definedNames);
            string attributes = "";
            if (generateNewStatement)
            {
                attributes += $"            {nodeName} = new {node.Name.Replace(":", ".")}();{Environment.NewLine}";
            }
            attributes += NodeAttrToCodeBase(node, nodeName);
            return attributes;
        }

        private static string NodeAttrToCode(XmlNode node, string nodeName, bool generateNewStatement = true)
        {
            if (node.OuterXml.StartsWith("<!--")) { return ""; }
            string attributes = "";
            if (generateNewStatement)
            {
                attributes += $"            {nodeName} = new {node.Name.Replace(":", ".")}();{Environment.NewLine}";
            }
            attributes += NodeAttrToCodeBase(node, nodeName);
            return attributes;
        }

        private static string NodeAttrToCodeBase(XmlNode node, string nodeName)
        {
            if (node.Attributes is null) { throw new ArgumentNullException(nameof(node)); }
            string attributes = "";
            for (int i = 0; i < node.Attributes.Count; i++)
            {
                XmlNode attr = node.Attributes[i];
                attributes += AxamlDictionary.Prepare(attr.Name, attr.InnerXml, nodeName) + Environment.NewLine;
            }
            return attributes;
        }
    }

    public static class AxamlDictionary
    {
        public static string DictionariesLocation => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "htttools", "axamlconvert");
        public static bool IsInitialized { get; set; } = false;

        public static void Init(bool clean = false)
        {
            if (clean)
            {
                Items.Clear();
            }
            if (!System.IO.Directory.Exists(DictionariesLocation))
            {
                System.IO.Directory.CreateDirectory(DictionariesLocation);
            }
            string defaultDictionaryLoc = Path.Combine(DictionariesLocation, "default.xml");
#if DEBUG

            // Copy over the default dictionary if we are debugging.
            using (var fstr = System.IO.File.Create(defaultDictionaryLoc))
            using (var writer = new StreamWriter(fstr))
            {
                writer.WriteLine(LicenseWindow.ReadResource("AxamlToCodeConvert.AvaloniaDictionary.xml"));
            }
#endif
            // Otherwise, copy if it does not exists.
            if (!System.IO.File.Exists(defaultDictionaryLoc))
            {
                using (var fstr = System.IO.File.Create(defaultDictionaryLoc))
                using (var writer = new StreamWriter(fstr))
                {
                    writer.WriteLine(LicenseWindow.ReadResource("AxamlToCodeConvert.AvaloniaDictionary.xml"));
                }
            }

            var dictionaries = System.IO.Directory.GetFiles(DictionariesLocation, "*.xml", System.IO.SearchOption.TopDirectoryOnly);
            for (int i = 0; i < dictionaries.Length; i++)
            {
                XmlDocument doc = new();
                using (var reader = new System.IO.StreamReader(dictionaries[i]))
                {
                    doc.LoadXml(reader.ReadToEnd());
                }
                if (doc.DocumentElement != null)
                {
                    LoadXml(doc.DocumentElement);
                }
            }
            IsInitialized = true;
        }

        private static void LoadXml(XmlNode node)
        {
            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                var types = node.ChildNodes[i];
                if (types != null && !types.OuterXml.StartsWith("<!--"))
                {
                    AxamlDictionaryItem item = new(types.Name);
                    for (int _i = 0; _i < types.ChildNodes.Count; _i++)
                    {
                        var action = types.ChildNodes[_i];
                        if (action != null)
                        {
                            switch (action.Name.ToLowerInvariant())
                            {
                                case "stringify":
                                    item.Actions.Add(new AxamlDictionaryActions.Stringify());
                                    break;

                                case "newify":
                                    item.Actions.Add(new AxamlDictionaryActions.Newify());
                                    break;

                                case "beforeline":
                                    item.Actions.Add(new AxamlDictionaryActions.BeforeLine(action.Attributes != null && action.Attributes["Input"] is XmlAttribute inputattr ? inputattr.Value : action.InnerXml));
                                    break;

                                case "afterline":
                                    item.Actions.Add(new AxamlDictionaryActions.AfterLine(action.Attributes != null && action.Attributes["Input"] is XmlAttribute inputattr2 ? inputattr2.Value : action.InnerXml));
                                    break;

                                case "replace":
                                    item.Actions.Add(new AxamlDictionaryActions.Replace(action.Attributes != null && action.Attributes["OldValue"] is XmlAttribute oldattr ? oldattr.Value : "",
                                        action.Attributes != null && action.Attributes["NewValue"] is XmlAttribute newattr ? newattr.Value : "",
                                        action.Attributes != null && action.Attributes["Regex"] is XmlAttribute reattr && (reattr.Value is null || reattr.Value == "true" || reattr.Value == "1" || reattr.Value == "yes" || reattr.Value == "y")));
                                    break;

                                case "delete":
                                    item.Actions.Add(new AxamlDictionaryActions.Delete());
                                    break;

                                case "insert":
                                    item.Actions.Add(new AxamlDictionaryActions.Insert(action.Attributes != null && action.Attributes["Input"] is XmlAttribute attr ? attr.Value : action.InnerXml,
                                        action.Attributes != null && action.Attributes["FullLine"] is XmlAttribute flattr && (flattr.Value is null || flattr.Value == "true" || flattr.Value == "1" || flattr.Value == "yes" || flattr.Value == "y")));
                                    break;
                            }
                        }
                    }
                    Items.Add(item);
                }
            }
        }

        public static List<AxamlDictionaryItem> Items { get; set; } = new List<AxamlDictionaryItem>();

        public static string Prepare(string attributeName, string attributeValue, string nodeName)
        {
            string attribute = attributeValue;
            var list = Items.FindAll(it => it.Name.ToLowerInvariant() == attributeName.ToLowerInvariant());
            if (list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    attribute = list[i].Prepare(attribute, nodeName);
                }
                return attribute;
            }
            else
            {
                return new AxamlDictionaryActions.Default().Prepare(attributeName, attributeValue, nodeName, true).Replace("$EQUALS$", $"            {nodeName}.{attributeName} = ");
            }
        }
    }

    public class AxamlDictionaryItem
    {
        public AxamlDictionaryItem(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Name { get; set; }
        public List<IAxamlDictionaryAction> Actions { get; set; } = new();

        public string Prepare(string attributeValue, string nodeName)
        {
            string attribute = attributeValue;
            for (int i = 0; i < Actions.Count; i++)
            {
                attribute = Actions[i].Prepare(Name, attribute, nodeName, i == Actions.Count - 1);
            }
            return attribute.Replace("$EQUALS$", $"            {nodeName}.{Name} = ");
        }
    }

    public interface IAxamlDictionaryAction
    {
        public string Prepare(string attributeName, string attributeValue, string nodeName, bool isLast);
    }

    public static class AxamlDictionaryActions
    {
        /// <summary>
        /// Does nothing.
        /// </summary>
        public class Default : IAxamlDictionaryAction
        {
            public string Prepare(string attributeName, string attributeValue, string nodeName, bool isLast)
            {
                return (isLast ? "$EQUALS$" : "") + attributeValue.Replace("" + Environment.NewLine, "\" + Environment.Newline +" + Environment.NewLine + "\"") + (isLast ? ";" : "");
            }
        }

        /// <summary>
        /// value -> "value"
        /// </summary>
        public class Stringify : IAxamlDictionaryAction
        {
            public string Prepare(string attributeName, string attributeValue, string nodeName, bool isLast)
            {
                return (isLast ? "$EQUALS$" : "") + "\"" + attributeValue + "\"" + (isLast ? ";" : "");
            }
        }

        /// <summary>
        /// value -> new Attribute(value);
        /// </summary>
        public class Newify : IAxamlDictionaryAction
        {
            public string Prepare(string attributeName, string attributeValue, string nodeName, bool isLast)
            {
                return (isLast ? "$EQUALS$" : "") + "new " + attributeName + "(" + attributeValue + ")" + (isLast ? ";" : "");
            }
        }

        /// <summary>
        /// Adds a line before the attribute line.
        /// </summary>

        public class BeforeLine : IAxamlDictionaryAction
        {
            public BeforeLine(string input)
            {
                Input = input ?? throw new ArgumentNullException(nameof(input));
            }

            public string Input { get; set; }

            public string Prepare(string attributeName, string attributeValue, string nodeName, bool isLast)
            {
                return Input + Environment.NewLine + "$EQUALS$" + (isLast ? ";" : "");
            }
        }

        /// <summary>
        /// Adds a line after the attribute line.
        /// </summary>
        public class AfterLine : IAxamlDictionaryAction
        {
            public AfterLine(string input)
            {
                Input = input ?? throw new ArgumentNullException(nameof(input));
            }

            public string Input { get; set; }

            public string Prepare(string attributeName, string attributeValue, string nodeName, bool isLast)
            {
                return "$EQUALS$" + (isLast ? ";" : "") + Environment.NewLine + Input;
            }
        }

        /// <summary>
        /// Replaces <see cref="OldValue"/> with <seealso cref="NewValue"/> with option for <seealso cref="EnableRegex"/>.
        /// </summary>
        public class Replace : IAxamlDictionaryAction
        {
            public Replace(string oldValue, string newValue, bool enableRegex = false)
            {
                OldValue = oldValue ?? throw new ArgumentNullException(nameof(oldValue));
                NewValue = newValue ?? throw new ArgumentNullException(nameof(newValue));
                EnableRegex = enableRegex;
            }

            public bool EnableRegex { get; set; } = false;
            public string OldValue { get; set; }
            public string NewValue { get; set; }

            public string Prepare(string attributeName, string attributeValue, string nodeName, bool isLast)
            {
                return (isLast ? "$EQUALS$" : "") + (EnableRegex ? Regex.Replace(attributeValue, OldValue, NewValue) : attributeValue.Replace(OldValue, NewValue)) + (isLast ? ";" : "");
            }
        }

        /// <summary>
        /// Pseudo class, deletes the line.
        /// </summary>
        public class Delete : IAxamlDictionaryAction
        {
            public string Prepare(string attributeName, string attributeValue, string nodeName, bool isLast)
            {
                return "";
            }
        }

        /// <summary>
        /// Inserts value to $value$, type to $type$ and control name to $control$ with option to apply as a line with <see cref="FullLine"/>.
        /// </summary>
        public class Insert : IAxamlDictionaryAction
        {
            public Insert(string input, bool fullLine = false)
            {
                Input = input ?? throw new ArgumentNullException(nameof(input));
                FullLine = fullLine;
            }

            public string Input { get; set; }
            public bool FullLine { get; set; } = false;

            public string Prepare(string attributeName, string attributeValue, string nodeName, bool isLast)
            {
                return (isLast ? "            " : "") + (!FullLine ? "$EQUALS$" : "") + Input.Replace("$value$", attributeValue, StringComparison.InvariantCultureIgnoreCase).Replace("$type$", attributeName, StringComparison.InvariantCultureIgnoreCase).Replace("$control$", nodeName, StringComparison.InvariantCultureIgnoreCase) + (isLast ? ";" : "");
            }
        }
    }
}