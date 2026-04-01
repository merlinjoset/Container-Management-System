namespace ContainerManagement.Application.Dtos.Voyages;

public class BunkerOnArrivalDto
{
    public string ReadingPoint { get; set; } = string.Empty;
    public decimal? VlsfoMts { get; set; }
    public decimal? MgoMts { get; set; }
    public decimal? HfoMts { get; set; }
}
