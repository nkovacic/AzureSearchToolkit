# AzureSearchToolkit
 AzureSearchToolkit is a lightweight set of tools for Azure Search to quickly build search functionalities for .NET Standard web apps. It leans heavily on [ElasticSearchLINQ](https://github.com/ElasticLINQ/ElasticLINQ).
 
 It features a rich LINQ provider which allows to write strongly typed queries based on your derived context and entity classes. A representation of the LINQ query is passed to the AzureSearch provider, to be translated in ODATA query language.

## How to use
1. Include latest package into your project using [NuGet](https://www.nuget.org/packages/AzureSearchToolkit/).

2. First define your model that will be used for your search index.
```csharp
[SerializePropertyNamesAsCamelCase]
class Listing
{
    [Key]
    [IsFilterable]
    public string Id { get; set; }

    [IsFilterable, IsSortable]
    public DateTime? CreatedAt { get; set; }

    [IsSearchable]
    public string Description { get; set; }

    [IsFacetable, IsFilterable, IsSortable]
    public double Price { get; set; }

    [IsFilterable, IsSearchable, IsSortable]
    public string Title { get; set; }

    [IsFilterable]
    public bool Active { get; set; }

    [IsFilterable]
    public string[] Tags { get; set; }

    [IsFilterable, IsSortable]
    public GeographyPoint Place { get; set; }
}
```
3. Use AzureSearchToolkit using direct instantiation.
```csharp
using AzureSearchToolkit;

...

using (var connection = new AzureSearchConnection("<your search instance name>", 
     "<your search instance key>", "<name of search index>"))
{
    var context = new AzureSearchContext(connection);

    //Return filtered, ordered, paginated results from Azure Search index using LINQ
    var listings = await context
        .Query<Listing>()
        .Where(q => q.Title.Contains("Your title"))
        .OrderByDescending(q => q.CreatedAt)
        .Skip(10)
        .Take(25)
        .ToListAsync();

    var firstListing = listings.First();

    firstListing.Title = "Different title";

    //Update entity
    var wasUpdateSuccessfull = await context.UpdateAsync(firstListing);

    //Delete entity
    var wasDeleteSuccessFull = await context.RemoveAsync(firstListing);

    var newListing = new Listing
    {
        Id = Guid.NewGuid(),
        Title = "Title"
    };

    //Create or update entity
    var wasAddingSuccessfull = await context.AddOrUpdateAsync(newListing);
}
```

or 

3. Use AzureSearchToolkit using Autofac dependency injection.
```csharp
using AzureSearchToolkit;

namespace Toolkit
{
    public class AzureSearchToolkitModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(ctx =>
                {
                    return new AzureSearchConnection(
                        "<your search instance name>", 
                        "<your search instance key>", 
                        "<name of search index>"
                        );
                })
                .As<IAzureSearchConnection>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<AzureSearchContext>()
                .As<IAzureSearchContext>().InstancePerLifetimeScope();
        }
    }
}
```

4. Inject IAzureSearchContext into your class
```csharp
using AzureSearchToolkit;

namespace Toolkit
{
    public class Example 
    {
        private IAzureSearchContext _context;

        public AzureSearchToolkitExample(IAzureSearchContext context) 
        {
            _context = context;
        }

        public void UseAzureSearchContext() 
        {
            //Return filtered, ordered, paginated results from Azure Search index using LINQ
            var listings = await _context
                .Query<Listing>()
                .Where(q => q.Title.Contains("Your title"))
                .OrderByDescending(q => q.CreatedAt)
                .Skip(10)
                .Take(25)
                .ToListAsync();
            }
        }
    }
}
```
