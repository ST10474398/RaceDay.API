using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace RaceDay_test
{
    public class EnrolmentsControllerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        private void SetupParticipantUser(EnrolmentsController controller, string userId = "participant-456")
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, "Participant")
            }, "mock"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        // ---------------------------------------------------------
        // TEST 1: Enrolment succeeds for a valid event
        // ---------------------------------------------------------
        [Fact]
        public async Task EnrolInEvent_ReturnsOk_WhenValid()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Events.Add(new Event
            {
                EventID = 1,
                Name = "Marathon",
                EventDate = DateTime.UtcNow.AddDays(10),
                MaxParticipants = 100
            });
            await context.SaveChangesAsync();

            var controller = new EnrolmentsController(context);
            SetupParticipantUser(controller);

            var request = new EnrolmentRequest { EventId = 1 };

            // Act
            var result = await controller.EnrolInEvent(request);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var savedEnrolment = await context.EventEnrollments.FirstOrDefaultAsync();
            Assert.NotNull(savedEnrolment);
            Assert.Equal("Pending", savedEnrolment.Status);
        }

        // ---------------------------------------------------------
        // TEST 2: Enrolment fails if event doesn't exist
        // ---------------------------------------------------------
        [Fact]
        public async Task EnrolInEvent_ReturnsNotFound_WhenEventDoesNotExist()
        {
            var context = GetInMemoryDbContext();
            var controller = new EnrolmentsController(context);
            SetupParticipantUser(controller);

            var request = new EnrolmentRequest { EventId = 999 };

            var result = await controller.EnrolInEvent(request);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        // ---------------------------------------------------------
        // TEST 3: Duplicate enrolment returns Conflict
        // ---------------------------------------------------------
        [Fact]
        public async Task EnrolInEvent_ReturnsConflict_WhenAlreadyEnrolled()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Events.Add(new Event
            {
                EventID = 1,
                Name = "Marathon",
                EventDate = DateTime.UtcNow.AddDays(10),
                MaxParticipants = 100
            });
            context.EventEnrollments.Add(new EventEnrollment
            {
                EventID = 1,
                UserID = "participant-456",
                Status = "Pending"
            });
            await context.SaveChangesAsync();

            var controller = new EnrolmentsController(context);
            SetupParticipantUser(controller);

            var request = new EnrolmentRequest { EventId = 1 };

            // Act
            var result = await controller.EnrolInEvent(request);

            // Assert
            Assert.IsType<ConflictObjectResult>(result);
        }
    }
}