using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Events.User;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateUserNameCommandHandler : UserCommandHandler<UpdateUserNameCommand, Result<UserEntity>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IValidator<UpdateUserNameCommand> _validator;    
    private readonly ILogger<UpdateUserNameCommandHandler> _logger;

    public UpdateUserNameCommandHandler(
        IOptions<UserUpdateConfiguration> userUpdateConfig,
        IEventRepository eventRepository,
        IDaprService daprService,
        IValidator<UpdateUserNameCommand> validator,
        ILogger<UpdateUserNameCommandHandler> logger) : base(userUpdateConfig, daprService, logger)
    {
        _eventRepository = eventRepository;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<UserEntity>> Handle(UpdateUserNameCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update user failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<UserEntity>.Error(errorMessage);
        }

        var result = default(Result<UserEntity>);

        try
        {
            var userStream = await _eventRepository.LoadStreamAsync(EventEntity.GetStreamId<UserEntity>(command.Id));

            if (userStream.Version >= 0)
            {
                var user = new UserEntity(userStream.Events);
                user.Apply(new UpdateName(command.FirstName, command.LastName));
                var success = await _eventRepository.AppendEventsAsync(user.StreamId, user.AtSequence - 1, user.GetEvents(user.AtSequence));

                await InvokeDaprMethods(user.Id, user.GetEvents(user.AtSequence));
                
                result = success
                    ? Result<UserEntity>.Success(user)
                    : Result<UserEntity>.Error("Failed to update user");
            }
            else
            {
                result = Result<UserEntity>.Error("User does not exist");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<UserEntity>.Error(ex);
        }

        return result;
    }
}