using System;
using System.Collections.Generic;
using System.Linq;

public static class Extension
{
	public static IEnumerable<TSource> ExceptBy<TSource, TKey>(
	this IEnumerable<TSource> source,
	IEnumerable<TSource> other,
	Func<TSource, TKey> keySelector)
	{
		var set = new HashSet<TKey>(other.Select(keySelector));
		foreach (var item in source)
			if (set.Add(keySelector(item)))
				yield return item;
	}
}
