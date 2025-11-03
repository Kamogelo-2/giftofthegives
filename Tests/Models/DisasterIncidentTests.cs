
using GiftOfTheGivers2.Models;
using GiftOfTheGivers2.Tests;
using Xunit;

namespace GiftOfTheGivers.Tests.Models
{
    public class DisasterIncidentTests
    {
        [Fact]
        public void DisasterIncident_ShouldInitializeWithDefaultValues()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var incident = TestDataBuilder.CreateTestIncident(userId);

            // Assert
            Assert.NotNull(incident.IncidentId);
            Assert.Equal("Reported", incident.Status);
            Assert.Equal("Medium", incident.SeverityLevel);
            Assert.True(incident.ReportedAt <= DateTime.Now);
            Assert.True(incident.UpdatedAt <= DateTime.Now);
        }

        [Theory]
        [InlineData("Low", 50)]
        [InlineData("Medium", 100)]
        [InlineData("High", 500)]
        [InlineData("Critical", 1000)]
        public void DisasterIncident_ShouldHandleDifferentSeverityLevels(string severity, int peopleAffected)
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var incident = TestDataBuilder.CreateTestIncident(userId);
            incident.SeverityLevel = severity;
            incident.PeopleAffected = peopleAffected;

            // Assert
            Assert.Equal(severity, incident.SeverityLevel);
            Assert.Equal(peopleAffected, incident.PeopleAffected);
        }

        [Fact]
        public void DisasterIncident_LocationCoordinates_ShouldBeValid()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var incident = TestDataBuilder.CreateTestIncident(userId);
            incident.Latitude = 40.7128m;
            incident.Longitude = -74.0060m;

            // Assert
            Assert.NotNull(incident.Latitude);
            Assert.NotNull(incident.Longitude);
            Assert.InRange(incident.Latitude.Value, -90m, 90m);
            Assert.InRange(incident.Longitude.Value, -180m, 180m);
        }
    }
}
