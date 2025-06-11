using System;
using System.Collections.Generic;
using UnityEngine;

public class BinaryHeap<T> where T : IComparable<T>
{
    private List<T> binaryHeap;

    public BinaryHeap()
    {
        binaryHeap = new List<T>();
    }

    public int Count => binaryHeap.Count;

    public void Insert(T item)
    {
        binaryHeap.Add(item);
        SortUp(Count - 1);
    }

    public T ExtractMin()
    {
        if (binaryHeap.Count == 0)
            Debug.LogError("Heap is empty.");


        T min = binaryHeap[0];
        binaryHeap[0] = binaryHeap[binaryHeap.Count - 1];
        binaryHeap.RemoveAt(binaryHeap.Count - 1);

        if (binaryHeap.Count > 0)
            SortDown(0);

        return min;
    }

    public T Peek()
    {
        if (binaryHeap.Count == 0)
            Debug.LogError("Heap is empty.");

        return binaryHeap[0];
    }

    public bool Contains(T item)
    {
        return binaryHeap.Contains(item);
    }

    private void SortUp(int index)
    {
        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;
            if (binaryHeap[index].CompareTo(binaryHeap[parentIndex]) >= 0)
                break;

            Swap(index, parentIndex);
            index = parentIndex;
        }
    }

    private void SortDown(int index)
    {

        while (true)
        {
            int leftChildIndex = 2 * index + 1;
            int rightChildIndex = 2 * index + 2;
            int smallestIndex = index;

            if (leftChildIndex < binaryHeap.Count && binaryHeap[leftChildIndex].CompareTo(binaryHeap[smallestIndex]) < 0)
                smallestIndex = leftChildIndex;

            if (rightChildIndex < binaryHeap.Count && binaryHeap[rightChildIndex].CompareTo(binaryHeap[smallestIndex]) < 0)
                smallestIndex = rightChildIndex;

            if (smallestIndex == index)
                break;

            Swap(index, smallestIndex);
            index = smallestIndex;
        }
    }

    private void Swap(int indexA, int indexB)
    {
        (binaryHeap[indexB], binaryHeap[indexA]) = (binaryHeap[indexA], binaryHeap[indexB]);
    }

    public void UpdateItem(T item)
    {
        int itemHeapIndex = binaryHeap.FindIndex(heapItem => heapItem.Equals(item)); // Could be optimized by storing a heapIndex in the Node when building the grid
        if (itemHeapIndex != -1)
        {
            SortUp(itemHeapIndex);
        }
    }
}
