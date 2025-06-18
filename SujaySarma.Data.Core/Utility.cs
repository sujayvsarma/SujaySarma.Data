using System;

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
        {
            return ((obj != null) ? obj.GetType().IsNumericType() : throw new NullReferenceException("The parameter 'obj' cannot be Null."));
        }

        /// <summary>
        /// Checks if the provided type is a numeric type. Numeric types are: int, short, long, etc. including their
        /// unsigned versions.
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>True if is a numeric type</returns>
        public static bool IsNumericType(this Type type)
        {
            return Type.Equals(type, typeof(sbyte)) 
                || Type.Equals(type, typeof(byte)) 
                || Type.Equals(type, typeof(short)) 
                || Type.Equals(type, typeof(ushort)) 
                || Type.Equals(type, typeof(int)) 
                || Type.Equals(type, typeof(uint)) 
                || Type.Equals(type, typeof(long)) 
                || Type.Equals(type, typeof(ulong)) 
                || Type.Equals(type, typeof(float)) 
                || Type.Equals(type, typeof(float)) 
                || Type.Equals(type, typeof(Decimal)) 
                || Type.Equals(type, typeof(double));
        }

        /// <summary>
        /// Throw the <see cref="ObjectDisposedException" /> if the object has been previously disposed.
        /// </summary>
        /// <param name="obj">Object to throw the exception on</param>
        /// <param name="isDisposed">Value of the internally maintained 'isDisposed' flag</param>
        /// <param name="nameOfDisposedObject">Class-name of the disposable object</param>
        public static void ThrowIfDisposed(this object obj, bool isDisposed, string nameOfDisposedObject)
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(isDisposed, obj);
#else
            if (isDisposed)
            {
                throw new ObjectDisposedException(nameOfDisposedObject);
            }
#endif
        }
    }

}
