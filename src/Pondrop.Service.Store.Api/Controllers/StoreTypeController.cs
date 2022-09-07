using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Pondrop.Service.Store.Api.Services;
using Pondrop.Service.Store.Api.Services.Interfaces;
using Pondrop.Service.Store.Application.Commands;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Queries;

namespace Pondrop.Service.Store.ApiControllers;

[ApiController]
[Route("[controller]")]
public class StoreTypeController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IServiceBusService _serviceBusService;
    private readonly IRebuildCheckpointQueueService _rebuildCheckpointQueueService;
    private readonly ILogger<StoreTypeController> _logger;
    private readonly ITokenProvider _jwtTokenProvider;

    public StoreTypeController(
        IMediator mediator,
        ITokenProvider jWTTokenProvider,
        IServiceBusService serviceBusService,
        IRebuildCheckpointQueueService rebuildCheckpointQueueService,
        ILogger<StoreTypeController> logger)
    {
        _mediator = mediator;
        _serviceBusService = serviceBusService;
        _jwtTokenProvider = jWTTokenProvider;
        _rebuildCheckpointQueueService = rebuildCheckpointQueueService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllStoreTypes()
    {
        var claimsPrincipal = _jwtTokenProvider.ValidateToken(Request?.Headers[HeaderNames.Authorization] ?? string.Empty);
        if (claimsPrincipal is null)
            return new UnauthorizedResult();

        var result = await _mediator.Send(new GetAllStoreTypesQuery());
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpGet]
    [Route("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetStoreTypeById([FromRoute] Guid id)
    {
        var claimsPrincipal = _jwtTokenProvider.ValidateToken(Request?.Headers[HeaderNames.Authorization] ?? string.Empty);
        if (claimsPrincipal is null)
            return new UnauthorizedResult();

        var result = await _mediator.Send(new GetStoreTypeByIdQuery() { Id = id });
        return result.Match<IActionResult>(
            i => i is not null ? new OkObjectResult(i) : new NotFoundResult(),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpPost]
    [Route("create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateStoreType([FromBody] CreateStoreTypeCommand command)
    {
        var claimsPrincipal = _jwtTokenProvider.ValidateToken(Request?.Headers[HeaderNames.Authorization] ?? string.Empty);
        if (claimsPrincipal is null)
            return new UnauthorizedResult();

        var result = await _mediator.Send(command);
        return await result.MatchAsync<IActionResult>(
            async i =>
            {
                await _serviceBusService.SendMessageAsync(new UpdateStoreTypeCheckpointByIdCommand() { Id = i!.Id });
                return StatusCode(StatusCodes.Status201Created, i);
            },
            (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    }

    [HttpPost]
    [Route("update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStoreType([FromBody] UpdateStoreTypeCommand command)
    {
        var claimsPrincipal = _jwtTokenProvider.ValidateToken(Request?.Headers[HeaderNames.Authorization] ?? string.Empty);
        if (claimsPrincipal is null)
            return new UnauthorizedResult();

        var result = await _mediator.Send(command);
        return await result.MatchAsync<IActionResult>(
            async i =>
            {
                await _serviceBusService.SendMessageAsync(new UpdateStoreTypeCheckpointByIdCommand() { Id = i!.Id });
                return new OkObjectResult(i);
            },
            (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    }

    [HttpPost]
    [Route("update/checkpoint")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCheckpoint([FromBody] UpdateStoreTypeCheckpointByIdCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpPost]
    [Route("rebuild/checkpoint")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public IActionResult RebuildCheckpoint()
    {
        _rebuildCheckpointQueueService.Queue(new RebuildStoreTypeCheckpointCommand());
        return new AcceptedResult();
    }
}