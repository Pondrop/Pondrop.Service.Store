using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pondrop.Service.Store.Application.Commands;
using Pondrop.Service.Store.Application.Queries;

namespace Pondrop.Service.Store.ApiControllers;

[ApiController]
[Route("[controller]")]
public class StoreTypeController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StoreTypeController> _logger;

    public StoreTypeController(
        IMediator mediator,
        ILogger<StoreTypeController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllStoreTypes()
    {
        var result = await _mediator.Send(new GetAllStoreTypesQuery());
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }
    
    [HttpGet]
    [Route("{id:guid}")]
    public async Task<IActionResult> GetStoreTypeById([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new GetStoreTypeByIdQuery() { Id = id });
        return result.Match<IActionResult>(
            i => i  is not null ? new OkObjectResult(i) : new NotFoundResult(),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateStoreTypeCommand([FromBody] CreateStoreTypeCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }
    
    [HttpPost]
    [Route("update")]
    public async Task<IActionResult> UpdateStoreType([FromBody] UpdateStoreTypeCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }
    
    [HttpPost]
    [Route("update/view")]
    public async Task<IActionResult> UpdateMaterializedView([FromBody] UpdateStoreTypeMaterializedViewByIdCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }
    
    [HttpPost]
    [Route("rebuild/view")]
    public async Task<IActionResult> RebuildMaterializedView()
    {
        var result = await _mediator.Send(new RebuildStoreTypeMaterializedViewCommand());
        return result.Match<IActionResult>(
            i => new OkResult(),
            (ex, msg) => new BadRequestObjectResult(msg));
    }
}