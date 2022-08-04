using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pondrop.Service.Store.Application.Commands;
using Pondrop.Service.Store.Application.Queries;

namespace Pondrop.Service.Store.ApiControllers;

[ApiController]
[Route("[controller]")]
public class TodoController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TodoController> _logger;

    public TodoController(
        IMediator mediator,
        ILogger<TodoController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [Route("items/{category}")]
    public async Task<IActionResult> GetItemsByCategory([FromRoute] string category)
    {
        var result = await _mediator.Send(new GetTodoByCategoryQuery()
        {
            Category = category
        });

        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpGet]
    [Route("items/{category}/{id}")]
    public async Task<IActionResult> GetSingleItem([FromRoute] string category, [FromRoute] Guid id)
    {
        var result = await _mediator.Send(new GetTodoByIdQuery()
        {
            Category = category,
            Id = id
        });

        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpPost]
    [Route("items/create")]
    public async Task<IActionResult> CreateTodoCommand([FromBody] CreateTodoCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }
}