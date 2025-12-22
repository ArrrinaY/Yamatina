using System;
using System.Collections.Generic;
using System.Threading;

namespace Lab4;

    public class ProducerConsumerSemaphore
{
    private const int BufferSize = 5;
    private static Queue<int> buffer = new Queue<int>();
    private static object bufferLock = new object();
    private static SemaphoreSlim emptySlots = new SemaphoreSlim(BufferSize, BufferSize);
    private static SemaphoreSlim filledSlots = new SemaphoreSlim(0, BufferSize);
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
            emptySlots.Wait(); 
            
            lock (bufferLock)
            {
                buffer.Enqueue(i);
                Console.WriteLine($"Производитель добавил: {i} (в буфере: {buffer.Count})");
                itemsProduced++;
            }
            
            filledSlots.Release();
            Thread.Sleep(new Random().Next(100, 500));
        }
    }

    private static void Consumer()
    {
        for (int i = 0; i < TotalItems; i++)
        {
            filledSlots.Wait(); 
            
            lock (bufferLock)
            {
                int item = buffer.Dequeue();
                Console.WriteLine($"Потребитель взял: {item} (в буфере: {buffer.Count})");
                itemsConsumed++;
            }
            
            emptySlots.Release(); 
            Thread.Sleep(new Random().Next(200, 700));
        }
    }
}

