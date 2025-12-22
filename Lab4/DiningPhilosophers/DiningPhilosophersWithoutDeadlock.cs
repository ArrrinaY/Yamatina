using System;
using System.Threading;

public class DiningPhilosophersWithoutDeadlock
{
    private const int PhilosopherCount = 5;
    private readonly object[] forks = new object[PhilosopherCount];
    private readonly Thread[] philosophers = new Thread[PhilosopherCount];
    private readonly SemaphoreSlim tableSemaphore = new SemaphoreSlim(PhilosopherCount - 1, PhilosopherCount - 1);

    public DiningPhilosophersWithoutDeadlock()
    {
        for (int i = 0; i < PhilosopherCount; i++)
        {
            forks[i] = new object();
        }
    }

    public void Start()
    {
        for (int i = 0; i < PhilosopherCount; i++)
        {
            int id = i;
            philosophers[i] = new Thread(() => PhilosopherLife(id));
            philosophers[i].Start();
        }
    }

    private void PhilosopherLife(int philosopherId)
    {
        int leftFork = philosopherId;
        int rightFork = (philosopherId + 1) % PhilosopherCount;

        while (true)
        {
            Think(philosopherId);
            tableSemaphore.Wait();
            lock (forks[leftFork])
            {
                lock (forks[rightFork])
                {
                    Eat(philosopherId);
                }
            }
            tableSemaphore.Release();
        }
    }

    private void Think(int philosopherId)
    {
        Console.WriteLine($"Философ {philosopherId} думает");
        Thread.Sleep(new Random().Next(100, 500));
    }

    private void Eat(int philosopherId)
    {
        Console.WriteLine($"Философ {philosopherId} ест");
        Thread.Sleep(new Random().Next(100, 500));
    }
}