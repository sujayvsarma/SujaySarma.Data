using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SujaySarma.Data.Azure.Tables
{
    /// <summary>
    /// Extension methods to make life easy when using <see cref="AzureTablesContext"/>
    /// </summary>
    public static class AzureTablesContextExtensions
    {

        /// <summary>
        /// Build an <see cref="IAsyncEnumerable{T}"/> into a <see cref="List{T}"/>
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="e">Instance of <see cref="IAsyncEnumerable{T}"/></param>
        /// <returns><see cref="List{T}"/> with information or an empty list</returns>
        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> e)
            where T : class
        {
            List<T> list = new();
            await foreach(T item in e)
            {
                list.Add(item);
            }

            return list;
        }


        /// <summary>
        /// Test to see if <see cref="IAsyncEnumerable{T}"/> contains any values that match the provided <paramref name="validation"/>. 
        /// If no <paramref name="validation"/> is provided, then we test if the sequence contains any non-null and non-default values.
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="e">Instance of <see cref="IAsyncEnumerable{T}"/></param>
        /// <param name="validation">The check to perform on each item of <paramref name="e"/></param>
        /// <returns>'True' if sequence contains a non-null and non-default value or passes the <paramref name="validation"/></returns>
        public static async Task<bool> AnyAsync<T>(this IAsyncEnumerable<T> e, Predicate<T>? validation = null)
            where T : class
        {
            validation ??= (t) => ((t != null) && (t != default));
            IAsyncEnumerator<T> en = e.GetAsyncEnumerator();
            while (await en.MoveNextAsync())
            {
                if (validation(en.Current))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
