# PTI.Microservices.Library.AudibleWeather

This is part of PTI.Microservices.Library set of packages

The purpose of this package is to facilitate generating audible weather information

**Examples:**

## SpeakCurrentWeatherAsync
    AzureSpeechService azureSpeechService =
        new AzureSpeechService(null, this.AzureSpeechConfiguration,
        new CustomHttpClient(new CustomHttpClientHandler(null)));
    AzureMapsService azureMapsService =
        new AzureMapsService(null, this.AzureMapsConfiguration,
        new CustomHttpClient(new CustomHttpClientHandler(null)));
    GeoCoordinates geoCoordinates =
        new GeoCoordinates() {
            Latitude = [LATITUDE],
            Longitude = [LONGITUDE]
        };
    AudibleWeatherService audibleWeatherService = new AudibleWeatherService(null, azureMapsService, azureSpeechService);
    await audibleWeatherService.SpeakCurrentWeatherAsync(geoCoordinates);

## SpeakWeekWeatherForecastAsync

    AzureSpeechService azureSpeechService =
        new AzureSpeechService(null, this.AzureSpeechConfiguration,
        new CustomHttpClient(new CustomHttpClientHandler(null)));
    AzureMapsService azureMapsService =
        new AzureMapsService(null, this.AzureMapsConfiguration,
        new CustomHttpClient(new CustomHttpClientHandler(null)));
    GeoCoordinates geoCoordinates =
        new GeoCoordinates()
        {
            Latitude = [LATITUDE],
            Longitude = [LONGITUDE]
        };
    AudibleWeatherService audibleWeatherService = new AudibleWeatherService(null, azureMapsService, azureSpeechService);
    await audibleWeatherService.SpeakWeekWeatherForecastAsync(geoCoordinates);