﻿using System;

namespace SujaySarma.Data.Core
{
    /// <summary>
    /// An utility-belt class containing library functions used in this library and other consumer libraries. 
    /// May also be used by consuming applications.
    /// </summary>
    public static class Utility
    {

        /// <summary>
        /// Checks if the provided type is a numeric type. Numeric types are: int, short, long, etc. including their 
        /// unsigned versions.
        /// </summary>
        /// <param name="obj">Non-Null instance of object to check</param>
        /// <returns>True if is a numeric type</returns>
        public static bool IsNumericType(this object obj)
            => ((obj == null) ? throw new NullReferenceException($"The parameter '{nameof(obj)}' cannot be Null.") : IsNumericType(obj.GetType()));


        /// <summary>
        /// Checks if the provided type is a numeric type. Numeric types are: int, short, long, etc. including their 
        /// unsigned versions.
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>True if is a numeric type</returns>
        public static bool IsNumericType(this Type type)
        {
            if ((type == typeof(sbyte)) || (type == typeof(byte)))
            {
                return true;
            }

            if ((type == typeof(short)) || (type == typeof(ushort)))
            {
                return true;
            }

            if ((type == typeof(int)) || (type == typeof(uint)))
            {
                return true;
            }

            if ((type == typeof(long)) || (type == typeof(ulong)))
            {
                return true;
            }

            if ((type == typeof(Single)) || (type == typeof(float)) || (type == typeof(decimal)) || (type == typeof(double)))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Throw the <see cref="ObjectDisposedException"/> if the object has been previously disposed.
        /// </summary>
        /// <param name="obj">Object to throw the exception on</param>
        /// <param name="isDisposed">Value of the internally maintained 'isDisposed' flag</param>
        /// <param name="nameOfDisposedObject">Class-name of the disposable object</param>
        public static void ThrowIfDisposed(this object obj, bool isDisposed, string nameOfDisposedObject)
        {
#if NET7_0_OR_GREATER
            ObjectDisposedException.ThrowIf((isDisposed == true), obj);
#else
            if (isDisposed)
            {
                throw new ObjectDisposedException(nameOfDisposedObject);
            }
#endif
        }

    }
}
