using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GiftOfTheGivers2.Controllers;
using GiftOfTheGivers2.Data;
using GiftOfTheGivers2.Models;
using Xunit;

namespace GiftOfTheGivers2.Tests.Controllers
{
    public class AccountControllerTests
    {
        private AccountController CreateControllerWithContext()
        {
            var context = TestUtilities.GetInMemoryDbContext();
            return new AccountController(context);
        }

        [Fact]
        public async Task Register_ValidUser_ShouldCreateUser()
        {
            // Arrange
            var controller = CreateControllerWithContext();
            var user = TestDataBuilder.CreateTestUser();

            // Act
            var result = await controller.Register(user, "TestPassword123");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ShouldReturnViewWithError()
        {
            // Arrange
            var controller = CreateControllerWithContext();
            var user1 = TestDataBuilder.CreateTestUser(email: "duplicate@test.com");
            var user2 = TestDataBuilder.CreateTestUser(email: "duplicate@test.com");

            // Act - Register first user
            await controller.Register(user1, "Password123");

            // Act - Try to register second user with same email
            var result = await controller.Register(user2, "Password123");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(controller.ModelState.ErrorCount > 0);
        }

        [Fact]
        public async Task Login_ValidCredentials_ShouldRedirectToHome()
        {
            // Arrange
            var controller = CreateControllerWithContext();
            var user = TestDataBuilder.CreateTestUser();
            var password = "TestPassword123";

            // Register user first
            await controller.Register(user, password);

            // Clear model state for login test
            controller.ModelState.Clear();

            // Act - Login with valid credentials
            var result = await controller.Login(user.Email, password);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ShouldReturnViewWithError()
        {
            // Arrange
            var controller = CreateControllerWithContext();

            // Act
            var result = await controller.Login("nonexistent@test.com", "WrongPassword");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(controller.ModelState.ErrorCount > 0);
        }
    }
}

