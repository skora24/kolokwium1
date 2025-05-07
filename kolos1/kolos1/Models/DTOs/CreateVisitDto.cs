namespace kolos1.Models.DTOs
{
    public class CreateVisitDto
    {
        public int VisitId { get; set; }
        public int ClientId { get; set; }
        public string MechanicLicenceNumber { get; set; }
        public List<CreateVisitServiceDto> Services { get; set; } = new List<CreateVisitServiceDto>();
    }

    public class CreateVisitServiceDto
    {
        public string ServiceName { get; set; }
        public decimal ServiceFee { get; set; }
    }
}


