using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

#if NET7_0_OR_GREATER
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif

namespace SujaySarma.Data.SqlServer.Builders;

/// <summary>
/// Helps build an EXEC call -- to a stored procedure or function.
/// </summary>
public class SqlExecBuilder
{

    /// <summary>
    /// Assemble all components of the builder into the statementType.
    /// </summary>
    /// <returns>Instance of a <see cref="StringBuilder"/> that can then be serialised to a string.</returns>
    public SqlCommand Build()
    {
        SqlCommand command = new SqlCommand()
        {
            CommandType = CommandType.StoredProcedure,
            CommandText = _procName
        };

        if (_parameters.Count > 0)
        {
            foreach(SqlParameter parameter in _parameters)
            {
                command.Parameters.Add(parameter);
            }
        }

        return command;
    }

    /// <summary>
    /// Adds the expectation of a return value to the procedure/function call.
    /// </summary>
    /// <returns>Instance of self.</returns>
    public SqlExecBuilder ExpectReturnValue()
    {
        _parameters.Add(new SqlParameter("@returnValue", null)
        {
            Direction = ParameterDirection.ReturnValue
        });
        return this;
    }

    /// <summary>
    /// Adds an input/output parameter to the procedure/function call.
    /// </summary>
    /// <param name="name">Name of the parameter.</param>
    /// <param name="value">Value. Cannot be NULL if <paramref name="direction"/> is OUTPUT or INPUT/OUTPUT.</param>
    /// <param name="direction">Direction of the parameter. Allowed: INPUT, OUTPUT, INPUT/OUTPUT.</param>
    /// <returns>Instance of self.</returns>
    public SqlExecBuilder AddParameter(string name, object? value, ParameterDirection direction = ParameterDirection.Input)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Parameter name cannot be null or empty.", nameof(name));
        }

        if (direction == ParameterDirection.ReturnValue)
        {
            throw new InvalidOperationException("Return value parameters cannot be added using this method.");
        }

        if (((direction == ParameterDirection.Output) || (direction == ParameterDirection.InputOutput)) && (value is null))
        {
            throw new InvalidOperationException("Parameters of OUTPUT and INPUT/OUTPUT cannot be NULL as SQL Server cannot guess its data type.");
        }

        _parameters.Add(
            new SqlParameter(
                name.StartsWith('@') ? name : "@" + name, 
                    value ?? DBNull.Value)
            {
                Direction = direction
            });

        return this;
    }

    /// <summary>
    /// Initialise a SqlExecBuilder targeted to the provided <paramref name="procedureName"/>.
    /// </summary>
    /// <param name="procedureName">Name of the procedure or function that shall be called.</param>
    /// <returns>An initialised instance of SqlExecBuilder.</returns>
    public static SqlExecBuilder ForProcedure(string procedureName)
    {
        if (string.IsNullOrWhiteSpace(procedureName))
        {
            throw new ArgumentException("Procedure name cannot be null or empty.", nameof(procedureName));
        }

        return new SqlExecBuilder(procedureName.EnsureIdentifierIsQuoted());
    }


    /// <summary>
    /// Private initialsier to prevent direct initialisation.
    /// </summary>
    /// <param name="procedureName">Name of the stored procedure or function.</param>
    private SqlExecBuilder(string procedureName)
    {
        _procName = procedureName;
        _parameters = new List<SqlParameter>();
    }

    /// <summary>
    /// Returns the name of the procedure or function.
    /// </summary>
    public string ProcedureName => _procName;

    /// <summary>
    /// Returns a list of INPUT direction parameters.
    /// </summary>
    public List<SqlParameter> InputParameters
        => _parameters.Where(p => ((p.Direction is ParameterDirection.Input) || (p.Direction is ParameterDirection.InputOutput))).ToList();

    /// <summary>
    /// Returns a list of OUTPUT direction parameters.
    /// </summary>
    public List<SqlParameter> OutputParameters
        => _parameters.Where(p => ((p.Direction is ParameterDirection.Output) || (p.Direction is ParameterDirection.InputOutput))).ToList();


    private readonly string _procName;
    private readonly List<SqlParameter> _parameters;
}
