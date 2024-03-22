// https://github.com/otac0n/markov

/*
 Copyright © 2018 John Gietzen and Contributors

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

Contributors:
    Zac Gross
 */

using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace Ramses.Lights;

/// <summary>
/// Represents a state in a Markov chain.
/// </summary>
/// <typeparam name="T">The type of the constituent parts of each state in the Markov chain.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="ChainState{T}"/> class with the specified items.
/// </remarks>
/// <param name="items">An array of <typeparamref name="T"/> items to be copied as a single state.</param>
public readonly struct ChainState<T>(ReadOnlyMemory<T> items) : IEquatable<ChainState<T>> where T : notnull, IEquatable<T>
{
	public static ChainState<T> Empty { get; } = new ChainState<T>(ReadOnlyMemory<T>.Empty);

	private readonly ReadOnlyMemory<T> _items = items;

	/// <inheritdoc />
	public int Count => _items.Length;

	/// <inheritdoc />
	public bool IsReadOnly => true;

	/// <inheritdoc />
	public T this[int index] => _items.Span[index];

	/// <summary>
	/// Determines whether two specified instances of <see cref="ChainState{T}"/> are not equal.
	/// </summary>
	/// <param name="a">The first <see cref="ChainState{T}"/> to compare.</param>
	/// <param name="b">The second <see cref="ChainState{T}"/> to compare.</param>
	/// <returns>true if <paramref name="a"/> and <paramref name="b"/> do not represent the same state; otherwise, false.</returns>
	public static bool operator !=(ChainState<T> a, ChainState<T> b) => !(a == b);

	/// <summary>
	/// Determines whether two specified instances of <see cref="ChainState{T}"/> are equal.
	/// </summary>
	/// <param name="a">The first <see cref="ChainState{T}"/> to compare.</param>
	/// <param name="b">The second <see cref="ChainState{T}"/> to compare.</param>
	/// <returns>true if <paramref name="a"/> and <paramref name="b"/> represent the same state; otherwise, false.</returns>
	public static bool operator ==(ChainState<T> a, ChainState<T> b) => a.Equals(b);

	/// <inheritdoc />
	public bool Contains(T item) => _items.Span.Contains(item);

	/// <inheritdoc />
	public override bool Equals(object? obj) => obj is ChainState<T> chain && Equals(chain);

	/// <summary>
	/// Indicates whether the current object is equal to another object of the same type.
	/// </summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.</returns>
	public bool Equals(ChainState<T> other) => _items.Span.SequenceEqual(other._items.Span);

	/// <inheritdoc />
	public override int GetHashCode()
	{
		var hash = new HashCode();

		foreach (var item in _items.Span)
		{
			hash.Add(item);
		}

		return hash.ToHashCode();
	}
}

public readonly struct Weigths<T>
{
	public ImmutableArray<(long Weight, T Key)> Values { get; }
	public long WeightsSum { get; }

	public Weigths(IEnumerable<KeyValuePair<T, long>> weights)
	{
		Values = weights
			.Select(kv => (kv.Value, kv.Key))
			.ToImmutableArray();
		WeightsSum = Values.Sum(kv => kv.Weight);
	}
}

public class MarkovBuilder<T> where T : IEquatable<T>
{
	private readonly Dictionary<ChainState<T>, Dictionary<T, long>> items = [];
	public int Order { get; }

	public MarkovBuilder(int order)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(order);

		Order = order;
	}

	/// <summary>
	/// Adds the items to the generator with the weight specified.
	/// </summary>
	/// <param name="items">The items to add to the generator.</param>
	/// <param name="weight">The weight at which to add the items.</param>
	public void AddPhrase(ReadOnlyMemory<T> items, long weight = 1)
	{
		if (items.Length == 0)
		{
			return;
		}

		AddNgram(ChainState<T>.Empty, items.Span[0], weight);

		for (var i = 1; i < Order && i < items.Length; i++)
		{
			AddNgram(new ChainState<T>(items[..i]), items.Span[i], weight);
		}

		for (var i = Order; i < items.Length; i++)
		{
			AddNgram(new ChainState<T>(items[(i - Order)..i]), items.Span[i], weight);
		}
	}

	/// <summary>
	/// Adds the item to the generator, with the specified items preceding it and the specified weight.
	/// </summary>
	/// <param name="previous">The items preceding the item.</param>
	/// <param name="item">The item to add.</param>
	/// <param name="weight">The weight of the item to add.</param>
	/// <remarks>
	/// This method does not add all of the preceding states to the generator.
	/// Notably, the empty state is not added, unless the <paramref name="previous"/> parameter is empty.
	/// </remarks>
	public void AddNgram(ReadOnlyMemory<T> previous, T item, long weight = 1)
	{
		ArgumentNullException.ThrowIfNull(previous);

		if (previous.Length > Order)
		{
			previous = previous[^Order..];
		}

		AddNgram(new ChainState<T>(previous), item, weight);
	}

	/// <summary>
	/// Adds the item to the generator, with the specified state preceding it and the specified weight.
	/// </summary>
	/// <param name="state">The state preceding the item.</param>
	/// <param name="next">The item to add.</param>
	/// <param name="weight">The weight of the item to add.</param>
	/// <remarks>
	/// This adds the state as-is.  The state may not be reachable if, for example, the
	/// number of items in the state is greater than the order of the generator, or if the
	/// combination of items is not available in the other states of the generator.
	///
	/// A negative weight may be passed, which will have the impact of reducing the weight
	/// of the specified state transition.  This can therefore be used to remove items from
	/// the generator. The resulting weight will never be allowed below zero.
	/// </remarks>
	public virtual void AddNgram(ChainState<T> state, T next, long weight = 1)
	{
		ref var weights = ref CollectionsMarshal.GetValueRefOrAddDefault(items, state, out _);
		weights ??= [];

		ref var curWeight = ref CollectionsMarshal.GetValueRefOrAddDefault(weights, next, out _);
		curWeight += weight;
	}

	public void Include(MarkovBuilder<T> chain)
	{
		foreach (var state in chain.items)
		{
			foreach (var item in state.Value)
			{
				AddNgram(state.Key, item.Key, item.Value);
			}
		}
	}

	public void AddPhrasesParallel(IEnumerable<ReadOnlyMemory<T>> dataset, long weight = 1)
	{
		var builder = FromPhrasesParallel(dataset, Order, weight);
		Include(builder);
	}

	public static MarkovBuilder<T> FromPhrasesParallel(IEnumerable<ReadOnlyMemory<T>> dataset, int order, long weight = 1)
	{
		return dataset
			.AsParallel()
			.AsUnordered()
			//.WithDegreeOfParallelism(1)
			.Aggregate(
				() => new MarkovBuilder<T>(order),
				(acc, item) => { acc.AddPhrase(item, weight); return acc; },
				(acc, agg) => { acc.Include(agg); return acc; },
				acc => acc
			);
	}

	public MarkovChain<T> Build()
	{
		var frozenItems = items
			.Select(weigths => KeyValuePair.Create(weigths.Key, new Weigths<T>(weigths.Value.Where(kv => kv.Value > 0))))
			.Where(kv => kv.Value.WeightsSum > 0)
			.ToFrozenDictionary();

		return new MarkovChain<T>(frozenItems, Order);
	}
}

