using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class SleepingBarber
{
    private readonly int maxWaitingChairs;
    private readonly Queue<Client> waitingClients = new Queue<Client>();
    private readonly SemaphoreSlim barberSemaphore = new SemaphoreSlim(0, 1);
    private readonly SemaphoreSlim clientSemaphore = new SemaphoreSlim(1, 1);
    private bool isBarberWorking = true;

    public SleepingBarber(int maxWaitingChairs)
    {
        this.maxWaitingChairs = maxWaitingChairs;
    }

    public void StartBarber()
    {
        Task.Run(() =>
        {
            while (isBarberWorking)
            {
                barberSemaphore.Wait();
                Client client = null;

                lock (waitingClients)
                {
                    if (waitingClients.Count > 0)
                    {
                        client = waitingClients.Dequeue();
                    }
                }

                if (client != null)
                {
                    CutHair(client);
                }
            }
        });
    }

    public bool TryAddClient(Client client)
    {
        clientSemaphore.Wait();
        try
        {
            if (waitingClients.Count < maxWaitingChairs)
            {
                waitingClients.Enqueue(client);
                barberSemaphore.Release();
                return true;
            }
            return false;
        }
        finally
        {
            clientSemaphore.Release();
        }
    }

    private void CutHair(Client client)
    {
        Console.WriteLine($"Парикмахер стрижет клиента {client.Id}");
        Thread.Sleep(new Random().Next(200, 600));
    }

    public void StopBarber()
    {
        isBarberWorking = false;
    }
}

public class Client
{
    public int Id { get; }

    public Client(int id)
    {
        Id = id;
    }
}