namespace IPOPulse.Models
{
    public class UserModel
    {
        public int Id { get; set; }  
        public string Name { get; set; }
        public string Contact { get; set; }
        public string AgeGroup { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }

}
