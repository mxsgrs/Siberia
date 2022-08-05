using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Siberia.SourceLibrary.Finders;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Siberia.SourceLibrary.Generators
{
    [Generator]
    public class ControllerGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new DataModelFinder());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            string controller = ""; // Store controller names
            HashSet<string> namespaceAliasHashSet = new(); // Store namespaces
            var contextList = ((DataModelFinder)context.SyntaxReceiver)?.ContextList; // Get classes that inherits DbContext
            foreach (var contextItem in contextList) // Iterate over each class that inherits DbContext
            {
                string contextName = contextItem.Identifier.ValueText; // Get context name
                namespaceAliasHashSet.Add(NamespaceFinder.GetNamespace(contextItem)); // Add namespace to hashset to avoid duplicates
                foreach (var member in contextItem.Members) // Iterate over each context class member
                {
                    if (member is PropertyDeclarationSyntax property && property.Type.ToString().Contains("DbSet")) // Test if member is a DbSet
                    {
                        string typeName = property.Identifier.ValueText; // Get property name
                        string typeClass = property.Type.ToString().Replace("DbSet<", "").Replace(">", ""); // Get property type
                        controller += "   " + $@"public class " + typeName + "Controller : GenericODataController<"
                            + typeClass + $@"> {{ " + "public " + typeName + "Controller(" + contextName + " context) { Context = context; }"
                            + $@" }}" + Environment.NewLine; // Controller declaration
                    }
                }
            }

            List<string> namespaceAliasList = namespaceAliasHashSet.Select(alias => "using " + alias + ";" + Environment.NewLine).ToList(); // Create usings
            string namespaceAliases = string.Join("", namespaceAliasList); // Join usings

            string source = $@"// Auto-generated code" + Environment.NewLine +
namespaceAliases + Environment.NewLine + 
"namespace " + context.Compilation.AssemblyName + ".Controllers" + $@"
{{" + Environment.NewLine + controller + $@"}}";

            context.AddSource("SiberiaController.g.cs", SourceText.From(source, Encoding.UTF8)); // Create new controller list class
        }
    }
}
