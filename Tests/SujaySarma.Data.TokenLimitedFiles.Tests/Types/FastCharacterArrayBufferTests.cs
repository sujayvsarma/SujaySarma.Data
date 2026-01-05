using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.TokenLimitedFiles.Types;

namespace SujaySarma.Data.TokenLimitedFiles.Tests.Types;

[TestClass]
public sealed class FastCharacterArrayBufferTests
{

    [TestMethod(DisplayName = "FastCharacterArrayBuffer: Basic functionality")]
    [TestCategory("Functional")]
    public void BasicFunctionality()
    {
        using FastCharacterArrayBuffer buffer = new FastCharacterArrayBuffer();

        Assert.AreEqual(0, buffer.Length, $"buffer.Length: Expected: [0], Actual: [{buffer.Length}]");

        buffer.Append('H').Append('e').Append('l').Append('l').Append('o');
        Assert.AreEqual(5, buffer.Length, $"buffer.Length: Expected: [5], Actual: [{buffer.Length}]");

        string value = buffer.ToString();
        Assert.AreEqual("Hello", value, $"buffer.ToString(): Expected: [Hello], Actual: [{value}]");

        buffer.Clear();
        value = buffer.ToString();

        Assert.AreEqual(0, buffer.Length, $"buffer.Length: Expected: [0], Actual: [{buffer.Length}]");        
        Assert.AreEqual(string.Empty, value, $"buffer.ToString(): Expected: [], Actual: [{value}]");
    }


    [TestMethod(DisplayName = "FastCharacterArrayBuffer: Insertion throughput (chars/ms)")]
    [TestCategory("Performance")]
    public void InsersionThroughput()
    {
        const int CHARS = 100_000; // Reduced from 4096 * 1024
        StringBuilder charBuffer = new StringBuilder(CHARS);
        Random random = new Random();
        for (int i = 0; i < CHARS; i++)
        {
            int r = random.Next(0, 25);
            charBuffer.Append((char)('A' + r));
        }

        using FastCharacterArrayBuffer buffer = new FastCharacterArrayBuffer();
        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < CHARS; i++)
        {
            buffer.Append(charBuffer[i]);
        }
        sw.Stop();

        Console.WriteLine($"Throughput: Writing {CHARS:N0} chars to buffer: {CHARS/sw.ElapsedMilliseconds:F2} chars/ms");
    }

    [TestMethod(DisplayName = "FastCharacterArrayBuffer: Memory usage (bytes/char)")]
    [TestCategory("Performance")]
    public void MemoryUsage()
    {
        const int CHARS = 100_000; // Reduced from 4096 * 1024
        Random random = new Random();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        using FastCharacterArrayBuffer buffer = new FastCharacterArrayBuffer();
        long initial = GC.GetTotalMemory(false);
        
        for (int i = 0; i < CHARS; i++)
        {
            int r = random.Next(0, 25);
            buffer.Append((char)('A' + r));
        }
        
        long final = GC.GetTotalMemory(false);
        long memoryUsed = final - initial;

        Console.WriteLine($"Memory pressure: {CHARS:N0} chars: {memoryUsed/CHARS:F2} bytes/char");
    }


    [TestMethod(DisplayName = "FastCharacterArrayBuffer: Memory usage - Repeated Clear/Fill, constant data size")]
    [TestCategory("Performance")]
    public void MemoryUsageWithRepeatedReuseWithConstantDataSize()
    {
        const int CHARS = 4096;
        const int ITERATIONS = 100_000;
        Random random = new Random();
        List<long> memoryUsage = new List<long>();

        long overhead = GC.GetTotalMemory(true);

        using FastCharacterArrayBuffer buffer = new FastCharacterArrayBuffer();
        long initial = GC.GetTotalMemory(false) - overhead;

        for (int iteration = 0; iteration < ITERATIONS; iteration++)
        {
            buffer.Clear();
            for (int i = 0; i < CHARS; i++)
            {
                int r = random.Next(0, 25);
                buffer.Append((char)('A' + r));
            }

            long final = GC.GetTotalMemory(false);
            long memoryUsed = final - initial;
            memoryUsage.Add(memoryUsed);
        }

        Console.WriteLine($"Iterations: === {ITERATIONS:N0} ===");
        Console.WriteLine($"Maximum memory: {memoryUsage.Max() / 1024.0:F2} bytes");
        Console.WriteLine($"Minimum memory: {memoryUsage.Min() / 1024.0:F2} bytes");
        Console.WriteLine($"Average memory: {memoryUsage.Average() / 1024.0:F2} bytes");

    }

    [TestMethod(DisplayName = "FastCharacterArrayBuffer: Memory usage - Repeated Clear/Fill, varying data size")]
    [TestCategory("Performance")]
    public void MemoryUsageWithRepeatedReuseWithVaryingDataSize()
    {
        const int MAX_CHARS = 4096;
        const int ITERATIONS = 100_000;
        Random random = new Random();
        Dictionary<int, double> bytesPerChar = new Dictionary<int, double>();

        long overhead = GC.GetTotalMemory(true);

        using FastCharacterArrayBuffer buffer = new FastCharacterArrayBuffer();
        long initial = GC.GetTotalMemory(false) - overhead;

        for (int iteration = 0; iteration < ITERATIONS; iteration++)
        {
            buffer.Clear();

            int maxCharsForIteration = 0, tries = 0;
            do
            {
                maxCharsForIteration = random.Next(0, MAX_CHARS);

            } while ((tries++ < 10) && bytesPerChar.ContainsKey(maxCharsForIteration));

            if (bytesPerChar.ContainsKey(maxCharsForIteration))
            {
                Console.WriteLine($"Terminating at iteration {iteration} due to lack of new chars/iteration generation.");
                break;
            }

            if (maxCharsForIteration != 0)
            {
                for (int i = 0; i < maxCharsForIteration; i++)
                {
                    int r = random.Next(0, 25);
                    buffer.Append((char)('A' + r));
                }

                long final = GC.GetTotalMemory(false);
                long memoryUsed = final - initial;

                bytesPerChar.Add(maxCharsForIteration, memoryUsed);
            }
        }

        Console.WriteLine($"Iterations: === {ITERATIONS:N0} ===");

        double maxMem = bytesPerChar.Max(b => b.Value);
        int maxMemChars = bytesPerChar.Where(b => (b.Value == maxMem)).Select(b => b.Key).First();
        
        int maxChars = bytesPerChar.Max(b => b.Key);
        double maxCharsMem = bytesPerChar.Where(b => (b.Key == maxChars)).Select(b => b.Value).First();

        double minMem = bytesPerChar.Min(b => b.Value);
        int minMemChars = bytesPerChar.Where(b => (b.Value == minMem)).Select(b => b.Key).First();

        int minChars = bytesPerChar.Min(b => b.Key);
        double minCharsMem = bytesPerChar.Where(b => (b.Key == minChars)).Select(b => b.Value).First();

        Console.WriteLine($"Minimum chars written: {minChars:N0} chars @ {minCharsMem/1024.0:F2} bytes");
        Console.WriteLine($"Maximum chars written: {maxChars:N0} chars @ {maxCharsMem/1024.0:F2} bytes");
        Console.WriteLine($"Minimum memory used: {minMem/1024.0:F2} bytes @ {minMemChars:N0} chars");
        Console.WriteLine($"Maximum memory used: {maxMem/1024.0:F2} bytes @ {maxMemChars:N0} chars");

    }

}
