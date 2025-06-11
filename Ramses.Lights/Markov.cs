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

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
public readonly struct ChainState<T>(ReadOnlyMemory<T> items) : IEquatable<ChainState<T>>, IComparable<ChainState<T>>
	where T : notnull, IEquatable<T>, IComparable<T>
{
	public static ChainState<T> Empty { get; } = new ChainState<T>(ReadOnlyMemory<T>.Empty);

	private readonly ReadOnlyMemory<T> _items = items;

	/// <inheritdoc />
	public int Count => _items.Length;

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

	public override string ToString() => string.Join(", ", _items.Span.ToArray());
}

public readonly struct Weights<T> where T : IEquatable<T>
{
	public ImmutableArray<(long Weight, T Key)> Values { get; }
	public long WeightsSum { get; }


	public Weights(WeigthsBuilder<T> weights)
	{
		var builder = ImmutableArray.CreateBuilder<(long, T)>(weights.Count);
		foreach (var item in weights)
		{
			builder.Add((item.Value, item.Key));
		}
		Values = builder.MoveToImmutable();
		WeightsSum = Values.Sum(kv => kv.Weight);
	}

	public Weights(IEnumerable<KeyValuePair<T, long>> weights)
	{
		Values = weights
			.Select(kv => (kv.Value, kv.Key))
			.ToImmutableArray();
		WeightsSum = Values.Sum(kv => kv.Weight);
	}

	public T PickWeigthed(Random? rand = default)
	{
		rand ??= Random.Shared;
		var value = rand.NextInt64(WeightsSum) + 1;

		long currentWeight = 0;
		foreach (var (weight, key) in Values)
		{
			currentWeight += weight;
			if (currentWeight >= value)
			{
				return key;
			}
		}

		throw new InvalidOperationException("No item was selected.");
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

			foreach (var item in state.Value)
			{
				weights.Add(item.Key, item.Value);
			}
		}
	}

	public static IEnumerable<KeyValuePair<ChainState<T>, T>> CreatePhraseIter(ReadOnlyMemory<T> items, int order)
	{
		if (items.Length == 0)
		{
			yield break;
		}

		yield return KeyValuePair.Create(ChainState<T>.Empty, items.Span[0]);

		for (var i = 1; i < order && i < items.Length; i++)
		{
			yield return KeyValuePair.Create(new ChainState<T>(items[..i]), items.Span[i]);
		}

		for (var i = order; i < items.Length; i++)
		{
			yield return KeyValuePair.Create(new ChainState<T>(items[(i - order)..i]), items.Span[i]);
		}
	}

	public static MarkovChain<T> FromPhrasesParallel(IEnumerable<ReadOnlyMemory<T>> dataset, int order, int weight = 1)
	{
		return
		 // FromPhrasesParallel1(dataset, order, weight);
		 FromPhrasesParallel2(dataset, order, weight);
		// FromPhrasesParallel3(dataset, order, weight);
	}

	public static MarkovChain<T> FromPhrasesParallel1(IEnumerable<ReadOnlyMemory<T>> dataset, int order, int weight = 1)
	{
		var sw = Stopwatch.StartNew();

		// Merge Lists
		sw.Restart();
		var mergedElements = dataset
			.AsParallel()
			.WithDegreeOfParallelism(Utils.Threads)
			.SelectMany(x => CreatePhraseIter(x, order))
			.GroupBy(x => x.Key, x => x.Value)
			.Select(x =>
			{
				var wb = new WeigthsBuilder<T>();
				foreach (var item in x)
				{
					wb.Add(item, 1);
				}

				return KeyValuePair.Create(
					x.Key,
					new Weights<T>(wb)
				);
			}
			)
			.ToFrozenDictionary();

		Console.WriteLine("Merged in {0}ms", sw.ElapsedMilliseconds);

		return new MarkovChain<T>(mergedElements, order);
	}

	public static MarkovChain<T> FromPhrasesParallel2(IEnumerable<ReadOnlyMemory<T>> dataset, int order, int weight = 1)
	{
		ConcurrentBag<MarkovBuilder<T>> builderPool = [.. Enumerable.Range(0, Environment.ProcessorCount).Select(_ => new MarkovBuilder<T>(order))];

		var sw = Stopwatch.StartNew();

		Parallel.ForEach(dataset,
			new ParallelOptions { MaxDegreeOfParallelism = Utils.Threads },
			phrase =>
		{
			if (!builderPool.TryTake(out var builder))
				builder = new MarkovBuilder<T>(order);

			builder.AddPhrase(phrase, weight);

			builderPool.Add(builder);
		});

		Console.WriteLine("Processed in {0}ms", sw.ElapsedMilliseconds);

		// Merge Lists

		var mergedElements = builderPool
			.SelectMany(x => x.items)
			.AsParallel()
			.WithDegreeOfParallelism(Utils.Threads)
			.GroupBy(x => x.Key, x => x.Value)
			.Select(x =>
			{
				return KeyValuePair.Create(
					x.Key,
					new Weights<T>(x
						.Aggregate((acc, w) =>
						{
							foreach (var item in w)
							{
								acc.Add(item.Key, item.Value);
							}
							return acc;
						})
					)
				);
			})
			.ToFrozenDictionary();

		Console.WriteLine("Merged in {0}ms", sw.ElapsedMilliseconds);

		return new MarkovChain<T>(mergedElements, order);
	}

	public static MarkovChain<T> FromPhrasesParallel3(IEnumerable<ReadOnlyMemory<T>> dataset, int order, int weight = 1)
	{
		var sw = Stopwatch.StartNew();

		var builder = dataset
			.AsParallel()
			.WithDegreeOfParallelism(Utils.Threads)
			.AsUnordered()
			.Aggregate(
				() => new MarkovBuilder<T>(order),
				(acc, item) => { acc.AddPhrase(item, weight); return acc; },
				(acc, agg) =>
				{
					Console.WriteLine("Aggregating {0} count into {1} count", agg.items.Count, acc.items.Count);
					var sw = Stopwatch.StartNew();
					acc.Include(agg);
					Console.WriteLine("Included in {0}ms", sw.ElapsedMilliseconds);
					return acc;
				},
				acc => acc
			);

		Console.WriteLine("Merged in {0}ms", sw.ElapsedMilliseconds);

		return builder.Build();
	}

	public MarkovChain<T> Build()
	{
		var frozenItems = items
			.Select(weigths => KeyValuePair.Create(weigths.Key, new Weights<T>(weigths.Value)))
			.Where(kv => kv.Value.WeightsSum > 0)
			.ToFrozenDictionary();

		return new MarkovChain<T>(frozenItems, Order);
	}
}

