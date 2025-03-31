namespace WorkSmart.Core.Dto.FavoriteJobDtos
{
    public class FavoriteJobDto
    {
        public int FavoriteJobID { get; set; }
        public int UserID { get; set; }
        public int JobID { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
