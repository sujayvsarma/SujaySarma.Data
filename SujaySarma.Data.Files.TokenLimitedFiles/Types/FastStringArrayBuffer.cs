using System;

namespace SujaySarma.Data.Files.TokenLimitedFiles.Types;

/// <summary>
/// A buffer backed by a string array, optimised for performance.
/// </summary>
internal sealed class FastStringArrayBuffer : IDisposable
{
    /// <summary>
    /// Current length of the buffer.
    /// </summary>
    public int Length
        => _insertionPointer;

    /// <summary>
    /// Retrieve the collection of values stored in the buffer.
    /// </summary>
    /// <returns>The collection of values as a <see cref="string"/> array.</returns>
    public string[] ToStringArray()
    {
        int len = Length;
        return ((len is 0) ? Array.Empty<string>() : _buffer[0..len]);
    }

    /// <summary>
    /// Append a string to the buffer. If we have run out of capacity, 
    /// the backing array is resized (by '_increaseCapacityBy') and then 
    /// the string is inserted.
    /// </summary>
    /// <param name="value">string to append.</param>
    /// <returns>Instance of self for method chaining.</returns>
    public FastStringArrayBuffer Append(string value)
    {
        if ((_insertionPointer + 1) > _buffer.Length)
        {
            Array.Resize<string>(ref _buffer, EXPANDTO());
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
    /// Initialise the buffer.
    /// </summary>
    public FastStringArrayBuffer()
    {
        _buffer = new string[CAPACITY];
        _insertionPointer = 0;
    }

    private int EXPANDTO() => (_buffer.Length + CAPACITY);

    private string[] _buffer = Array.Empty<string>();
    private int _insertionPointer;
    private const int CAPACITY = 48;


    private bool _isDisposed = false;
    public void Dispose()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;

            int generation = GC.GetGeneration(_buffer);
            _buffer = null!;
            GC.Collect(generation, GCCollectionMode.Forced);
        }
    }

}
