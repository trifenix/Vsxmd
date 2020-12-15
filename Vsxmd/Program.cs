//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Junle Li">
//     Copyright (c) Junle Li. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Vsxmd
{
    
    using System;
    

    using System.Runtime.Serialization.Json;    
  using System.Collections.Generic;
  using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Xml.Linq;
    using Vsxmd.Units;
  using System.Runtime.Serialization;




  /// <summary>
  /// Program entry.
  /// </summary>
  /// <remarks>
  /// Usage syntax:
  /// <code>Vsxmd.exe &lt;input-XML-path&gt; [output-Markdown-path]</code>
  /// <para>The <c>input-XML-path</c> argument is required. It references to the VS generated XML documentation file.</para>
  /// <para>The <c>output-Markdown-path</c> argument is optional. It indicates the file path for the Markdown output file. When not specific, it will be a <c>.md</c> file with same file name as the XML documentation file, path at the XML documentation folder.</para>
  /// </remarks>
  internal static class Program
    {
        internal static string Serialize<T>(T instance) where T : class
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, instance);
                return Encoding.Default.GetString(stream.ToArray());
            }
        }

        public static string MainPagePackage(string header, string description, string summary, string package_icon, string nugeturl, string githuburl, string devopsurl, string badge = "") {
            var sb = new StringBuilder();
            sb.Append(GetHeader($"package {header}", description));
            sb.AppendLine($"# ![icon_package]({package_icon}){header}");
            sb.AppendLine();
            sb.AppendLine($" {description}");
            sb.AppendLine();
            sb.AppendLine("## Descripción");
            sb.AppendLine();
            sb.AppendLine($"### {header}");
            sb.AppendLine();
            sb.AppendLine($"{ConvertToSummary(summary)}");
            sb.AppendLine();
            sb.AppendLine("| Tipo | Fuente |");
            sb.AppendLine("|---|---|");
            sb.AppendLine($"|![nuget](https://logos.trifenix.io/nuget.24x24.png) | [Paquete en Nuget.org]({nugeturl})|");
            sb.AppendLine($"|![nuget](https://logos.trifenix.io/nuget.24x24.png) | [Paquete en devops]({devopsurl})|");
            sb.AppendLine($"|![github](https://logos.trifenix.io/github.24x24.png) | [Código fuente]({githuburl})|");
            if (!string.IsNullOrWhiteSpace(badge))
            {
                sb.AppendLine();
                sb.AppendLine($"![release badge]({badge})");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static string ConvertToSummary(string summary) => summary.Split("\n").Select(s => s.Trim()).Join("\n");

        public static string MainNamespace(string name, string ns, string summary) {
            var sb = new StringBuilder();
            sb.Append(GetHeader($"{name}", ns));
            sb.AppendLine("# Descripción del namespace");
            sb.AppendLine();
            sb.AppendLine(ConvertToSummary(summary));
            return sb.ToString();
        }

        public static string GetHeader(string header, string description)
        {
            var sb = new StringBuilder();
            sb.AppendLine("---");
            sb.AppendLine($"title : {header}");
            sb.AppendLine($"description: {description}");
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine();
            return sb.ToString();
        }

        /// <summary>
        /// Program main function entry.
        /// </summary>
        /// <param name="args">Program arguments.</param>
        /// <seealso cref="Program"/>
        internal static void Main(string[] args)
        {
            try
            {
                if (args == null || args.Length < 2)
                {
                    return;
                }
                string xmlPath = args[0];
                var folderDestination = args[1];
                string folderDestionation = args.ElementAtOrDefault(1);
                var dict = new Dictionary<string, string>();

                string markdownPath = args.ElementAtOrDefault(1);

                if (string.IsNullOrWhiteSpace(markdownPath))
                {
                    // replace extension with `md` extension
                    markdownPath = Path.ChangeExtension(xmlPath, ".md");
                }
                var document = XDocument.Load(xmlPath);

                // paso 1, sacar información desde la misma ruta que el xmlpath, del csproj.
                var baseXmlPath = Path.GetDirectoryName(xmlPath);

                // obtengo archivo del proyecto.
                var proj = Path.Combine(baseXmlPath, $"{Path.GetFileNameWithoutExtension(xmlPath)}.csproj");

                // cargo el proyecto
                var projDocument = XDocument.Load(proj);
                var rootProject = projDocument.Root;
                var propertyGroups = rootProject.Element("PropertyGroup");
                var package_title = propertyGroups.Element("title").Value;
                var idPackage = propertyGroups.Element("id").Value;
                var package_description = propertyGroups.Element("description").Value;
                var package_summary = propertyGroups.Element("summary").Value;
                var main_icon = propertyGroups.Element("iconUrl").Value;
                var nuget_url = propertyGroups.Element("nugetUrl").Value;
                var devops_url = propertyGroups.Element("devopsUrl").Value;
                var github_url = propertyGroups.Element("RepositoryUrl").Value;
                var relasebadge_url = propertyGroups.Element("releaseBadgeUrl").Value;
                var mainMarkdown = Path.Combine(baseXmlPath, $"{folderDestionation}/{idPackage}/index.md");
                var baseMarkdown = Path.GetDirectoryName(mainMarkdown);

                Directory.CreateDirectory(baseMarkdown);

                File.WriteAllText(mainMarkdown, MainPagePackage(package_title, package_description, package_summary, main_icon, nuget_url, github_url, devops_url, relasebadge_url));

                dict.Add("Descripción", $"/{folderDestionation}/{idPackage}/");

                var members = document.Root.Element("members");
                var elementMembers = members.Elements("member");
                var types = elementMembers.Where(s => s.Attributes().Any(a => a.Value.Contains("T:")));

                var namespaces = types.Where(s => s.Attribute("name").Value.ToLower().Contains("namespace"));


                foreach (var m in namespaces)
                {

                    // las member names comienzan con T:elnombre de la clase
                    var splt = m.Attribute("name").Value.Split(":")[1].ToLower().Replace(".namespace","").Split(".");
                    var nsms = splt.Join(".");
                    var summary = m.Element("summary").Value;
                    var title = nsms.Split(".").Last();
                    var md = Path.Combine(baseMarkdown, $"{nsms}/index.md");
                    var fld = Path.GetDirectoryName(md);
                    Directory.CreateDirectory(fld);
                    File.WriteAllText(md, MainNamespace(title, nsms, summary));
                    dict.Add(nsms, $"/{folderDestionation}/{idPackage}/{nsms}/");

                }
                var typesWithoutNamespace = types.Where(s => !s.Attribute("name").Value.ToLower().Contains("namespace"));

                var converter = new Converter(document);

                var markdown = converter.ToMarkdown();

                foreach (var item in markdown)
                {
                    var folder = Path.Combine(baseMarkdown, $"{item.Key}/");
                    Directory.CreateDirectory(folder);
                    var file = Path.Combine(folder, "index.md");
                    File.WriteAllText(file, item.Value);
                    dict.Add(item.Key, $"/{folderDestionation}/{idPackage}/{item.Key}/");
                }

                // generate json

                var ordered = dict.OrderBy(s => s.Value);

                var jsn = new Jsn
                {
                    title = package_title,
                    path = $"/{folderDestionation}/{idPackage}/",
                    pages = dict.Select(d => new Jsn
                    {
                        title = d.Key.Split(".").Last().Replace("_T","<T>"),
                        path = d.Value,
                    }).ToList(),

                };

                
                


                File.WriteAllText(Path.Combine(baseMarkdown,"menu.json"), Serialize(jsn));

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                // Ignore errors. Do not impact on project build
                return;
            }
        }

        [DataContract]
        public class Jsn{

            [DataMember]
            public string title { get; set; }

            [DataMember]
            public string path { get; set; }

            [DataMember]
            public List<Jsn> pages { get; set; }
            
            

        }
        private class Test
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Test"/> class.
            /// <para>Test constructor without parameters.</para>
            /// <para>See <see cref="Test()"/>.</para>
            /// </summary>
            /// <permission cref="Program">Just for test.</permission>
            internal Test()
            {
            }
#pragma warning disable SA1614 // Element parameter documentation must have text
            /// <summary>
            /// Test a param tag without description.
            /// </summary>
            /// <param name="p"></param>
            /// <returns>Nothing.</returns>
            internal string TestParamWithoutDescription(string p) => null;
#pragma warning restore SA1614 // Element parameter documentation must have text
            /// <summary>
            /// Test generic reference type.
            /// <para>See <see cref="TestGenericParameter{T1, T2}(Expression{Func{T1, T2, string}})"/>.</para>
            /// </summary>
            /// <returns>Nothing.</returns>
            internal string TestGenericReference() => null;
            /// <summary>
            /// Test generic parameter type.
            /// <para>See <typeparamref name="T1"/> and <typeparamref name="T2"/>.</para>
            /// </summary>
            /// <typeparam name="T1">Generic type 1.</typeparam>
            /// <typeparam name="T2">Generic type 2.</typeparam>
            /// <param name="expression">The linq expression.</param>
            /// <returns>Nothing.</returns>
            internal string TestGenericParameter<T1, T2>(
                Expression<Func<T1, T2, string>> expression) =>
                null;
            /// <summary>
            /// Test generic exception type.
            /// </summary>
            /// <returns>Nothing.</returns>
            /// <exception cref="TestGenericParameter{T1, T2}(Expression{Func{T1, T2, string}})">Just for test.</exception>
            internal string TestGenericException() => null;
            /// <summary>
            /// Test generic exception type.
            /// </summary>
            /// <returns>Nothing.</returns>
            /// <permission cref="TestGenericParameter{T1, T2}(Expression{Func{T1, T2, string}})">Just for test.</permission>
            internal string TestGenericPermission() => null;
            /// <summary>
            /// Test backtick characters in summary comment.
            /// <para>See `should not inside code block`.</para>
            /// <para>See <c>`backtick inside code block`</c>.</para>
            /// <para>See `<c>code block inside backtick</c>`.</para>
            /// </summary>
            /// <returns>Nothing.</returns>
            internal string TestBacktickInSummary() => null;
            /// <summary>
            /// Test space after inline elements.
            /// <para>See <c>code block</c> should follow a space.</para>
            /// <para>See a value at the end of a <value>sentence</value>.</para>
            /// <para>See <see cref="TestSpaceAfterInlineElements"/> as a link.</para>
            /// <para>See <paramref name="space" /> after a param ref.</para>
            /// <para>See <typeparamref name="T" /> after a type param ref.</para>
            /// </summary>
            /// <returns>Nothing.</returns>
            internal bool TestSpaceAfterInlineElements<T>(bool space) => space;
            /// <summary>
            /// Test see tag with langword attribute. See <see langword="true"/>.
            /// </summary>
            /// <returns>Nothing.</returns>
            internal string TestSeeLangword() => null;
        }
        /// <summary>
        /// Test generic type.
        /// <para>See <see cref="TestGenericType{T1, T2}"/>.</para>
        /// </summary>
        /// <typeparam name="T1">Generic type 1.</typeparam>
        /// <typeparam name="T2">Generic type 2.</typeparam>
        private class TestGenericType<T1, T2>
        {
            /// <summary>
            /// Test generic method.
            /// <para>See <see cref="TestGenericMethod{T3, T4}"/>.</para>
            /// </summary>
            /// <typeparam name="T3">Generic type 3.</typeparam>
            /// <typeparam name="T4">Generic type 4.</typeparam>
            /// <returns>Nothing.</returns>
            internal string TestGenericMethod<T3, T4>() => null;
        }
    }
}
