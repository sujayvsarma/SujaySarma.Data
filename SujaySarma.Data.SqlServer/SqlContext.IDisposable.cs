using System;

namespace SujaySarma.Data.SqlServer
{
    // Implements IDisposable centrally for the SqlContext class.
    public partial class SqlContext : IDisposable
    {
        /// <summary>
        /// Dispose the resource-costly members of the instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the resource-costly members of the instance.
        /// </summary>
        /// <param name="reallyDispose">When set, we actually dispose stuff.</param>
        public void Dispose(bool reallyDispose = false)
        {
            if (reallyDispose && (!_isDisposed))
            {
                // variable: SqlContext.Debug.cs
                if (_debugDumpFile != null)
                {
                    _debugDumpFile.Flush();
                    _debugDumpFile.Close();
                }
            }
        }

        private readonly bool _isDisposed = false;
    }
}
