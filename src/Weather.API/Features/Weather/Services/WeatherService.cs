﻿using Ardalis.GuardClauses;
using AutoMapper;
using FluentResults;
using Weather.API.Features.Weather.Abstractions;
using Weather.API.Features.Weather.Dtos;
using Weather.API.Features.Weather.Resources;
using Weather.API.Shared.Dtos;
using Weather.API.Shared.Extensions;
using Wheaterbit.Client.Abstractions;

namespace Weather.API.Features.Weather.Services
{
    internal sealed class WeatherService : IWeatherService
    {
        private readonly IWeatherbitHttpClient _weatherbitHttpClient;
        private readonly IMapper _mapper;

        public WeatherService(IWeatherbitHttpClient weatherbitHttpClient, IMapper mapper)
        {
            _weatherbitHttpClient = Guard.Against.Null(weatherbitHttpClient);
            _mapper = Guard.Against.Null(mapper);
        }

        public async Task<Result<CurrentWeatherDto>> GetCurrentWeather(LocationDto locationDto, CancellationToken cancellationToken)
        {
            var currentWeatherResult = await _weatherbitHttpClient.GetCurrentWeather(locationDto.Latitude, locationDto.Longitude, cancellationToken);
            if (currentWeatherResult.IsFailed)
            {
                return Result.Fail(currentWeatherResult.Errors);
            }

            if (currentWeatherResult.Value is null || !currentWeatherResult.Value.Data.HasAny())
            {
                return Result.Fail(ServiceErrorMessages.ExternalClientGetDataFailed_EmptyOrNull);
            }

            if (currentWeatherResult.Value.Data.Count != 1)
            {
                return Result.Fail(string.Format(ServiceErrorMessages.ExternalClientGetDataFailed_CorruptedData_InvalidCount, currentWeatherResult.Value.Data.Count));
            }

            return _mapper.Map<CurrentWeatherDto>(currentWeatherResult.Value.Data.Single());
        }

        public async Task<Result<ForecastWeatherDto>> GetForecastWeather(LocationDto locationDto, CancellationToken cancellationToken)
        {
            var forecastWeatherResult = await _weatherbitHttpClient.GetSixteenDayForecast(locationDto.Latitude, locationDto.Longitude, cancellationToken);
            if (forecastWeatherResult.IsFailed)
            {
                return Result.Fail(forecastWeatherResult.Errors);
            }

            if (forecastWeatherResult.Value is null || !forecastWeatherResult.Value.Data.Any())
            {
                return Result.Fail(ServiceErrorMessages.ExternalClientGetDataFailed_EmptyOrNull);
            }

            return _mapper.Map<ForecastWeatherDto>(forecastWeatherResult.Value);
        }
    }
}
