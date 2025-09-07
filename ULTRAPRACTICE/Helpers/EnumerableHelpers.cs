using System;
using System.Collections.ObjectModel;

namespace ULTRAPRACTICE.Helpers;

public static class EnumerableHelpers
{
    public static ReadOnlyCollection<T> AsReadOnly<T>(this T[] array)
    {
        return Array.AsReadOnly(array);
    }
}