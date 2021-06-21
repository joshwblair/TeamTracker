using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TeamTrackerApi.Models
{
    public class Team
    {
        [Key]
        public int Id {get; set;}

        [Required]
        public string Name { get; set; }

        [Required]
        public string Location { get; set; }
        public List<Player> Players { get; set; } = new List<Player>();

    }
}