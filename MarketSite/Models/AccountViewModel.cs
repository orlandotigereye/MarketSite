using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MarketSite.Models
{
    public class AccountViewModel
    {
    }
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "帳號")]
        [StringLength(10, ErrorMessage = "帳號長度在6~10個字元之間。", MinimumLength = 6)]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "帳號錯誤")]
        public string ExUserId { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string ExUserPassword { get; set; }

    }
    public partial class ExtendUser
    {
        [Required]
        [Display(Name = "帳號")]
        [StringLength(10, ErrorMessage = "帳號長度在6~10個字元之間。", MinimumLength = 6)]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "帳號錯誤")]
        public string ExUserId { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        [StringLength(100, ErrorMessage = "密碼長度最少{2}個字元。", MinimumLength = 6)]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-zA-Z]).*$", ErrorMessage = "密碼需包含英數字")]        
        public string ExUserPassword { get; set; }
        [Required]
        [Display(Name = "使用者名稱")]
        [StringLength(30, ErrorMessage = "字元長度最多30。")]
        public string ExUserName { get; set; }
        [Required]
        [Display(Name = "部門單位")]
        [StringLength(30, ErrorMessage = "字元長度最多30。")]
        public string ExUserDep { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "目前密碼")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "長度至少必須為 {2} 個字元。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "新密碼")]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-zA-Z]).*$", ErrorMessage = "密碼需包含英數字")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "確認新密碼")]
        [Compare("NewPassword", ErrorMessage = "新密碼與確認密碼不相符。")]
        public string ConfirmPassword { get; set; }
    }
}