using MiniHttpServer.Core;
using MiniHttpServer.Core.Attributes;
using MiniHttpServer.Models;
using MiniHttpServer.Settings;
using MiniORM;
using TemplateEngine;
using System.Net;
using System.Text.RegularExpressions;

namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    public class AdminEndpoint : BaseEndpoint
    {
        private readonly ORMContext _orm;

        public AdminEndpoint()
        {
            var settings = Singleton.GetInstance().Settings;
            _orm = new ORMContext(settings.ConnectionString);
        }
        
        [HttpGet("dashboard")]
        public void Dashboard(HttpListenerContext context)
        {
            if (!IsAdmin(context)) return;

            var tours = _orm.ReadAll<Tour>("tours");
            var renderer = new HtmlTemplateRenderer();
            
            string html = renderer.RenderFromFile("Public/admin/dashboard.html", new { Tours = tours });
            SendHtml(context, html);
        }
        
        [HttpGet("add")]
        public void AddPage(HttpListenerContext context)
        {
            var renderer = new HtmlTemplateRenderer();
            string html = renderer.RenderFromFile("Public/admin/edit_tour.html", new Tour { Id = 0, Error = "" });
            SendHtml(context, html);
        }
        
        [HttpGet("edit")]
        public void EditPage(HttpListenerContext context)
        {
            string idStr = context.Request.QueryString["id"];
            if (int.TryParse(idStr, out int id))
            {
                var tour = _orm.ReadById<Tour>("tours", id);
                if (tour != null)
                {
                    var renderer = new HtmlTemplateRenderer();
                    string html = renderer.RenderFromFile("Public/admin/edit_tour.html", tour); 
                    SendHtml(context, html);
                    return;
                }
            }
            Redirect(context, "/admin/dashboard");
        }


        [HttpPost("save")]
        public void SaveTour(HttpListenerContext context)
        {
            //if (!IsAdmin(context)) return;

            string formData = ReadRequestBody(context);
            var data = ParseFormData(formData);

            string idStr = data.ContainsKey("Id") ? data["Id"] : "0";
            string name = data.ContainsKey("Name") ? data["Name"] : "";
            string location = data.ContainsKey("Location") ? data["Location"] : "";
            string price = data.ContainsKey("Price") ? data["Price"] : "";
            string date = data.ContainsKey("Date") ? data["Date"] : "";
            string duration = data.ContainsKey("Duration") ? data["Duration"] : "";
            string mealPlan = data.ContainsKey("MealPlan") ? data["MealPlan"] : "";
            string type = data.ContainsKey("Type") ? data["Type"] : "";
            
            bool isEarlyBooking = data.ContainsKey("IsEarlyBooking");
            bool isDiscount = data.ContainsKey("IsDiscount");

            string errorMessage = "";

            if (!Regex.IsMatch(date, @"^\d{2}\.\d{2}\.\d{4}$") && !Regex.IsMatch(date, @"^\d{2}\.\d{2}\.\d{4}\s*-\s*\d{2}\.\d{2}\.\d{4}$"))
            {
                errorMessage = "Ошибка: Дата должна быть в формате ДД.ММ.ГГГГ (например, 19.12.2025)";
            }
            else if (!Regex.IsMatch(duration, @"\d+"))
            {
                errorMessage = "Ошибка: Длительность должна содержать число (например, '10 ночей')";
            }
            else if (!Regex.IsMatch(price, @"^[\d\s]+"))
            {
                errorMessage = "Ошибка: Цена должна содержать цифры";
            }
            else if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(location))
            {
                errorMessage = "Ошибка: Название и Локация обязательны";
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                var tempTour = new Tour 
                { 
                    Id = int.Parse(idStr),
                    Name = name, 
                    Location = location, 
                    Price = price,
                    Date = date,
                    Duration = duration,
                    MealPlan = mealPlan,
                    Type = type,
                    IsEarlyBooking = isEarlyBooking,
                    IsDiscount = isDiscount,
                    Error = errorMessage
                };

                var renderer = new HtmlTemplateRenderer();
                string html = renderer.RenderFromFile("Public/admin/edit_tour.html", tempTour);
                SendHtml(context, html);
                return; 
            }

            int id = int.Parse(idStr);
            string image = "hotels/default.jpg";

            string sqlEarly = isEarlyBooking ? "true" : "false";
            string sqlDiscount = isDiscount ? "true" : "false";
            
            var newTour = new Tour 
            { 
                Id = int.Parse(idStr),
                Name = name, 
                Location = location, 
                Price = price,
                Date = date,
                Duration = duration,
                MealPlan = mealPlan,
                Type = type,
                IsEarlyBooking = isEarlyBooking,
                IsDiscount = isDiscount,
            };

            if (id == 0)
            {
                /*
                string sql = $@"INSERT INTO ""tours"" 
                    (""Name"", ""Location"", ""Image"", ""Price"", ""Date"", ""Duration"", ""MealPlan"", ""Type"", ""IsEarlyBooking"", ""IsDiscount"") 
                    VALUES 
                    ('{name}', '{location}', '{image}', '{price}', '{date}', '{duration}', '{mealPlan}', '{type}', {sqlEarly}, {sqlDiscount})";
                
                _orm.ExecuteSql(sql);
                */
                
                _orm.Insert("tours", newTour);
            }
            else
            {
                /*
                string sql = $@"UPDATE ""tours"" SET 
                    ""Name""='{name}', 
                    ""Location""='{location}', 
                    ""Price""='{price}',
                    ""Date""='{date}',
                    ""Duration""='{duration}',
                    ""MealPlan""='{mealPlan}',
                    ""Type""='{type}',
                    ""IsEarlyBooking""={sqlEarly},
                    ""IsDiscount""={sqlDiscount}
                    WHERE ""Id""={id}";
                
                _orm.ExecuteSql(sql);
                */
                
                
                newTour.Image = _orm.GetValuById("tours", "Image", id);
                _orm.Update("tours", newTour, id);
            }

            Redirect(context, "/admin/dashboard");
        }
        
        [HttpGet("delete")]
        public void DeleteTour(HttpListenerContext context)
        {
            string idStr = context.Request.QueryString["id"];
            int id = int.Parse(idStr);
            _orm.Delete("tours", id);
            
            Redirect(context, "/admin/dashboard");
        }
        
         private bool IsAdmin(HttpListenerContext context)
        {
            var user = SessionManager.GetUser(context);
            if (user == null || user.Role != "admin")
            {
                Redirect(context, "/auth/login");
                return false;
            }
            return true;
        }
    }
}