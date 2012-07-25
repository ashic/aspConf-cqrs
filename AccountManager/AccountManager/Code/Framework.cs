using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AccountManager.Code
{
    public interface ICommandService
    {
        void Handle(object command);
    }

    public interface IRepository
    {
        T GetById<T>(object id) where T : Aggregate;
        void Save(Aggregate aggregate);
    }

    public interface IEventStore
    {
        void StoreEvents(object streamId, IEnumerable<object> events, long expectedInitialVersion);
        IEnumerable<object> LoadEvents(object id, long version = 0);
        IEnumerable<object> GetAllEventsEver();
    }

    public interface IPublisher
    {
        void Publish(IEnumerable<object> events);
    }

    public abstract class Aggregate
    {
        private readonly List<object> _uncommittedEvents = new List<object>();
        public object Id { get; protected set; }
        public long Version { get; private set; }

        protected Aggregate(object id)
        {
            Id = id;
        }

        protected Aggregate() { }

        public IEnumerable<object> GetUncommittedChanges()
        {
            return _uncommittedEvents;
        }

        internal void ClearUncommittedChanges()
        {
            _uncommittedEvents.Clear();
        }

        public void LoadFromHistory(IEnumerable<object> events)
        {
            foreach (var @event in events)
            {
                AggregateUpdater.Update(this, @event);
                Version++;
            }
        }

        protected void Apply(object @event)
        {
            _uncommittedEvents.Add(@event);
            AggregateUpdater.Update(this, @event);
            Version++;
        }

        private static class AggregateUpdater
        {
            private static readonly ConcurrentDictionary<Tuple<Type, Type>, Action<Aggregate, object>> Cache = new ConcurrentDictionary<Tuple<Type, Type>, Action<Aggregate, object>>();

            public static void Update(Aggregate instance, object @event)
            {
                var tuple = new Tuple<Type, Type>(instance.GetType(), @event.GetType());
                var action = Cache.GetOrAdd(tuple, ActionFactory);
                action(instance, @event);
            }

            private static Action<Aggregate, object> ActionFactory(Tuple<Type, Type> key)
            {
                var eventType = key.Item2;
                var aggregateType = key.Item1;

                const string methodName = "UpdateFrom";
                var method = aggregateType.GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SingleOrDefault(x => x.Name == methodName && x.GetParameters().Single().ParameterType.IsAssignableFrom(eventType));

                if (method == null)
                    return (x, y) => { };

                return (instance, @event) => method.Invoke(instance, new[] { @event });
            }
        }
    }

    public class DomainRepository : IRepository
    {
        readonly IEventStore _store;

        public DomainRepository(IEventStore store)
        {
            _store = store;
        }

        public T GetById<T>(object id) where T : Aggregate
        {
            var events = _store.LoadEvents(id);
            var aggregate = (T)Activator.CreateInstance(typeof(T), true);
            aggregate.LoadFromHistory(events);

            return aggregate;
        }

        public void Save(Aggregate aggregate)
        {
            var newEvents = aggregate.GetUncommittedChanges().ToList();
            var currentVersion = aggregate.Version;
            var initialVersion = currentVersion - newEvents.Count;

            _store.StoreEvents(aggregate.Id, newEvents, initialVersion);
            aggregate.ClearUncommittedChanges();
        }
    }

    public class MessageBus : IPublisher, ICommandService
    {
        readonly Dictionary<Type, List<Action<object>>> _handlers = new Dictionary<Type, List<Action<object>>>();

        public void Publish(IEnumerable<object> events)
        {
            foreach (var @event in events)
                PublishEvent(@event);
        }

        private void PublishEvent(object @event)
        {
            var type = @event.GetType();
            var keys = _handlers.Keys.Where(x => x.IsAssignableFrom(type));

            foreach (var key in keys)
                foreach (var handler in _handlers[key])
                    handler(@event);
        }

        public void RegisterHandler<T>(Action<T> handler)
        {
            List<Action<object>> handlers;

            if (!_handlers.TryGetValue(typeof(T), out handlers))
            {
                handlers = new List<Action<object>>();
                _handlers.Add(typeof(T), handlers);
            }

            handlers.Add(x => handler((T)x));
        }

        public void Handle(object command)
        {
            List<Action<object>> handlers;

            if (!_handlers.TryGetValue(command.GetType(), out handlers))
                throw new InvalidOperationException(string.Format("No handler registered for command type {0}", command.GetType()));
            if (handlers.Count != 1) throw new InvalidOperationException("Cannot send to more than one handler");

            handlers[0](command);
        }
    }
}