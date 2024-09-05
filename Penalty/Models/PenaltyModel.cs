using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace Penalty.Models
{
    [Table("Penalty")]
    public class PenaltyModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(6, ErrorMessage = "Must be 6 symbols", MinimumLength = 6)]
        [Display(Name = "CarNumber")]
        public string CarNumber { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; } //Email

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int Speed { get; set; }

        public int Summa { get; set; }

        public virtual ApplicationUser User { get; set; }

        public void CalculateSumma()
        {
            int speedOver = Speed;

            if (speedOver <= 20)
            {
                Summa = 50;
            }
            else if (speedOver <= 40)
            {
                Summa = 100;
            }
            else if (speedOver <= 60)
            {
                Summa = 200;
            }
            else
            {
                Summa = 400;
            }
        }

    }
}