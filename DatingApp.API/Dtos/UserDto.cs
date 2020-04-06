using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
    public class UserDto
    {
        [Required]
        public string UserName{ get; set; }

        [Required]
        [StringLength(8, MinimumLength =4, ErrorMessage="Password Incorrecto")]
        public string Password { get; set; }
    }
}