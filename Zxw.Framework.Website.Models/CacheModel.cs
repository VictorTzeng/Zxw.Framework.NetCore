using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.Website.Models
{
    [Table("Cache")]
    public class Cache : IBaseModel<int>
    {
        [Key]
        [Column("CacheId")]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string CacheKey { get; set; }

        [Required]
        [MaxLength(20)]
        public string CacheType { get; set; }

        [Required]
        public int CacheExpiration { get; set; }

        public DateTime? CreatedTime { get; set; }

        public DateTime? ExpiredTime { get; set; }
    }
}
