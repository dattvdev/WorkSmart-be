﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.PackageDtos
{
    public class CreateUpdatePackageDto
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public int? JobPostLimitPerDay { get; set; } // Giới hạn số bài đăng mỗi ngày
        public int? FeaturedJobPostLimit { get; set; } // Số bài đăng có thể hiển thị nổi bật
        public bool? AccessToPremiumCandidates { get; set; } // Quyền truy cập ứng viên cao cấp

        // Dành cho Candidate
        public int? CVLimit { get; set; } // Số lượng CV có thể tạo
        public bool? HighlightProfile { get; set; } // Hồ sơ được hiển thị nổi bật
        public bool? AccessToExclusiveJobs { get; set; }
    }
}
