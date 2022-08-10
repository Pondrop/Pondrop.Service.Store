using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pondrop.Service.Store.Api.Services;
using Pondrop.Service.Store.Application.Commands;
using Pondrop.Service.Store.Application.Interfaces.ServiceBus;
using Pondrop.Service.Store.Application.Queries;
using System.Net;

namespace Pondrop.Service.Store.ApiControllers;

[ApiController]
[Route("[controller]")]
public class RetailerController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IServiceBusMessagingService<UpdateRetailerMaterializedViewByIdCommand> _updateRetailerServiceBusMessagingService;
    private readonly IServiceBusMessagingService<RebuildRetailerMaterializedViewCommand> _rebuildRetailerServiceBusMessagingService;
    private readonly ILogger<RetailerController> _logger;

    public RetailerController(
        IMediator mediator,
        IServiceBusMessagingService<UpdateRetailerMaterializedViewByIdCommand> updateRetailerServiceBusMessagingService,
        IServiceBusMessagingService<RebuildRetailerMaterializedViewCommand> rebuildRetailerServiceBusMessagingService,
        ILogger<RetailerController> logger)
    {
        _mediator = mediator;
        _updateRetailerServiceBusMessagingService = updateRetailerServiceBusMessagingService;
        _rebuildRetailerServiceBusMessagingService = rebuildRetailerServiceBusMessagingService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllRetailers()
    {
        var result = await _mediator.Send(new GetAllRetailersQuery());
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpGet]
    [Route("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRetailerById([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new GetRetailerByIdQuery() { Id = id });
        return result.Match<IActionResult>(
            i => i is not null ? new OkObjectResult(i) : new NotFoundResult(),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpPost]
    [Route("create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRetailer([FromBody] CreateRetailerCommand command)
    {
        var result = await _mediator.Send(command);
        return await result.Match<Task<IActionResult>>(
            async i =>
            {
                await _updateRetailerServiceBusMessagingService.SendMessageAsync(new UpdateRetailerMaterializedViewByIdCommand() { Id = i!.Id });
                await _updateRetailerServiceBusMessagingService.SendMessageAsync(new UpdateRetailerMaterializedViewByIdCommand() { Id = i!.Id });
                return StatusCode(StatusCodes.Status201Created, i);
            },
            async (ex, msg) => await Task.FromResult(new BadRequestObjectResult(msg)));
    }

    [HttpPost]
    [Route("update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateRetailer([FromBody] UpdateRetailerCommand command)
    {
        var result = await _mediator.Send(command);
        return await result.Match<Task<IActionResult>>(
       async i =>
       {
           await _updateRetailerServiceBusMessagingService.SendMessageAsync(new UpdateRetailerMaterializedViewByIdCommand() { Id = i!.Id });
           await _updateRetailerServiceBusMessagingService.SendMessageAsync(new UpdateRetailerMaterializedViewByIdCommand() { Id = i!.Id });
           return new OkObjectResult(i);
       },
       async (ex, msg) => await Task.FromResult(new BadRequestObjectResult(msg)));
    }

    [HttpPost]
    [Route("update/view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateMaterializedView([FromBody] UpdateRetailerMaterializedViewByIdCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpPost]
    [Route("rebuild/view")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> RebuildMaterializedView()
    {
        await _rebuildRetailerServiceBusMessagingService.SendMessageAsync(new RebuildRetailerMaterializedViewCommand());
        return new AcceptedResult();
    }
}