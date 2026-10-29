using System.ComponentModel.DataAnnotations;

namespace FarmaDiBusiness.DTOs.UsersDto
{
    public class UpdateUserDto
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido de usuario es obligatorio.")]
        public string UserLastName { get; set; } = string.Empty;

        [StringLength(200, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{6,100}$", ErrorMessage =
            "La contraseña debe contener mayúsculas, minúsculas, números y símbolos.")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        public string Mail { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
        [Phone(ErrorMessage = "El formato del número de teléfono no es válido.")]
        public string UserPhone { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
