using System;
using System.Diagnostics.Contracts;

namespace HydraService.PriorityQueue
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
            _queue.Enqueue(new QueueItem {Value = item}, DateTime.Now + delay);
        }

        public T Peek()
        {
            return _queue.First != null ? _queue.First.Value : default(T);
        }

        public T Dequeue()
        {
            return _queue.Dequeue().Value;
        }

        public void Clear()
        {
            _queue.Clear();
        }

        private class QueueItem : PriorityQueueNode<DateTime>
        {
            public T Value { get; set; }
        }
    }
}