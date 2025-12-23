using System;

namespace CollectionsPerformanceLab;

class Program
{
    static void Main()
    {
        Console.WriteLine("Сравнение производительности коллекций");
        
        new ListTester().Test();
        new LinkedListTester().Test();
        new QueueTester().Test();
        new StackTester().Test();
        new ImmutableListTester().Test();
    }
}