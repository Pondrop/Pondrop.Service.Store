using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pondrop.Service.Store.Application.Commands;
using Pondrop.Service.Store.Application.Queries;

namespace Pondrop.Service.Store.ApiControllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserController> _logger;

    public UserController(
        IMediator mediator,
        ILogger<UserController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _mediator.Send(new GetAllUsersQuery());
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }
    
    [HttpGet]
    [Route("with/shoppinglists")]
    public async Task<IActionResult> GetAllUsersWithShoppingLists()
    {
        var result = await _mediator.Send(new GetAllUsersWithShoppingListsQuery());
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }
    
    [HttpGet]
    [Route("{id:guid}")]
    public async Task<IActionResult> GetUserById([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new GetUserByIdQuery() { Id = id });
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateUserCommand([FromBody] CreateUserCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }
    
    [HttpPost]
    [Route("update/name")]
    public async Task<IActionResult> UpdateUserName([FromBody] UpdateUserNameCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }
    
    [HttpPost]
    [Route("update/email")]
    public async Task<IActionResult> UpdateUserEmail([FromBody] UpdateUserEmailCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }
    
    [HttpPost]
    [Route("update/view")]
    public async Task<IActionResult> UpdateMaterializedView([FromBody] UpdateUserMaterializedViewCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }
}