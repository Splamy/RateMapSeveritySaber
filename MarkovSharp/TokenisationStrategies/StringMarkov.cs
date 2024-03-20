using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace MarkovSharp.TokenisationStrategies;

public class StringMarkov(ILogger<StringMarkov> logger, int level = 2) : GenericMarkov<string, string>(logger, level)
{
	public override IEnumerable<string> SplitTokens(string input)
	{
		if (input == null)
		{
			return [GetPrepadUnigram()];
		}

		input = input.Trim();
		return input.Split(' ');
	}

	public override string RebuildPhrase(IEnumerable<string> tokens)
	{
		return string.Join(" ", tokens);
	}

	public override string GetTerminatorUnigram()
	{
		return null;
	}

	public override string GetPrepadUnigram()
	{
		return "";
	}
}
