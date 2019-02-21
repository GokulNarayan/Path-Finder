using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//must guarantee that the type T variable follows the interface.
public class Heap<T> where T : IHeapItem<T> {

    /// <summary>
    /// contains the array of the nodes (Type T generic)
    /// </summary>
    T[] items;

    /// <summary>
    /// used for keeping track of current item count.
    /// </summary>
    int currentItemCount;

    //each item in the heap should be able to hold its own index in the heap.
    // the items should be able to compare with each other. 


    public Heap(int maxheapSize)
    {
        items = new T[maxheapSize];
    }

    //check if the item being passed is in the heap.
    public bool Contains(T item)
    {
        return Equals(items[item.HeapIndex], item);
    }

    //used to update nodes position in the heap.
    //if the fcost of a node is changed due to a new path being found,
    public void UpdateItem(T item)
    {
        SortUp(item);
    }

    public int Count
    {
        get
        {

            return currentItemCount;
        }
    }

    public T RemoveFirst()
    {
        T firstItem = items[0];
        //once removed, the heap has one less item.
        currentItemCount--;
        //place the last item in the heap and get its righ position.
        items[0] = items[currentItemCount];
        //set that items heap index to be zero.
        items[0].HeapIndex = 0;
        //sort the heap going down.
        SortDown(items[0]);
        return firstItem;
    }

    void SortDown(T item)
    {
        while (true)
        {
            //get indices of items children
            int childIndexLeft = (2 * item.HeapIndex) + 1;
            int childIndexRight = (2 * item.HeapIndex) + 2;
            int swapIndex = 0;

            //check if the item has atleast one child on the right.
            if (childIndexLeft < currentItemCount)
            {
                swapIndex = childIndexLeft;


                //check if it does have a second child.
                if (childIndexRight < currentItemCount)
                {
                    //check which one should be swapped(the one with the lower fcost)
                    if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
                    {
                        //basically if childIndexLeft is lower priority than childIndex Right. => lower priority means higher fCost (it doesn't need to go up)
                        swapIndex = childIndexRight;
                    }
                }

                    //check if the item is actually lower priority than its children.
                    if (item.CompareTo(items[swapIndex]) < 0)
                    {
                        Swap(item, items[swapIndex]);
                    }
                    else
                    {
                        //if nothing is requied
                        return;
                    }
                }
           
            else
            {
                //if the item has no children.
                return;
            }


        }
    }

    public void Add(T item)
    {
        //add it to the last position.
        item.HeapIndex = currentItemCount;
        
        items[currentItemCount] = item;

        //sort up the item
        SortUp(item);
        //since an item is being added, increase the current count.
        currentItemCount++;
    }

    void SortUp(T item)
    {
        //find the index of the parent of the item.
        int parentIndex = (item.HeapIndex - 1) / 2;

        while (true)
        {
            T parentItem = items[parentIndex];

            //if the parentItem has a higher priority than the child it is being compared to,
            //in this code, it will compare the fCost of both the items.
            if (item.CompareTo(parentItem)>0)
            {
                Swap(item, parentItem);
            }else
            {
                break;
            }

            // set the next parentIndex:
            parentIndex = (item.HeapIndex - 1) / 2;

        }
    }

    void Swap(T itemA,T itemB)
    {
        //swap both the values.
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;
        //swap the indexes in the array.
        int itemAIndex = itemA.HeapIndex;
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = itemAIndex;

    }
   

}

//this interface will make sure that the type T item must have a HeapIndex.
//this item can be compared to another.
public interface IHeapItem<T> : IComparable<T>{

    int HeapIndex
    {
        get;
        set;
    }
}
