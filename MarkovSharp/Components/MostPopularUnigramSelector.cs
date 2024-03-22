using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MarkovSharp.Components;

public class MostPopularUnigramSelector<T> : IUnigramSelector<T>
{
	public T SelectUnigram(IReadOnlyList<T> ngrams)
	{
		if (ngrams is not { Count: > 0 })
		{
			return default;
		}
		Dictionary<T, int> counts = [];
		foreach (var ngram in ngrams)
		{
			ref var cnt = ref CollectionsMarshal.GetValueRefOrAddDefault(counts, ngram, out _);
			cnt++;
		}
		return counts.MaxBy(x => x.Value).Key;
	}
}
