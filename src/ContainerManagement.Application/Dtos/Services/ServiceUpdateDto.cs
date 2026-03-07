using System.ComponentModel.DataAnnotations;

namespace ContainerManagement.Application.Dtos.Services
{
    public class ServiceUpdateDto
    {
        public Guid Id { get; set; }
        [StringLength(50)]
        public string? ServiceCode { get; set; }
        [StringLength(150)]
        public string? ServiceName { get; set; }
        public Guid ModifiedBy { get; set; }
    }
}
