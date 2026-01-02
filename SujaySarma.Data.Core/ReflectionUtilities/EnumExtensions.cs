using System;
using System.Collections.Generic;

namespace SujaySarma.Data.Core.ReflectionUtilities;

/// <summary>
/// Extensions for Enums
/// </summary>
public static class EnumExtensions
{

    /// <summary>
    /// Test if the provided 'flag' is a power-of-2 flag 
    /// (a real value from the provided enumeration).
    /// </summary>
    /// <param name="value">Enum value to check.</param>
    /// <returns>True if the value is a real flag.</returns>
    public static bool IsSingleBitFlag(this Enum value)
    {
        int intValue = Convert.ToInt32(value);

#if NET7_0_OR_GREATER
        return int.IsPow2(intValue);
#else
        return ((intValue is 0) || ((intValue & (intValue - 1)) == 0));
#endif
    }

    /// <summary>
    /// The value of <paramref name="hint"/> may be an OR'ed composite of multiple hints. 
    /// Pull it out into individual values.
    /// </summary>
    /// <param name="hint">The hint to decompose.</param>
    /// <param name="includeZeroValueFlag">When TRUE, includes the flag with the zero (0) value. If the zero valued enum instance is meant only 
    /// to indicate the flag has not been set then this should be FALSE. Only set it to TRUE if zero is a valid flag value other than meaning "not set").</param>
    /// <returns>A list of individual hints.</returns>
    public static List<T> GetIndividualFlags<T>(this T hint, bool includeZeroValueFlag = false)
        where T : struct, Enum
    {
        List<T> list = new List<T>();
        foreach (T flag in Enum.GetValues<T>())
        {
            if ((flag is 0) && (!includeZeroValueFlag))
            {
                continue;
            }

            if (flag.IsSingleBitFlag() && hint.HasFlag(flag) && (! list.Contains(flag)))
            {
                list.Add(flag);
            }
        }

        return list;
    }

}
