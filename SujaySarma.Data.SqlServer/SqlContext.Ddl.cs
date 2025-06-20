using SujaySarma.Data.Core.Reflection;
using SujaySarma.Data.SqlServer.Attributes;

using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// Implements DDL (data definition language) functionality such as Create/Delete tables.
    /// </summary>
    public partial class SqlContext
    {
        #region Create Table

        /// <summary>
        /// Create a SQL Server table that represents the <typeparamref name="TObject"/> class/struct/record.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to create a table for.</typeparam>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public ExecutionResult CreateTable<TObject>()
        {
            StringBuilder script = GenerateCreateScript<TObject>();
            return ExecuteNonQuery(script);
        }

        /// <summary>
        /// Create a SQL Server table that represents the <typeparamref name="TObject"/> class/struct/record.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to create a table for.</typeparam>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public async Task<ExecutionResult> CreateTableAsync<TObject>()
        {
            StringBuilder script = GenerateCreateScript<TObject>();
            return await ExecuteNonQueryAsync(script);
        }

        /// <summary>
        /// Generates the script to create a table.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to create the table for.</typeparam>
        /// <returns>StringBuilder populated with the script.</returns>
        private static StringBuilder GenerateCreateScript<TObject>()
        {
            ContainerTypeInfo typeInfo = TypeDiscoveryFactory.Resolve<TObject>();
            StringBuilder builder = new StringBuilder();

            builder.Append($"CREATE TABLE [{typeInfo.Container.CreateQualifiedName()}] (");
            int columnsCount = 0;
            foreach (MemberTypeInfo member in typeInfo.Members.Values)
            {
                StringBuilder columnBuilder = new StringBuilder();

                if (columnsCount > 0)
                {
                    columnBuilder.Append(", ");
                }
                columnBuilder.Append(member.Column.CreateQualifiedName()).Append(' ');

                Type clrType = Core.ReflectionUtils.GetFieldOrPropertyDataType(member.FieldOrPropertyInfo);
                string sqlType = ReflectionUtils.GetSqlTypeForClrType(clrType);
                columnBuilder.Append(sqlType).Append(' ');

                // Gather additional metadata
                switch (sqlType)
                {
                    case "char":
                    case "nchar":
                    case "varchar":
                    case "nvarchar":
                        StringLengthAttribute? stringLength = member.FieldOrPropertyInfo.GetCustomAttribute<StringLengthAttribute>();
                        if (stringLength != null)
                        {
                            columnBuilder.Append('(').Append(stringLength.MaximumLength).Append(')');
                        }
                        else
                        {
                            columnBuilder.Append("(50)");   // arbitrary!
                        }
                        break;

                    case "binary":
                        MaxLengthAttribute? binaryLength = member.FieldOrPropertyInfo.GetCustomAttribute<MaxLengthAttribute>();
                        if (binaryLength != null)
                        {
                            int length = binaryLength.Length;
                            if (length < 1)
                            {
                                length = 1;
                            }
                            else if (length > 8000)
                            {
                                length = 8000;
                            }

                            columnBuilder.Append('(').Append(length).Append(')');
                        }
                        else
                        {
                            columnBuilder.Append("(50)");   // arbitrary!
                        }
                        break;

                    case "varbinary":
                        MaxLengthAttribute? varbinaryLength = member.FieldOrPropertyInfo.GetCustomAttribute<MaxLengthAttribute>();
                        if (varbinaryLength != null)
                        {
                            int length = varbinaryLength.Length;
                            if ((length > 8000) || (length < 1))
                            {
                                length = -1;
                            }

                            columnBuilder.Append('(').Append(((length == -1) ? "MAX" : length)).Append(')');
                        }
                        else
                        {
                            columnBuilder.Append("(50)");   // arbitrary!
                        }
                        break;

                    case "decimal":
                    case "numeric":
                        ColumnPrecisionAttribute? dprecision = member.FieldOrPropertyInfo.GetCustomAttribute<ColumnPrecisionAttribute>();
                        ColumnScaleAttribute? dscale = member.FieldOrPropertyInfo.GetCustomAttribute<ColumnScaleAttribute>();
                        if (dprecision != null)
                        {
                            if (dprecision.Precision < 0)
                            {
                                dprecision.Precision = 1;
                            }

                            if (dprecision.Precision > 38)
                            {
                                dprecision.Precision = 38;
                            }

                            columnBuilder.Append('(').Append(dprecision.Precision);
                            if (dscale != null)
                            {
                                if (dscale.Scale < 0)
                                {
                                    dscale.Scale = 0;
                                }

                                if (dscale.Scale > dprecision.Precision)
                                {
                                    dscale.Scale = dprecision.Precision;
                                }

                                columnBuilder.Append(',').Append(dscale.Scale);
                            }
                            else
                            {
                                columnBuilder.Append(",0");
                            }

                            columnBuilder.Append(')');
                        }
                        break;

                    case "float":
                        ColumnPrecisionAttribute? fprecision = member.FieldOrPropertyInfo.GetCustomAttribute<ColumnPrecisionAttribute>();
                        if (fprecision != null)
                        {
                            if (fprecision.Precision < 0)
                            {
                                fprecision.Precision = 1;
                            }

                            if (fprecision.Precision > 38)
                            {
                                fprecision.Precision = 38;
                            }

                            columnBuilder.Append('(').Append(fprecision.Precision).Append(')');
                        }
                        break;

                    case "datetime2":
                    case "time":
                    case "datetimeoffset":
                        ColumnPrecisionAttribute? dtprecision = member.FieldOrPropertyInfo.GetCustomAttribute<ColumnPrecisionAttribute>();
                        if (dtprecision != null)
                        {
                            if (dtprecision.Precision < 0)
                            {
                                dtprecision.Precision = 0;
                            }

                            if (dtprecision.Precision > 7)
                            {
                                dtprecision.Precision = 7;
                            }

                            columnBuilder.Append('(').Append(dtprecision.Precision).Append(')');
                        }
                        break;

                    case "tinyint":
                    case "smallint":
                    case "int":
                    case "bigint":
                        IdentityAttribute? identity = member.FieldOrPropertyInfo.GetCustomAttribute<IdentityAttribute>();
                        if (identity != null)
                        {
                            columnBuilder.Append($"IDENTITY({identity.Seed},{identity.Increment})");
                        }
                        break;
                }
                columnBuilder.Append(' ');

                Type? nullActualType = Nullable.GetUnderlyingType(clrType);
                if (nullActualType == null)
                {
                    columnBuilder.Append("NOT ");
                }
                columnBuilder.Append("NULL");

                builder.Append(columnBuilder);
                columnBuilder.Clear();
                columnsCount++;
            }

            builder.Append(");");
            return builder;
        }

        #endregion

        #region Drop Table

        /// <summary>
        /// Drop a SQL Server table that represents the <typeparamref name="TObject"/> class/struct/record.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to drop the table for.</typeparam>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public ExecutionResult DropTable<TObject>()
        {
            StringBuilder script = GenerateDropScript<TObject>();
            return ExecuteNonQuery(script);
        }

        /// <summary>
        /// Drop a SQL Server table that represents the <typeparamref name="TObject"/> class/struct/record.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to drop the table for.</typeparam>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public async Task<ExecutionResult> DropTableAsync<TObject>()
        {
            StringBuilder script = GenerateDropScript<TObject>();
            return await ExecuteNonQueryAsync(script);
        }

        /// <summary>
        /// Generates the script to drop a table.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to drop the table for.</typeparam>
        /// <returns>StringBuilder populated with the script.</returns>
        private static StringBuilder GenerateDropScript<TObject>()
        {
            ContainerTypeInfo typeInfo = TypeDiscoveryFactory.Resolve<TObject>();
            StringBuilder builder = new StringBuilder();

            builder.Append($"DROP TABLE [{typeInfo.Container.CreateQualifiedName()}];");
            return builder;
        }

        #endregion

        #region Truncate Table

        /// <summary>
        /// Truncate a SQL Server table that represents the <typeparamref name="TObject"/> class/struct/record.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to truncate the table for.</typeparam>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public ExecutionResult TruncateTable<TObject>()
        {
            StringBuilder script = GenerateTruncateScript<TObject>();
            return ExecuteNonQuery(script);
        }

        /// <summary>
        /// Truncate a SQL Server table that represents the <typeparamref name="TObject"/> class/struct/record.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to truncate the table for.</typeparam>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public async Task<ExecutionResult> TruncateTableAsync<TObject>()
        {
            StringBuilder script = GenerateTruncateScript<TObject>();
            return await ExecuteNonQueryAsync(script);
        }

        /// <summary>
        /// Generates the script to truncate a table.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to truncate the table for.</typeparam>
        /// <returns>StringBuilder populated with the script.</returns>
        private static StringBuilder GenerateTruncateScript<TObject>()
        {
            ContainerTypeInfo typeInfo = TypeDiscoveryFactory.Resolve<TObject>();
            StringBuilder builder = new StringBuilder();

            builder.Append($"TRUNCATE TABLE [{typeInfo.Container.CreateQualifiedName()}];");
            return builder;
        }

        #endregion
    }
}
