namespace SujaySarma.Data.Health.BaseTypes
{
    /// <summary>
    /// Interface defining methods for version generating methods
    /// </summary>
    /// <typeparam name="T">Type of object that will generate versions</typeparam>
    public interface IHealthVersionableObject<T>
        where T : class
    {
        /// <summary>
        /// Create a new version of current object
        /// </summary>
        /// <returns>New version</returns>
        T CreateVersion();
    }
}
