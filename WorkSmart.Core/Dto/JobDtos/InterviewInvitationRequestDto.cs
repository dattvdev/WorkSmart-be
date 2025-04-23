using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.JobDtos
{
    public class InterviewInvitationRequestDto
    {
        public int ApplicationId { get; set; }  // Sử dụng ApplicationID thay vì CandidateId
        public DateTime InterviewDate { get; set; }
        public int Duration { get; set; } = 60; // Mặc định 60 phút
        public int InterviewType { get; set; }  // 1=In-Person, 2=Virtual
        public string Location { get; set; }    // Cho phỏng vấn trực tiếp
        public string MeetingLink { get; set; } // Cho phỏng vấn online
        public string InterviewerName { get; set; }
        public string InterviewerPosition { get; set; }

        // chỉ cần gọi trong send-email thôi, ko làm gì cả!
        public string CompanyName { get; set; }
        public string CompanyEmail { get; set; }
    }


}
