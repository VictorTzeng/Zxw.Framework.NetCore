using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zxw.Framework.Models;

namespace Zxw.Framework.Sample.Models
{
    [Table("TutorClassType")]
    public class TutorClassType:IBaseModel<int>
    {
        [Key]
        [Column("TutorClassTypeId")]
        public int Id { get; set; }

        [Required]
        [StringLength(maximumLength:20)]
        public string TutorClassTypeName { get; set; }

        public bool Active { get; set; } = true;

        [StringLength(maximumLength:200)]
        public string Remark { get; set; }
        public int TutorClassCount { get; set; }
    }
}
