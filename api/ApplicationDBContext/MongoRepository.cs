using MongoDB.Bson;
using MongoDB.Driver;

namespace api.ApplicationDBContext
{
    public class MongoRepository<T> where T : class
    {
        private readonly IMongoCollection<T> _collection;

        public MongoRepository(IMongoDatabase database, string collectionName)
        {
            _collection = database.GetCollection<T>(collectionName);
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<T> GetByIdAsync(string id)
        {
            var objectId = ObjectId.Parse(id);
            return await _collection.Find(Builders<T>.Filter.Eq("_id", objectId)).FirstOrDefaultAsync();
        }

        public async Task InsertAsync(T entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(string id, T entity)
        {
            var objectId = ObjectId.Parse(id);
            await _collection.ReplaceOneAsync(Builders<T>.Filter.Eq("_id", objectId), entity);
        }

        public async Task DeleteAsync(string id)
        {
            var objectId = ObjectId.Parse(id);
            await _collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", objectId));
        }
    }
}

