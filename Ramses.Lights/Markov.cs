using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Ramses.Lights;


/// <summary>
/// Represents a state in a Markov chain.
/// </summary>
/// <typeparam name="T">The type of the constituent parts of each state in the Markov chain.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="ChainState{T}"/> class with the specified items.
/// </remarks>
/// <param name="items">An array of <typeparamref name="T"/> items to be copied as a single state.</param>
public readonly struct ChainState<T>(ImmutableArray<T> items) : IEquatable<ChainState<T>>, IReadOnlyList<T> where T : notnull
{
	private readonly ImmutableArray<T> _items = items;

	/// <summary>
	/// Initializes a new instance of the <see cref="ChainState{T}"/> class with the specified items.
	/// </summary>
	/// <param name="items">An <see cref="IEnumerable{T}"/> of items to be copied as a single state.</param>
	public ChainState(IEnumerable<T> items) : this(items.ToImmutableArray()) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="ChainState{T}"/> class with the specified items.
	/// </summary>
	/// <param name="items">An array of <typeparamref name="T"/> items to be copied as a single state.</param>
	public ChainState(params T[] items) : this(items.ToImmutableArray()) { }

	/// <inheritdoc />
	public int Count => _items.Length;

	/// <inheritdoc />
	public bool IsReadOnly => true;

	/// <inheritdoc />
	public T this[int index]
	{
		get { return _items[index]; }
	}

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
	public bool Contains(T item) => ((IList<T>)_items).Contains(item);

	/// <inheritdoc />
	public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

	/// <inheritdoc />
	public override bool Equals(object? obj)
	{
		if (obj is ChainState<T> chain)
		{
			return Equals(chain);
		}

		return false;
	}

	/// <summary>
	/// Indicates whether the current object is equal to another object of the same type.
	/// </summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.</returns>
	public bool Equals(ChainState<T> other) => _items.SequenceEqual(other._items);

	/// <inheritdoc />
	public IEnumerator<T> GetEnumerator() => ((IList<T>)_items).GetEnumerator();

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <inheritdoc />
	public override int GetHashCode()
	{
		var hash = new HashCode();

		foreach (var item in _items)
		{
			hash.Add(item);
		}

		return hash.ToHashCode();
	}
}

