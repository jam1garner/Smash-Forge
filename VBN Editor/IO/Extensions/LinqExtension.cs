// Copyright (c) Sammi Husky. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System
{
    public static class LinqExtension
    {
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunks)
        {
            var count = source.Count();

            return source.Select((x, i) => new { value = x, index = i })
                .GroupBy(x => x.index / (int)Math.Ceiling(count / (double)chunks))
                .Select(x => x.Select(z => z.value).ToArray()).ToArray();
        }
    }
}
