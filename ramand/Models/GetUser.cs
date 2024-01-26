using System.ComponentModel.DataAnnotations;

namespace ramand.Models
{
    public class GetUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserName { get; set; }
    }
}
