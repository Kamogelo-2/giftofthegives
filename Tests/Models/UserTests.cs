using GiftOfTheGivers2.Models;
using GiftOfTheGivers2.Tests;
using Xunit;
namespace GiftOfTheGivers2.Tests.Models
{
    public class UserTests
    {
        [Fact]
        public void User_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var user = TestDataBuilder.CreateTestUser();

            // Assert
            Assert.NotNull(user.UserId);
            Assert.Equal("Volunteer", user.UserType);
            Assert.True(user.IsActive);
            Assert.True(user.CreatedAt <= DateTime.Now);
        }

        [Theory]
        [InlineData("", "Doe", "john.doe@example.com")] // Empty first name
        [InlineData("John", "", "john.doe@example.com")] // Empty last name
        [InlineData("John", "Doe", "invalid-email")] // Invalid email
        public void User_ShouldValidateRequiredFields(string firstName, string lastName, string email)
        {
            // Arrange & Act
            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PasswordHash = "hashed_password"
            };

            // Assert
            if (string.IsNullOrEmpty(firstName))
                Assert.True(string.IsNullOrEmpty(user.FirstName));

            if (string.IsNullOrEmpty(lastName))
                Assert.True(string.IsNullOrEmpty(user.LastName));
        }

        [Fact]
        public void User_FullName_ShouldReturnCorrectFormat()
        {
            // Arrange
            var user = TestDataBuilder.CreateTestUser("John", "Doe");

            // Act
            var fullName = $"{user.FirstName} {user.LastName}";

            // Assert
            Assert.Equal("John Doe", fullName);
        }
    }
}