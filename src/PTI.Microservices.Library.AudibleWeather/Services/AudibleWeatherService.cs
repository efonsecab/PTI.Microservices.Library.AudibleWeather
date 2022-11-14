using Microsoft.Extensions.Logging;
using PTI.Microservices.Library.Models.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PTI.Microservices.Library.Services.Specialized
{
    /// <summary>
    /// Service in charge of allowing to generate audio of weather information
    /// </summary>
    public sealed class AudibleWeatherService
    {
        private ILogger<AudibleWeatherService> Logger { get; }
        private AzureMapsService AzureMapsService { get; }
        private AzureSpeechService AzureSpeechService { get; }

        /// <summary>
        /// Createsa new instance of <see cref="AudibleWeatherService"/>
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="azureMapsService"></param>
        /// <param name="azureSpeechService"></param>
        public AudibleWeatherService(ILogger<AudibleWeatherService> logger, AzureMapsService azureMapsService,
            AzureSpeechService azureSpeechService)
        {
            this.Logger = logger;
            this.AzureMapsService = azureMapsService;
            this.AzureSpeechService = azureSpeechService;
        }

        /// <summary>
        /// Plays an uaido of the current weather for the specified coordiantes
        /// </summary>
        /// <param name="geoCoordinates"></param>
        /// <param name="outputStream"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SpeakCurrentWeatherToStreamAsync(GeoCoordinates geoCoordinates,
            MemoryStream outputStream, CancellationToken cancellationToken = default)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                Models.AzureMapsService.GetCurrentConditions.GetCurrentConditionsResponse currentWeather = 
                    await GetWeatherForLocation(geoCoordinates, stringBuilder, cancellationToken);
                if (currentWeather.results?.Length == 0)
                {
                    stringBuilder.AppendLine("No results found:");
                }
                foreach (var singleResult in currentWeather.results)
                {
                    stringBuilder.AppendLine($"Report from: {singleResult.dateTime.ToLongDateString()}");
                    string daynightString = singleResult.isDayTime ? "day" : "night";
                    stringBuilder.AppendLine($"It's a {singleResult.phrase} {daynightString}");
                    string unitString = singleResult.temperature.unit == "C" ? "Celsius" : "Farenheit";
                    stringBuilder.AppendLine($"{singleResult.temperature.value} {unitString}");
                    stringBuilder.AppendLine($"Wind speed {singleResult.wind.speed.value} {singleResult.wind.speed.unit}");
                }
                await this.AzureSpeechService.TalkToStreamAsync(stringBuilder.ToString(), outputStream);
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex, ex.Message);
                throw;
            }
        }

        private async Task<Models.AzureMapsService.GetCurrentConditions.GetCurrentConditionsResponse> GetWeatherForLocation(GeoCoordinates geoCoordinates, StringBuilder stringBuilder, CancellationToken cancellationToken)
        {
            var addressForLocation = await this.AzureMapsService.GetSearchAddressReverse(geoCoordinates, cancellationToken);
            if (addressForLocation.addresses?.Count() > 0 && !String.IsNullOrWhiteSpace(addressForLocation.addresses.First().address.freeformAddress))
            {
                stringBuilder.AppendLine($"Weather information for " +
                    $"{addressForLocation.addresses.First().address.freeformAddress}");
            }
            else
            {
                stringBuilder.AppendLine($"Weather information for " +
                    $"latitude: {geoCoordinates.Latitude}, longitude: {geoCoordinates.Longitude}");
            }
            var currentWeather = await this.AzureMapsService.GetCurrentConditionsAsync(geoCoordinates, cancellationToken);
            return currentWeather;
        }

        /// <summary>
        /// Plays an uaido of the current weather for the specified coordiantes
        /// </summary>
        /// <param name="geoCoordinates"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SpeakCurrentWeatherAsync(GeoCoordinates geoCoordinates, CancellationToken cancellationToken = default)
        {
            try
            {
                var currentWeather = await this.AzureMapsService.GetCurrentConditionsAsync(geoCoordinates, cancellationToken);
                await this.AzureSpeechService.TalkToDefaultSpeakersAsync($"Weather information for " +
                    $"latitude: {geoCoordinates.Latitude}, longitude: {geoCoordinates.Longitude}");
                foreach (var singleResult in currentWeather.results)
                {
                    await this.AzureSpeechService.TalkToDefaultSpeakersAsync($"Report from: {singleResult.dateTime.ToLongDateString()}");
                    string daynightString = singleResult.isDayTime ? "day" : "night";
                    await this.AzureSpeechService.TalkToDefaultSpeakersAsync($"It's a {singleResult.phrase} {daynightString}");
                    string unitString = singleResult.temperature.unit == "C" ? "Celsius" : "Farenheit";
                    await this.AzureSpeechService.TalkToDefaultSpeakersAsync($"{singleResult.temperature.value} {unitString}");
                    await this.AzureSpeechService.TalkToDefaultSpeakersAsync($"Wind speed {singleResult.wind.speed.value} {singleResult.wind.speed.unit}");
                }
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Plays an audio of the Weather Forecast for the specified coordinates
        /// </summary>
        /// <param name="geoCoordinates"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SpeakWeekWeatherForecastAsync(GeoCoordinates geoCoordinates, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await this.AzureMapsService.GetDailyForecastAsync(geoCoordinates, cancellationToken);
                await this.AzureSpeechService.TalkToDefaultSpeakersAsync($"Weather information for " +
                    $"latitude: {geoCoordinates.Latitude}, longitude: {geoCoordinates.Longitude}");
                await this.AzureSpeechService.TalkToDefaultSpeakersAsync($"{response.summary.phrase}");
                foreach (var singleResult in response.forecasts)
                {
                    await this.AzureSpeechService.TalkToDefaultSpeakersAsync($"Weather for: {singleResult.date}");
                    string minTempUnitString = singleResult.temperature.minimum.unit == "C" ? "Celsius" : "Farenheit";
                    string maxTempUnitString = singleResult.temperature.maximum.unit == "C" ? "Celsius" : "Farenheit";
                    await this.AzureSpeechService.TalkToDefaultSpeakersAsync($"Minimum temperture: {singleResult.temperature.minimum.value} {minTempUnitString}");
                    await this.AzureSpeechService.TalkToDefaultSpeakersAsync($"Maximum temperture: {singleResult.temperature.maximum.value} {maxTempUnitString}");
                }
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
