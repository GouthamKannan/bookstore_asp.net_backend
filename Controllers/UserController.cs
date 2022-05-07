using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Web.Helpers;
using System.Linq;
using System.Web.Http;
using System.Net.Http;
using System.Net.Http.Headers;

using BookStore.Schemas;
using BookStore.Helper;
using BookStore.Models;
using System.Net;

namespace BookStore.Controllers
{
    [RoutePrefix("user")]
    public class UserController : ApiController
    {
        [HttpPost]
        [Route("login")]
        public string Login(LoginData data)
        {

            // Check if username exists in db
            BookStoreEntities dbContext = new BookStoreEntities();
            var user = dbContext.users.FirstOrDefault(cur_user => cur_user.email == data.email);

            if (user != null)
            {
                // Check for password
                if (!Crypto.VerifyHashedPassword(user.password, data.password))
                {
                    Response response = new Response();
                    response.success = false;
                    response.data = "Invalid password";
                    return JsonConvert.SerializeObject(response);
                }
                // Check for account status
                else if (user.status == "inactive")
                {
                    Response response = new Response();
                    response.success = false;
                    response.data = "Email ID not verified";
                    return JsonConvert.SerializeObject(response);
                }
                // Create jwt
                else
                {
                    string Token = JwtHandler.CreateToken(data.email);

                    LoginResponse response = new LoginResponse();
                    response.success = true;
                    response.user_name = user.user_name;
                    response.session_id = Token;

                    return JsonConvert.SerializeObject(response);
                }
            }
            else
            {
                Response response = new Response();
                response.success = false;
                response.data = "Invalid Email ID";
                return JsonConvert.SerializeObject(response);
            }
        }

        [HttpPost]
        [Route("signup")]
        public string Signup(SignupData data)
        {
            Response response = new Response();
            // Check if username and email already exists
            BookStoreEntities dbContext = new BookStoreEntities();
            var user = dbContext.users.FirstOrDefault(cur_user => cur_user.user_name == data.user_name && cur_user.email == data.email);
            if (user != null)
            {
                response.success = false;
                response.data = "User name or Email Id already exists";
                return JsonConvert.SerializeObject(response);
            }

            // Generate verification code
            var HashedPassword = Crypto.HashPassword(data.password);

            // Send verification link to user's email
            string AbsolutePath = Request.RequestUri.OriginalString;
            string LocalPath = Request.RequestUri.LocalPath;
            string ApiHost = AbsolutePath.Replace(LocalPath, "");
            bool sent = MailHandler.SendVerificationCode(data.user_name, data.email, HashedPassword, ApiHost);

            if (sent)
            {
                response.success = true;
                response.data = "Signup successful";
                return JsonConvert.SerializeObject(response);
            }
            else
            {
                response.success = false;
                response.data = "Cannot send verification email";
                return JsonConvert.SerializeObject(response);
            }
        }


        [HttpGet]
        [Route("logout")]
        public string Logout()
        {
            // Clear JWT Token from database
            CookieHeaderValue user_name = Request.Headers.GetCookies("user_name").FirstOrDefault();
            if (user_name != null)
            {
                JwtHandler.ClearToken(user_name["user_name"].Value);
            }
            Response response = new Response();
            response.success = true;
            response.data = "Logout";
            return JsonConvert.SerializeObject(response);
        }

        [HttpPost]
        [Route("send_reset_link")]
        public string ForgetPassword(ForgetPasswordData data)
        {
            // Get user details based on user email
            BookStoreEntities dbContext = new BookStoreEntities();
            var user = dbContext.users.FirstOrDefault(cur_user => cur_user.email == data.email);

            if (user != null)
            {
                // Send reset link to user's email
                string AbsolutePath = Request.RequestUri.OriginalString;
                string LocalPath = Request.RequestUri.LocalPath;
                string ApiHost = AbsolutePath.Replace(LocalPath, "");
                bool sent = MailHandler.SendResetCode(data.email, ApiHost);
                if (sent)
                {
                    Response response = new Response();
                    response.success = true;
                    response.data = "Password reset link is sent to registered email ID";
                    return JsonConvert.SerializeObject(response);
                }
                else
                {
                    Response response = new Response();
                    response.success = false;
                    response.data = "Invalid email ID";
                    return JsonConvert.SerializeObject(response);
                }
            }
            else
            {
                Response response = new Response();
                response.success = false;
                response.data = "Email ID not found";
                return JsonConvert.SerializeObject(response);
            }
        }

        [HttpGet]
        [Route("get_reset_form/{reset_code}")]
        public HttpResponseMessage GetResetForm(string reset_code)
        {
            // Check if reset_code exists in database
            BookStoreEntities dbContext = new BookStoreEntities();
            var user = dbContext.users.FirstOrDefault(cur_user => cur_user.reset_code == reset_code);

            Response response = new Response();
            if (user != null)
            {
                // Redirect to reset password page
                string RedirectUrl = ConfigurationManager.AppSettings["UiHost"] + "/reset-password/" + reset_code;
                var redir_response = Request.CreateResponse(HttpStatusCode.Moved);
                redir_response.Headers.Location = new Uri(RedirectUrl);
                return redir_response;
            }

            else
            {
                response.success = false;
                response.data = "Invalid Link";
                var redir_response = Request.CreateResponse(HttpStatusCode.Found, JsonConvert.SerializeObject(response));
                return redir_response;
            }
        }

        [HttpPost]
        [Route("reset_password")]
        public string ResetPassword(ResetPasswordData data)
        {
            Response response = new Response();

            // Get the user details
            BookStoreEntities dbContext = new BookStoreEntities();
            var user = dbContext.users.FirstOrDefault(cur_user => cur_user.reset_code == data.reset_code && cur_user.email == data.email);

            if (user != null)
            {
                // Hash and store the new password
                user.password = Crypto.HashPassword(data.password);
                dbContext.SaveChanges();
                response.success = true;
                response.data = "Password Reset";
                return JsonConvert.SerializeObject(response);
            }
            else
            {
                response.success = false;
                response.data = "Invalid Link";
                return JsonConvert.SerializeObject(response);
            }
        }

        [HttpGet]
        [Route("verify_email/{ver_code}")]
        public HttpResponseMessage Verify(string ver_code)
        {
            // Get user details using verification code
            BookStoreEntities dbContext = new BookStoreEntities();
            var user = dbContext.users.FirstOrDefault(cur_user => cur_user.ver_code == ver_code);

            var redir_response = Request.CreateResponse();
            if (user != null)
            {
                // Save the changes in database
                user.ver_code = "";
                user.status = "verified";
                dbContext.SaveChanges();
                string RedirectUrl = ConfigurationManager.AppSettings["UiHost"] + "/home/email_verified";

                // Redirect to home page
                redir_response = Request.CreateResponse(HttpStatusCode.Moved);
                redir_response.Headers.Location = new Uri(RedirectUrl);
                return redir_response;
            }
            Response response = new Response();
            response.success = false;
            response.data = "Invalid Link";
            redir_response = Request.CreateResponse(HttpStatusCode.Found, JsonConvert.SerializeObject(response));
            return redir_response;

        }
    }
}