using System;
using System.Collections.Generic;

namespace MarkovSharp.Components;

public class WeightedRandomUnigramSelector<T> : IUnigramSelector<T>
{
	public T SelectUnigram(IReadOnlyList<T> ngrams)
	{
		var index = Random.Shared.Next(0, ngrams.Count);
		return ngrams[index];
	}
}
