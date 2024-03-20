using System.Collections.Generic;

namespace MarkovSharp.Components;

public interface IUnigramSelector<TUnigram>
{
	TUnigram SelectUnigram(IReadOnlyList<TUnigram> ngrams);
}
