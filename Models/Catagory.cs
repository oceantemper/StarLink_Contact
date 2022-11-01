
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace StarLink_Contact.Models
{
    public class Catagory
    {

        //Primary Key

        public int Id { get; set; }

        [Required]
        public string? AppUserId { get; set; }

        [Required]
        [Display(Name= "Category Name")]
        public string? Name { get; set; }


        //Navigation Properties 

        public virtual AppUser? AppUser { get; set; }

        public virtual ICollection<Contact> Contacts { get; set; } = new HashSet<Contact>();

    }
}