/// <summary>
/// Builds and walks interconnected states based on a weighted probability.
/// </summary>
/// <typeparam name="T">The type of the constituent parts of each state in the Markov chain.</typeparam>
public class MarkovChain<T> where T : IEquatable<T>
{
	private readonly FrozenDictionary<ChainState<T>, Weigths<T>> _items;

	/// <summary>
	/// Initializes a new instance of the <see cref="MarkovChain{T}"/> class.
	/// </summary>
	/// <param name="order">Indicates the desired order of the <see cref="MarkovChain{T}"/>.</param>
	/// <remarks>
	/// <para>The <paramref name="order"/> of a generator indicates the depth of its internal state.  A generator
	/// with an order of 1 will choose items based on the previous item, a generator with an order of 2
	/// will choose items based on the previous 2 items, and so on.</para>
	/// <para>Zero is not classically a valid order value, but it is allowed here.  Choosing a zero value has the
	/// effect that every state is equivalent to the starting state, and so items will be chosen based on their
	/// total frequency.</para>
	/// </remarks>
	internal MarkovChain(FrozenDictionary<ChainState<T>, Weigths<T>> items, int order)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(order);

		_items = items;
		Order = order;
	}

	/// <summary>
	/// Gets the order of the chain.
	/// </summary>
	public int Order { get; }


	/// <summary>
	/// Randomly walks the chain.
	/// </summary>
	/// <param name="previous">The items preceding the first item in the chain.</param>
	/// <param name="rand">The random number source for the chain.</param>
	/// <returns>An <see cref="IEnumerable{T}"/> of the items chosen.</returns>
	public IEnumerable<T> Chain(IEnumerable<T>? previous = default, Random? rand = default)
	{
		previous ??= [];
		rand ??= new Random();

		var state = new Queue<T>(previous);
		while (true)
		{
			while (state.Count > Order)
			{
				state.Dequeue();
			}

			var key = new ChainState<T>(state.ToArray());

			var weightsOpt = GetNextStates(key);
			if (weightsOpt is not { } weights)
			{
				// TODO Find most similar state instead of breaking
				yield break;
			}

			var total = weights.WeightsSum;
			var value = rand.NextInt64(total) + 1;

			if (value > total)
			{
				yield break;
			}

			long currentWeight = 0;
			foreach (var nextItem in weights.Values)
			{
				currentWeight += nextItem.Weight;
				if (currentWeight >= value)
				{
					yield return nextItem.Key;
					state.Enqueue(nextItem.Key);
					break;
				}
			}
		}
	}


	/// <summary>
	/// Gets the items from the generator that follow from an empty state.
	/// </summary>
	/// <returns>A dictionary of the items and their weight.</returns>
	public Weigths<T>? GetInitialStates() => GetNextStates(new ChainState<T>(ReadOnlyMemory<T>.Empty));

	/// <summary>
	/// Gets the items from the generator that follow from the specified items preceding it.
	/// </summary>
	/// <param name="previous">The items preceding the items of interest.</param>
	/// <returns>A dictionary of the items and their weight.</returns>
	public Weigths<T>? GetNextStates(IEnumerable<T> previous)
	{
		var state = new ChainState<T>(previous.Take(^Order..).ToArray());
		return GetNextStates(state);
	}

	/// <summary>
	/// Gets the items from the generator that follow from the specified state preceding it.
	/// </summary>
	/// <param name="state">The state preceding the items of interest.</param>
	/// <returns>A dictionary of the items and their weight.</returns>
	public Weigths<T>? GetNextStates(ChainState<T> state) => _items.TryGetValue(state, out var w) ? w : null;

	/// <summary>
	/// Gets all of the states that exist in the generator.
	/// </summary>
	/// <returns>An enumerable collection of <see cref="ChainState{T}"/> containing all of the states in the generator.</returns>
	public virtual IEnumerable<ChainState<T>> GetStates() => _items.Keys;
}
