using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;


namespace MarketSite.Models
{
    public class AdminViewModel
    {
    }
    public class RoleViewModel
    {
        public string Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "角色名稱")]
        public string Name { get; set; }
    }

    public class EditUserViewModel
    {
        public Guid Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "帳號")]        
        [StringLength(10, ErrorMessage = "帳號長度在6~10個字元之間。", MinimumLength = 6)]
        public string AccountId { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "使用者名稱")]
        [StringLength(30, ErrorMessage = "字元長度最多30。")]
        public string UserName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "部門單位")]
        [StringLength(30, ErrorMessage = "字元長度最多30。")]
        public string UserDep { get; set; }

        public IEnumerable<SelectListItem> RolesList { get; set; }
        public object AgentList { get; set; }

    }
    public class AssignedAgent
    {
        public string ComBrand { get; set; }
        public string ComID { get; set; }
        public string ComName { get; set; }
        public bool Assigned { get; set; }
    }

    public class StoreEditViewModel
    {
        public Guid Id { get; set; }
        public object StoreList { get; set; }
    }
    public class AssignedStoreData
    {
        public string StoreBrand { get; set; }
        public string StoreId { get; set; }
        public string StoreName { get; set; }
        public bool Assigned { get; set; }
    }


}

