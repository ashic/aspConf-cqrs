using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using Newtonsoft.Json;

namespace AccountManager.Code
{
    public class SqlEventStore : IEventStore
    {
        readonly MessageBus _bus;
        const string ConnectionStringName = "SqlEventStore";

        public SqlEventStore(MessageBus bus)
        {
            _bus = bus;
        }

        public void StoreEvents(object streamId, IEnumerable<object> events, long expectedInitialVersion)
        {
            events = events.ToArray();

            var connectionString = ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString;
            var serializedEvents = events.Select(x=> new Tuple<string, string>(x.GetType().FullName, JsonConvert.SerializeObject(x)));
            
            using(var con = new SqlConnection(connectionString))
            {
                con.Open();
                
                const string commandText = "Select Top 1 CurrentSequence from Streams where StreamId = @StreamId;";
                long? existingSequence;
                using(var command = new SqlCommand(commandText, con))
                {
                    command.Parameters.AddWithValue("StreamId", streamId.ToString());
                    var current = command.ExecuteScalar();
                    existingSequence = current == null ? (long?) null : (long) current;

                    if(existingSequence != null && ((long)existingSequence) > expectedInitialVersion)
                        throw new ConcurrencyException();
                }

                using(var t = new TransactionScope())
                {
                    var nextVersion = insertEventsAndReturnLastVersion(streamId, con, expectedInitialVersion, serializedEvents);

                    if (existingSequence == null)
                        startNewSequence(streamId, nextVersion, con);
                    else
                        updateSequence(streamId, expectedInitialVersion, nextVersion, con);

                    t.Complete();
                }
            }

            _bus.Publish(events);
        }

        static void updateSequence(object streamId, long expectedInitialVersion, long nextVersion, SqlConnection con)
        {
            const string cmdText =
                "Update Streams SET CurrentSequence = @CurrentSequence WHERE StreamID = @StreamID AND CurrentSequence = @OriginalSequence;";
            using (var cmd = new SqlCommand(cmdText, con))
            {
                cmd.Parameters.AddWithValue("StreamID", streamId.ToString());
                cmd.Parameters.AddWithValue("CurrentSequence", nextVersion);
                cmd.Parameters.AddWithValue("OriginalSequence", expectedInitialVersion);

                var rows = cmd.ExecuteNonQuery();
                if (rows != 1)
                    throw new ConcurrencyException();
            }
        }

        static void startNewSequence(object streamId, long nextVersion, SqlConnection con)
        {
            const string cmdText = "Insert into Streams (StreamId, CurrentSequence) values (@StreamId, @CurrentSequence);";
            using (var cmd = new SqlCommand(cmdText, con))
            {
                cmd.Parameters.AddWithValue("StreamId", streamId.ToString());
                cmd.Parameters.AddWithValue("CurrentSequence", nextVersion);

                int rows = cmd.ExecuteNonQuery();
                if (rows != 1)
                {
                    throw new ConcurrencyException();
                }
            }
        }

        static long insertEventsAndReturnLastVersion(object streamId, SqlConnection con, long nextVersion, IEnumerable<Tuple<string, string>> serializedEvents)
        {
            foreach (var e in serializedEvents)
            {
                const string insertText =
                    "Insert into EventWrappers (EventId, StreamId, Sequence, TimeStamp, EventType, Body) values (@EventId, @StreamId, @Sequence, @TimeStamp, @EventType, @Body);";
                using (var command = new SqlCommand(insertText, con))
                {
                    command.Parameters.AddWithValue("EventId", Guid.NewGuid());
                    command.Parameters.AddWithValue("StreamId", streamId.ToString());
                    command.Parameters.AddWithValue("Sequence", ++nextVersion);
                    command.Parameters.AddWithValue("TimeStamp", DateTime.UtcNow);
                    command.Parameters.AddWithValue("EventType", e.Item1);
                    command.Parameters.AddWithValue("Body", e.Item2);

                    command.ExecuteNonQuery();
                }
            }

            return nextVersion;
        }

        public IEnumerable<object> LoadEvents(object id, long version = 0)
        {
            const string cmdText = "SELECT EventType, BODY from EventWrappers WHERE StreamId = @StreamId AND Sequence >= @Sequence ORDER BY TimeStamp";
            var connectionString = ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString;
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(cmdText, con))
            {
                cmd.Parameters.AddWithValue("StreamId", id.ToString());
                cmd.Parameters.AddWithValue("Sequence", version);

                cmd.Connection.Open();

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var eventTypeString = reader["EventType"].ToString();
                    var eventType = Type.GetType(eventTypeString);
                    var serializedBody = reader["Body"].ToString();
                    yield return JsonConvert.DeserializeObject(serializedBody, eventType);
                }
            }
        }

        public IEnumerable<object> GetAllEventsEver()
        {
            const string cmdText = "SELECT EventType, BODY from EventWrappers ORDER BY TimeStamp";
            var connectionString = ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString;
            using(var con = new SqlConnection(connectionString))
            using(var cmd = new SqlCommand(cmdText, con))
            {
                cmd.Connection.Open();

                var reader = cmd.ExecuteReader();
                while(reader.Read())
                {
                    var eventTypeString = reader["EventType"].ToString();
                    var eventType = Type.GetType(eventTypeString);
                    var serializedBody = reader["Body"].ToString();
                    yield return JsonConvert.DeserializeObject(serializedBody, eventType);
                }
            }
        }
    }
}