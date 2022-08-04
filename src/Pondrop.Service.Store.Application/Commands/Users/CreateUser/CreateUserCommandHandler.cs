using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class CreateUserCommandHandler : UserCommandHandler<CreateUserCommand, Result<UserRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateUserCommand> _validator;    
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(
        IOptions<UserUpdateConfiguration> userUpdateConfig,
        IEventRepository eventRepository,
        IDaprService daprService,
        IMapper mapper,
        IValidator<CreateUserCommand> validator,
        ILogger<CreateUserCommandHandler> logger) : base(userUpdateConfig, daprService, logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<UserRecord>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create user failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<UserRecord>.Error(errorMessage);
        }

        var result = default(Result<UserRecord>);

        try
        {
            var user = new UserEntity(command.FirstName, command.LastName, command.Email);
            var success = await _eventRepository.AppendEventsAsync(user.StreamId, 0, user.GetEvents());

            await InvokeDaprMethods(user.Id, user.GetEvents());
            
            result = success
                ? Result<UserRecord>.Success(_mapper.Map<UserRecord>(user))
                : Result<UserRecord>.Error("Failed to create user");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<UserRecord>.Error(ex);
        }

        return result;
    }
}