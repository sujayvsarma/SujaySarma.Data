using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using SujaySarma.Data.Core.Reflection;

namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// DDL operations through SqlTableContext
    /// </summary>
    public partial class SqlTableContext
    {

        /// <summary>
        /// Attempt to create a table for the provided CLR object
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        public async Task CreateTableAsync<TObject>()
        {
            ContainerTypeInformation metadata = TypeDiscoveryFactory.Resolve<TObject>() ?? throw new TypeLoadException($"Type '{typeof(TObject).Name}' is not appropriately decorated."); ;
            List<string> statements = new List<string>()
            {
                "CREATE TABLE",
                metadata.ContainerDefinition.CreateQualifiedName(),
                "("
            }, 
            primaryKeys = new List<string>(),
            columns = new List<string>();

            foreach(ContainerMemberTypeInformation member in metadata.Members.Values)
            {
                List<string> columnDefinition = new List<string>()
                {
                    member.ContainerMemberDefinition.CreateQualifiedName()
                };

                Type clrType = SujaySarma.Data.Core.Reflection.ReflectionUtils.GetFieldOrPropertyDataType(member.FieldOrPropertyInfo);
                string sqlType = ReflectionUtils.GetSqlTypeForClrType(clrType);

                if (sqlType == "varbinary")
                {
                    sqlType = "varbinary(max)";
                }

                // do we have any validation attributes?
                List<ValidationAttribute> validationAttributes = member.FieldOrPropertyInfo.GetCustomAttributes<ValidationAttribute>(true).ToList();
                if (validationAttributes.Count > 0)
                {
                    if ((sqlType == "nchar") || (sqlType == "nvarchar"))
                    {
                        StringLengthAttribute? length = (StringLengthAttribute?)validationAttributes.FirstOrDefault(a => (a is StringLengthAttribute));
                        sqlType = ((length != null) ? $"{sqlType}({length.MaximumLength})" : $"{sqlType}(max)");
                        columnDefinition.Add(sqlType);
                    }
                    else
                    {
                        columnDefinition.Add(sqlType);
                    }

                    columnDefinition.Add(
                            (validationAttributes.Any(a => (a is RequiredAttribute)))
                            ? "NOT NULL"
                            : "NULL"
                        );
                }
                else
                {
                    Type? srcActualType = Nullable.GetUnderlyingType(clrType);
                    columnDefinition.Add((srcActualType == null) ? $"{sqlType} NOT NULL" : $"{sqlType} NULL");
                }

                // push to table columns
                columns.Add(string.Join(' ', columnDefinition));

                if (member.ContainerMemberDefinition.IsSearchKey)
                {
                    primaryKeys.Add(member.ContainerMemberDefinition.CreateQualifiedName());
                }
            }

            string sql = string.Join(
                    ' ',
                    string.Join(' ', statements),
                    string.Join(',', columns),
                    ',',
                    $"CONSTRAINT PK_{metadata.ContainerDefinition.CreateQualifiedName().Replace(' ', '_')} PRIMARY KEY ({string.Join(',', primaryKeys)})",
                    ");"
                );


            await ExecuteNonQueryAsync(sql);
        }

        /// <summary>
        /// Attempt to drop the table for the provided CLR object
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        public async Task DropTableAsync<TObject>()
        {
            ContainerTypeInformation metadata = TypeDiscoveryFactory.Resolve<TObject>() ?? throw new TypeLoadException($"Type '{typeof(TObject).Name}' is not appropriately decorated.");
            string sql = $"DROP TABLE {metadata.ContainerDefinition.CreateQualifiedName()};";
            await ExecuteNonQueryAsync(sql);
        }
    }
}
