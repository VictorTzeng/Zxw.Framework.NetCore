using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.Website.Models
{
    [Table("SysUser")]
    public class SysUser:BaseModel<int>
    {
        [Key]
        [Column("SysUserId")]
        public override int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string SysUserName { get; set; }

        [Required]
        [MaxLength(100)]
        public string SysPassword { get; set; }

        public bool Activable { get; set; } = true;
    }
}
