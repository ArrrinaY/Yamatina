namespace Models;

public class LinkedListNode<T>
{
    public T Value { get; set; }
    public LinkedListNode<T>? Next { get; internal set; }
    public LinkedListNode<T>? Previous { get; internal set; }
    
    public LinkedListNode(T value)
    {
        Value = value;
    }
}