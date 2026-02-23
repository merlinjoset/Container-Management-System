using System.ComponentModel.DataAnnotations;

namespace ContainerManagement.Application.Dtos.Terminals
{
    public class TerminalCreateDto
    {
        [Required]
        public Guid PortId { get; set; }

        [Required]
        [StringLength(150)]
        public string TerminalName { get; set; }

        [Required]
        [StringLength(50)]
        public string TerminalCode { get; set; }

        public Guid CreatedBy { get; set; }
    }
}

