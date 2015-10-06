using System;
using System.Collections.Generic;

namespace Granikos.Hydra.Service.PriorityQueue
{
    /// <summary>
    ///     The IPriorityQueue interface.  This is mainly here for purists, and in case I decide to add more implementations
    ///     later.
    ///     For speed purposes, it is actually recommended that you *don't* access the priority queue through this interface,
    ///     since the JIT can
    ///     (theoretically?) optimize method calls from concrete-types slightly better.
    /// </summary>
    public interface IPriorityQueue<K, T> : IEnumerable<T>
        where T : PriorityQueueNode<K>
        where K : IComparable<K>
    {
        T First { get; }
        int Count { get; }
        int MaxSize { get; }
        void Remove(T node);
        void UpdatePriority(T node, K priority);
        void Enqueue(T node, K priority);
        T Dequeue();
        void Clear();
        bool Contains(T node);
    }
}