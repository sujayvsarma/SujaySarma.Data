﻿using System;
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
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="e">Instance of <see cref="IAsyncEnumerable{T}"/></param>
        /// <returns><see cref="List{T}"/> with information or an empty list</returns>
        public static async Task<List<TObject>> ToListAsync<TObject>(this IAsyncEnumerable<TObject> e)
        {
            List<TObject> list = new List<TObject>();
            await foreach(TObject item in e)
            {
                list.Add(item);
            }

            return list;
        }


        /// <summary>
        /// Test to see if <see cref="IAsyncEnumerable{T}"/> contains any values that match the provided <paramref name="validation"/>. 
        /// If no <paramref name="validation"/> is provided, then we test if the sequence contains any non-null and non-default values.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="e">Instance of <see cref="IAsyncEnumerable{T}"/></param>
        /// <param name="validation">The check to perform on each item of <paramref name="e"/></param>
        /// <returns>'True' if sequence contains a non-null and non-default value or passes the <paramref name="validation"/></returns>
        public static async Task<bool> AnyAsync<TObject>(this IAsyncEnumerable<TObject> e, Predicate<TObject>? validation = null)
        {
            validation ??= ((t) => ((t != null) ? true : false));

            IAsyncEnumerator<TObject> en = e.GetAsyncEnumerator();
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
