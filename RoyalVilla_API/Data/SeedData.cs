using RoyalVilla_API.Models;

namespace RoyalVilla_API.Data;

public class SeedData
{
    private static DateTime Utc(int year, int month, int day) => new(year, month, day, 0, 0, 0, DateTimeKind.Utc);
    public static void Initialize(ApplicationDbContext context)
    {
        if (context.Villa.Any()) return;

        context.Villa.AddRange(
            new Villa
            {
                Name = "Royal Villa",
                Details = "Luxurious villa with stunning ocean views and private beach access",
                Rate = 500.0,
                Sqft = 2500,
                Occupancy = 6,
                CreatedDate = Utc(2024, 1, 1),
                ImageUrl = string.Empty,
                UpdateDate = Utc(2024, 1, 1)
            },
            new Villa
            {
                Name = "Azure Bay Retreat",
                Details = "Modern villa overlooking a calm bay with an infinity pool and open-air lounge",
                Rate = 420.0,
                Sqft = 2100,
                Occupancy = 5,
                CreatedDate = Utc(2024, 1, 5),
                ImageUrl = string.Empty,
                UpdateDate = Utc(2024, 1, 5)
            },
            new Villa
            {
                Name = "Mountain Crest Lodge",
                Details = "Cozy alpine-style villa with a stone fireplace and panoramic mountain views",
                Rate = 380.0,
                Sqft = 1900,
                Occupancy = 6,
                CreatedDate = Utc(2024, 1, 12),
                ImageUrl = string.Empty,
                UpdateDate = Utc(2024, 1, 12)
            },
            new Villa
            {
                Name = "Sunset Palms Estate",
                Details = "Spacious estate surrounded by palm trees, with an outdoor kitchen and sunset terrace",
                Rate = 650.0,
                Sqft = 3200,
                Occupancy = 8,
                CreatedDate = Utc(2024, 1, 20),
                ImageUrl = string.Empty,
                UpdateDate = Utc(2024, 1, 20)
            },
            new Villa
            {
                Name = "Olive Grove Cottage",
                Details = "Charming countryside villa nestled among olive trees, ideal for a quiet getaway",
                Rate = 210.0,
                Sqft = 1200,
                Occupancy = 3,
                CreatedDate = Utc(2024, 2, 2),
                ImageUrl = string.Empty,
                UpdateDate = Utc(2024, 2, 2)
            },
            new Villa
            {
                Name = "Skyline Penthouse Villa",
                Details = "Rooftop villa with floor-to-ceiling windows, a private jacuzzi and city skyline views",
                Rate = 720.0,
                Sqft = 2800,
                Occupancy = 4,
                CreatedDate = Utc(2024, 2, 14),
                ImageUrl = string.Empty,
                UpdateDate = Utc(2024, 2, 14)
            },
            new Villa
            {
                Name = "Lakeside Haven",
                Details = "Peaceful lakefront villa with a private dock, kayaks and a wraparound deck",
                Rate = 340.0,
                Sqft = 1800,
                Occupancy = 5,
                CreatedDate = Utc(2024, 2, 28),
                ImageUrl = string.Empty,
                UpdateDate = Utc(2024, 2, 28)
            },
            new Villa
            {
                Name = "Desert Mirage Villa",
                Details = "Contemporary desert villa with a plunge pool, fire pit and star-gazing deck",
                Rate = 450.0,
                Sqft = 2300,
                Occupancy = 6,
                CreatedDate = Utc(2024, 3, 10),
                ImageUrl = string.Empty,
                UpdateDate = Utc(2024, 3, 10)
            },
            new Villa
            {
                Name = "Coral Reef Bungalow",
                Details = "Beachfront bungalow steps from the reef, perfect for snorkeling and diving enthusiasts",
                Rate = 290.0,
                Sqft = 1400,
                Occupancy = 4,
                CreatedDate = Utc(2024, 3, 22),
                ImageUrl = string.Empty,
                UpdateDate = Utc(2024, 3, 22)
            },
            new Villa
            {
                Name = "Imperial Garden Mansion",
                Details = "Grand mansion with manicured gardens, a private chef's kitchen and a home cinema",
                Rate = 980.0,
                Sqft = 4500,
                Occupancy = 12,
                CreatedDate = Utc(2024, 4, 1),
                ImageUrl = string.Empty,
                UpdateDate = Utc(2024, 4, 1)
            }
        );

        context.SaveChanges();
    }
}