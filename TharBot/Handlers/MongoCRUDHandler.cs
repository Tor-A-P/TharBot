using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using TharBot.DBModels;

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

        public async Task InsertRecordAsync<T>(string table, T record)
        {
            try
            {
                var collection = _db.GetCollection<T>(table);
                await collection.InsertOneAsync(record);
            }
            catch (Exception ex)
            {
                await LoggingHandler.LogCriticalAsync("database", null, ex);
            }
        }

        public async Task<List<T>>? LoadRecordsAsync<T>(string table)
        {
            try
            {
                var collection = _db.GetCollection<T>(table);
                return await collection.Find(x => true).ToListAsync();
            }
            catch (Exception ex)
            {
                await LoggingHandler.LogCriticalAsync("database", null, ex);
                return null;
            }
        }

        public async Task<T>? LoadRecordByIdAsync<T>(string table, ulong id)
        {
            try
            {
                var collection = _db.GetCollection<T>(table);
                var filter = Builders<T>.Filter.Eq("_id", id);

                return await collection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await LoggingHandler.LogCriticalAsync("database", null, ex);
                return default;
            }
        }
        
        public async Task<T>? LoadRecordByIdAsync<T>(string table, string id)
        {
            try
            {
                var collection = _db.GetCollection<T>(table);
                var filter = Builders<T>.Filter.Eq("_id", id);

                return await collection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await LoggingHandler.LogCriticalAsync("database", null, ex);
                return default;
            }
        }

        public async Task UpsertServerAsync<T>(string table, ulong id, UpdateDefinition<ServerSpecifics> update, UpdateOptions? options = null)
        {
            var collection = _db.GetCollection<ServerSpecifics>(table);
            UpdateResult updateResult;

            var revisionUpdate = Builders<ServerSpecifics>.Update
                .Inc(x => x.Revision, 1);
            var combinedUpdate = Builders<ServerSpecifics>.Update
                .Combine(update, revisionUpdate);

            do
            {
                var document = await collection.Find(x => x.ServerId == id).SingleAsync();

                updateResult = await collection.UpdateOneAsync(x => x.ServerId == id && x.Revision == document.Revision, combinedUpdate, options);
            } while (updateResult.ModifiedCount == 0);
        }

        public async Task UpsertUserAsync<T>(string table, ulong id, UpdateDefinition<GameUser> update, UpdateOptions? options = null)
        {
            var collection = _db.GetCollection<GameUser>(table);
            UpdateResult updateResult;

            var revisionUpdate = Builders<GameUser>.Update
                .Inc(x => x.Revision, 1);
            var combinedUpdate = Builders<GameUser>.Update
                .Combine(update, revisionUpdate);

            do
            {
                var document = await collection.Find(x => x.UserId == id).SingleAsync();

                updateResult = await collection.UpdateOneAsync(x => x.UserId == id && x.Revision == document.Revision, combinedUpdate, options);
            } while (updateResult.ModifiedCount == 0);
        }

        public async Task UpsertRecordAsync<T>(string table, ulong id, T record)
        {
            try
            {
                var collection = _db.GetCollection<T>(table);
                await collection.ReplaceOneAsync(
                    new BsonDocument("_id", (decimal)id),
                    record,
                    new ReplaceOptions { IsUpsert = true });
            }
            catch (Exception ex)
            {
                await LoggingHandler.LogCriticalAsync("database", null, ex);
            }
        }

        public async Task DeleteRecordAsync<T>(string table, ulong id)
        {
            try
            {
                var collection = _db.GetCollection<T>(table);
                var filter = Builders<T>.Filter.Eq("_id", id);

                collection.DeleteOne(filter);
            }
            catch (Exception ex)
            {
                await LoggingHandler.LogCriticalAsync("database", null, ex);
            }
        }
    }
}
