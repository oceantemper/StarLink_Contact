using System.ComponentModel.DataAnnotations;

namespace StarLink_Contact.Models
{
    public class EmailData
    {
        [Required]
        public string EmailAddress { get; set; } = string.Empty;
        [Required]
        public string EmailSubject { get; set; } = string.Empty;
        [Required]
        public string EmailBody { get; set; } = string.Empty;
        public int? Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? GroupName { get; set; }
    }
}
