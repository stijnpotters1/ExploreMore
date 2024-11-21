﻿namespace PairUpScraper;

public class ScraperBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private const int ScrapeFrequencyDays = 1;

    public ScraperBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunScraperAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromDays(ScrapeFrequencyDays), stoppingToken);

            await RunScraperAsync(stoppingToken);
        }
    }

    private async Task RunScraperAsync(CancellationToken cancellationToken)
    {
        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var webScraper = scope.ServiceProvider.GetRequiredService<IWebScraper>();
                var activities = await webScraper.ScrapeActivitiesAsync();

                await webScraper.SaveActivitiesToDatabaseAsync(activities);

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
            }
        }
        catch (HttpRequestException)
        {
            throw new HttpRequestException();
        }
        catch (TimeoutException)
        {
            throw new TimeoutException();
        }
        catch (Exception exception)
        {
            throw new Exception($"An unexpected error occurred: {exception.Message}");
        }
    }
}