/// <summary>
/// Builds and walks interconnected states based on a weighted probability.
/// </summary>
/// <typeparam name="T">The type of the constituent parts of each state in the Markov chain.</typeparam>
public class MarkovChain<T>
	where T : IEquatable<T>
{
	private readonly ConcurrentDictionary<ChainState<T>, ConcurrentDictionary<T, int>> items = [];
	private readonly int order;

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
	public MarkovChain(int order)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(order);

		this.order = order;
	}

	/// <summary>
	/// Gets the order of the chain.
	/// </summary>
	public int Order => order;

	/// <summary>
	/// Adds the items to the generator with a weight of one.
	/// </summary>
	/// <param name="items">The items to add to the generator.</param>
	public void Add(IEnumerable<T> items) => Add(items, 1);

	public void AddParallel(IEnumerable<IEnumerable<T>> dataset, int weight)
	{
		ArgumentNullException.ThrowIfNull(items);

		Parallel.ForEach(dataset, items => Add(items, weight));
	}

	/// <summary>
	/// Adds the items to the generator with the weight specified.
	/// </summary>
	/// <param name="items">The items to add to the generator.</param>
	/// <param name="weight">The weight at which to add the items.</param>
	public void Add(IEnumerable<T> items, int weight)
	{
		ArgumentNullException.ThrowIfNull(items);

		var previous = new Queue<T>();
		foreach (var item in items)
		{
			var key = new ChainState<T>(previous);

			Add(key, item, weight);

			previous.Enqueue(item);
			if (previous.Count > order)
			{
				previous.Dequeue();
			}
		}
	}

	/// <summary>
	/// Adds the item to the generator, with the specified items preceding it.
	/// </summary>
	/// <param name="previous">The items preceding the item.</param>
	/// <param name="item">The item to add.</param>
	/// <remarks>
	/// See <see cref="MarkovChain{T}.Add(IEnumerable{T}, T, int)"/> for remarks.
	/// </remarks>
	public void Add(IEnumerable<T> previous, T item)
	{
		ArgumentNullException.ThrowIfNull(previous);

		var state = new Queue<T>(previous);
		while (state.Count > order)
		{
			state.Dequeue();
		}

		Add(new ChainState<T>(state), item, 1);
	}

	/// <summary>
	/// Adds the item to the generator, with the specified state preceding it.
	/// </summary>
	/// <param name="state">The state preceding the item.</param>
	/// <param name="next">The item to add.</param>
	/// <remarks>
	/// See <see cref="MarkovChain{T}.Add(ChainState{T}, T, int)"/> for remarks.
	/// </remarks>
	public void Add(ChainState<T> state, T next) => Add(state, next, 1);

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
	public void Add(IEnumerable<T> previous, T item, int weight)
	{
		ArgumentNullException.ThrowIfNull(previous);

		Add(new ChainState<T>(previous.Take(^order..)), item, weight);
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
	public virtual void Add(ChainState<T> state, T next, int weight)
	{
		ArgumentNullException.ThrowIfNull(state);

		var weights = items.GetOrAdd(state, _ => []);

		weights.AddOrUpdate(next,
			static (_, weight) => weight,
			static (_, curWeigth, weight) => Math.Max(0, curWeigth + weight),
			weight);

		//if (newWeight == 0)
		//{
		//	weights.Remove(next);
		//	if (weights.Count == 0)
		//	{
		//		items.Remove(state);
		//	}
		//}
		//else
	}

	/// <summary>
	/// Randomly walks the chain.
	/// </summary>
	/// <returns>An <see cref="IEnumerable{T}"/> of the items chosen.</returns>
	/// <remarks>Assumes an empty starting state.</remarks>
	public IEnumerable<T> Chain() => Chain([], new Random());

	/// <summary>
	/// Randomly walks the chain.
	/// </summary>
	/// <param name="previous">The items preceding the first item in the chain.</param>
	/// <returns>An <see cref="IEnumerable{T}"/> of the items chosen.</returns>
	public IEnumerable<T> Chain(IEnumerable<T> previous) => Chain(previous, new Random());

	/// <summary>
	/// Randomly walks the chain.
	/// </summary>
	/// <param name="seed">The seed for the random number generator, used as the random number source for the chain.</param>
	/// <returns>An <see cref="IEnumerable{T}"/> of the items chosen.</returns>
	/// <remarks>Assumes an empty starting state.</remarks>
	public IEnumerable<T> Chain(int seed) => Chain([], new Random(seed));

	/// <summary>
	/// Randomly walks the chain.
	/// </summary>
	/// <param name="previous">The items preceding the first item in the chain.</param>
	/// <param name="seed">The seed for the random number generator, used as the random number source for the chain.</param>
	/// <returns>An <see cref="IEnumerable{T}"/> of the items chosen.</returns>
	public IEnumerable<T> Chain(IEnumerable<T> previous, int seed) => Chain(previous, new Random(seed));

	/// <summary>
	/// Randomly walks the chain.
	/// </summary>
	/// <param name="rand">The random number source for the chain.</param>
	/// <returns>An <see cref="IEnumerable{T}"/> of the items chosen.</returns>
	/// <remarks>Assumes an empty starting state.</remarks>
	public IEnumerable<T> Chain(Random rand) => Chain([], rand);

	/// <summary>
	/// Randomly walks the chain.
	/// </summary>
	/// <param name="previous">The items preceding the first item in the chain.</param>
	/// <param name="rand">The random number source for the chain.</param>
	/// <returns>An <see cref="IEnumerable{T}"/> of the items chosen.</returns>
	public IEnumerable<T> Chain(IEnumerable<T> previous, Random rand)
	{
		ArgumentNullException.ThrowIfNull(previous);
		ArgumentNullException.ThrowIfNull(rand);

		var state = new Queue<T>(previous);
		while (true)
		{
			while (state.Count > order)
			{
				state.Dequeue();
			}

			var key = new ChainState<T>(state);

			var weights = GetNextStatesInternal(key);
			if (weights == null)
			{
				yield break;
			}

			var total = weights.Sum(w => w.Value);
			var value = rand.Next(total) + 1;

			if (value > total)
			{
				yield break;
			}

			var currentWeight = 0;
			foreach (var nextItem in weights)
			{
				currentWeight += nextItem.Value;
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
	public Dictionary<T, int>? GetInitialStates() => GetNextStates(new ChainState<T>(Enumerable.Empty<T>()));

	/// <summary>
	/// Gets the items from the generator that follow from the specified items preceding it.
	/// </summary>
	/// <param name="previous">The items preceding the items of interest.</param>
	/// <returns>A dictionary of the items and their weight.</returns>
	public Dictionary<T, int>? GetNextStates(IEnumerable<T> previous)
	{
		var state = new Queue<T>(previous);
		while (state.Count > order)
		{
			state.Dequeue();
		}

		return GetNextStates(new ChainState<T>(state));
	}

	/// <summary>
	/// Gets the items from the generator that follow from the specified state preceding it.
	/// </summary>
	/// <param name="state">The state preceding the items of interest.</param>
	/// <returns>A dictionary of the items and their weight.</returns>
	public Dictionary<T, int>? GetNextStates(ChainState<T> state)
	{
		var weights = GetNextStatesInternal(state);
		return weights != null ? new Dictionary<T, int>(weights) : null;
	}

	/// <summary>
	/// Gets all of the states that exist in the generator.
	/// </summary>
	/// <returns>An enumerable collection of <see cref="ChainState{T}"/> containing all of the states in the generator.</returns>
	public virtual IEnumerable<ChainState<T>> GetStates()
	{
		foreach (var state in items.Keys)
		{
			yield return state;
		}
	}

	/// <summary>
	/// Gets the items from the generator that follow from the specified state preceding it without copying the values.
	/// </summary>
	/// <param name="state">The state preceding the items of interest.</param>
	/// <returns>The raw dictionary of the items and their weight.</returns>
	protected internal virtual IReadOnlyDictionary<T, int>? GetNextStatesInternal(ChainState<T> state) => items.GetValueOrDefault(state);
}
