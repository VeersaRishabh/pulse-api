using Microsoft.Extensions.Options;
using MongoDB.Driver;
using api;
using MongoDB.Bson;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add MongoDB configuration
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDB"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);
});

var app = builder.Build();

// Database initialization (insert a dummy document to ensure database and collection are created)
var mongoClient = new MongoClient(builder.Configuration["MongoDB:ConnectionString"]);
var database = mongoClient.GetDatabase(builder.Configuration["MongoDB:DatabaseName"]);
var collection = database.GetCollection<BsonDocument>("TestCollection");

if (!await collection.Find(_ => true).AnyAsync())
{
    collection.InsertOne(new BsonDocument { { "TestField", "TestValue" } });
    Console.WriteLine("Inserted a dummy document to initialize the database and collection.");
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.UseCors(options =>
{
    options.AllowAnyHeader();
    options.AllowAnyMethod();
    options.AllowAnyOrigin();
});

app.MapControllers();

app.Run();
