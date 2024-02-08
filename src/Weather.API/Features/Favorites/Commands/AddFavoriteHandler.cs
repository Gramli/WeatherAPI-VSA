﻿using Ardalis.GuardClauses;
using Validot;
using Weather.API.Features.Favorites.Abstractions;
using Weather.API.Features.Favorites.Commands;
using Weather.API.Resources;
using Weather.API.Shared.Extensions;
using WeatherApi.Shared.Http;
using WeatherApi.Shared.Logging;

namespace Weather.Core.Features.Favorites.Commands
{
    internal sealed class AddFavoriteHandler : IAddFavoriteHandler
    {
        private readonly IValidator<AddFavoriteCommand> _addFavoriteCommandValidator;
        private readonly ILogger<IAddFavoriteHandler> _logger;
        private readonly IFavoritesCommandsRepository _weatherCommandsRepository;
        public AddFavoriteHandler(IFavoritesCommandsRepository weatherCommandsRepository, IValidator<AddFavoriteCommand> addFavoriteCommandValidator, ILogger<IAddFavoriteHandler> logger)
        {
            _weatherCommandsRepository = Guard.Against.Null(weatherCommandsRepository);
            _addFavoriteCommandValidator = Guard.Against.Null(addFavoriteCommandValidator);
            _logger = Guard.Against.Null(logger);
        }

        public async Task<HttpDataResponse<bool>> HandleAsync(AddFavoriteCommand request, CancellationToken cancellationToken)
        {
            if (!_addFavoriteCommandValidator.IsValid(request))
            {
                return HttpDataResponses.AsBadRequest<bool>(string.Format(ErrorMessages.RequestValidationError, request));
            }

            var addResult = await _weatherCommandsRepository.AddFavoriteLocation(request, cancellationToken);
            if (addResult.IsFailed)
            {
                _logger.LogError(LogEvents.FavoriteWeathersStoreToDatabase, addResult.Errors.JoinToMessage());
                return HttpDataResponses.AsInternalServerError<bool>(ErrorMessages.CantStoreLocation);
            }

            return HttpDataResponses.AsOK(true);
        }
    }
}
