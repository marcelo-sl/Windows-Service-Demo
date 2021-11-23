using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Windows_Service_Demo
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private List<string> Urls = new List<string> { "https://google.com" };

        public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PollUrls();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occured while polling urls");
                }
                finally
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }

        private async Task PollUrls()
        {
            var tasks = new List<Task>();

            foreach (var url in Urls)
            {
                tasks.Add(PollUrl(url));
            }

            await Task.WhenAll(tasks);
        }

        private async Task PollUrl(string url)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("{Url} is online", url);
                }
                else
                {
                    _logger.LogWarning("{Url} is offline", url);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{Url} is offline", url);
            }
        }
    }
}
