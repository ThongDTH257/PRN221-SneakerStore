﻿namespace SneakerStore.Models
{
    public class ChangePasswordViewModel
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set;}
    }
}
