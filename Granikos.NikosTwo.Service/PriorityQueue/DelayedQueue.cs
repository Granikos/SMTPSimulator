using System;
using System.Diagnostics.Contracts;

namespace Granikos.NikosTwo.Service.PriorityQueue
{
    public class DelayedQueue<T>
    {
        private readonly HeapPriorityQueue<DateTime, QueueItem> _queue;

        public DelayedQueue(int maxItems)
        {
            Contract.Requires<ArgumentOutOfRangeException>(maxItems > 0, "maxItems");
            _queue = new HeapPriorityQueue<DateTime, QueueItem>(maxItems);
        }

        public event EventHandler OnQueueChanged;

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
            
            TiggerQueueChanged();
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
                var hasValue = Peek() != null;
                var value = hasValue ? _queue.Dequeue().Value : default(T);

                if (hasValue)
                {
                    TiggerQueueChanged();
                }
                    
                return value;
            }
        }

        public void Clear()
        {
            lock (_queue)
            {
                _queue.Clear();
            }

            TiggerQueueChanged();
        }

        private class QueueItem : PriorityQueueNode<DateTime>
        {
            public T Value { get; set; }
        }

        protected void TiggerQueueChanged()
        {
            var handler = OnQueueChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}