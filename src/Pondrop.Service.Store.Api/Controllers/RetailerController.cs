using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pondrop.Service.Store.Application.Commands;
using Pondrop.Service.Store.Application.Queries;

namespace Pondrop.Service.Store.ApiControllers;

[ApiController]
[Route("[controller]")]
public class RetailerController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RetailerController> _logger;

    public RetailerController(
        IMediator mediator,
        ILogger<RetailerController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllRetailers()
    {
        var result = await _mediator.Send(new GetAllRetailersQuery());
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }
    
    [HttpGet]
    [Route("{id:guid}")]
    public async Task<IActionResult> GetRetailerById([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new GetRetailerByIdQuery() { Id = id });
        return result.Match<IActionResult>(
            i => i is not null ? new OkObjectResult(i) : new NotFoundResult(),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateRetailerCommand([FromBody] CreateRetailerCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }
    
    [HttpPost]
    [Route("update")]
    public async Task<IActionResult> UpdateRetailer([FromBody] UpdateRetailerCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }
    
    [HttpPost]
    [Route("update/view")]
    public async Task<IActionResult> UpdateMaterializedView([FromBody] UpdateRetailerMaterializedViewByIdCommand command)
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
        var result = await _mediator.Send(new RebuildRetailerMaterializedViewCommand());
        return result.Match<IActionResult>(
            i => new OkResult(),
            (ex, msg) => new BadRequestObjectResult(msg));
    }
}