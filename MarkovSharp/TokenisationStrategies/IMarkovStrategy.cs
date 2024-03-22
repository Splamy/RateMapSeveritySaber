using MarkovSharp.Models;
using System;
using System.Collections.Generic;

namespace MarkovSharp.TokenisationStrategies;

public interface IMarkovStrategy<TPhrase, TUnigram>
{
	Type UnigramType { get; }

	Type PhraseType { get; }

	IEnumerable<TUnigram> SplitTokens(TPhrase input);

	TPhrase RebuildPhrase(IEnumerable<TUnigram> tokens);

	void Learn(IReadOnlyCollection<TPhrase> phrases, bool ignoreAlreadyLearnt = true);

	void Learn(TPhrase phrase);

	void Retrain(int newLevel);

	IEnumerable<TPhrase> Walk(int lines = 1, TPhrase seed = default);

	List<TUnigram> GetMatches(TPhrase input);

	TUnigram GetTerminatorUnigram();

	TUnigram GetPrepadUnigram();

	IEnumerable<StateStatistic<TUnigram>> GetStatistics();

	ChainPhraseProbability<TPhrase> GetFit(TPhrase test);

	double GetTransitionProbabilityUnigram(TPhrase currentState, TUnigram nextStates);

	double GetTransitionProbabilityPhrase(TPhrase currentState, TPhrase nextStates);
}
