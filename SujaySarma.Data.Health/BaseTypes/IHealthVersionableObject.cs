namespace SujaySarma.Data.Health.BaseTypes
{
    /// <summary>
    /// Interface defining methods for version generating methods
    /// </summary>
    /// <typeparam name="TObject">Type of .NET class, structure or record that will generate versions of its data</typeparam>
    public interface IHealthVersionableObject<TObject>
    {
        /// <summary>
        /// Create a new version of current object
        /// </summary>
        /// <returns>New version</returns>
        TObject CreateVersion();
    }
}
