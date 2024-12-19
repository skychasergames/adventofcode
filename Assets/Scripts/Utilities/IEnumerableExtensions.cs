using System;
using System.Collections;
using System.Collections.Generic;

public static class IEnumerableExtensions
{
	public static ulong Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, ulong> selector)
	{
		ulong total = 0;

		foreach (var item in source)
		{
			total += selector(item);
		}

		return total;
	}
}
