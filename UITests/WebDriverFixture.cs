using GiftOfTheGivers2.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using Xunit;

namespace GiftOfTheGivers2.UITests
{
    public class WebDriverFixture : IDisposable
    {
        public IWebDriver Driver { get; private set; }
        public string BaseUrl { get; private set; }
        private readonly WebApplicationFactory<Program> _factory;
        private readonly IServiceScope _scope;

        public WebDriverFixture()
        {
            // Setup WebApplicationFactory for testing
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Replace database with in-memory for testing
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        services.AddDbContext<ApplicationDbContext>(options =>
                        {
                            options.UseInMemoryDatabase("UITestDb");
                        });
                    });
                });

            _scope = _factory.Services.CreateScope();
            BaseUrl = "https://localhost:5001"; // or your test URL

            // Initialize WebDriver
            Driver = WebDriverFactory.CreateDriver(BrowserType.ChromeHeadless);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Clear existing data
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Add test data here
            // ... (similar to previous test data seeding)

            context.SaveChanges();
        }

        public void Dispose()
        {
            try
            {
                Driver?.Quit();
                Driver?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disposing WebDriver: {ex.Message}");
            }

            _scope?.Dispose();
            _factory?.Dispose();
        }
    }

    [CollectionDefinition("UI Tests")]
    public class WebDriverCollection : ICollectionFixture<WebDriverFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
