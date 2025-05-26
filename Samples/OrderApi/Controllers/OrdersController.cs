using LevapTech.SimpleMediator.Abstractions;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ISimpleMediator _mediator;

    public OrdersController(ISimpleMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
    {
        var orderId = await _mediator.SendAsync<CreateOrderCommand, Guid>(command);
        await _mediator.PublishAsync(new OrderCreatedEvent(orderId));
        return CreatedAtAction(nameof(GetById), new { id = orderId }, new { orderId });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var order = await _mediator.QueryAsync<GetOrderByIdQuery, Order?>(new GetOrderByIdQuery(id));
        if (order == null) return NotFound();
        return Ok(order);
    }
}
