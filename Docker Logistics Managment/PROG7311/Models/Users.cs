namespace PROG7311.Models
{
    public class Users
    {
        public int UsersId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public int? ClientId { get; set; }
        public Client? Client { get; set; }
    }
}
