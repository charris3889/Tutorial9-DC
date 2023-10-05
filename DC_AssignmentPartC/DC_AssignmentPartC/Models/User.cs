using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace DC_AssignmentPartC.Models
{
    public class User
    {
        public int Id { get; set; }
        public string ipAddress { get; set; }
        public string port { get; set; }
    }
}
