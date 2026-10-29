using System.ComponentModel.DataAnnotations;

namespace FarmaDiBusiness.DTOs.UsersDto
{
    public class UpdateUserRoleDto
    {
        [Required(ErrorMessage = "El identificador del nuevo rol es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El identificador del rol debe ser mayor a cero.")]
        public int NewRoleId { get; set; }
    }
}
