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
            var collection = _db.GetCollection<T>(table);
            collection.InsertOne(record);
        }

        public List<T> LoadRecords<T>(string table)
        {
            var collection = _db.GetCollection<T>(table);
            return collection.Find(new BsonDocument()).ToList();
        }

        public T LoadRecordById<T>(string table, ulong id)
        {
            var collection = _db.GetCollection<T>(table);
            var filter = Builders<T>.Filter.Eq("_id", id);

            return collection.Find(filter).FirstOrDefault();
        }

        public T LoadRecordById<T>(string table, Guid id)
        {
            var collection = _db.GetCollection<T>(table);
            var filter = Builders<T>.Filter.Eq("_id", id);

            return collection.Find(filter).FirstOrDefault();
        }

        public T LoadRecordById<T>(string table, string id)
        {
            var collection = _db.GetCollection<T>(table);
            var filter = Builders<T>.Filter.Eq("_id", id);

            return collection.Find(filter).FirstOrDefault();
        }

        public void UpsertRecord<T>(string table, ulong id, T record)
        {
            var collection = _db.GetCollection<T>(table);
            collection.ReplaceOne(
                new BsonDocument("_id", (decimal)id),
                record,
                new ReplaceOptions { IsUpsert = true });
        }
        public void UpsertRecord<T>(string table, Guid id, T record)
        {
            var collection = _db.GetCollection<T>(table);
            collection.ReplaceOne(
                new BsonDocument("_id", id),
                record,
                new ReplaceOptions { IsUpsert = true });
        }
        public void UpsertRecord<T>(string table, string id, T record)
        {
            var collection = _db.GetCollection<T>(table);
            collection.ReplaceOne(
                new BsonDocument("_id", id),
                record,
                new ReplaceOptions { IsUpsert = true });
        }

        public void DeleteRecord<T>(string table, ulong id)
        {
            var collection = _db.GetCollection<T>(table);
            var filter = Builders<T>.Filter.Eq("_id", id);

            collection.DeleteOne(filter);
        }
        public void DeleteRecord<T>(string table, Guid id)
        {
            var collection = _db.GetCollection<T>(table);
            var filter = Builders<T>.Filter.Eq("_id", id);

            collection.DeleteOne(filter);
        }
        public void DeleteRecord<T>(string table, string id)
        {
            var collection = _db.GetCollection<T>(table);
            var filter = Builders<T>.Filter.Eq("_id", id);

            collection.DeleteOne(filter);
        }
    }
}
