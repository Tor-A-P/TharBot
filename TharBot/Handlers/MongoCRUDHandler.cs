using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace TharBot.Handlers
{
    public class MongoCRUDHandler
    {
        private readonly IMongoDatabase _db;
        private readonly IConfiguration _config;

        public MongoCRUDHandler(string database, IConfiguration config)
        {
            _config = config;
            var client = new MongoClient(_config["MongoDB ConnectionString"]);
            _db = client.GetDatabase(database);
        }

        public void InsertRecord<T>(string table, T record)
        {
            try
            {
                var collection = _db.GetCollection<T>(table);
                collection.InsertOne(record);
            }
            catch (Exception ex)
            {
                LoggingHandler.LogCriticalAsync("database", null, ex);
            }
        }

        public List<T>? LoadRecords<T>(string table)
        {
            try
            {
                var collection = _db.GetCollection<T>(table);
                return collection.Find(new BsonDocument()).ToList();
            }
            catch (Exception ex)
            {
                LoggingHandler.LogCriticalAsync("database", null, ex);
                return null;
            }
        }

        public T? LoadRecordById<T>(string table, ulong id)
        {
            try
            {
                var collection = _db.GetCollection<T>(table);
                var filter = Builders<T>.Filter.Eq("_id", id);

                return collection.Find(filter).FirstOrDefault();
            }
            catch (Exception ex)
            {
                LoggingHandler.LogCriticalAsync("database", null, ex);
                return default;
            }
        }

        public T? LoadRecordById<T>(string table, Guid id)
        {
            try
            {
                var collection = _db.GetCollection<T>(table);
                var filter = Builders<T>.Filter.Eq("_id", id);

                return collection.Find(filter).FirstOrDefault();
            }
            catch (Exception ex)
            {
                LoggingHandler.LogCriticalAsync("database", null, ex);
                return default;
            }
        }

        public T? LoadRecordById<T>(string table, string id)
        {
            try
            {
                var collection = _db.GetCollection<T>(table);
                var filter = Builders<T>.Filter.Eq("_id", id);

                return collection.Find(filter).FirstOrDefault();
            }
            catch (Exception ex)
            {
                LoggingHandler.LogCriticalAsync("database", null, ex);
                return default;
            }
        }

        public void UpsertRecord<T>(string table, ulong id, T record)
        {
            try
            {
                var collection = _db.GetCollection<T>(table);
                collection.ReplaceOne(
                    new BsonDocument("_id", (decimal)id),
                    record,
                    new ReplaceOptions { IsUpsert = true });
            }
            catch (Exception ex)
            {
                LoggingHandler.LogCriticalAsync("database", null, ex);
            }
        }

        public void UpsertRecord<T>(string table, Guid id, T record)
        {
            try
            {
                var collection = _db.GetCollection<T>(table);
                collection.ReplaceOne(
                    new BsonDocument("_id", id),
                    record,
                    new ReplaceOptions { IsUpsert = true });
            }
            catch (Exception ex)
            {
                LoggingHandler.LogCriticalAsync("database", null, ex);
            }
        }

        public void UpsertRecord<T>(string table, string id, T record)
        {
            try
            {
                var collection = _db.GetCollection<T>(table);
                collection.ReplaceOne(
                    new BsonDocument("_id", id),
                    record,
                    new ReplaceOptions { IsUpsert = true });
            }
            catch (Exception ex)
            {
                LoggingHandler.LogCriticalAsync("database", null, ex);
            }
        }

        public void DeleteRecord<T>(string table, ulong id)
        {
            try
            {
                var collection = _db.GetCollection<T>(table);
                var filter = Builders<T>.Filter.Eq("_id", id);

                collection.DeleteOne(filter);
            }
            catch (Exception ex)
            {
                LoggingHandler.LogCriticalAsync("database", null, ex);
            }
        }

        public void DeleteRecord<T>(string table, Guid id)
        {
            try
            {
                var collection = _db.GetCollection<T>(table);
                var filter = Builders<T>.Filter.Eq("_id", id);

                collection.DeleteOne(filter);
            }
            catch (Exception ex)
            {
                LoggingHandler.LogCriticalAsync("database", null, ex);
            }
        }

        public void DeleteRecord<T>(string table, string id)
        {
            try
            {
                var collection = _db.GetCollection<T>(table);
                var filter = Builders<T>.Filter.Eq("_id", id);

                collection.DeleteOne(filter);
            }
            catch (Exception ex)
            {
                LoggingHandler.LogCriticalAsync("database", null, ex);
            }
        }
    }
}
