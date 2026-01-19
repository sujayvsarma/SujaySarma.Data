using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using SujaySarma.Data.Core.ReflectionUtilities;
using SujaySarma.Data.Core.TypeDiscovery;
using SujaySarma.Data.TokenLimitedFiles.Attributes;

namespace SujaySarma.Data.TokenLimitedFiles;

/// <summary>
/// Serialises business entities into string arrays, ready to be written to token-delimited files or streams. And when 
/// given string arrays read from token-delimited files or streams, deserialises them to rehydrate business entities.
/// </summary>
public class Serialiser
{
    #region Deserialisation

    /// <summary>
    /// Deserialises data into a business entity class, struct or record.
    /// </summary>
    /// <param name="values">The values to set for the member properties/fields of the instance of type <see cref="Type"/>.</param>
    /// <returns>An instantiated and rehydrated entity instance.</returns>
    public object Deserialise(string[] values)
    {
        object instance = (object?)Activator.CreateInstance(_container.EntityType, nonPublic: true)
                        ?? throw new TypeLoadException($"Could not instantiate an object of type '{_container.EntityType.GetUsableTypeName()}'.");

        // Discovery options makes sure that we get this.
        Flatfile.FieldReferencesAre referenceMode = ((Flatfile)_container.PersistenceInfo).FieldReferenceMode;

        // Ensure values are de-quoted, or they will fail deserialisation!
        values = UnquoteStrings(values);

        switch (referenceMode)
        {
            case Flatfile.FieldReferencesAre.Indexes:
                for (int i = 0; i < _headers.Length; i++)
                {
                    //BUGFIX: File may contain lesser fields than expected by the header row.
                    if (values.Length <= i)
                    {
                        break;
                    }

                    if (_headers[i] != string.Empty)
                    {
                        if (!_container.TryGetMember(_headers[i], out PersistenceContainerMemberInfo? member))
                        {
                            throw new InvalidOperationException($"Unable to find member by name '{_headers[i]}'.");
                        }

                        instance.SetValue(member, values[i]);
                    }
                }
                break;

            case Flatfile.FieldReferencesAre.Names:
                for (int i = 0; i < _headers.Length; i++)
                {
                    //BUGFIX:  File may contain lesser fields than expected by the header row.
                    if (values.Length <= i)
                    {
                        break;
                    }

                    if (_headers[i] != QUOTED_EMPTY_STRING)
                    {
                        if (!_container.TryGetMemberByPersistenceColumnName(_headers[i], out PersistenceContainerMemberInfo? member))
                        {
                            throw new InvalidOperationException($"Unable to find member by name '{_headers[i]}'.");
                        }

                        instance.SetValue(member, values[i]);
                    }
                }
                break;
        }

        return instance;
    }

    /// <summary>
    /// Replace the automatically discovered header row information with the provided one. 
    /// This is required if the data being read contains header column definitions in a different order etc.
    /// </summary>
    /// <param name="newPreamble">The new sequence of columns/fields.</param>
    internal void ReplacePreamble(string[] newPreamble)
    {
        if (newPreamble.Length != 0)
        {
            if (_serialisableHeaders.Length != 0)
            {
                _serialisableHeaders = QuoteStrings(newPreamble);
            }
            _headers = newPreamble;
        }
    }

    #endregion

    #region Serialisation

    /// <summary>
    /// Serialise the provided <paramref name="entity"/> (an entire business entity class, record or struct) into a 
    /// collection of strings ready to be written to a token-delimited file or stream.
    /// </summary>
    /// <param name="entity">The business entity class, record or struct to be serialised.</param>
    /// <returns>A collection of strings compatible with being written. When <paramref name="entity"/> is NULL, returns an empty array of the 
    /// length of the number of fields/columns that maybe otherwise present (i.e., length of the header row). 
    /// NOTE: Elements ARE appropriately quoted at this point.</returns>
    public string[] SerialiseEntity(object? entity)
    {
        if (entity is null)
        {
            return new string[_headers.Length];
        }

        // Discovery options makes sure that we get this.
        Flatfile.FieldReferencesAre referenceMode = ((Flatfile)_container.PersistenceInfo).FieldReferenceMode;
        string[] values = Array.Empty<string>();

        switch (referenceMode)
        {
            case Flatfile.FieldReferencesAre.Indexes:
                values = new string[_headers.Length];
                for (int i = 0; i < _headers.Length; i++)
                {
                    if (_headers[i] == string.Empty)
                    {
                        values[i] = QUOTED_EMPTY_STRING;
                    }
                    else
                    {
                        if (!_container.TryGetMember(_headers[i], out PersistenceContainerMemberInfo? member))
                        {
                            throw new InvalidOperationException($"Unable to find member by name '{_headers[i]}'.");
                        }

                        values[i] = SerialiseValue(entity.GetValue(member, true), _fieldDelimiter, _recordDelimiter);
                    }
                }
                break;

            case Flatfile.FieldReferencesAre.Names:
                values = new string[_headers.Length];
                for (int i = 0; i < _headers.Length; i++)
                {
                    if (_headers[i] == string.Empty)
                    {
                        values[i] = QUOTED_EMPTY_STRING;
                    }
                    else
                    {
                        if (!_container.TryGetMemberByPersistenceColumnName(_headers[i], out PersistenceContainerMemberInfo? member))
                        {
                            throw new InvalidOperationException($"Unable to find member by name '{_headers[i]}'.");
                        }

                        values[i] = SerialiseValue(entity.GetValue(member, true), _fieldDelimiter, _recordDelimiter);
                    }
                }
                break;
        }

        return values;
    }

