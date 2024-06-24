namespace Fenrir.ECS
{
    internal enum ArchetypeQueryType
    {
        /// <summary>
        /// Finds archetypes that contain all given types
        /// </summary>
        All,

        /// <summary>
        /// Finds archetypes that contain any of the given type
        /// </summary>
        Any,
    }
}
