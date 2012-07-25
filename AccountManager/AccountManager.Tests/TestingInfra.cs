using System;
using System.Collections.Generic;
using AccountManager.Code;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;
using System.Linq;

namespace AccountManager.Tests
{
    [TestFixture]
    public abstract class TestBase
    {
        public abstract Dictionary<object, List<object>> GivenTheseEvents();
        public abstract object WhenThisHappens();
        public virtual IEnumerable<object> TheseEventsShouldOccur() {yield break;}
        public virtual Exception ThisExceptionShouldOccur() { return null; }
        public abstract void RegisterHandler(MessageBus bus, IRepository repo);

        [Test]
        public void Test()
        {
            var newMessages = new List<object>();
            var bus = new MessageBus();
            bus.RegisterHandler<object>(newMessages.Add);

            var eventStore = new InMemoryEventStore(bus, GivenTheseEvents());
            var repository = new DomainRepository(eventStore);

            RegisterHandler(bus, repository);


            try
            {
                bus.Handle(WhenThisHappens());
            }
            catch(Exception e)
            {
                var expectedException = ThisExceptionShouldOccur();
                if(expectedException == null)
                    Assert.Fail(e.Message);

                if(expectedException.GetType().IsAssignableFrom(e.GetType()))
                {
                    Assert.AreEqual(expectedException.Message, e.Message);
                    return;
                }
                
                Assert.Fail("Expected exception of type {0} with message {1} but got exception of type {2} with message {3}", expectedException.GetType(), expectedException.Message, e.GetType(), e.Message);
            }

            var expectedEvents = TheseEventsShouldOccur().ToList();
            
            var comparer = new CompareObjects();

            if(!comparer.Compare(expectedEvents, newMessages))
            {
                Assert.Fail(comparer.DifferencesString);
            }           
        }
    }
}