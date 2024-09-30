using Integration.Service;
using Integration.Service.LockManagement.Handler;
using Integration.Service.LockManagement.Service;

namespace Integration;

public abstract class Program
{
    public static void Main(string[] args)
    {

        /*
            IF YOU WANT TO RUN WITH DOCKER
            
            FIRST COMMAND

                docker network create redis-network

            SECOND COMMAND
                
                docker-compose build
            
            THIRD COMMAND
                
                docker-compose up
         */


        /* NOTES 

         1. SaveItem method in ItemOperationBackend changed 40_000 millisecondsTimeout (40 sn) for testing and realizing real world scenario. Also tests may take a LONG TIME avarage 60-80 seconds
            
         2. Run all tests separately, because test asserts are written accordingly

         3. Call CreateLocalService method for Single Server Scenario and you can use the other implementation CreateRedisServer or CreateMultiRedisServer for Single Server Scenario

         4. Call CreateMultiRedisServer (recommended solution) or CreateRedisServer Distributed System Scenario
        */


        var service = CreateMultiRedisServer(); // CreateMultiRedisServer Or CreateRedisServer

        ConcurrentRequestTest(service);
        Thread.Sleep(1000);
        Console.WriteLine("TEST COMPLETED");
        Console.WriteLine("Everything recorded:");
        service.GetAllItems().ForEach(Console.WriteLine);

        //DifferentContentConcurrentRequestTest(service);
        //Thread.Sleep(1000);
        //Console.WriteLine("TEST COMPLETED");
        //Console.WriteLine("Everything recorded:");
        //service.GetAllItems().ForEach(Console.WriteLine);

        //DuplicateRequestTest(service);
        //Thread.Sleep(1000);
        //Console.WriteLine("TEST COMPLETED");
        //Console.WriteLine("Everything recorded:");
        //service.GetAllItems().ForEach(Console.WriteLine);

        Console.ReadLine();

    }

    // for redlock algorithm, this is the recommended solution
    static ItemIntegrationService CreateMultiRedisServer()
    {
        string[] connectionStrings = new string[5]
        {
            "redis1:6379,abortConnect=false,connectTimeout=10000",
            "redis2:6379,abortConnect=false,connectTimeout=10000",
            "redis3:6379,abortConnect=false,connectTimeout=10000",
            "redis4:6379,abortConnect=false,connectTimeout=10000",
            "redis5:6379,abortConnect=false,connectTimeout=10000"
        };

        return new ItemIntegrationService(new LockHandler(new LockService(new RedisRedLockService(connectionStrings))));
    }
    static ItemIntegrationService CreateRedisServer()
    {
        string[] connectionStrings = new string[1]
        {
            "redis1:6379,abortConnect=false,connectTimeout=10000"
        };

        return new ItemIntegrationService(new LockHandler(new LockService(new RedisRedLockService(connectionStrings))));
    }

    // Used In-Memory cache 
    static ItemIntegrationService CreateLocalService()
    {
        return new ItemIntegrationService(new LockHandler(new LockService(new LocalLockService())));
    }
    // 1. Same content concurrent request test
    static void ConcurrentRequestTest(ItemIntegrationService service)
    {
        string content = "test-content";

        for (int i = 1; i < 10; i++)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                var result = service.SaveItem(content);
            });
        }

        // Wait complete all request, 60 seconds is selected according to the lock release time.
        Thread.Sleep(60_000);

        bool passed =
            service.GetAllItems().Count == 1
            &&
            service.GetAllItems().Select(p => p.Content == "test-content").First();

        Console.WriteLine($"\u001b[31m[TEST]\u001b[0m ConcurrentRequestTest TEST PASSED = {passed}");
    }
    // 2. Different content with concurrent request test
    static void DifferentContentConcurrentRequestTest(ItemIntegrationService service)
    {
        for (int i = 1; i < 5; i++)
        {
            string content = $"test-content-{i}";

            ThreadPool.QueueUserWorkItem(_ =>
            {
                var result = service.SaveItem(content);
            });
        }

        // Wait complete all request, 60 seconds is selected according to the lock release time.
        Thread.Sleep(60_000);

        var ids = service.GetAllItems().Select(p => p.Id).ToList();

        bool passed =
            service.GetAllItems().Count == 4
            &&
            ids.Count == ids.Distinct().Count();

        Console.WriteLine($"\u001b[31m[TEST]\u001b[0m DifferentContentConcurrentRequestTest TEST PASSED = {passed}");
    }

    // 3. Same content duplicate request test
    static void DuplicateRequestTest(ItemIntegrationService service)
    {
        string content = "duplicate-content";

        // First request have to success
        var result1 = service.SaveItem(content);

        // Second request have to unsuccess (already saved duplicated data)
        var result2 = service.SaveItem(content);

        // 60 seconds is selected according to the lock release time.
        Thread.Sleep(60_000);
        bool passed =
            result1.Success == true
            &&
            result2.Success == false;
        Console.WriteLine($"\u001b[31m[TEST]\u001b[0m DuplicateRequestTest TEST PASSED = {passed}");
    }
}