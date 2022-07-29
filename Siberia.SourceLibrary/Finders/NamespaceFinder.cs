using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Siberia.SourceLibrary.Finders
{
    public class NamespaceFinder
    {
        public static string GetNamespace(BaseTypeDeclarationSyntax syntax)
        {
            string dbContextNamespace = string.Empty;
            var potentialNamespaceParent = syntax.Parent;
            if (potentialNamespaceParent is not null && potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
            {
                dbContextNamespace = namespaceParent.Name.ToString();
            }
            return dbContextNamespace;
        }
    }
}
