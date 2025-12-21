using Xunit;
using Models;
using System.Collections.Generic;

namespace Tests;

public class LinkedListTests
{
    [Fact]
    public void Add_ShouldIncreaseCount()
    {
        var list = new Models.LinkedList<int>();
        
        list.Add(1);
        list.Add(2);
        list.Add(3);
        
        Assert.Equal(3, list.Count);
    }
    
    [Fact]
    public void AddFirst_ShouldAddToBeginning()
    {
        var list = new Models.LinkedList<int>();
        list.Add(2);
        list.Add(3);
        
        list.AddFirst(1);
        
        Assert.Equal(1, list[0]);
        Assert.Equal(3, list.Count);
    }
    
    [Fact]
    public void AddLast_ShouldAddToEnd()
    {
        var list = new Models.LinkedList<int>();
        list.Add(1);
        list.Add(2);
        
        list.AddLast(3);
        
        Assert.Equal(3, list[2]);
        Assert.Equal(3, list.Count);
    }
    
    [Fact]
    public void Remove_ShouldDecreaseCount()
    {
        var list = new Models.LinkedList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);
        
        bool removed = list.Remove(2);
        
        Assert.True(removed);
        Assert.Equal(2, list.Count);
        Assert.False(list.Contains(2));
    }
    
    [Fact]
    public void RemoveFirst_ShouldRemoveFirstElement()
    {
        var list = new Models.LinkedList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);
        
        list.RemoveFirst();
        
        Assert.Equal(2, list[0]);
        Assert.Equal(2, list.Count);
    }
    
    [Fact]
    public void RemoveLast_ShouldRemoveLastElement()
    {
        var list = new Models.LinkedList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);
        
        list.RemoveLast();
        
        Assert.Equal(2, list[1]);
        Assert.Equal(2, list.Count);
    }
    
    [Fact]
    public void Insert_ShouldAddItemAtSpecifiedIndex()
    {
        var list = new Models.LinkedList<int>();
        list.Add(1);
        list.Add(3);
        
        list.Insert(1, 2);
        
        Assert.Equal(3, list.Count);
        Assert.Equal(2, list[1]);
        Assert.Equal(3, list[2]);
    }
    
    [Fact]
    public void Foreach_ShouldIterateAllItems()
    {
        var list = new Models.LinkedList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);
        
        int sum = 0;
        foreach (var item in list)
        {
            sum += item;
        }
        
        Assert.Equal(6, sum);
    }
    
    [Fact]
    public void Indexer_GetAndSetWork()
    {
        var list = new Models.LinkedList<int>();
        list.Add(5);
        list.Add(10);
        list.Add(15);
        
        Assert.Equal(10, list[1]);
        list[1] = 12;
        Assert.Equal(12, list[1]);
    }
    
    [Fact]
    public void Contains_ReturnsTrueForExistingItem()
    {
        var list = new Models.LinkedList<string>();
        list.Add("cat");
        list.Add("dog");
        list.Add("bird");
        
        Assert.True(list.Contains("dog"));
        Assert.False(list.Contains("fish"));
    }
    
    [Fact]
    public void Clear_RemovesAllItems()
    {
        var list = new Models.LinkedList<string>();
        list.Add("a");
        list.Add("b");
        list.Add("c");
        list.Add("d");
        list.Add("e");
        
        list.Clear();
        
        Assert.Equal(0, list.Count);
    }
    
    [Fact]
    public void IndexOf_ReturnsCorrectIndex()
    {
        var list = new Models.LinkedList<string>();
        list.Add("first");
        list.Add("second");
        list.Add("third");
        
        Assert.Equal(1, list.IndexOf("second"));
        Assert.Equal(-1, list.IndexOf("fourth"));
    }
    
    [Fact]
    public void CopyTo_CopiesToArray()
    {
        var list = new Models.LinkedList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);
        
        var array = new int[5];
        list.CopyTo(array, 2);
        
        Assert.Equal(0, array[0]);
        Assert.Equal(0, array[1]);
        Assert.Equal(1, array[2]);
        Assert.Equal(2, array[3]);
        Assert.Equal(3, array[4]);
    }
    
    [Fact]
    public void RemoveAt_RemovesAtIndex()
    {
        var list = new Models.LinkedList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);
        
        list.RemoveAt(1);
        
        Assert.Equal(2, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(3, list[1]);
        Assert.False(list.Contains(2));
    }
    
    [Fact]
    public void IsReadOnly_ReturnsFalse()
    {
        var list = new Models.LinkedList<int>();
        Assert.False(list.IsReadOnly);
    }
}