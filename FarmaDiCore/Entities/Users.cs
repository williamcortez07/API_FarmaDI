using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiCore.Entities
{
    public class Users
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserLastName { get; set; }
        public string PasswordHash { get; set; }
        public string Mail { get; set; }
        public string UserPhone { get; set; }
        // Por defecto todo nuevo usuario debe inicializarse como activo para permitir el flujo de autenticación
        public bool IsActive { get; set; } = true;

        public List<Roles> Roles { get; set; }



    }
}
