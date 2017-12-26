using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.Website.Models
{
    [Serializable]
    public class TutorClassType:IBaseModel<int>
    {
        [Key]
        [Column("TutorClassTypeId")]
        public int Id { get; set; }

        [Required]
        [StringLength(maximumLength:50)]
        public string TutorClassTypeName { get; set; }
        public bool Active { get; set; } = true;
        [StringLength(maximumLength:200)]
        public string Remark { get; set; }
        public int TutorClassCount { get; set; }
    }
}
