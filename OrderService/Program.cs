var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);
builder.Services.AddAWSService<Amazon.SQS.IAmazonSQS>();
// Bind ProductServiceUrl from config (appsettings, or env var override)
var productServiceUrl = builder.Configuration["Services:ProductServiceUrl"];
if (string.IsNullOrWhiteSpace(productServiceUrl))
{
    throw new InvalidOperationException(
        "Services:ProductServiceUrl is not configured. Set it via appsettings.json locally, " +
        "or the Services__ProductServiceUrl environment variable in Lambda.");
}

builder.Services.AddHttpClient("ProductService", client =>
{
    client.BaseAddress = new Uri(productServiceUrl);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => Results.Ok("OrderService is running"));

app.Run();