using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GCPTestAPI.Controllers
{
    public static class Extensions
    {
        public static async Task<IEnumerable<TResult>> SelectManyAsync<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, Task<IEnumerable<TResult>>> selector)
            => (await Task.WhenAll(source.Select(selector))).SelectMany(e => e);

        public static string DecodePath(this string path) => path.Replace("%2F", "/");

        public static DateTime? Convert(this Timestamp? timestamp) =>
            timestamp.HasValue
            ? (DateTime?)timestamp.Value.ToDateTime()
            : null;
    }
}
