using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Events.User;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateUserEmailCommandHandler : UserCommandHandler<UpdateUserEmailCommand, Result<UserRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateUserEmailCommand> _validator;    
    private readonly ILogger<UpdateUserEmailCommandHandler> _logger;

    public UpdateUserEmailCommandHandler(
        IOptions<UserUpdateConfiguration> userUpdateConfig,
        IEventRepository eventRepository,
        IDaprService daprService,
        IMapper mapper,
        IValidator<UpdateUserEmailCommand> validator,
        ILogger<UpdateUserEmailCommandHandler> logger) : base(userUpdateConfig, daprService, logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<UserRecord>> Handle(UpdateUserEmailCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update user failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<UserRecord>.Error(errorMessage);
        }

        var result = default(Result<UserRecord>);

        try
        {
            var userStream = await _eventRepository.LoadStreamAsync(EventEntity.GetStreamId<UserEntity>(command.Id));

            if (userStream.Version >= 0)
            {
                var user = new UserEntity(userStream.Events);
                user.Apply(new UpdateEmail(command.Email));
                var success = await _eventRepository.AppendEventsAsync(user.StreamId, user.AtSequence - 1, user.GetEvents(user.AtSequence));

                await InvokeDaprMethods(user.Id, user.GetEvents(user.AtSequence));
                
                result = success
                    ? Result<UserRecord>.Success(_mapper.Map<UserRecord>(user))
                    : Result<UserRecord>.Error("Failed to update user");
            }
            else
            {
                result = Result<UserRecord>.Error("User does not exist");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<UserRecord>.Error(ex);
        }

        return result;
    }
}