    /// <summary>
    /// Return the headers as a string array, ready to be written to the file or stream.
    /// </summary>
    /// <returns>String array containing the column/field names for the header row. Will be Empty if the entity defines positional 
    /// columns/fields (and not named columns/fields).</returns>
    public string[] SerialiseHeaders()
    {
        return _serialisableHeaders;
    }

    /// <summary>
    /// Serialise the provided <paramref name="data"/> element (for a field, presumably) into a 
    /// string that is ready to be written to a token-delimited file or stream.
    /// </summary>
    /// <typeparam name="T">Type of <paramref name="data"/>.</typeparam>
    /// <param name="data">A single property/field or standalone value to be serialised.</param>
    /// <param name="fieldDelimiter">The delimiter between fields. Default is comma (',').</param>
    /// <param name="recordDelimiter">The delimiter between records. Default is CRLF ("\r\n").</param>
    /// <returns>String, compatible with being written. NOTE: This IS quoted at this point.</returns>
    public static string SerialiseValue<T>(T? data, char fieldDelimiter = ',', string recordDelimiter = "\r\n")
    {
        if (data is null)
        {
            if (typeof(T) == typeof(string))
            {
                return QUOTED_EMPTY_STRING;
            }

            return string.Empty;
        }

        bool mustQuote = false;
        if (data is string s)
        {
            mustQuote = true;
        }
        else
        {
            s = (string?)data.ConvertTo(typeof(string)) ?? string.Empty;
            if (s == "NULL")
            {
                s = string.Empty;
            }
        }


        // Some types serialised to string may end up containing ','s and other chars.
        // Eg: numerics converted using local culture info.
        if (s.Contains(fieldDelimiter) || s.Contains(recordDelimiter) || s.Contains('"'))
        {
            mustQuote = true;
        }

        // Check for common problems...
        if (s.Contains('\0'))
        {
            throw new InvalidDataException("Value of field contains a NULL character. This is not allowed!");
        }

        if (mustQuote)
        {
            //BUGFIX: escaping quotes was reversed!
            s = $"\"{s.Replace("\"", "\"\"")}\"";
        }

        return s;
    }

    #endregion

    /// <summary>
    /// Unquote quoted strings in the provided array.
    /// </summary>
    /// <param name="strings">Strings to unquote.</param>
    /// <returns>Array of unquoted strings.</returns>
    private string[] UnquoteStrings(string[] strings)
    {
        string[] results = new string[strings.Length];
        for (int i = 0; i < strings.Length; i++)
        {
            string s = strings[i];
            if ((!string.IsNullOrEmpty(s)) && (s.Length >= 2) && (s[0] == '"') && (s[^1] == '"'))
            {
                results[i] = s[1..^1].Replace("\"\"", "\"");
            }
            else
            {
                results[i] = s;
            }
        }

        return results;
    }

    /// <summary>
    /// Quote strings in the provided array.
    /// </summary>
    /// <param name="strings">Strings to quote.</param>
    /// <returns>Array of quoted strings.</returns>
    private string[] QuoteStrings(string[] strings)
    {
        string[] results = new string[strings.Length];
        for (int i = 0; i < strings.Length; i++)
        {
            string s = strings[i];
            if ((!string.IsNullOrEmpty(s)) && (s.Length >= 2) && (s[0] != '"') && (s[^1] != '"'))
            {
                //BUGFIX: escaping quotes was reversed!
                results[i] = $"\"{s.Replace("\"", "\"\"")}\"";
            }
            else
            {
                results[i] = s;
            }
        }

        return results;
    }

    #region --- Constructors/Initialisers ---

    /// <summary>
    /// Sets the field delimiter. The default is comma (',') and this function does not need to be 
    /// called to set it to a comma. 
    /// NOTE: In the context of this Serialiser, delimiters are used only during serialisation (in the Serialise... methods) 
    /// to determine if content including the delimiter needs to be escaped or enclosed in quotes.
    /// </summary>
    /// <param name="delimiter">The delimiter between fields. Default is comma (',').</param>
    /// <returns>Instance of self for method chaining.</returns>
    public Serialiser WithFieldDelimiter(char delimiter = ',')
    {
        _fieldDelimiter = delimiter;
        return this;
    }

    /// <summary>
    /// Sets the record delimiter. The default is CRLF ("\r\n") and this function does not need to be 
    /// called to set it to CRLF ("\r\n").
    /// NOTE: In the context of this Serialiser, delimiters are used only during serialisation (in the Serialise... methods) 
    /// to determine if content including the delimiter needs to be escaped or enclosed in quotes.
    /// </summary>
    /// <param name="delimiter">The delimiter between records. Default is CRLF ("\r\n").</param>
    /// <returns>Instance of self for method chaining.</returns>
    public Serialiser WithRecordDelimiter(string delimiter = "\r\n")
    {
        _recordDelimiter = delimiter;
        return this;
    }

    /// <summary>
    /// Create a serialiser instance for the provided <typeparamref name="T"/> type.
    /// </summary>
    /// <typeparam name="T">Type to create the serialiser for.</typeparam>
    /// <returns>Instantiated serialiser.</returns>
    public static Serialiser For<T>()
        => For(typeof(T));

    /// <summary>
    /// Create a serialiser instance for the provided <paramref name="type"/> type.
    /// </summary>
    /// <param name="type">Type to create the serialiser for.</param>
    /// <returns>Instantiated serialiser.</returns>
    public static Serialiser For(Type type)
        => new Serialiser(type);

    /// <summary>
    /// Instantiate the serialiser
    /// </summary>
    /// <param name="type">Type to create the serialiser for.</param>
    private Serialiser(Type type)
    {
        TypeDiscoveryOptions options = new TypeDiscoveryOptions()
        {
            PersistenceContainerAttributeRestriction = typeof(Flatfile),
            PersistenceContainerMemberAttributeRestriction = typeof(FlatfileField)
        };

        PersistenceContainerInfo? container;
        if (!TypeDiscoveryFactory.TryResolve(type, out container, options))
        {
            throw new InvalidOperationException($"The type '{type.GetUsableTypeName()}' could not be discovered.");
        }

        _container = container;

        // options above makes sure that we get this.
        Flatfile.FieldReferencesAre referenceMode = ((Flatfile)container.PersistenceInfo).FieldReferenceMode;

        Dictionary<uint, string> orderedNames = new Dictionary<uint, string>();
        foreach (PersistenceContainerMemberInfo member in container.Members)
        {
            switch (referenceMode)
            {
                case Flatfile.FieldReferencesAre.Indexes when member.PersistenceInfo is FlatfileField indexed:
                    if (!orderedNames.ContainsKey(indexed.Position))
                    {
                        orderedNames.Add(indexed.Position, member.Member.Name);
                    }
                    break;

                case Flatfile.FieldReferencesAre.Names when member.PersistenceInfo is FlatfileNamedField named:
                    if (!orderedNames.ContainsKey(named.Position))
                    {
                        orderedNames.Add(named.Position, named.TableFieldName);
                    }
                    break;

                default:
                    throw new InvalidOperationException($"Mismatch between 'FieldReferenceMode' on entity's 'Flatfile' attribute value and actual attribute found on member '{member.Member.Name}' (type: '{container.EntityType.GetUsableTypeName()}').");
            }
        }

        _serialisableHeaders = Array.Empty<string>();

        if (orderedNames.Count == 0)
        {
            _headers = Array.Empty<string>();
        }
        else
        {
            uint min = orderedNames.Keys.Min();
            uint expectedCount = orderedNames.Keys.Max() - min + 1;

            _headers = new string[expectedCount];
            if (referenceMode is Flatfile.FieldReferencesAre.Names)
            {
                _serialisableHeaders = new string[expectedCount];
            }

            for (uint i = 0; i < expectedCount; i++)
            {
                uint position = min + i;
                if (orderedNames.ContainsKey(position))
                {
                    _headers[i] = orderedNames[position];
                    if (referenceMode is Flatfile.FieldReferencesAre.Names)
                    {
                        _serialisableHeaders[i] = $"\"{orderedNames[position]}\"";
                    }
                }
                else
                {
                    _headers[i] = string.Empty;
                    if (referenceMode is Flatfile.FieldReferencesAre.Names)
                    {
                        _serialisableHeaders[i] = QUOTED_EMPTY_STRING;
                    }
                }
            }
        }
    }

    #endregion

    // Having this as 'internal' enables TokenLimitedFileContext to reuse it.
    internal readonly PersistenceContainerInfo _container;
    private string[] _headers, _serialisableHeaders;

    private char _fieldDelimiter = ',';
    private string _recordDelimiter = "\r\n";

    private const string QUOTED_EMPTY_STRING = "\"\"";


}
