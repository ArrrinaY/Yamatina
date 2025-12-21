using System.Collections;

namespace Models;

public class LinkedList<T> : IEnumerable<T>, ICollection<T>, IList<T>
{
    private LinkedListNode<T>? _head;
    private LinkedListNode<T>? _tail;
    private int _count;
    
    public LinkedList()
    {
    }
    
    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= _count)
                throw new ArgumentOutOfRangeException(nameof(index));
            
            var node = GetNodeAt(index);
            return node.Value;
        }
        set
        {
            if (index < 0 || index >= _count)
                throw new ArgumentOutOfRangeException(nameof(index));
            
            var node = GetNodeAt(index);
            node.Value = value;
        }
    }
    
    public int Count => _count;
    
    public bool IsReadOnly => false;
    
    public void Add(T item)
    {
        AddLast(item);
    }
    
    public void AddLast(T item)
    {
        var newNode = new LinkedListNode<T>(item);
        
        if (_tail == null)
        {
            _head = newNode;
            _tail = newNode;
        }
        else
        {
            _tail.Next = newNode;
            newNode.Previous = _tail;
            _tail = newNode;
        }
        
        _count++;
    }
    
    public void AddFirst(T item)
    {
        var newNode = new LinkedListNode<T>(item);
        
        if (_head == null)
        {
            _head = newNode;
            _tail = newNode;
        }
        else
        {
            newNode.Next = _head;
            _head.Previous = newNode;
            _head = newNode;
        }
        
        _count++;
    }
    
    public void Clear()
    {
        var current = _head;
        while (current != null)
        {
            var next = current.Next;
            current.Previous = null;
            current.Next = null;
            current = next;
        }
        
        _head = null;
        _tail = null;
        _count = 0;
    }
    
    public bool Contains(T item)
    {
        return Find(item) != null;
    }
    
    public void CopyTo(T[] array, int arrayIndex)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0 || arrayIndex > array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < _count)
            throw new ArgumentException("Недостаточно места в массиве");
        
        var current = _head;
        int index = arrayIndex;
        while (current != null)
        {
            array[index++] = current.Value;
            current = current.Next;
        }
    }
    
    public IEnumerator<T> GetEnumerator()
    {
        var current = _head;
        while (current != null)
        {
            yield return current.Value;
            current = current.Next;
        }
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    public int IndexOf(T item)
    {
        int index = 0;
        var current = _head;
        
        while (current != null)
        {
            if (EqualityComparer<T>.Default.Equals(current.Value, item))
                return index;
            
            current = current.Next;
            index++;
        }
        
        return -1;
    }
    
    public void Insert(int index, T item)
    {
        if (index < 0 || index > _count)
            throw new ArgumentOutOfRangeException(nameof(index));
        
        if (index == 0)
        {
            AddFirst(item);
        }
        else if (index == _count)
        {
            AddLast(item);
        }
        else
        {
            var currentNode = GetNodeAt(index);
            var newNode = new LinkedListNode<T>(item);
            
            newNode.Previous = currentNode.Previous;
            newNode.Next = currentNode;
            
            if (currentNode.Previous != null)
                currentNode.Previous.Next = newNode;
            
            currentNode.Previous = newNode;
            _count++;
        }
    }
    
    public bool Remove(T item)
    {
        var node = Find(item);
        if (node != null)
        {
            RemoveNode(node);
            return true;
        }
        return false;
    }
    
    public void RemoveAt(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));
        
        var node = GetNodeAt(index);
        RemoveNode(node);
    }
    
    public void RemoveFirst()
    {
        if (_head == null)
            throw new InvalidOperationException("Список пуст");
        
        RemoveNode(_head);
    }
    
    public void RemoveLast()
    {
        if (_tail == null)
            throw new InvalidOperationException("Список пуст");
        
        RemoveNode(_tail);
    }
    
    private LinkedListNode<T>? Find(T item)
    {
        var current = _head;
        while (current != null)
        {
            if (EqualityComparer<T>.Default.Equals(current.Value, item))
                return current;
            
            current = current.Next;
        }
        return null;
    }
    
    private LinkedListNode<T> GetNodeAt(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));
        
        LinkedListNode<T> current;
        if (index < _count / 2)
        {
            current = _head!;
            for (int i = 0; i < index; i++)
                current = current!.Next;
        }
        else
        {
            current = _tail!;
            for (int i = _count - 1; i > index; i--)
                current = current!.Previous;
        }
        
        return current!;
    }
    
    private void RemoveNode(LinkedListNode<T> node)
    {
        if (node.Previous != null)
            node.Previous.Next = node.Next;
        else
            _head = node.Next;
        
        if (node.Next != null)
            node.Next.Previous = node.Previous;
        else
            _tail = node.Previous;
        
        _count--;
    }
}