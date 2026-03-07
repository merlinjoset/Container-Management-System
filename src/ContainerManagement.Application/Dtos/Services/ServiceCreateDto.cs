using System.ComponentModel.DataAnnotations;

namespace ContainerManagement.Application.Dtos.Services
{
    public class ServiceCreateDto
    {
        [StringLength(50)]
        public string? ServiceCode { get; set; }
        [StringLength(150)]
        public string? ServiceName { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
