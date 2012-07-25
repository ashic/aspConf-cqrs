using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AccountManager.Code
{
    public class InMemoryEventStore : IEventStore
    {
        readonly MessageBus bus;
        readonly Dictionary<object, List<object>> _eventsByAggregate;

        public InMemoryEventStore(MessageBus bus, Dictionary<object, List<object>> eventsByAggregate)
        {
            this.bus = bus;
            this._eventsByAggregate = eventsByAggregate;
        }

        public void StoreEvents(object streamId, IEnumerable<object> events, long expectedInitialVersion)
        {
            var es = events.ToList();

            if (_eventsByAggregate.ContainsKey(streamId))
                _eventsByAggregate[streamId].AddRange(es);
            else
                _eventsByAggregate[streamId] = es;

            bus.Publish(es);
        }

        public IEnumerable<object> LoadEvents(object id, long version = 0)
        {
            if (_eventsByAggregate.ContainsKey(id) == false)
                return new object[0];

            return _eventsByAggregate[id];
        }

        public IEnumerable<object> GetAllEventsEver()
        {
            return _eventsByAggregate.Values.SelectMany(x=>x);
        }
    }
}