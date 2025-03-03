﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.AccountDtos
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Fullname is required.")]
        [RegularExpression(@"^[a-zA-Z_ ]+$", ErrorMessage = "Fullname can only contain letters, numbers, and underscores.")]
        [Display(Name = "Fullname")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d])\S+$", ErrorMessage = "The password must contain at least one lowercase letter, one uppercase letter, one digit, and one special character.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; } //candidate and employer

        public string? Avatar { get; set; }

        //employer
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public string? CompanyName { get; set; }
        public string? WorkLocation { get; set; }
    }
}
