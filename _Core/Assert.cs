using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Phuntasia
{
    public static class Assert
    {
        [Conditional("DEBUG")]
        public static void Compare<T>(T value, T assertion)
            where T : IComparable
        {
            if (!EqualityComparer<T>.Default.Equals(value, assertion))
            {
                throw new AssertionException($"{value} is not {assertion}.");
            }
        }
    }
}