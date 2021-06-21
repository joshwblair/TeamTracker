using System.ComponentModel.DataAnnotations;

namespace TeamTrackerApi.Models
{
    public class Player
    {
        [Key]
        public int Id {get; set;}

        [Required]
        public string FirstName { get; set; }
        
        [Required]
        public string LastName { get; set; }

        public bool IsOnTeam { get; set; }

    }
}