using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.Website.Models
{
    [Table("SysMenu")]
    public class SysMenu:IBaseModel<int>
    {
        [Key]
        [Column("SysMenuId")]
        public int Id { get; set; }

        public int ParentId { get; set; } = 0;

        [MaxLength(50)]
        public string MenuPath { get; set; }

        [Required]
        [MaxLength(20)]
        public string MenuName { get; set; }

        [MaxLength(50)]
        public string MenuIcon { get; set; } = "fa fa-link";

        [Required]
        [MaxLength(100)]
        public string Identity { get; set; }

        [Required]
        [MaxLength(200)]
        public string RouteUrl { get; set; }

        public bool Visiable { get; set; } = true;

        public bool Activable { get; set; } = true;

        public int SortIndex { get; set; }
    }
}
