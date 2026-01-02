using System;

namespace SujaySarma.Data.Files.TokenLimitedFiles.Types;

/// <summary>
/// A buffer backed by a character array, optimised for performance.
/// </summary>
internal sealed class FastCharacterArrayBuffer : IDisposable
{
    /// <summary>
    /// Current length of the buffer.
    /// </summary>
    public int Length
        => _insertionPointer;

    /// <summary>
    /// Returns the character at the provided <paramref name="index"/>.
    /// </summary>
    /// <param name="index">Index of character in buffer to examine/return.</param>
    /// <returns>Character at the provided <paramref name="index"/>.</returns>
    public char this[int index]
    {
        get
        {
            if ((index < 0) || (index >= Length))
            {
                throw new IndexOutOfRangeException($"'{nameof(index)}' must lie between '0' and current length '{Length}'.");
            }

            return _buffer[index];
        }
    }


    /// <summary>
    /// Returns the value as a string.
    /// </summary>
    /// <returns>String value containing the entire contents of this buffer.</returns>
    public override string ToString()
    {
        return ((Length is 0) ? string.Empty : new string(_buffer, 0, Length));
    }

    /// <summary>
    /// Append a character to the buffer. If we have run out of capacity, 
    /// the backing array is resized (by '_increaseCapacityBy') and then 
    /// the character is inserted.
    /// </summary>
    /// <param name="value">Character to append.</param>
    /// <returns>Instance of self for method chaining.</returns>
    public FastCharacterArrayBuffer Append(char value)
    {
        if ((_insertionPointer + 1) > _buffer.Length)
        {
            Array.Resize<char>(ref _buffer, EXPANDTO());
        }

        _buffer[_insertionPointer++] = value;
        return this;
    }

    /// <summary>
    /// Only resets the pointer, retaining current memory allocation.
    /// </summary>
    public void Clear()
    {
        _insertionPointer = 0;
    }

    /// <summary>
    /// Initialises a character array buffer with a 1024 element capacity.
    /// </summary>
    public FastCharacterArrayBuffer()
    {
        _buffer = new char[CAPACITY];
        _insertionPointer = 0;
    }


    private int EXPANDTO() => (_buffer.Length + CAPACITY);

    private char[] _buffer = default!;
    private int _insertionPointer = 0;

    // 1,024 chars (2,048 bytes = 2KB, chars in .NET are UTF-16) is sufficient for most purposes:
    // in our case, reading FIELDS from csv data!
    // This is also neatly aligned.
    private const int CAPACITY =  1024;


    private bool _isDisposed = false;
    public void Dispose()
    {
        if (! _isDisposed)
        {
            _isDisposed = true;
            _buffer = null!;
        }
    }
}
