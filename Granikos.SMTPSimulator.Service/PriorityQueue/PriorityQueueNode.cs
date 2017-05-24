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
namespace Granikos.SMTPSimulator.Service.PriorityQueue
{
    public class PriorityQueueNode<K>
    {
        /// <summary>
        ///     The Priority to insert this node at.  Must be set BEFORE adding a node to the queue
        /// </summary>
        public K Priority { get; set; }

        /// <summary>
        ///     <b>Used by the priority queue - do not edit this value.</b>
        ///     Represents the order the node was inserted in
        /// </summary>
        public long InsertionIndex { get; set; }

        /// <summary>
        ///     <b>Used by the priority queue - do not edit this value.</b>
        ///     Represents the current position in the queue
        /// </summary>
        public int QueueIndex { get; set; }
    }
}