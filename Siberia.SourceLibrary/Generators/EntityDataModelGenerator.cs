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
    public class EntityDataModelGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new DataModelFinder());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            Dictionary<(string, string), SyntaxList<MemberDeclarationSyntax>> contextMembers = new(); // Store DbContext childs with their members
            HashSet<ClassDeclarationSyntax> contextList = ((DataModelFinder)context.SyntaxReceiver)?.ContextList; // Get classes that inherits DbContext
            Dictionary<string, HashSet<string>> primaryKeyDictionary = ((DataModelFinder)context.SyntaxReceiver)?.TypeKeysDictionary; // Get entity type primary keys
            string h = ((DataModelFinder)context.SyntaxReceiver)?.MyProperty;

            foreach (var contextItem in contextList) // Iterate over each class that inherits DbContext
            {
                var contextNamespace = NamespaceFinder.GetNamespace(contextItem); // Namespace of the class
                var contextId = contextItem.Identifier.ValueText; // Name of the class
                var contextTuple = (contextId, contextNamespace); // Identifier of the class
                if (contextMembers.ContainsKey(contextTuple)) // Check if partial class is already present
                {
                    contextMembers[contextTuple] = contextMembers[contextTuple]
                        .AddRange(contextItem.Members.AsEnumerable()); // Add class members
                }
                else { contextMembers.Add(contextTuple, contextItem.Members); } // Add class members also
            }

            foreach (var contextMember in contextMembers) // Iterate over each unique context
            {
                string entitySet = ""; // Stores all entity set declarations
                string contextName = contextMember.Key.Item1; // Get context name
                foreach (var member in contextMember.Value) // Iterate over each context class member
                {
                    if (member is PropertyDeclarationSyntax property && property.Type.ToString().Contains("DbSet")) // Test if member is a DbSet
                    {
                        string typeName = property.Identifier.ValueText; // Get property name
                        string typeClass = property.Type.ToString().Replace("DbSet<", "").Replace(">", "").Replace("?", ""); // Get property type
                        entitySet += "            " + $@"builder.EntitySet<" + typeClass + ">(\"" + typeName + "\")"; // Entity set declaration

                        if (primaryKeyDictionary.ContainsKey(typeClass) && primaryKeyDictionary[typeClass].Count > 1) // Class has composite key
                        {
                            var compositeKey = primaryKeyDictionary[typeClass].Select(key => "entityType." + key); // Create composite key description
                            entitySet += ".EntityType.HasKey(entityType => new { " + string.Join(", ", compositeKey) + " })"; // Add composite key to entity data model
                        }
                        entitySet += ";" + Environment.NewLine; // Go to next line
                    }
                }

                string source = $@"// Auto-generated code
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using " + contextMember.Key.Item2 + ";" + Environment.NewLine + Environment.NewLine +
"namespace " + context.Compilation.AssemblyName + ".Models.EntityDataModel" + $@"
{{
    public class " + contextName.Replace("Context", "") + $@"EntityDataModel
    {{
        public static IEdmModel GeEntityTypeDataModel()
        {{
            ODataConventionModelBuilder builder = new();" + Environment.NewLine + entitySet + $@"            return builder.GetEdmModel();
        }}
    }}
}}" + "//" + h;

                context.AddSource(contextName.Replace("Context", "") + "EntityDataModel.g.cs", SourceText.From(source, Encoding.UTF8)); // Create new entity data model class
            }
        }
    }
}
