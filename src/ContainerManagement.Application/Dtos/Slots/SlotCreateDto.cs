using System.ComponentModel.DataAnnotations;

namespace ContainerManagement.Application.Dtos.Slots
{
    public class SlotCreateDto
    {
        [StringLength(150)]
        public string? SlotName { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
