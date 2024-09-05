using Microsoft.AspNetCore.Identity;

namespace FruitSA_Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }  
        public string LastName { get; set; }  
    }
}
