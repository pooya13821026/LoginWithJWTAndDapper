using System.ComponentModel.DataAnnotations;

namespace ramand.Models
{
    public class Register
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
