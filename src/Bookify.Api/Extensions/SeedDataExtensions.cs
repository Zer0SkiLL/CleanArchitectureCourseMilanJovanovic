﻿using Bogus;
using Bookify.Application.Abstractions.Data;
using Bookify.Domain.Apartments;
using Dapper;

namespace Bookify.Api.Extensions;

public static class SeedDataExtensions
{
    public static void SeedData(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        var sqlConnectionFactory = scope.ServiceProvider.GetRequiredService<ISqlConnectionFactory>();
        using var connection = sqlConnectionFactory.CreateConnection();
        
        // prevent unlimited apartments from being created
        var apartmentsExist = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM public.apartments") >= 100;

        if (apartmentsExist)
        {
            return;
        }

        var faker = new Faker();

        List<object> apartments = new();
        for (var i = 0; i < 100; i++)
        {
            apartments.Add(new
            {
                Id = Guid.NewGuid(),
                Name = faker.Company.CompanyName(),
                Description = "Amazing view",
                Country = faker.Address.Country(),
                State = faker.Address.State(),
                ZipCode = faker.Address.ZipCode(),
                City = faker.Address.City(),
                Street = faker.Address.StreetAddress(),
                PriceAmount = faker.Random.Decimal(50, 1000),
                PriceCurrency = "USD",
                CleaningFeeAmount = faker.Random.Decimal(25, 200),
                CleaningFeeCurrency = "USD",
                Amenities = new List<int> { (int)Amenity.Parking, (int)Amenity.MountainView },
                LastBookedOn = DateTime.MinValue
            });
        }

        const string sql = """
            INSERT INTO public.apartments
            (id, "name", description, address_country, address_state, address_zip_code, address_city, address_street, price_amount, price_currency, cleaning_fee_amount, cleaning_fee_currency, amenities, last_booked_on_utc)
            VALUES(@Id, @Name, @Description, @Country, @State, @ZipCode, @City, @Street, @PriceAmount, @PriceCurrency, @CleaningFeeAmount, @CleaningFeeCurrency, @Amenities, @LastBookedOn);
            """;

        connection.Execute(sql, apartments);
        
    //     // Seed Users
    //     var user = new
    //     {
    //         Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
    //         IdentityId = "test-keycloak-id-1",
    //         FirstName = "John",
    //         LastName = "Doe",
    //         Email = "john.doe@test.com"
    //     };
    //     
    //     const string userSql = """
    //                            INSERT INTO public.users
    //                            (id, identity_id, first_name, last_name, email)
    //                            VALUES(@Id, @IdentityId, @FirstName, @LastName, @Email);
    //                            """;
    //     
    //     connection.Execute(userSql, user);
    }
}
