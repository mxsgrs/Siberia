using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Siberia.SourceLibrary.Finders
{
    internal class DataModelFinder : ISyntaxReceiver
    {
        public HashSet<ClassDeclarationSyntax> ContextList { get; } = new();
        public Dictionary<string, HashSet<string>> TypeKeysDictionary { get; } = new();
        public string MyProperty { get; set; } = "";

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclaration && classDeclaration.BaseList?.Types is not null)
            {
                if (classDeclaration.BaseList.Types.ToString().Contains("DbContext"))
                {
                    ContextList.Add(classDeclaration);
                }
            }

            if (syntaxNode is ClassDeclarationSyntax typeClassDeclaration)
            {
                var list = typeClassDeclaration.ChildNodes()
                    .Where(node => node is PropertyDeclarationSyntax property)
                    .Select(node => (PropertyDeclarationSyntax)node)
                    .Where(property => property.AttributeLists.Where(item => item.Attributes
                        .Where(att => att.Name.ToFullString() == "Key").Any())
                        .Any())
                    .Select(property => property.Identifier.ValueText)
                    .ToList();

                string classDeclarationName = typeClassDeclaration.Identifier.ValueText;

                if (TypeKeysDictionary.ContainsKey(classDeclarationName))
                {
                    TypeKeysDictionary[classDeclarationName].UnionWith(list);
                }
                else
                {
                    var hashset = new HashSet<string>(list);
                    TypeKeysDictionary.Add(classDeclarationName, hashset);
                }
            }
        }
    }
}
