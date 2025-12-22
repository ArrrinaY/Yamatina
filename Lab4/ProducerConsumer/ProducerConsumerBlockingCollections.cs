using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Lab4;
    public class ProducerConsumerBlockingCollection
{
    private static BlockingCollection<int> buffer = new BlockingCollection<int>(5); 
    private static int itemsProduced = 0;
    private static int itemsConsumed = 0;
    private const int TotalItems = 20;

    public static void Run()
    {
        Thread producerThread = new Thread(Producer);
        Thread consumerThread = new Thread(Consumer);
        
        producerThread.Start();
        consumerThread.Start();
        
        producerThread.Join();
        consumerThread.Join();
        
        Console.WriteLine($"Произведено: {itemsProduced}, Потреблено: {itemsConsumed}");
    }

    private static void Producer()
    {
        for (int i = 1; i <= TotalItems; i++)
        {
            buffer.Add(i);
            Console.WriteLine($"Производитель добавил: {i} (в буфере: {buffer.Count})");
            itemsProduced++;
            Thread.Sleep(new Random().Next(100, 500));
        }
        buffer.CompleteAdding();
    }

    private static void Consumer()
    {
        foreach (var item in buffer.GetConsumingEnumerable())
        {
            Console.WriteLine($"Потребитель взял: {item} (в буфере: {buffer.Count})");
            itemsConsumed++;
            Thread.Sleep(new Random().Next(200, 700));
        }
    }
}


