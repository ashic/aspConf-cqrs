using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AccountManager.Code;
using AccountManager.Models.Domain;
using AccountManager.Tests;

namespace Formatter
{
    class Program
    {
        static int Main()
        {
            return new Program().Run();
        }

        bool _suuccess = true;

        int Run()
        {
            var testTypes = typeof(AccountShouldBeLockedAfterThreeOverdraws).Assembly.GetTypes().Where(x => typeof(TestBase).IsAssignableFrom(x) && x.IsAbstract == false);
            foreach (var type in testTypes)
            {
                var test = Activator.CreateInstance(type) as TestBase;

                var newMessages = new List<object>();
                var bus = new MessageBus();
                bus.RegisterHandler<object>(newMessages.Add);

                var eventStore = new InMemoryEventStore(bus, test.GivenTheseEvents());
                var repository = new DomainRepository(eventStore);

                test.RegisterHandler(bus, repository);
                printGivens(type, test);

                var command = test.WhenThisHappens();
                Exception exception = null;
                var expectedException = test.ThisExceptionShouldOccur();

                try
                {
                    bus.Handle(command);
                }
                catch(Exception e)
                {
                    exception = e;
                }

                var foundException = printException(expectedException, exception, command);

                if(foundException)
                    continue; ;


                printThens(test.TheseEventsShouldOccur().ToList(), newMessages, command);

                Console.WriteLine();
                Console.WriteLine(new string('=', 40));
                Console.WriteLine();
            }

            return _suuccess ? 0 : -1;
        }

        void printThens(List<object> expectedEvents, List<object> actualEvents, object command)
        {
            var comparer = new KellermanSoftware.CompareNetObjects.CompareObjects();
            if(!comparer.Compare(expectedEvents, actualEvents))
            {
                _suuccess = false;
                printCommand(command, ConsoleColor.Red);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Then - ");
                Console.WriteLine("Expected: ");
                Console.ForegroundColor = ConsoleColor.Red;
                foreach(var e in expectedEvents)
                    Console.WriteLine(e);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Found: ");
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var e in actualEvents)
                    Console.WriteLine(e);
            }
            else
            {
                printCommand(command, ConsoleColor.Green);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Then - ");
                Console.ForegroundColor = ConsoleColor.Green;
                foreach (var e in expectedEvents)
                    Console.WriteLine(e);
            }
        }

        static void printGivens(Type type, TestBase test)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(type.Name);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Given - ");
            Console.ForegroundColor = ConsoleColor.Green;

            var events = test.GivenTheseEvents().Values.SelectMany(x => x).ToList();

            foreach (var e in events)
                Console.WriteLine(e);
        }

        bool printException(Exception expectedException, Exception exception, object command)
        {
            if (exception == null && expectedException == null)
                return false;

            if (expectedException == null)
            {
                _suuccess = false;
                printCommand(command, ConsoleColor.Red);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Then - ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Did not expect any exception.");
                Console.WriteLine("Found: {0}", exception.Message);
                return true;
            }

            if (exception == null)
            {
                _suuccess = false;
                printCommand(command, ConsoleColor.Red);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Then - ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Expected: {0}", expectedException.Message);
                Console.WriteLine("Did not find any exception.");
                return true;
            }

            if(expectedException.GetType().IsAssignableFrom(exception.GetType()))
            {
                if(exception.Message == expectedException.Message)
                {
                    printCommand(command, ConsoleColor.Green);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Then - ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Expected: {0}", exception.Message);
                    return true;
                }
            }

            _suuccess = false;
            printCommand(command, ConsoleColor.Red);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Then - ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Expected: {0}", expectedException.Message);
            Console.WriteLine("Found: {0}", exception.Message);

            return true;
        }

        static void printCommand(object command, ConsoleColor color)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("When - ");
            Console.ForegroundColor = color;

            Console.WriteLine(command);
        }
    }
}
