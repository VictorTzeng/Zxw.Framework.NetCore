using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.Website.Models
{
    [Table("SysUser")]
    public class SysUser:IBaseModel<int>
    {
        [Key]
        [Column("SysUserId")]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string SysUserName { get; set; }

        [Required]
        [MaxLength(100)]
        public string SysPassword { get; set; }

        public bool Activable { get; set; } = true;
    }
}
