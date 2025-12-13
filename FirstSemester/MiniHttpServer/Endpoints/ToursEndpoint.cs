using MiniHttpServer.Core.Attributes;
using MiniHttpServer.Models;   
using MiniHttpServer.Settings;   
using MiniORM;                  
using TemplateEngine;         
using System.Text.Json;
using System.Net;
using System.Text;
using System.Globalization;
using MiniHttpServer.Core;

namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    public class ToursEndpoint : BaseEndpoint
    {
        private readonly ORMContext _orm;

        public ToursEndpoint()
        {
            var settings = Singleton.GetInstance().Settings;
            
            _orm = new ORMContext(settings.ConnectionString);
        }

        [HttpGet("list")]
        public void GetToursList(HttpListenerContext context)
        {
            var currentUser = SessionManager.GetUser(context);

            var toursFromDb = _orm.ReadAll<Tour>("tours");

            var data = new TourPageViewModel
            {
                PageTitle = "Туры в Турцию 2025",
                Tours = toursFromDb,
        
                IsLoggedIn = currentUser != null,
                UserName = currentUser?.Name ?? "",
                IsAdmin = currentUser?.Role == "admin"
            };

            var renderer = new HtmlTemplateRenderer();
            string html = renderer.RenderFromFile("Public/listing.html", data);
            SendHtml(context, html);
        }

        [HttpGet("detail")]
        public void GetTourDetail(HttpListenerContext context)
        {
            try
            {
                string idStr = context.Request.QueryString["id"];
                if (int.TryParse(idStr, out int id))
                {
                    
                    var tour = _orm.ReadById<Tour>("tours", id);
                    
                    if (tour != null)
                    {
                        var detailsList = _orm.ReadWhere<HotelDetail>("hoteldetails", "TourId", id);
                        var detail = detailsList.Count > 0 ? detailsList[0] : new HotelDetail();

                        var gallery = _orm.ReadWhere<HotelImage>("hotelimages", "TourId", id);

                        var rooms = _orm.ReadWhere<RoomOffer>("roomoffers", "TourId", id);

                        var weather = _orm.ReadWhere<WeatherStat>("weatherstats", "TourId", id);

                        var amenities = _orm.ReadWhere<Amenity>("amenities", "TourId", id);

                        var freeDates = _orm.ReadWhere<FreeDate>("freedates", "TourId", id);
                        var reviews = _orm.ReadWhere<Review>("reviews", "TourId", id);
                        
                        var allTours = _orm.ReadAll<Tour>("tours");
                        var similarTours = new List<Tour>();
                        foreach(var t in allTours) {
                            if (t.Id != tour.Id) similarTours.Add(t);
                            if (similarTours.Count >= 2) break; 
                        }
                        
                        var currentUser = SessionManager.GetUser(context);

                        var viewModel = new TourViewModel
                        {
                            Id = tour.Id,
                            Name = tour.Name,
                            Location = tour.Location,
                            Image = tour.Image,
                            Price = tour.Price,
                            Date = tour.Date,
                            Duration = tour.Duration,
                            MealPlan = tour.MealPlan,
                            IsEarlyBooking = tour.IsEarlyBooking,
                            IsDiscount = tour.IsDiscount,
                            
                            Detail = detail,
                            Gallery = gallery,
                            Rooms = rooms,
                            Weather = weather,
                            Amenities = amenities,
                            FreeDates = freeDates,
                            Reviews = reviews,
                            SimilarTours = similarTours,
                            IsLoggedIn = currentUser != null,
                            IsAdmin = currentUser?.Role == "admin",
                            UserName = currentUser?.Name ?? ""
                        };

                        var renderer = new HtmlTemplateRenderer();
                        string html = renderer.RenderFromFile("Public/details.html", viewModel);
                        SendHtml(context, html);
                        return;
                    }
                }
                SendError(context, 404, "Tour not found");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                SendError(context, 500, ex.Message);
            }
        }
        
        [HttpPost("filter")]
        public async void FilterTours(HttpListenerContext context)
        {
            try
            {
                string json;
                using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                {
                    json = reader.ReadToEnd();
                }

                var filters = JsonSerializer.Deserialize<FilterRequest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var allTours = _orm.ReadAll<Tour>("tours");
                
                var filteredTours = allTours.AsEnumerable();
                
                if (filters.days != null && filters.days.Length > 0)
                {
                    filteredTours = filteredTours.Where(t => 
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(t.Duration, @"\d+");
                        if (match.Success) {
                            string daysStr = match.Value; 
                            return filters.days.Contains(daysStr);
                        }
                        return false;
                    });
                }
                
                if ((filters.months != null && filters.months.Length > 0) || 
                    (filters.seasons != null && filters.seasons.Length > 0))
                {
                    filteredTours = filteredTours.Where(t =>
                    {
                        if (DateTime.TryParseExact(t.Date, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                        {
                            int m = dt.Month; 

                            if (filters.months != null && filters.months.Length > 0)
                            {
                                if (filters.months.Contains(m.ToString())) return true;
                            }

                            if (filters.seasons != null && filters.seasons.Length > 0)
                            {
                                if (filters.seasons.Contains("winter") && (m == 12 || m == 1 || m == 2)) return true;
                                if (filters.seasons.Contains("spring") && (m >= 3 && m <= 5)) return true;
                                if (filters.seasons.Contains("summer") && (m >= 6 && m <= 8)) return true;
                                if (filters.seasons.Contains("autumn") && (m >= 9 && m <= 11)) return true;
                            }
                            
                            return false; 
                        }
                        
                        return false;
                    });
                }

                if (filters.types != null && filters.types.Length > 0)
                {
                    filteredTours = filteredTours.Where(t => 
                    {
                        bool match = false;
                        
                        if (filters.types.Contains("hot") && (t.IsDiscount || t.IsEarlyBooking)) match = true;

                        if (filters.types.Contains("weekend")) 
                        {
                             var durMatch = System.Text.RegularExpressions.Regex.Match(t.Duration, @"\d+");
                             if (durMatch.Success && int.Parse(durMatch.Value) <= 4) match = true;
                        }

                        if (filters.types.Contains("beach")) match = true;

                        return match;
                    });
                }

                var data = new TourPageViewModel
                {
                    Tours = filteredTours.ToList()
                };

                var renderer = new HtmlTemplateRenderer();
                string html = renderer.RenderFromFile("Public/listing_partial.html", data);
                
                SendHtml(context, html);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Filter error: {ex.Message}");
                context.Response.StatusCode = 500;
                context.Response.Close();
            }
        }
    }
}