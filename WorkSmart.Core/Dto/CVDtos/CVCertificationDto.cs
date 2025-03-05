namespace WorkSmart.Core.Dto.CVDtos
{
    public class CVCertificationDto
    {
        public int CertificationID { get; set; }
        public int CVID { get; set; }
        public string? CertificateName { get; set; }
        public string? Description { get; set; }
        public DateTime? CreateAt { get; set; }
    }

}
