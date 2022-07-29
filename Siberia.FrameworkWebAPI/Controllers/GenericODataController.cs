using Microsoft.AspNet.OData;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;

namespace Siberia.FrameworkWebAPI.Controllers
{
    /// <summary>
    /// Handle CRUD operations 
    /// </summary>
    /// <typeparam name="EntityType">Entity declaration class</typeparam>
    public class GenericODataController<EntityType> : ODataController where EntityType : class
    {
        protected DbContext Context; // Dependency injection in child classes

        /// <summary>
        /// Read operation
        /// </summary>
        /// <returns>All entities</returns>
        [EnableQuery]
        public IHttpActionResult Get()
        {
            var result = Context.Set<EntityType>(); // Query all entities
            return Ok(result); // Return all entities
        }

        /// <summary>
        /// Read operation
        /// </summary>
        /// <param name="key">Entity primary key</param>
        /// <returns>Corresponding entity</returns>
        [EnableQuery]
        public async Task<IHttpActionResult> Get(int key)
        {
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
        public async Task<IHttpActionResult> Post([FromBody] EntityType entity)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); } // Verify input respects model
            Context.Set<EntityType>().Add(entity); // Add entity to database
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
        public async Task<IHttpActionResult> Patch(int key, Delta<EntityType> entity)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); } // Verify input respects model
            var existingEntity = await Context.Set<EntityType>().FindAsync(key); // Check if entity already exists
            if (existingEntity is null) { return NotFound(); } // Entity doesn't exists

            entity.Patch(existingEntity); // Overwrite original entity with new values
            try
            {
                await Context.SaveChangesAsync(); // Save changes in database
            }
            catch (Exception) // Database was modified during request
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
        public async Task<IHttpActionResult> Put(int key, [FromBody] EntityType entity)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); } // Verify input respects model
            foreach (PropertyInfo property in typeof(EntityType).GetProperties()) // Iterate over all entity type properties
            {
                if (Attribute.GetCustomAttribute(property, typeof(KeyAttribute)) != null) // Test if property is primary key
                {
                    if ((int)property.GetValue(entity) == key) { break; } // Entity primary key is equal to URL key, request is valid
                    else { return BadRequest(); } // Entity primary key is not equal to URL key, request is not valid
                }
            }

            Context.Entry(entity).State = EntityState.Modified; // Entitty target is declared as modified
            try
            {
                await Context.SaveChangesAsync(); // Save changes in database
            }
            catch (Exception) // Database was modified during request
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
        public async Task<IHttpActionResult> Delete(int key)
        {
            var entity = await Context.Set<EntityType>().FindAsync(key); // Check if entity already exists
            if (entity is null) { return NotFound(); } // Entity doesn't exists
            Context.Set<EntityType>().Remove(entity); // Delete entity
            await Context.SaveChangesAsync(); // Save changes in database
            return Ok(); // Return HTTP 200
        }
    }
}