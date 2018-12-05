using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SRPGDemo.Utility
{
    public class TestNode : IComparable<TestNode>
    {
        public int value;

        public int CompareTo(TestNode other)
        {
            return value.CompareTo(other.value);
        }
    }

    internal static class Extensions
    {
        public static int Parent(this int idx)
        {
            return ((idx + 1) / 2) - 1;
        }

        public static int LeftChild(this int idx)
        {
            return (idx * 2) + 1;
        }

        public static int RightChild(this int idx)
        {
            return LeftChild(idx) + 1;
        }
    }

    public class PriorityQueue<T> where T : IComparable<T>
    {
        #region Queue content

        T[] content;
        int _count = 0;

        #endregion

        #region Constructors

        public PriorityQueue(int capacity = 0)
        {
            content = new T[capacity];
        }

        public PriorityQueue(IEnumerable<T> data)
        {
            content = data.ToArray() as T[];
            Count = data.Count();

            for (int idx = lastIdx.Parent(); idx >= 0; idx--)
            {
                SettleDown(idx);
            }
        }

        #endregion

        #region Public interface

        public bool isEmpty { get { return Count == 0; } }
        public int Count { get { return _count; } private set { _count = value; } }

        public void Enqueue(T newItem)
        {
            while (Count >= content.Length)
                Expand();

            // Add to bottom, rise appropriately
            content[Count++] = newItem;
            SettleUp();
        }

        public T Peek()
        {
            if (Count <= 0)
                throw new InvalidOperationException("Can't peek on an empty queue.");
            return content[rootIdx];
        }

        public T Dequeue()
        {
            if (Count <= 0)
                throw new InvalidOperationException("Can't pop from an empty queue.");

            // Save top item
            T result = Peek();

            // Move lowest node to highest spot, resettle.
            SwapEntries(rootIdx, lastIdx);
            Count -= 1;
            SettleDown(rootIdx);

            return result;
        }

        #endregion

        #region Heap management logic

        private static int rootIdx { get { return 0; } }
        private int lastIdx { get { return (Count > 0)?(Count - 1) :(0); } }

        private void Expand()
        {
            T[] newContent = new T[(content.Length * 2) + 1];
            Array.Copy(content, newContent, content.Length);
            content = newContent;
        }

        private void SwapEntries(int idx1, int idx2)
        {
            T swap = content[idx1];
            content[idx1] = content[idx2];
            content[idx2] = swap;
        }

        private bool InBounds(int idx)
        {
            return idx < Count;
        }

        private bool YoungerThan(int idx1, int idx2)
        {
            return content[idx1].CompareTo(content[idx2]) < 0;
        }

        private bool Invariant(int idx)
        {
            if (!InBounds(idx))
                return true;
            else if (InBounds(idx.LeftChild()) && YoungerThan(idx, idx.LeftChild()))
                return false;
            else if (InBounds(idx.RightChild()) && YoungerThan(idx, idx.RightChild()))
                return false;
            else
                return true;
        }

        private int EldestChild(int idx)
        {
            if (!InBounds(idx.LeftChild()))
                return idx;
            else if (!InBounds(idx.RightChild()))
                return idx.LeftChild();
            else if (YoungerThan(idx.LeftChild(), idx.RightChild()))
                return idx.RightChild();
            else
                return idx.LeftChild();
        }

        private void SettleUp()
        {
            int idx = lastIdx;
            while (idx > rootIdx)
            {
                if (YoungerThan(idx.Parent(), idx))
                {
                    SwapEntries(idx, idx.Parent());
                    idx = idx.Parent();
                }
                else break;
            }
        }

        private void SettleDown(int idx)
        {
            while (!Invariant(idx))
            {
                int swapIdx = EldestChild(idx);
                SwapEntries(EldestChild(idx), idx);
                idx = swapIdx;
            }
        }

        #endregion
    }
}
