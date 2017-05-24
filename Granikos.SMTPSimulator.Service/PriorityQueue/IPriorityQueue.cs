// MIT License
// 
// Copyright (c) 2017 Granikos GmbH & Co. KG (https://www.granikos.eu)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.Collections.Generic;

namespace Granikos.SMTPSimulator.Service.PriorityQueue
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