using System.ComponentModel.DataAnnotations;

namespace TwitterBot.Models
{
    public class AdminManager
    {
        public int Id { get; set; }
        [Required]
        public string User { get; set; }
    }
}