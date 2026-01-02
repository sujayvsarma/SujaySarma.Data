using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SujaySarma.Data.SqlServer;

// Implements IDisposable for SqlContext
public partial class SqlContext : IDisposable
{

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
        }
    }
    private bool _isDisposed = false;

}
