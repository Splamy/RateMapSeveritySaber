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

using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;

namespace Ramses.Lights;

/// <summary>
/// Represents a state in a Markov chain.
/// </summary>
/// <typeparam name="T">The type of the constituent parts of each state in the Markov chain.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="ChainState{T}"/> class with the specified items.
/// </remarks>
/// <param name="items">An array of <typeparamref name="T"/> items to be copied as a single state.</param>
public readonly struct ChainState<T>(ReadOnlyMemory<T> items) : IEquatable<ChainState<T>>, IComparable<ChainState<T>>
	where T : notnull, IEquatable<T>, IComparable<T>
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

	public int CompareTo(ChainState<T> other)
	{
		if (_items.Length != other._items.Length)
		{
			return _items.Length.CompareTo(other._items.Length);
		}

		for (var i = 0; i < _items.Length && i < other._items.Length; i++)
		{
			var cmp = Comparer<T>.Default.Compare(_items.Span[i], other._items.Span[i]);
			if (cmp != 0)
			{
				return cmp;
			}
		}
		return 0;
	}
}

public readonly struct Weigths<T>
{
	public ImmutableArray<(long Weight, T Key)> Values { get; }
	public long WeightsSum { get; }


	public Weigths(IEnumerable<KeyValuePair<T, int>> weights)
	{
		Values = weights
			.Select(kv => ((long)kv.Value, kv.Key))
			.ToImmutableArray();
		WeightsSum = Values.Sum(kv => kv.Weight);
	}

	public Weigths(IEnumerable<KeyValuePair<T, long>> weights)
	{
		Values = weights
			.Select(kv => (kv.Value, kv.Key))
			.ToImmutableArray();
		WeightsSum = Values.Sum(kv => kv.Weight);
	}
}

public class MarkovBuilder<T> where T : IEquatable<T>, IComparable<T>
{
	private readonly Dictionary<ChainState<T>, WeigthsBuilder<T>> items = [];
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
	public void AddPhrase(ReadOnlyMemory<T> items, int weight = 1)
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
	public void AddNgram(ReadOnlyMemory<T> previous, T item, int weight = 1)
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
	public virtual void AddNgram(ChainState<T> state, T next, int weight = 1)
	{
		ref var weights = ref CollectionsMarshal.GetValueRefOrAddDefault(items, state, out _);
		weights ??= new WeigthsBuilder<T>();

		weights.Add(next, weight);
	}

	public void Include(MarkovBuilder<T> chain)
	{
		foreach (var state in chain.items)
		{
			ref var weights = ref CollectionsMarshal.GetValueRefOrAddDefault(items, state.Key, out _);
			weights ??= new WeigthsBuilder<T>();

			foreach (var item in state.Value.Get())
			{
				weights.Add(item.Key, item.Value);
			}
		}
	}

	//public void AddPhrasesParallel(IEnumerable<ReadOnlyMemory<T>> dataset, int weight = 1)
	//{
	//	var builder = FromPhrasesParallel(dataset, Order, weight);
	//	Include(builder);
	//}