public class WeigthsBuilder<T> : IEnumerable<KeyValuePair<T, int>> where T : notnull, IEquatable<T>
{
	const int SmallDictSize = 16;

	public BufferDict SmallDict;
	public Dictionary<T, int>? Dict;
	public int SmallDictCount = 0;

	public int Count => Dict?.Count ?? SmallDictCount;

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

	public Enumerator GetEnumerator() => Dict != null ? new Enumerator(Dict.GetEnumerator()) : new Enumerator(this);

	IEnumerator<KeyValuePair<T, int>> IEnumerable<KeyValuePair<T, int>>.GetEnumerator() => Dict != null ? Dict.GetEnumerator() : new Enumerator(this);
	IEnumerator IEnumerable.GetEnumerator() => Dict != null ? Dict.GetEnumerator() : new Enumerator(this);

	[InlineArray(SmallDictSize)]
	public struct BufferDict
	{
		public KeyValuePair<T, int> _element0;
	}

	public struct Enumerator : IEnumerator<KeyValuePair<T, int>>
	{
		private readonly WeigthsBuilder<T>? _self;
		private int _index;
		private Dictionary<T, int>.Enumerator _dictEnumerator;

		/// <summary>Initialize the enumerator.</summary>
		/// <param name="span">The span to enumerate.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Enumerator(Dictionary<T, int>.Enumerator enumerator)
		{
			_dictEnumerator = enumerator;
			_self = null;
		}

		/// <summary>Initialize the enumerator.</summary>
		/// <param name="span">The span to enumerate.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Enumerator(WeigthsBuilder<T> self)
		{
			_dictEnumerator = default;
			_self = self;
			_index = -1;
		}

		/// <summary>Advances the enumerator to the next element of the span.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			if (_self != null)
			{
				return ++_index < _self.SmallDictCount;
			}
			else
			{
				return _dictEnumerator.MoveNext();
			}
		}

		public void Reset()
		{
			if (_self != null)
			{
				_index = -1;
			}
		}

		public void Dispose() { }

		/// <summary>Gets the element at the current position of the enumerator.</summary>
		public KeyValuePair<T, int> Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _self != null ? _self.SmallDict[_index] : _dictEnumerator.Current;
		}

		object IEnumerator.Current => Current;
	}
}


/// <summary>
/// Builds and walks interconnected states based on a weighted probability.
/// </summary>
/// <typeparam name="T">The type of the constituent parts of each state in the Markov chain.</typeparam>
public class MarkovChain<T> where T : IEquatable<T>, IComparable<T>
{
	public FrozenDictionary<ChainState<T>, Weights<T>> Items { get; }

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
	internal MarkovChain(FrozenDictionary<ChainState<T>, Weights<T>> items, int order)
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
	public Weights<T>? GetInitialStates() => GetNextStates(new ChainState<T>(ReadOnlyMemory<T>.Empty));

	/// <summary>
	/// Gets the items from the generator that follow from the specified items preceding it.
	/// </summary>
	/// <param name="previous">The items preceding the items of interest.</param>
	/// <returns>A dictionary of the items and their weight.</returns>
	public Weights<T>? GetNextStates(IEnumerable<T> previous)
	{
		var state = new ChainState<T>(previous.Take(^Order..).ToArray());
		return GetNextStates(state);
	}

	/// <summary>
	/// Gets the items from the generator that follow from the specified state preceding it.
	/// </summary>
	/// <param name="state">The state preceding the items of interest.</param>
	/// <returns>A dictionary of the items and their weight.</returns>
	public Weights<T>? GetNextStates(ChainState<T> state) => Items.TryGetValue(state, out var w) ? w : null;

	/// <summary>
	/// Gets all of the states that exist in the generator.
	/// </summary>
	/// <returns>An enumerable collection of <see cref="ChainState{T}"/> containing all of the states in the generator.</returns>
	public virtual IEnumerable<ChainState<T>> GetStates() => Items.Keys;
}
