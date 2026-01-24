using Bogus;
using Nest;

class Program
{
    private static ElasticClient CreateClient()
    {
        var settings = new ConnectionSettings(new Uri("http://81.17.96.160:9200/"))
            .DefaultIndex("default-index");
        return new ElasticClient(settings);
    }

    static async Task Main()
    {
        var client = CreateClient();

        var indexes = new List<string> { "products", "people", "orders", "companies", "reviews", "categories", "addresses", "transactions", "books", "movies" };

        foreach (var index in indexes)
        {
            Console.WriteLine($"Seeding index: {index}");

            if ((await client.Indices.ExistsAsync(index)).Exists)
            {
                await client.Indices.CreateAsync(index);
            }

            await SeedIndex(client, index, 1_000_000);
        }

        Console.WriteLine("Seeding completed!");
    }

    private static async Task SeedIndex(ElasticClient client, string index, int count)
    {
        var batchSize = 10_000;
        int batches = count / batchSize;

        for (int i = 0; i < batches; i++)
        {
            var documents = GenerateData(index, batchSize);

            var bulkResponse = await client.BulkAsync(b => b
                .Index(index)
                .IndexMany(documents)
            );

            if (bulkResponse.Errors)
                Console.WriteLine($"Error in batch {i}: {bulkResponse.ItemsWithErrors.Count()}");
            else
                Console.WriteLine($"Batch {i + 1}/{batches} inserted successfully");
        }
    }

    private static IEnumerable<object> GenerateData(string index, int count)
    {
        switch (index)
        {
            case "products":
                return new Faker<Product>()
                    .RuleFor(p => p.Id, f => Guid.NewGuid().ToString())
                    .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                    .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
                    .RuleFor(p => p.Price, f => f.Random.Decimal(5, 2000))
                    .RuleFor(p => p.SKU, f => f.Commerce.Ean13())
                    .RuleFor(p => p.Category, f => f.Commerce.Categories(1)[0])
                    .RuleFor(p => p.Brand, f => f.Company.CompanyName())
                    .RuleFor(p => p.Rating, f => f.Random.Decimal(1, 5))
                    .RuleFor(p => p.Stock, f => f.Random.Int(0, 500))
                    .RuleFor(p => p.CreatedAt, f => f.Date.Past(3))
                    .RuleFor(p => p.Tags, f => f.Lorem.Words(5))
                    .Generate(count);

            case "people":
                return new Faker<Person>()
                    .RuleFor(p => p.Id, f => Guid.NewGuid().ToString())
                    .RuleFor(p => p.FirstName, f => f.Name.FirstName())
                    .RuleFor(p => p.LastName, f => f.Name.LastName())
                    .RuleFor(p => p.Email, f => f.Internet.Email())
                    .RuleFor(p => p.Username, f => f.Internet.UserName())
                    .RuleFor(p => p.Password, f => f.Internet.Password())
                    .RuleFor(p => p.Phone, f => f.Phone.PhoneNumber())
                    .RuleFor(p => p.Address, f => f.Address.FullAddress())
                    .RuleFor(p => p.City, f => f.Address.City())
                    .RuleFor(p => p.Country, f => f.Address.Country())
                    .RuleFor(p => p.DateOfBirth, f => f.Date.Past(50, DateTime.Today.AddYears(-18)))
                    .RuleFor(p => p.Company, f => f.Company.CompanyName())
                    .Generate(count);

            case "orders":
                return new Faker<Order>()
                    .RuleFor(o => o.Id, f => Guid.NewGuid().ToString())
                    .RuleFor(o => o.OrderNumber, f => f.Commerce.Ean13())
                    .RuleFor(o => o.ProductName, f => f.Commerce.ProductName())
                    .RuleFor(o => o.Quantity, f => f.Random.Int(1, 10))
                    .RuleFor(o => o.UnitPrice, f => f.Random.Decimal(5, 1000))
                    .RuleFor(o => o.TotalPrice, (f, o) => o.Quantity * o.UnitPrice)
                    .RuleFor(o => o.Status, f => f.PickRandom(new[] { "Pending", "Shipped", "Delivered", "Cancelled" }))
                    .RuleFor(o => o.CustomerName, f => f.Name.FullName())
                    .RuleFor(o => o.CustomerEmail, f => f.Internet.Email())
                    .RuleFor(o => o.OrderDate, f => f.Date.Past(2))
                    .RuleFor(o => o.ShippingAddress, f => f.Address.FullAddress())
                    .RuleFor(o => o.ShippingCost, f => f.Random.Decimal(0, 50))
                    .RuleFor(o => o.Discount, f => f.Random.Decimal(0, 100))
                    .Generate(count);

            default:
                return new Faker<GenericDocument>()
                    .RuleFor(g => g.Id, f => Guid.NewGuid().ToString())
                    .RuleFor(g => g.Content, f => f.Lorem.Sentences(3))
                    .Generate(count);
        }
    }
}

// Data models with 10+ properties
class Product
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string SKU { get; set; }
    public string Category { get; set; }
    public string Brand { get; set; }
    public decimal Rating { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; }
    public string[] Tags { get; set; }
}

class Person
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Company { get; set; }
}

class Order
{
    public string Id { get; set; }
    public string OrderNumber { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; }
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public DateTime OrderDate { get; set; }
    public string ShippingAddress { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Discount { get; set; }
}

class GenericDocument
{
    public string Id { get; set; }
    public string Content { get; set; }
}
