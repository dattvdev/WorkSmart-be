﻿namespace WorkSmart.Core.Dto.ApplicationDtos
{
    public class UpdateCandidateRequest
    {
        public int JobId { get; set; }
        public string? RejectionReason { get; set; }
    }
}
