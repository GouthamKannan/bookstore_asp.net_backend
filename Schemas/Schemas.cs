namespace BookStore.Schemas
{
    public class SignupData
    {
        public string user_name { get; set; }
        public string email { get; set; }
        public string password { get; set; }
    }

    public class LoginData
    {
        public string email { get; set; }
        public string password { get; set; }
    }

    public class ForgetPasswordData
    {
        public string email { get; set; }
    }

    public class ResetPasswordData
    {
        public string email { get; set; }
        public string password { get; set; }
        public string reset_code { get; set; }
    }

    public class Response
    {
        public bool success { get; set; }
        public string data { get; set; }
    }

    public class LoginResponse
    {
        public bool success { get; set; }
        public string user_name { get; set; }
        public string user_type { get; set; }
        public string session_id { get; set; }
    }

    public class ProductData
    {
        public int id { get; set; }
        public string name { get; set; }
        public string image { get; set; }
        public float price { get; set; }
    }

    public class NewOrder
    {
        public string user_name { get; set; }
        public string cart { get; set; }
    }

    public class GetHistory
    {
        public string user_name { get; set; }
    }
}