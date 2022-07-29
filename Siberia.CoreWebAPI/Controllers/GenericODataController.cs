using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace Siberia.CoreWebAPI.Controllers
{
    /// <summary>
    /// Handle CRUD operations 
    /// </summary>
    /// <typeparam name="EntityType">Entity declaration class</typeparam>
    public class GenericODataController<EntityType> : ODataController where EntityType : class
    {
        protected DbContext? Context; // Dependency injection in child classes

        /// <summary>
        /// Read operation
        /// </summary>
        /// <returns>All entities</returns>
        [EnableQuery]
        public IActionResult Get()
        {
            if (Context is null) { return UnprocessableEntity(); } // Request is correct but not possible
            var result = Context.Set<EntityType>(); // Query all entities
            return Ok(result); // Return all entities
        }

        /// <summary>
        /// Read operation
        /// </summary>
        /// <param name="key">Entity primary key</param>
        /// <returns>Corresponding entity</returns>
        [EnableQuery]
        public async Task<IActionResult> Get(int key)
        {
            if (Context is null) { return UnprocessableEntity(); } // Request is correct but not possible
            var result = await Context.Set<EntityType>().FindAsync(key); // Select corresponding entity
            if (result is null) { return NotFound(); } // Entity doesn't exist
            return Ok(result); // Return entity corresponding to primary key
        }

        /// <summary>
        /// Create operation
        /// </summary>
        /// <param name="entity">New entity</param>
        /// <returns>Request result</returns>
        [EnableQuery]
        public async Task<IActionResult> Post([FromBody] EntityType entity)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); } // Verify input respects model
            if (Context is null) { return UnprocessableEntity(); } // Request is correct but not possible
            await Context.AddAsync(entity); // Add entity to database
            await Context.SaveChangesAsync(); // Save changes in database
            return Created(entity); // Return action result of entity creation
        }

        /// <summary>
        /// Update operation
        /// </summary>
        /// <param name="key">Entity primary key</param>
        /// <param name="entity">Entity new values</param>
        /// <returns>Request result</returns>
        [EnableQuery]
        public async Task<IActionResult> Patch(int key, Delta<EntityType> entity)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); } // Verify input respects model
            if (Context is null) { return UnprocessableEntity(); } // Request is correct but not possible

            var existingEntity = await Context.Set<EntityType>().FindAsync(key); // Check if entity already exists
            if (existingEntity is null) { return NotFound(); } // Entity doesn't exists

            entity.Patch(existingEntity); // Overwrite original entity with new values
            try
            {
                await Context.SaveChangesAsync(); // Save changes in database
            }
            catch (DbUpdateConcurrencyException) // Database was modified during request
            {
                var concurrencyEntity = await Context.Set<EntityType>().FindAsync(key); // Verify entity still exist
                if (concurrencyEntity is null) { return NotFound(); } // Entity doesn't exist anymore
                else { throw; } // Throw exception
            }
            return Updated(entity); // Return action result of entity update
        }

        /// <summary>
        /// Update operation
        /// </summary>
        /// <param name="key">Entity primary key</param>
        /// <param name="entity">Entity new values</param>
        /// <returns>Request result</returns>
        [EnableQuery]
        public async Task<IActionResult> Put(int key, [FromBody] EntityType entity)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); } // Verify input respects model
            if (Context is null) { return UnprocessableEntity(); } // Request is correct but not possible

            var entityType = Context.Model.FindEntityType(typeof(EntityType)); // Gets entity type
            var primaryKeyName = entityType?.FindPrimaryKey()?.Properties.Select(x => x.Name).Single(); // Find primary key name
            if (primaryKeyName is null) { return UnprocessableEntity(); } // Check if primary key exists
            var primaryKeyValue = (int?)entity?.GetType()?.GetProperty(primaryKeyName)?.GetValue(entity, null); // Get primary key value
            if (key != primaryKeyValue || entity is null) { return BadRequest(); } // Check if primary keys are the same

            Context.Entry(entity).State = EntityState.Modified; // Entitty target is declared as modified
            try
            {
                await Context.SaveChangesAsync(); // Save changes in database
            }
            catch (DbUpdateConcurrencyException) // Database was modified during request
            {
                var concurrencyEntity = await Context.Set<EntityType>().FindAsync(key); // Verify entity still exist
                if (concurrencyEntity is null) { return NotFound(); } // Entity doesn't exist anymore
                else { throw; } // Throw exception
            }
            return Updated(entity); // Return action result of entity update
        }

        /// <summary>
        /// Delete operation
        /// </summary>
        /// <param name="key">Entity primary key</param>
        /// <returns>Request result</returns>
        [EnableQuery]
        public async Task<IActionResult> Delete(int key)
        {
            if (Context is null) { return UnprocessableEntity(); } // Request is correct but not possible
            var entity = await Context.Set<EntityType>().FindAsync(key); // Check if entity already exists
            if (entity is null) { return NotFound(); } // Entity doesn't exists
            Context.Remove(entity); // Delete entity
            await Context.SaveChangesAsync(); // Save changes in database
            return Ok(); // Return HTTP 200
        }
    }
}
