using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

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
		var index = Random.Shared.Next(0, distinctNgrams.Length);
		return distinctNgrams[index];
	}
}
