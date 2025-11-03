using GiftOfTheGivers2.Data;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers2.Tests
{
    public static class TestUtilities
    {
        public static ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        public static void SeedTestData(ApplicationDbContext context)
        {
            // Add test categories
            var foodCategory = TestDataBuilder.CreateTestCategory("Food");
            var medicalCategory = TestDataBuilder.CreateTestCategory("Medical");

            context.ResourceCategories.AddRange(foodCategory, medicalCategory);
            context.SaveChanges();
        }
    }
}
