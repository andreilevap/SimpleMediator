using LevapTech.SimpleMediator.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register SimpleMediator and handlers
builder.Services.AddSingleton<ISimpleMediator, SimpleMediator>();
builder.Services.AddTransient<ICommandHandler<CreateOrderCommand, Guid>, CreateOrderCommandHandler>();
builder.Services.AddTransient<IQueryHandler<GetOrderByIdQuery, Order?>, GetOrderByIdQueryHandler>();
builder.Services.AddTransient<IEventHandler<OrderCreatedEvent>, OrderCreatedEventHandler>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Redirect root URL "/" to "/swagger"
app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

app.MapControllers();
app.Run();