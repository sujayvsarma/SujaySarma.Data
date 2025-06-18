namespace SujaySarma.Data.Azure.Tables
{
    /// <summary>
    /// Mode of update
    /// </summary>
    public enum UpdateModes
    {
        /// <summary>
        /// Merge record in place.
        /// Will not insert or update any records if the indicated record is missing.
        /// </summary>
        Merge = 0,

        /// <summary>
        /// Completely replace existing record.
        /// Will not insert or update any records if the indicated record is missing.
        /// </summary>
        Replace = 1,

        /// <summary>
        /// Merge record in place.
        /// If record is missing, inserts the provided data.
        /// </summary>
        InsertIfMissingOrMerge = 2,

        /// <summary>
        /// Completely replace existing record.
        /// If record is missing, inserts the provided data.
        /// </summary>
        InsertIfMissingOrReplace = 4
    }
}
