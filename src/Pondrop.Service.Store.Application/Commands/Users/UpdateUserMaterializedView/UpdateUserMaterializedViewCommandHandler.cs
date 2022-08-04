using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Events.User;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateUserMaterializedViewCommandHandler : IRequestHandler<UpdateUserMaterializedViewCommand, Result<UserEntity>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMaterializedViewRepository<UserEntity> _userMaterializedViewRepository;
    private readonly IValidator<UpdateUserMaterializedViewCommand> _validator;    
    private readonly ILogger<UpdateUserMaterializedViewCommandHandler> _logger;

    public UpdateUserMaterializedViewCommandHandler(
        IEventRepository eventRepository,
        IMaterializedViewRepository<UserEntity> userMaterializedViewRepository,
        IValidator<UpdateUserMaterializedViewCommand> validator,
        ILogger<UpdateUserMaterializedViewCommandHandler> logger)
    {
        _eventRepository = eventRepository;
        _userMaterializedViewRepository = userMaterializedViewRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<UserEntity>> Handle(UpdateUserMaterializedViewCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update user materialized view failed, errors on validation {validation}";
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
                user = await _userMaterializedViewRepository.UpsertAsync(user);

                result = user is not null && user.Id != command.Id
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