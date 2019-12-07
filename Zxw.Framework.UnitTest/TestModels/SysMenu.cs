using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Zxw.Framework.NetCore.DbContextCore;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.UnitTest.TestModels
{
    [Serializable]
    [DbContext(typeof(SqlServerDbContext))]
    [Table("SysMenu")]
    public class SysMenu:BaseModel<string>
    {
        [Key]
        [Column("SysMenuId")]
        [MaxLength(36)]
        public override string Id { get; set; }

        [MaxLength(255)]
        public string ParentId { get; set; } = String.Empty;

        [MaxLength(255)]
        public string MenuPath { get; set; }

        [Required]
        [MaxLength(20)]
        public string MenuName { get; set; }

        [MaxLength(50)]
        public string MenuIcon { get; set; }

        [Required]
        [MaxLength(100)]
        public string Identity { get; set; }

        [Required]
        [MaxLength(200)]
        public string RouteUrl { get; set; }

        public bool Visible { get; set; } = true;

        public bool Active { get; set; } = true;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SortIndex { get; set; }
    }
}
