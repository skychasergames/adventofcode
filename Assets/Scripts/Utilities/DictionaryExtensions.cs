using System.Collections;
using System.Collections.Generic;

public static class DictionaryExtensions
{
	public static void AddIfUnique<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
	{
		if (!dict.ContainsKey(key))
		{
			dict.Add(key, value);
		}
	}
}
