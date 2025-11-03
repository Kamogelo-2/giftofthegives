using GiftOfTheGivers2.Data;
using GiftOfTheGivers2.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace GiftOfTheGivers2.Tests.Services
{
    public class AccountServiceTests
    {
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        [Fact]
        public void HashPassword_ShouldReturnConsistentResults()
        {
            // Arrange
            var password = "TestPassword123";

            // Act
            var hash1 = HashPassword(password);
            var hash2 = HashPassword(password);

            // Assert
            Assert.Equal(hash1, hash2);
            Assert.NotEqual(password, hash1);
        }

        [Theory]
        [InlineData("password123", true)]
        [InlineData("wrongpassword", false)]
        [InlineData("", false)]
        public void VerifyPassword_ShouldValidateCorrectly(string inputPassword, bool expectedResult)
        {
            // Arrange
            var originalPassword = "password123";
            var storedHash = HashPassword(originalPassword);

            // Act
            var result = HashPassword(inputPassword) == storedHash;

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }
}

