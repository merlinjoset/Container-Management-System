using System.ComponentModel.DataAnnotations;

namespace ContainerManagement.Application.Dtos.Slots
{
    public class SlotUpdateDto
    {
        public Guid Id { get; set; }

        [StringLength(150)]
        public string? SlotName { get; set; }
        public Guid ModifiedBy { get; set; }
    }
}