	public static MarkovChain<T> FromPhrasesParallel(IEnumerable<ReadOnlyMemory<T>> dataset, int order, int weight = 1)
	{
		ConcurrentBag<MarkovBuilder<T>> builderPool = [.. Enumerable.Range(0, Environment.ProcessorCount).Select(_ => new MarkovBuilder<T>(order))];

		Parallel.ForEach(dataset, phrase =>
		{
			if (!builderPool.TryTake(out var builder))
				builder = new MarkovBuilder<T>(order);

			builder.AddPhrase(phrase, weight);

			builderPool.Add(builder);
		});

		var sw = Stopwatch.StartNew();
		var mergedElements = builderPool
			.SelectMany(x => x.items)
			.AsParallel()
			.GroupBy(x => x.Key, x => x.Value)
			.Select(x =>
				KeyValuePair.Create(
					x.Key,
					new Weigths<T>(x
						.Aggregate((acc, w) =>
						{
							foreach (var item in w.Get())
							{
								acc.Add(item.Key, item.Value);
							}
							return acc;
						})
						.Get()
						//.Where(kv => kv.Value > 0)
					)
				)
			)
			//.Where(kv => kv.Value.WeightsSum > 0)
			.ToFrozenDictionary();

		return new MarkovChain<T>(mergedElements, order);

		//Console.WriteLine("Sorted in {0}ms", sw.ElapsedMilliseconds);

		//// Merge consecutive elements with the same key
		//sw.Restart();

		//var mergedElements = new List<KeyValuePair<ChainState<T>, WeigthsBuilder<T>>>();
		//var current = sortedElements[0];
		//for (var i = 1; i < sortedElements.Count; i++)
		//{
		//	var next = sortedElements[i];
		//	if (current.Key.Equals(next.Key))
		//	{
		//		foreach (var item in next.Value.Get())
		//		{
		//			current.Value.Add(item.Key, item.Value);
		//		}
		//	}
		//	else
		//	{
		//		mergedElements.Add(current);
		//		current = next;
		//	}
		//}
		//mergedElements.Add(current);

		Console.WriteLine("Merged in {0}ms", sw.ElapsedMilliseconds);

		//sw.Restart();
		//var finalBuild = new MarkovBuilder<T>(order);

		//foreach (var (key, value) in mergedElements)
		//{
		//	foreach (var item in value.Get())
		//	{
		//		finalBuild.AddNgram(key, item.Key, item.Value);
		//	}
		//}
		//Console.WriteLine("Finalized in {0}ms", sw.ElapsedMilliseconds);

		//return finalBuild;

		//foreach (var builder in builderPool)
		//{
		//	var swx = Stopwatch.StartNew();
		//	var sortedList = builder.items.OrderBy(x => x.Key).ToList();
		//	Console.WriteLine("Sorted in {0}ms", swx.ElapsedMilliseconds);
		//}

		//var sortedChunks = builderPool.Select(builder =>
		//{
		//	var swx = Stopwatch.StartNew();
		//	var sortedList = builder.items.OrderBy(x => x.Key).ToList();
		//	Console.WriteLine("Sorted in {0}ms", swx.ElapsedMilliseconds);
		//	return sortedList;
		//});
		//var indexes = sortedChunks.Select(x => 0).ToArray();

		//// Merge Lists
		//var finalBuilder = new MarkovBuilder<T>(order);
		//while(true)
		//{

		//}


		//var sw = Stopwatch.StartNew();
		//var linAgg = builderPool.Aggregate((acc, agg) =>
		//{
		//	acc.Include(agg);
		//	return acc;
		//});
		//Console.WriteLine("Linear Aggregation in {0}ms", sw.ElapsedMilliseconds);

		//return linAgg;

		//return dataset
		//	.AsParallel()
		//	.AsUnordered()
		//	//.WithDegreeOfParallelism(1)
		//	.Aggregate(
		//		() => new MarkovBuilder<T>(order),
		//		(acc, item) => { acc.AddPhrase(item, weight); return acc; },
		//		(acc, agg) =>
		//		{
		//			Console.WriteLine("Aggregating {0} count into {1} count", agg.items.Count, acc.items.Count);
		//			var sw = Stopwatch.StartNew();
		//			//var sortedList = agg.items.OrderBy(x => x.Key).ToList();
		//			//Console.WriteLine("Sorted in {0}ms", sw.ElapsedMilliseconds);

		//			sw.Restart();
		//			acc.Include(agg);
		//			Console.WriteLine("Included in {0}ms", sw.ElapsedMilliseconds);
		//			return acc;
		//		},
		//		acc => acc
		//	);

	}

	public MarkovChain<T> Build()
	{
		var frozenItems = items
			.Select(weigths => KeyValuePair.Create(weigths.Key, new Weigths<T>(weigths.Value.Get().Where(kv => kv.Value > 0))))
			.Where(kv => kv.Value.WeightsSum > 0)
			.ToFrozenDictionary();

		return new MarkovChain<T>(frozenItems, Order);
	}
}

class WeigthsBuilder<T> where T : notnull, IEquatable<T>
{
	const int SmallDictSize = 16;

	public BufferDict SmallDict;
	public Dictionary<T, int>? Dict;
	public int SmallDictCount = 0;

	public void Add(T key, int weight)
	{
		if (Dict == null)
		{
			for (var i = 0; i < SmallDictCount; i++)
			{
				ref var x = ref Unsafe.Add(ref SmallDict._element0, i);

				if (x.Key.Equals(key))
				{
					x = KeyValuePair.Create(key, SmallDict[i].Value + weight);
					return;
				}
			}

			if (SmallDictCount < SmallDictSize)
			{
				SmallDict[SmallDictCount] = KeyValuePair.Create(key, weight);
				SmallDictCount++;
				return;
			}

			Dict = new(SmallDictSize * 2);
			for (var i = 0; i < SmallDictSize; i++)
			{
				ref var x = ref Unsafe.Add(ref SmallDict._element0, i);
				Dict.Add(x.Key, x.Value);
			}
		}

		ref var curWeight = ref CollectionsMarshal.GetValueRefOrAddDefault(Dict, key, out _);
		curWeight += weight;
	}

	public IEnumerable<KeyValuePair<T, int>> Get() => Dict != null ? Dict : SmallDict[..].ToArray()!;

	[System.Runtime.CompilerServices.InlineArray(SmallDictSize)]
	public struct BufferDict
	{
		public KeyValuePair<T, int> _element0;
	}
}


/// <summary>
/// Builds and walks interconnected states based on a weighted probability.
/// </summary>
/// <typeparam name="T">The type of the constituent parts of each state in the Markov chain.</typeparam>
public class MarkovChain<T> where T : IEquatable<T>, IComparable<T>
{
	public FrozenDictionary<ChainState<T>, Weigths<T>> Items { get; }

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

		Items = items;
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
	public Weigths<T>? GetNextStates(ChainState<T> state) => Items.TryGetValue(state, out var w) ? w : null;

	/// <summary>
	/// Gets all of the states that exist in the generator.
	/// </summary>
	/// <returns>An enumerable collection of <see cref="ChainState{T}"/> containing all of the states in the generator.</returns>
	public virtual IEnumerable<ChainState<T>> GetStates() => Items.Keys;
}
