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
		Ngrams = [.. args];
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
		var hash = new HashCode();

		foreach (var item in Ngrams)
		{
			hash.Add(item);
		}

		return hash.ToHashCode();
	}

	public override string ToString()
	{
		return JsonSerializer.Serialize(Ngrams);
	}
}
