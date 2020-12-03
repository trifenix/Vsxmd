//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Junle Li">
//     Copyright (c) Junle Li. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Vsxmd
{
    using System;
  using System.Collections.Generic;
  using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Xml.Linq;
    using Vsxmd.Units;
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
        /// <summary>
        /// Program main function entry.
        /// </summary>
        /// <param name="args">Program arguments.</param>
        /// <seealso cref="Program"/>
        internal static void Main(string[] args)
        {
            try
            {
                if (args == null || args.Length < 1)
                {
                    return;
                }
                string xmlPath = args[0];
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
                // ---
                // title: package mdm
                // description: Atributo que determina que un campo es autonumérico
                // ---
                var package_title = propertyGroups.Element("title").Value;
                var package_description = propertyGroups.Element("description").Value;
                var package_summary = propertyGroups.Element("summary").Value;
                var main_icon = propertyGroups.Element("iconUrl").Value;
                var nuget_url = propertyGroups.Element("nugetUrl").Value;
                var devops_url = propertyGroups.Element("devopsUrl").Value;
                var github_url = propertyGroups.Element("RepositoryUrl").Value;
                var relasebadge_url = propertyGroups.Element("releaseBadgeUrl").Value;


                var mainMarkdown = Path.Combine(baseXmlPath,"markdown/index.md");
                var pathDirectory = Path.GetDirectoryName(mainMarkdown);
                
                Directory.CreateDirectory(pathDirectory);

                
                var toMainMarkDown = new string[]
                {
                    $"---\ntitle : package {package_title}\ndescription: {package_description}\n---",
                    "<Hero slots='image, heading, text' background='rgb(64, 34, 138)'/>",
                    "![Hero image](https://images.trifenix.io/great-job.png)",
                    $"# {package_title}",
                    $"{package_description}",
                    "<Resources slots='image, heading, links' />",
                    "#### Resumen",
                    new List<string>
                    {
                        $"![image]({main_icon})",
                        "* [Descripción](./index.md#descripción)",
                        "* [Especificación](./index.md#especificación)",
                    }
                    .Join("\n"),
                    $"## Descripción",
                    $"### {package_title}",
                    $"{package_summary}",
                    "## Especificación",
                    new List<string> {
                        "| Tipo | Fuente |",
                        "|---|---|",
                        $"| ![nuget](https://logos.trifenix.io/nuget.24x24.png) | [Paquete en Nuget.org]({nuget_url})|",
                        $"|![nuget](https://logos.trifenix.io/nuget.24x24.png) | [Paquete en devops (puede pedir autenticación)]({devops_url})|",
                        $"|![github](https://logos.trifenix.io/github.24x24.png) | [Código Fuente]({github_url})|",
                    }.Join("\n"),
                    $"![release badge]({relasebadge_url})"
                }.ToList().Join("\n\n")
                .Suffix("\n");
                Console.WriteLine(toMainMarkDown);
                Console.WriteLine(mainMarkdown);
                File.WriteAllText(mainMarkdown, toMainMarkDown);
                

                var members = document.Root.Element("members");

                var elementMembers = members.Elements("member");

                

                var types = elementMembers.Where(s => s.Attributes().Any(a => a.Value.Contains("T:")));

                var namespaces = types.Where(s=>s.Attribute("name").Value.ToLower().Contains("namespace"));
                
                foreach (var m in namespaces){
                    var splt = m.Value.Split(":")[1];
                    

                }
                

                return;

                var converter = new Converter(document);
                
                var markdown = converter.ToMarkdown();
                File.WriteAllText(markdownPath, markdown);
                string vsxmdAutoDeleteXml = args.ElementAtOrDefault(2);
                if (string.IsNullOrWhiteSpace(vsxmdAutoDeleteXml))
                {
                    return;
                }
                var shouldDelete = Convert.ToBoolean(vsxmdAutoDeleteXml, CultureInfo.InvariantCulture);
                if (shouldDelete)
                {
                    File.Delete(xmlPath);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                // Ignore errors. Do not impact on project build
                return;
            }
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
