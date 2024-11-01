using Bookify.Application.Apartments.SearchApartments;
using Bookify.Application.Bookings.GetBooking;
using Bookify.Application.Bookings.ReserveBooking;
using Bookify.Application.IntegrationTests.Infrastructure;
using Bookify.Application.Users.GetLoggedInUser;
using Bookify.Application.Users.RegisterUser;
using Bookify.Domain.Bookings;
using FluentAssertions;

namespace Bookify.Application.IntegrationTests.Bookings;

public class GetBookingTests : BaseIntegrationTest
{
    private static readonly Guid BookingId = Guid.NewGuid();
    
    public GetBookingTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetBooking_ShouldReturnFailure_WhenBookingIsNotFound()
    {
        // Arrange
        var query = new GetBookingQuery(BookingId);
        
        // Act
        var result = await Sender.Send(query);
        
        // Assert
        result.Error.Should().Be(BookingErrors.NotFound);
    }
    
    [Fact]
    public async Task GetBooking_ShouldReturnBooking_WhenBookingIsFound()
    {
        // Arrange
        var apartmentQuery = new SearchApartmentsQuery(new DateOnly(1900, 1, 1), new DateOnly(2024, 1, 10));
        var apartmentResult = await Sender.Send(apartmentQuery);
        
        var bookingCommand = new ReserveBookingCommand(apartmentResult.Value.First().Id, Guid.Parse("00000000-0000-0000-0000-000000000001"), new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 10));
        var bookingResult = await Sender.Send(bookingCommand);
        
        var bookingQuery = new GetBookingQuery(bookingResult.Value);
        
        // Act
        var result = await Sender.Send(bookingQuery);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Status.Should().Be((int)BookingStatus.Reserved);
        result.Value.ApartmentId.Should().Be(apartmentResult.Value.First().Id);
    }
}