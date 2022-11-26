namespace RappelJdr.Database
{
    using System.Collections.Generic;

    /// <summary>
    /// Classe implémentant les méthodes principales partagées des services.
    /// </summary>
    /// <typeparam name="T">Type of the service to instantiate.</typeparam>
    public abstract class AbstractService<T>
    {
        /// <summary>
        /// Class handling the datas.
        /// </summary>
        public static JsonHandler JsonHandler = new JsonHandler();

        /// <summary>
        /// Add an entity to the database.
        /// </summary>
        /// <param name="entity">Entity to add.</param>
        public void Add(T entity)
        {
            JsonHandler.Add(entity);
        }

        /// <summary>
        /// Get an entity by id.
        /// </summary>
        /// <param name="id">Id of an entity.</param>
        /// <returns>An entity.</returns>
        public T GetById(int id)
        {
            return JsonHandler.GetById<T>(id);
        }

        /// <summary>
        /// Return a list of entities.
        /// </summary>
        /// <returns>List of entities.</returns>
        public List<T> GetEntities()
        {
            return JsonHandler.GetEntities<T>();
        }

        /// <summary>
        /// Delete an entity from the database.
        /// </summary>
        /// <param name="entity">Entity to remove.</param>
        public void Remove(T entity)
        {
            JsonHandler.Remove(entity);
        }

        /// <summary>
        /// Delete an entity from the database from it's id.
        /// </summary>
        /// <param name="id">Id of an entity.</param>
        public void RemoveById(int id)
        {
            JsonHandler.RemoveById<T>(id);
        }

        /// <summary>
        /// Update an entity.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        public void Update(T entity)
        {
            JsonHandler.Update(entity);
        }
    }
}