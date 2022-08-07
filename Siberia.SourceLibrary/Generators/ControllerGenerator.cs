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
            HashSet<ClassDeclarationSyntax> contextList = ((DataModelFinder)context.SyntaxReceiver)?.ContextList; // Get classes that inherits DbContext
            Dictionary<string, HashSet<(string, string)>> primaryKeyDictionary = ((DataModelFinder)context.SyntaxReceiver)?.TypeKeysDictionary; // Get entity type primary keys

            foreach (var contextItem in contextList) // Iterate over each class that inherits DbContext
            {
                string contextName = contextItem.Identifier.ValueText; // Get context name
                namespaceAliasHashSet.Add(NamespaceFinder.GetNamespace(contextItem)); // Add namespace to hashset to avoid duplicates
                foreach (var member in contextItem.Members) // Iterate over each context class member
                {
                    if (member is PropertyDeclarationSyntax property && property.Type.ToString().Contains("DbSet")) // Test if member is a DbSet
                    {
                        string entityName = property.Identifier.ValueText; // Get property name
                        string entityType = property.Type.ToString().Replace("DbSet<", "").Replace(">", ""); // Get property type

                        if (primaryKeyDictionary[entityType].Count == 1) // No composite key for this entity type
                        {
                            controller += $@"    public class " + entityName + "Controller : GenericODataController<"
                            + entityType + $@"> {{ " + "public " + entityName + "Controller(" + contextName + $@" context) {{ Context = context; }}"
                            + $@" }}" + Environment.NewLine; // Controller declaration
                        }
                        else // Composite key for this entity type
                        {
                            string compositeKeysDeclaration = "";
                            string compositeKeys = "";
                            string compositeKeysDictionary = "";
                            foreach(var item in primaryKeyDictionary[entityType]) 
                            { 
                                compositeKeysDeclaration += "[FromODataUri] " + item.Item1 + " key" + item.Item2 + ", ";
                                compositeKeys += "key" + item.Item2 + ", ";
                                compositeKeysDictionary += $@"{{ """ + item.Item2 + $@""", key" + item.Item2 + $@" }}," 
                                    + Environment.NewLine + "                ";
                            };
                            compositeKeysDeclaration = compositeKeysDeclaration.Remove(compositeKeysDeclaration.Length - 2);
                            compositeKeys = compositeKeys.Remove(compositeKeys.Length - 2);
                            compositeKeysDictionary = compositeKeysDictionary.Remove(compositeKeysDictionary.Length - 19);

                            string compositeKeyController = $@"    /// <summary>
    /// Handle CRUD operations 
    /// </summary>
    public class " + entityName + $@"Controller : ODataController
    {{
        protected DbContext? Context; // Dependency injection

        public " + entityName + $@"Controller(" + contextName + $@" context)
        {{
            Context = context;
        }}

        /// <summary>
        /// Read operation
        /// </summary>
        /// <returns>All entities</returns>
        [EnableQuery]
        public IActionResult Get()
        {{
            if (Context is null) {{ return UnprocessableEntity(); }} // Request is correct but not possible
            var result = Context.Set<" + entityType + $@">(); // Query all entities
            return Ok(result); // Return all entities
        }}

        /// <summary>
        /// Read operation
        /// </summary>
        /// <param name=""key"">Entity primary key</param>
        /// <returns>Corresponding entity</returns>
        [EnableQuery]
        public async Task<IActionResult> Get(" + compositeKeysDeclaration + $@")
        {{
            if (Context is null) {{ return UnprocessableEntity(); }} // Request is correct but not possible
            var result = await Context.Set<" + entityType + $@">().FindAsync(" + compositeKeys + $@"); // Select corresponding entity
            if (result is null) {{ return NotFound(); }} // Entity doesn't exist
            return Ok(result); // Return entity corresponding to primary key
        }}

        /// <summary>
        /// Create operation
        /// </summary>
        /// <param name=""entity"">New entity</param>
        /// <returns>Request result</returns>
        [EnableQuery]
        public async Task<IActionResult> Post([FromBody] " + entityType + $@" entity)
        {{
            if (!ModelState.IsValid) {{ return BadRequest(ModelState); }} // Verify input respects model
            if (Context is null) {{ return UnprocessableEntity(); }} // Request is correct but not possible
            await Context.AddAsync(entity); // Add entity to database
            await Context.SaveChangesAsync(); // Save changes in database
            return Created(entity); // Return action result of entity creation
        }}

        /// <summary>
        /// Update operation
        /// </summary>
        /// <param name=""key"">Entity primary key</param>
        /// <param name=""entity"">Entity new values</param>
        /// <returns>Request result</returns>
        [EnableQuery]
        public async Task<IActionResult> Patch(" + compositeKeysDeclaration + $@", Delta<" + entityType + $@"> entity)
        {{
            if (!ModelState.IsValid) {{ return BadRequest(ModelState); }} // Verify input respects model
            if (Context is null) {{ return UnprocessableEntity(); }} // Request is correct but not possible

            var existingEntity = await Context.Set<" + entityType + $@">().FindAsync(" + compositeKeys + $@"); // Check if entity already exists
            if (existingEntity is null) {{ return NotFound(); }} // Entity doesn't exists

            entity.Patch(existingEntity); // Overwrite original entity with new values
            try
            {{
                await Context.SaveChangesAsync(); // Save changes in database
            }}
            catch (DbUpdateConcurrencyException) // Database was modified during request
            {{
                var concurrencyEntity = await Context.Set<" + entityType + $@">().FindAsync(" + compositeKeys + $@"); // Verify entity still exist
                if (concurrencyEntity is null) {{ return NotFound(); }} // Entity doesn't exist anymore
                else {{ throw; }} // Throw exception
            }}
            return Updated(entity); // Return action result of entity update
        }}

        /// <summary>
        /// Update operation
        /// </summary>
        /// <param name=""key"">Entity primary key</param>
        /// <param name=""entity"">Entity new values</param>
        /// <returns>Request result</returns>
        [EnableQuery]
        public async Task<IActionResult> Put(" + compositeKeysDeclaration + $@", [FromBody] " + entityType + $@" entity)
        {{
            if (!ModelState.IsValid) {{ return BadRequest(ModelState); }} // Verify input respects model
            if (Context is null) {{ return UnprocessableEntity(); }} // Request is correct but not possible

            var entityType = Context.Model.FindEntityType(typeof(" + entityType + $@")); // Gets entity type
            var primaryKeyNames = entityType?.FindPrimaryKey()?.Properties.Select(property => property.Name); // Find primary key names
            if (primaryKeyNames is null) {{ return UnprocessableEntity(); }} // Check if primary key exists

            Dictionary<string, object?> matchingKeys = new()
            {{
                " + compositeKeysDictionary + $@"
            }};

            foreach (var primaryKeyName in primaryKeyNames)
            {{
                var primaryKeyValue = entity?.GetType()?.GetProperty(primaryKeyName)?.GetValue(entity, null); // Get primary key value
                if (!Equals(primaryKeyValue, matchingKeys[primaryKeyName]) || entity is null) {{ return BadRequest(); }} // Check if primary keys are the same
            }}

            Context.Entry(entity).State = EntityState.Modified; // Entitty target is declared as modified
            try
            {{
                await Context.SaveChangesAsync(); // Save changes in database
            }}
            catch (DbUpdateConcurrencyException) // Database was modified during request
            {{
                var concurrencyEntity = await Context.Set<" + entityType + $@">().FindAsync(" + compositeKeys + $@"); // Verify entity still exist
                if (concurrencyEntity is null) {{ return NotFound(); }} // Entity doesn't exist anymore
                else {{ throw; }} // Throw exception
            }}
            return Updated(entity); // Return action result of entity update
        }}

        /// <summary>
        /// Delete operation
        /// </summary>
        /// <param name=""key"">Entity primary key</param>
        /// <returns>Request result</returns>
        [EnableQuery]
        public async Task<IActionResult> Delete(" + compositeKeysDeclaration + $@")
        {{
            if (Context is null) {{ return UnprocessableEntity(); }} // Request is correct but not possible
            var entity = await Context.Set<" + entityType + $@">().FindAsync(" + compositeKeys + $@"); // Check if entity already exists
            if (entity is null) {{ return NotFound(); }} // Entity doesn't exists
            Context.Remove(entity); // Delete entity
            await Context.SaveChangesAsync(); // Save changes in database
            return Ok(); // Return HTTP 200
        }}
    }}" + Environment.NewLine;

                            List<string> namespaceAliasListComposite = namespaceAliasHashSet.Select(alias => "using " + alias + ";" + Environment.NewLine).ToList(); // Create usings
                            string namespaceAliasesComposite = string.Join("", namespaceAliasListComposite); // Join usings

                            string sourceComposite = $@"// Auto-generated code" + Environment.NewLine +
$@"using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;" + Environment.NewLine +
namespaceAliasesComposite + Environment.NewLine +
"namespace " + context.Compilation.AssemblyName + ".Controllers" + $@"
{{" + Environment.NewLine + compositeKeyController + $@"}}";

                            context.AddSource(entityName + "Controller.g.cs", SourceText.From(sourceComposite, Encoding.UTF8)); // Create new controller list class
                        }
                        
                    }
                }
            }

            List<string> namespaceAliasList = namespaceAliasHashSet.Select(alias => "using " + alias + ";" + Environment.NewLine).ToList(); // Create usings
            string namespaceAliases = string.Join("", namespaceAliasList); // Join usings

            string source = $@"// Auto-generated code" + Environment.NewLine +
namespaceAliases + Environment.NewLine + 
"namespace " + context.Compilation.AssemblyName + ".Controllers" + $@"
{{" + Environment.NewLine + controller + $@"}}";

            context.AddSource("SiberiaControllers.g.cs", SourceText.From(source, Encoding.UTF8)); // Create new controller list class
        }
    }
}
