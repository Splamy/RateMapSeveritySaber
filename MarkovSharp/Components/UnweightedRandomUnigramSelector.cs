using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkovSharp.Components;

public class UnweightedRandomUnigramSelector<T> : IUnigramSelector<T>
{
	public T SelectUnigram(IReadOnlyList<T> ngrams)
	{
		if (ngrams is not { Count: > 0 })
		{
			return default;
		}
		var distinctNgrams = ngrams.Distinct().ToArray();
		return distinctNgrams[Random.Shared.Next(0, distinctNgrams.Length)];
	}
}
