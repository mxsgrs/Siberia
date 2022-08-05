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
                var list = typeClassDeclaration.GetAnnotatedNodes(new SyntaxAnnotation("Key"))
                    //.Where(node => node is PropertyDeclarationSyntax property && property.get)
                    .Select(node => (PropertyDeclarationSyntax)node)
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

                //PropertyDeclarationSyntax prop;
                //foreach (var item in prop.AttributeLists)
                //{
                //    item.Attributes.Contains();
                //}
            }
        }
    }
}
