using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;

namespace MarkovSharp.Models;

public class NgramContainer<T>
{
	internal ImmutableArray<T> Ngrams { get; }

	public NgramContainer(IEnumerable<T> args)
	{
		Ngrams = [.. args.Order()];
	}

	public override bool Equals(object o)
	{
		if (o is NgramContainer<T> testObj)
		{
			return Ngrams.SequenceEqual(testObj.Ngrams);
		}

		return false;
	}

	public override int GetHashCode()
	{
		unchecked
		{
			int hash = 17;
			var defaultVal = default(T);
			foreach (var member in Ngrams.Where(a => a != null && !a.Equals(defaultVal)))
			{
				hash = hash * 23 + member.GetHashCode();
			}
			return hash;
		}
	}

	public override string ToString()
	{
		return JsonSerializer.Serialize(Ngrams);
	}
}
