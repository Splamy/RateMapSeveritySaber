using System;
using System.Buffers;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Ramses.Lights;
internal struct FixedBufferList<T>
	: IDisposable, IList<T> where T : IEquatable<T>
{
	public readonly T this[int index]
	{
		get => RawBuffer.Span[index];
		set => RawBuffer.Span[index] = value;
	}

	private readonly T[]? ownPooledBuffer;
	public int Count { readonly get; private set; }
	public readonly bool IsReadOnly => false;
	public readonly Memory<T> RawBuffer { get; }
	public readonly Memory<T> Items => RawBuffer[..Count];

	public FixedBufferList(int count)
	{
		ownPooledBuffer = ArrayPool<T>.Shared.Rent(count);
		RawBuffer = ownPooledBuffer;
	}

	public FixedBufferList(Memory<T> buffer)
	{
		ownPooledBuffer = null;
		RawBuffer = buffer;
	}

	public void Add(T item) => RawBuffer.Span[Count++] = item;

	public void Clear() => Count = 0;

	public readonly bool Contains(T item) => Items.Span.Contains(item);

	public readonly void CopyTo(T[] array, int arrayIndex) => Items.Span.CopyTo(array.AsSpan(arrayIndex));

	readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);

	public readonly Span<T>.Enumerator GetEnumerator() => Items.Span.GetEnumerator();

	public readonly int IndexOf(T item) => Items.Span.IndexOf(item);

	public void Insert(int index, T item)
	{
		if (index < 0 || index > Count)
		{
			throw new ArgumentOutOfRangeException(nameof(index));
		}
		if (Count == RawBuffer.Length)
		{
			throw new InvalidOperationException("List is full");
		}
		var dataSpan = RawBuffer.Span;
		dataSpan[index..Count].CopyTo(dataSpan[(index + 1)..]);
		dataSpan[index] = item;
		Count++;
	}

	public bool Remove(T item)
	{
		var index = IndexOf(item);
		if (index == -1)
		{
			return false;
		}
		RemoveAt(index);
		return true;
	}

	public void RemoveAt(int index)
	{
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException(nameof(index));
		}
		var dataSpan = RawBuffer.Span;
		dataSpan[(index + 1)..Count].CopyTo(dataSpan[index..]);
		Count--;
	}

	// Stolen form https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/List.cs,962
	public int RemoveAll(Predicate<T> match)
	{
		int freeIndex = 0;
		var dataSpan = Items.Span;

		while (freeIndex < dataSpan.Length && !match(dataSpan[freeIndex])) freeIndex++;
		if (freeIndex >= dataSpan.Length) return 0;

		int current = freeIndex + 1;
		while (current < dataSpan.Length)
		{
			// Find the first item which needs to be kept.
			while (current < dataSpan.Length && match(dataSpan[current])) current++;

			if (current < dataSpan.Length)
			{
				// copy item to the free slot.
				dataSpan[freeIndex++] = dataSpan[current++];
			}
		}

		if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
		{
			dataSpan[freeIndex..].Clear(); // Clear the elements so that the gc can reclaim the references.
		}

		int result = dataSpan.Length - freeIndex;
		Count = freeIndex;
		return result;
	}

	readonly IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();

	public readonly void Dispose()
	{
		if (ownPooledBuffer is not null)
		{
			ArrayPool<T>.Shared.Return(ownPooledBuffer);
		}
	}

	private sealed class Enumerator(FixedBufferList<T> list) : IEnumerator<T>
	{
		private readonly FixedBufferList<T> list = list;
		private int index = -1;

		public T Current => list[index];
		object IEnumerator.Current => Current;
		public void Dispose() { }
		public bool MoveNext() => ++index < list.Count;
		public void Reset() => index = -1;
	}
}
