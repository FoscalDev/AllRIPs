using AllRIPs.DTOS;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AllRIPs.SERVICES
{
    public class MongoService
    {
        private readonly string DATABASE = "dbMinistry";
        private readonly string COLLECTION = "ResponseData";
        private readonly IMongoDatabase dataBase;

        public MongoService(IConfiguration config)
        {
            var connectionString = config.GetValue<string>("MongoDB:Client"); //config.GetConnectionString("MongoDB:Client");

            var mongoClient = new MongoClient(connectionString);
            dataBase = mongoClient.GetDatabase(DATABASE);
        }

        public async Task SaveResponseMinistry(ResponseMongoDTO data)
        {
            var collection = dataBase.GetCollection<ResponseMongoDTO>(COLLECTION);
            await collection.InsertOneAsync(data);
        }

        public async Task<ResponseMongoDTO> GetDataMinistry(string numFactura)
        {
            var collection = dataBase.GetCollection<ResponseMongoDTO>("ResponseData");

            var response = await collection
                .Find(x => x.numFactura == numFactura)
                .Project<ResponseMongoDTO>(Builders<ResponseMongoDTO>.Projection.Exclude("_id"))
                .SortByDescending(x => x.CreateDate)
                .FirstOrDefaultAsync();

            return response;
        }
    }
}
