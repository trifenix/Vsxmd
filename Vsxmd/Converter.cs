//-----------------------------------------------------------------------
// <copyright file="Converter.cs" company="Junle Li">
//     Copyright (c) Junle Li. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Vsxmd
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using Vsxmd.Units;

    /// <inheritdoc/>
    public class Converter : IConverter
    {

        public string GetClassMd(string header, string header2, string description, string ctors, string constants, string props, string funcs)
        {
            var sb = new StringBuilder();
            sb.AppendLine("---");
            sb.AppendLine($"title : {header.Replace("<T>", "&lt;T&gt;")}");
            sb.AppendLine($"description: {header2}");
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine($"# {header.Replace("<T>", "&lt;T&gt;")}");
            sb.AppendLine();
            sb.AppendLine("<CodeBlock slots = 'heading, code' repeat = '1' languages = 'C#' />");
            sb.AppendLine();
            sb.AppendLine("#### Clase");
            sb.AppendLine($"```");
            sb.AppendLine($"{header}");
            sb.AppendLine($"```");
            sb.AppendLine();
            sb.AppendLine("## Descripción");
            sb.AppendLine($"{description}");

        
            
            sb.AppendLine("## Constructores");
            sb.AppendLine();
            if (!string.IsNullOrWhiteSpace(ctors))
            {
                sb.Append(ctors);
            }
            else
            {
                sb.AppendLine("no existen constructores");
            }
            sb.AppendLine();

            sb.AppendLine();
            sb.AppendLine("## Funciones");

            sb.AppendLine();

            if (!string.IsNullOrWhiteSpace(funcs))
            {
                sb.Append(funcs);
            }
            else
            {
                sb.AppendLine("no existen funciones");
            }
            sb.AppendLine();


            sb.AppendLine("## Propiedades");
            sb.AppendLine();
            if (!string.IsNullOrWhiteSpace(props))
            {
                sb.Append(props);
            }
            else
            {
                sb.AppendLine("no existen propidades");
            }
            sb.AppendLine();

            sb.AppendLine("## Constantes");
            if (!string.IsNullOrWhiteSpace(constants))
            {
                sb.Append(constants);
            }
            else
            {
                sb.AppendLine("no existen campos");
            }


            sb.AppendLine();
            return sb.ToString();
        }

        private readonly XDocument document;

        public readonly Dictionary<string, string> dict;

        /// <summary>
        /// Initializes a new instance of the <see cref="Converter"/> class.
        /// </summary>
        /// <param name="document">The XML document.</param>
        public Converter(XDocument document)
        {
            this.document = document;
            dict = new Dictionary<string, string>();
        }

        /// <summary>
        /// Convert VS XML document to Markdown syntax.
        /// </summary>
        /// <param name="document">The XML document.</param>
        /// <returns>The generated Markdown content.</returns>
        public static Dictionary<string, string> ToMarkdown(XDocument document) =>
            new Converter(document).ToMarkdown();



        public Dictionary<string, string> ToMarkdown() {
            var grps = ToUnits(this.document.Root);
            var localdict = new Dictionary<string, string>();
            foreach (var item in grps)
            {                
                var classItem = item.FirstOrDefault(s => s.Kind == MemberKind.Type && !s.name.TypeShortName.ToLower().Contains("namespace"));
                if (classItem == null) continue;

                var sb = new StringBuilder();
                var ctorMd = item.Where(s => s.Kind == MemberKind.Constructor).SelectMany(s => s.ToMarkdown()).Join("\n");
                var funcMd = item.Where(s => s.Kind == MemberKind.Method).SelectMany(s=>s.ToMarkdown()).Join("\n");
                var propsMd = item.Where(s => s.Kind == MemberKind.Property).SelectMany(s => s.ToMarkdown()).Join("\n");
                var constantsMd = item.Where(s => s.Kind == MemberKind.Constants).SelectMany(s => s.ToMarkdown()).Join("\n");
                localdict.Add(classItem.TypeName.Replace("`1", "_T"), GetClassMd(TakeOffComma(classItem.name.TypeShortName), classItem.TypeName, classItem.Summary.Join("\n"), ctorMd, constantsMd, propsMd, funcMd));
            }

            return localdict;
        }
        public static string TakeOffComma(string element) => element.Replace("`1", "<T>");


        private static IEnumerable<IGrouping<string, MemberUnit>> ToUnits(XElement docElement)
        {
            
            var baseunit = docElement
                .Element("members")
                .Elements("member")
                .Select(element => new MemberUnit(element))
                .Where(member => member.Kind != MemberKind.NotSupported)
                .GroupBy(unit => unit.TypeName);
            return baseunit;
        }
    }
}
