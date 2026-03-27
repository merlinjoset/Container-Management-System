using System.ComponentModel.DataAnnotations;

namespace ContainerManagement.Application.Dtos.Jobs
{
    public class JobUpdateDto
    {
        public Guid Id { get; set; }

        [Required, MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        public int Status { get; set; }

        [MaxLength(100)]
        public string? Tag { get; set; }

        [MaxLength(30)]
        public string? TagColor { get; set; }

        public Guid ModifiedBy { get; set; }
    }
}
