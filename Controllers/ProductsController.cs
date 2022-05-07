using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Http;
using System.Net.Http;
using System.Net.Http.Headers;

using BookStore.Schemas;
using BookStore.Helper;
using BookStore.Models;

namespace BookStore.Controllers
{
    [RoutePrefix("product")]
    public class ProductsController : ApiController
    {
        // Get booking history of the given user
        [HttpPost]
        [Route("get_history")]
        public string GetHistory(GetHistory data)
        {
            // Get JWT token from cookie
            CookieHeaderValue user_name = Request.Headers.GetCookies("user_name").FirstOrDefault();
            CookieHeaderValue session_id = Request.Headers.GetCookies("session_id").FirstOrDefault();

            // Verify JWT token
            if (user_name != null && session_id != null)
            {
                bool verified = JwtHandler.VerifyToken(user_name["user_name"].Value, session_id["session_id"].Value);

                if (verified == false)
                {
                    Response auth_response = new Response();
                    auth_response.success = false;
                    auth_response.data = "Authorization failed";
                    return JsonConvert.SerializeObject(auth_response);
                }
            }

            // Get the order details from the database
            BookStoreEntities dbContext = new BookStoreEntities();
            var result = from order in dbContext.orders where order.user_name == data.user_name
            select new
            {
                id = order.id,
                date = order.date,
                cart = order.cart
            };

            Response response = new Response();
            response.success = true;
            response.data = JsonConvert.SerializeObject(result.ToArray());
            return JsonConvert.SerializeObject(response);
        }

        // Get product details from the database
        [HttpGet]
        [Route("get_products")]
        public string GetProducts()
        {
            // Get JWT token from the cookie
            CookieHeaderValue user_name = Request.Headers.GetCookies("user_name").FirstOrDefault();
            CookieHeaderValue session_id = Request.Headers.GetCookies("session_id").FirstOrDefault();

            // Verify JWT token
            if (user_name != null && session_id != null)
            {
                bool verified = JwtHandler.VerifyToken(user_name["user_name"].Value, session_id["session_id"].Value);

                if (verified == false)
                {
                    Response auth_response = new Response();
                    auth_response.success = false;
                    auth_response.data = "Authorization failed";
                    return JsonConvert.SerializeObject(auth_response);
                }
            }

            // Get the product details from the database
            BookStoreEntities dbContext = new BookStoreEntities();
            var result = from product in dbContext.products
            select new
            {
                id = product.id,
                name = product.name,
                image = product.image,
                price = product.price
            };

            Response response = new Response();
            response.success = true;
            response.data = JsonConvert.SerializeObject(result.ToArray());
            return JsonConvert.SerializeObject(response);
        }

        // Place order using the given cart details
        [HttpPost]
        [Route("place_order")]
        public string PlaceOrder(NewOrder order)
        {
            // Get JWT token from cookie
            CookieHeaderValue user_name = Request.Headers.GetCookies("user_name").FirstOrDefault();
            CookieHeaderValue session_id = Request.Headers.GetCookies("session_id").FirstOrDefault();

            // Verify JWT token
            if (user_name != null && session_id != null)
            {
                bool verified = JwtHandler.VerifyToken(user_name["user_name"].Value, session_id["session_id"].Value);

                if (verified == false)
                {
                    Response auth_response = new Response();
                    auth_response.success = false;
                    auth_response.data = "Authorization failed";
                    return JsonConvert.SerializeObject(auth_response);
                }
            }

            // Store the order details in the database
            BookStoreEntities dbContext = new BookStoreEntities();
            order OrderData = new order
            {
                date = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                user_name = order.user_name,
                cart = order.cart
            };
            dbContext.orders.Add(OrderData);
            dbContext.SaveChanges();

            Response response = new Response
            {
                success = true,
                data = "Order Placed"
            };
            return JsonConvert.SerializeObject(response);
        }
    }
}
