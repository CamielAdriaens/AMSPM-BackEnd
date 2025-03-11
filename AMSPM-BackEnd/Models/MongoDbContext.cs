// MongoDbContext.cs
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly IGridFSBucket _gridFS;

    public MongoDbContext(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
        _database = client.GetDatabase("AMSPM");
        _gridFS = new GridFSBucket(_database);
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<Customer> Customers => _database.GetCollection<Customer>("customers");
    public IMongoCollection<Quote> Quotes => _database.GetCollection<Quote>("quotes");

    // GridFS methoden
    public ObjectId UploadFile(string filename, Stream source) =>
        _gridFS.UploadFromStream(filename, source);

    public byte[] DownloadFile(ObjectId fileId)
    {
        return _gridFS.DownloadAsBytes(fileId);
    }
}

public class User
{
    public ObjectId Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
}

public class Customer
{
    public ObjectId Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
}

public class Quote
{
    public ObjectId Id { get; set; }
    public ObjectId CustomerId { get; set; }
    public string ProjectName { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public DateTime ValidUntil { get; set; }
    public ObjectId PdfFileId { get; set; } // Verwijzing naar GridFS-bestand
}
public class WorkOrder
{
    public ObjectId Id { get; set; }
    public ObjectId CustomerId { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }
    public ObjectId PdfFileId { get; set; } // Verwijzing naar GridFS-bestand
}
