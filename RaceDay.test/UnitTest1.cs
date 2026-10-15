using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.Models;
using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Linq;

namespace RaceDay_test
{
    public class EventsControllerTests
    {
        // Helper method to create a fresh In-Memory Database for every test
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        // Helper method to simulate an Organiser being logged in
        private void SetupOrganiserUser(EventsController controller)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "organiser-123"),
                new Claim(ClaimTypes.Role, "Organiser")
            }, "mock"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        // ---------------------------------------------------------
        // TEST 1: Get All Events (Should succeed)
        // ---------------------------------------------------------
        [Fact]
        public async Task GetAllEvents_ReturnsOkResult_WhenEventsExist()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Events.Add(new Event { EventID = 1, Name = "Test Marathon", EventDate = DateTime.UtcNow.AddDays(10) });
            await context.SaveChangesAsync();
            var controller = new EventsController(context);

            // Act
            var result = await controller.GetAllEvents();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        // ---------------------------------------------------------
        // TEST 2: Organiser can create an event (Success)
        // ---------------------------------------------------------
        [Fact]
        public async Task CreateEvent_ReturnsOk_WhenOrganiserCreatesEvent()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new EventsController(context);
            SetupOrganiserUser(controller);

            var newEvent = new Event
            {
                Name = "Charity Run",
                Description = "A fun run",
                EventDate = DateTime.UtcNow.AddDays(5),
                Location = "Central Park",
                MaxParticipants = 100,
                CategoryID = 1
            };

            // Act
            var result = await controller.CreateEvent(newEvent);

            // Assert
            Assert.IsType<OkObjectResult>(result);

            var savedEvent = await context.Events.FirstOrDefaultAsync(e => e.Name == "Charity Run");
            Assert.NotNull(savedEvent);
            Assert.Equal("organiser-123", savedEvent.OrganiserID);
            Assert.Equal("Upcoming", savedEvent.Status);
        }

        // ---------------------------------------------------------
        // TEST 3: Getting an event by ID that doesn't exist
        // ---------------------------------------------------------
        [Fact]
        public async Task GetEventById_ReturnsNotFound_WhenEventDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new EventsController(context);
            SetupOrganiserUser(controller);

            // Act
            var result = await controller.GetEventById(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        // ---------------------------------------------------------
        // TEST 4: Delete Event Returns NotFound if ID doesn't exist
        // ---------------------------------------------------------
        [Fact]
        public async Task DeleteEvent_ReturnsNotFound_WhenEventDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new EventsController(context);
            SetupOrganiserUser(controller);

            // Act
            var result = await controller.DeleteEvent(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        // ---------------------------------------------------------
        // TEST 5: Delete Event Success
        // ---------------------------------------------------------
        [Fact]
        public async Task DeleteEvent_ReturnsOk_WhenEventExists()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Events.Add(new Event { EventID = 5, Name = "To Be Deleted", EventDate = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var controller = new EventsController(context);
            SetupOrganiserUser(controller);

            // Act
            var result = await controller.DeleteEvent(5);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var deletedEvent = await context.Events.FindAsync(5);
            Assert.Null(deletedEvent);
        }
    }
}