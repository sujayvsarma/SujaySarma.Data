namespace SujaySarma.Data.Files.TokenLimitedFiles.Constants;

/// <summary>
/// The reason why a read operation exited.
/// </summary>
public enum ReaderExitReason
{
    /// <summary>
    /// A field delimiter was encountered.
    /// </summary>
    FieldDelimiterEncountered = 0,

    /// <summary>
    /// A record delimiter was encountered.
    /// </summary>
    RecordDelimiterEncountered,

    /// <summary>
    /// A blank line was encountered.
    /// </summary>
    BlankLineEncountered,

    /// <summary>
    /// An explicit NULL character in the stream was encountered.
    /// </summary>
    InContentNullCharacter,

    /// <summary>
    /// An end of file or stream was encountered.
    /// </summary>
    EndOfFileOrStream,

    /// <summary>
    /// An error condition.
    /// </summary>
    Error
}

/// <summary>
/// Extension methods to deal with returned values using <see cref="ReaderExitReason"/> enumeration.
/// </summary>
public static class ReaderExitReasonExtensions
{

    /// <summary>
    /// Returns if the provided reason is a "normal" exit while reading a field.
    /// </summary>
    /// <param name="reason">Reason to test.</param>
    /// <returns>True if it is a "normal" exit.</returns>
    public static bool IsNormalFieldExit(this ReaderExitReason reason)
        => (reason is ReaderExitReason.FieldDelimiterEncountered or ReaderExitReason.RecordDelimiterEncountered or ReaderExitReason.EndOfFileOrStream);

    /// <summary>
    /// Returns if the provided reason is a "normal" exit while reading a record.
    /// </summary>
    /// <param name="reason">Reason to test.</param>
    /// <returns>True if it is a "normal" exit.</returns>
    public static bool IsNormalRecordExit(this ReaderExitReason reason)
        => (reason is ReaderExitReason.RecordDelimiterEncountered or ReaderExitReason.EndOfFileOrStream);

    /// <summary>
    /// Returns if the provided reason is an abnormal or error condition while any reading operation.
    /// </summary>
    /// <param name="reason">Reason to test.</param>
    /// <returns>True if it is an abnormal or error condition exit.</returns>
    public static bool IsError(this ReaderExitReason reason)
        => (reason is ReaderExitReason.Error or ReaderExitReason.BlankLineEncountered or ReaderExitReason.InContentNullCharacter);


}
