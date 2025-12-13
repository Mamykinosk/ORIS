using System.Collections.Generic;

namespace MiniHttpServer.Models
{
    public class Tour
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Image { get; set; }
        public string Price { get; set; }
        public string Date { get; set; }
        public string Duration { get; set; }
        public string MealPlan { get; set; }
        public string Type { get; set; }
        public bool IsEarlyBooking { get; set; }
        public bool IsDiscount { get; set; }
        public int RatingStars { get; set; }
        public string Error { get; set; }
    }

    public class HotelDetail
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public int FoundationYear { get; set; }
        public string RenovationYear { get; set; }
        public string Area { get; set; }
        public string City { get; set; }
        public string DistanceCity { get; set; }
        public string DistanceAirport { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Site { get; set; }
        public string Description { get; set; }
    }

    public class HotelImage
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public string ImageUrl { get; set; }
        public int Number { get; set; }
    }

    public class RoomOffer
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Dates { get; set; }
        public string OldPrice { get; set; }
        public string Price { get; set; }
        public string DiscountTag { get; set; }
    }

    public class WeatherStat
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public string Month { get; set; }
        public int AirTemp { get; set; }
        public int WaterTemp { get; set; }
    }

    public class Amenity
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }

    public class FreeDate
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public string StartDate { get; set; }
        public string Duration { get; set; }
        public string MealPlan { get; set; }
        public string Price { get; set; }
    }

    public class Review
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public string Author { get; set; }
        public string Date { get; set; }
        public string TripType { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
    }

    public class SimilarTour
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Image { get; set; }
        public string Price { get; set; }
        public string Date { get; set; }
    }
    
    public class TourPageViewModel
    {
        public string PageTitle { get; set; }
        public List<Tour> Tours { get; set; }
        
        public bool IsLoggedIn { get; set; }
        public string UserName { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class TourViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Image { get; set; }
        public string Price { get; set; }
        public string Date { get; set; }
        public string Duration { get; set; }
        public string MealPlan { get; set; }
        public bool IsEarlyBooking { get; set; }
        public bool IsDiscount { get; set; }
        public bool IsLoggedIn { get; set; }
        public bool IsAdmin { get; set; }
        public string UserName { get; set; }

        public HotelDetail Detail { get; set; }
        public List<HotelImage> Gallery { get; set; }       
        public List<RoomOffer> Rooms { get; set; }          
        public List<WeatherStat> Weather { get; set; }      
        public List<Amenity> Amenities { get; set; }        
        public List<FreeDate> FreeDates { get; set; }
        public List<Review> Reviews { get; set; }
        
        public List<Tour> SimilarTours { get; set; }      
    }
    
    public class FilterRequest
    {
        public string[] months { get; set; }  
        public string[] seasons { get; set; }
        public string[] types { get; set; } 
        public string[] days { get; set; }    
    }
    
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } 
        public string Name { get; set; }
    }
}