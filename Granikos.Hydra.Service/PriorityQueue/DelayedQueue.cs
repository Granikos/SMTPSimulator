using System;
using System.Diagnostics.Contracts;

namespace Granikos.Hydra.Service.PriorityQueue
{
    public class DelayedQueue<T>
    {
        private readonly HeapPriorityQueue<DateTime, QueueItem> _queue;

        public DelayedQueue(int maxItems)
        {
            Contract.Requires<ArgumentOutOfRangeException>(maxItems > 0, "maxItems");
            _queue = new HeapPriorityQueue<DateTime, QueueItem>(maxItems);
        }

        public int Count
        {
            get { return _queue.Count; }
        }

        public int MaxSize
        {
            get { return _queue.MaxSize; }
        }

        public void Enqueue(T item, TimeSpan delay)
        {
            lock (_queue)
            {
                _queue.Enqueue(new QueueItem {Value = item}, DateTime.Now + delay);
            }
        }

        public T Peek()
        {
            var first = _queue.First;
            if (first == null) return default(T);

            return first.Priority <= DateTime.Now ? first.Value : default(T);
        }

        public T Dequeue()
        {
            lock (_queue)
            {
                return Peek() != null ? _queue.Dequeue().Value : default(T);
            }
        }

        public void Clear()
        {
            lock (_queue)
            {
                _queue.Clear();
            }
        }

        private class QueueItem : PriorityQueueNode<DateTime>
        {
            public T Value { get; set; }
        }
    }
}