using System.Collections.Generic;
using System.Linq;

namespace MarkovSharp.Components;

public class AlphabeticUnigramSelectorDesc<T> : IUnigramSelector<T>
{
	public T SelectUnigram(IReadOnlyList<T> ngrams) => ngrams.Max();
}
