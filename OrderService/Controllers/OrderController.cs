using Microsoft.AspNetCore.Mvc;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAmazonSQS _sqsClient;
    private readonly IConfiguration _configuration;
    private static readonly List<Order> _orders = new();

    public OrdersController(IHttpClientFactory httpClientFactory, IAmazonSQS sqsClient, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _sqsClient = sqsClient;
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<ActionResult<Order>> Create([FromBody] Order order)
    {
        var client = _httpClientFactory.CreateClient("ProductService");

        // Check the product exists via Product Service
        var response = await client.GetAsync($"/api/products/{order.ProductId}");
        if (!response.IsSuccessStatusCode)
        {
            return BadRequest($"Product {order.ProductId} not found.");
        }

        order.Id = _orders.Count + 1;
        order.Status = "Confirmed";
        _orders.Add(order);
        // Publish OrderCreated event to SQS — fire and forget, doesn't block the response
        var queueUrl = _configuration["Aws:OrderEventsQueueUrl"];
        if (!string.IsNullOrWhiteSpace(queueUrl))
        {
            var messageBody = JsonSerializer.Serialize(new
            {
                EventType = "OrderCreated",
                OrderId = order.Id,
                order.ProductId,
                order.Quantity,
                Timestamp = DateTime.UtcNow
            });

            await _sqsClient.SendMessageAsync(new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = messageBody
            });
        }
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpGet("{id}")]
    public ActionResult<Order> GetById(int id)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        return order is null ? NotFound() : Ok(order);
    }
}