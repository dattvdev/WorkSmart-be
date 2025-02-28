using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.EmployerDtos
{
    public class GetEmployerProfileDto
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string CompanyName { get; set; }
        public string CompanyDescription { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Avatar {  get; set; }
        public bool IsPrivated { get; set; }
    }
}
