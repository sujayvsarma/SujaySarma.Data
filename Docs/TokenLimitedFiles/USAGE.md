
You can use the library to read comma, tab, space or other delimited text files from either local disk, network share, a HTTP(s) URL to an FTP(s) URL. At this time, the HTTP(s)/FTP(s) sources do not support authentication (that is, I have not yet added support for you to be able to pass in any sort of auth token, etc) -- that support will be added either when a sufficient number of people request it, or I end up needing it for some project, which ever may come first.

#### Preparing your business classes for use with `TokenLimitedFileReader` and `TokenLimitedFileWriter`
You need to anotate your class properties/fields with the `FileField` attribute. 

```
// the attribute is defined in the SujaySarma.Data.Files.TokenLimitedFiles.Attributes namespace:
using SujaySarma.Data.Files.TokenLimitedFiles.Attributes;

public class FooType 
{
    [FielField(0)]
    public int Id { get; private set; }

    [FileField("name")]
    public string Name { get; set; }

    [FileField(2, "internal_type")]
    private TypeId _internalTypeId;

    public bool NotReadOrWritten { get; set; }
}
```

**Index or Name**: The attribute requires at least an index or a name. The Index is a zero-based index of the column element in the file the data is read from or written to. If the file has a header, the name is the value that will appear for this element in the header row. If **both** the index and the name are given, the code will prefer the index. 

**Scope of properties and fields:** Neither the TokenLimitedFileReader nor the TokenLimitedFileWriter cares if your properties/fields are `public`, `private` or whatever. It also does not care about the visibility of its `get`/`set` methods. 

However, you must implement:

- `get` if you want to use that property with the `TokenLimitedFileWriter`
- `set` if you want to use that property with the `TokenLimitedFileReader`

**Fields:** A class field can always be read from. But it can only be written to if it is **not** set as `readonly`.

>**Column order handling by `TokenLimitedFileWriter`:**
> The `TokenLimitedFileWriter` enumerates the class properties/fields that have the column index defined. These are written in the ascending order of the column index (the index value itself is not considered). Then other properties/fields that have no index, but have a name defined are written -- these are written in whatever order .NET Reflection hands them to the method.
> Therefore, if you require the columns to be in a specific order, make sure you define the index value in the `FielField` attribute annotation.

#### To read a delimited file

Use the class `TokenLimitedFileReader` and it's methods. You may either create an instance of the class using one of its constructors and then use its `ReadRow` method to get the data. This will give you the data as `string` arrays:

```
using TokenLimitedFileReader reader = new(path, encoding, autoDetectEncoding, leaveStreamOpen);
DataTable table = new("Table 1");

while (!reader.EndOfStream)
{
    int count = reader.ReadRow();
    if (count > -1)
    {
        if (hasHeaderRow && (reader.RowCount == headerRowIndex))
        {
            for (int i = 0; i < reader.LastRowRead!.Length; i++)
            {
                table.Columns.Add((string.IsNullOrWhiteSpace(reader.LastRowRead[i]) ? $"Column {i}" : reader.LastRowRead[i]));
            }

            continue;
        }

        DataRow newRow = table.NewRow();
        for (int i = 0; i < reader.LastRowRead!.Length; i++)
        {
            try
            {
                newRow[i] = reader.LastRowRead[i];
            }
            catch
            {
                throw new InvalidCastException($"Column {i} ('{table.Columns[i].ColumnName}') expects a '{table.Columns[i].DataType.Name}' type value.");
            }
        }
        table.Rows.Add(newRow);
    }
}

// use *table*
```

As you see above, `ReadRow` does not return the row itself. The row is populated in the `TokenLimitedFileReader` instance's `LastRowRead` property. The `ReadRow` method will return the number of fields read -- if this is `-1`, then nothing was read.

**Alternately**, you may use the provided `static` methods to achieve the same result in a performance-tuned way:

```
DataTable table = TokenLimitedFileReader.GetTable(filePath);
```

**Other than tables**, `TokenLimitedFileReader` can also return `IEnumerable<T>` or `List<T>` data. When using these methods, you can provide an additional parameter:

```
... Action<DataTable>? action
```

Pass a named or anonymous method/delegate to this parameter. This function will be called after reading in the data (into a `DataTable` as with the `GetTable`) but before re-serializing it into the `IEnumerable<T>` or `List<T>`. The function will be passed a single parameter of type `DataTable` (fully populated), and expects no return / cancellation tokens / etc. A typical use I use this delegate for is to modify some read-in values to standardize them (eg: turn "no" values in a purported `Boolean` field to "false", etc).

#### To write a delimited file

The writer will write files always in `Create` (no file exists) or `Truncate` (file exists) mode! As with the `TokenLimitedFileReader`, the `TokenLimitedFileWriter` has instance methods that you can use for finer flow control. Else, you can use the provided `static` methods to write the records either from `DataTable`s or `IEnumerable<T>` or `List<T>`.

```
ulong records = TokenLimitedFileWriter.WriteRecords(table, export1, quoteAllStrings: false);

// OR

ulong records = TokenLimitedFileWriter.WriteRecords<Airport>(list, export2, quoteAllStrings: false);

```

#### Performance

![image](https://user-images.githubusercontent.com/7371750/170833798-e909518d-987f-4adf-a0dc-3ad9f33118c8.png)

The above test was conducted on a comma-seperated (Internet-downloaded) flatfile containing 55,485 rows with headers in the first row. As you can see, the library provides < 0.5 second load times in all cases.

----
If you find a bug, unsupported feature or have a feature request, please file an issue.

