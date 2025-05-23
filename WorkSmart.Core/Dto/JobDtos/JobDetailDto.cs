﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Enums;

namespace WorkSmart.Core.Dto.JobDtos
{
    public class JobDetailDto
    {
        public int JobID { get; set; }
        public int UserID { get; set; }
        public string Title { get; set; }
        public string? CategoryID { get; set; }
        public string Description { get; set; }
        public string? Level { get; set; }
        public string? Education { get; set; }
        public int? NumberOfRecruitment { get; set; }
        public string? WorkType { get; set; }
        public string? Location { get; set; }
        public string? JobPosition { get; set; }
        public string? Salary { get; set; }
        public int? Exp { get; set; }
        public bool Priority { get; set; } = false;
        public DateTime? Deadline { get; set; }
        public JobStatus? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        //compay info
        public string? Industry { get; set; }
        public string? Avatar { get; set; }
        public string? CompanySize { get; set; }
        public string? CompanyWebsite { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyDescription { get; set; }
        public string? WorkLocation { get; set; }
        public List<int>? Tags { get; set; }
        public List<JobDetailTagDto>? JobDetailTags { get; set; }
    }

    public class JobDetailTagDto
    {
        public int TagID { get; set; }
        public string TagName { get; set; }
    }
}
