using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Siberia.SourceLibrary.Finders
{
    internal class DbContextFinder : ISyntaxReceiver
    {
        public HashSet<ClassDeclarationSyntax> ContextList { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclaration && classDeclaration.BaseList?.Types is not null)
            {
                if (classDeclaration.BaseList.Types.ToString().Contains("DbContext"))
                {
                    ContextList.Add(classDeclaration);
                }
            }
        }
    }
}
