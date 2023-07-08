using Microsoft.Extensions.Options;
using NLog.Web;
using RESTMicroservice.Configuration;
using RESTMicroservice.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Logging.ClearProviders();
builder.Host.UseNLog();

//TODO почитать по DependencyIngection контейнер в ASP NET Core
builder.WebHost.ConfigureKestrel((context, serverOptions) =>
{
    var kestrelSection = context.Configuration.GetSection("Http");
    serverOptions.Configure(kestrelSection);
});

builder.Host.ConfigureServices((hostBuilderContext, serviceCollection) =>
{
    serviceCollection.AddOptions<StorageConfiguration>()
        .Bind(hostBuilderContext.Configuration.GetSection("Storage"))
        .Validate(storageConfiguration => storageConfiguration.SelfValidate());

    serviceCollection.AddSingleton<IStorage>(sp =>
    {
        var options = sp.GetRequiredService<IOptions<StorageConfiguration>>();
        var storageConfiguration = options.Value;

        IStorage storage = storageConfiguration.Type switch
        {
            StorageType.DB => new DBStorage(storageConfiguration.DB!),
            StorageType.FileSystem => new FileSystemStorage(storageConfiguration.FileSystem!),
            _ => throw new ArgumentOutOfRangeException(nameof(storageConfiguration.Type), storageConfiguration.Type, "Value not supported")
        };

        return storage;
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();