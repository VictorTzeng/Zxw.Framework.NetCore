using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.Website.Models
{
    public class PostModel : IBaseModel<int>
    {
        [Key]
        [Column("PostId")]
        public int Id { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }

        public virtual List<PostTagModel> PostTags { get; set; }
    }

    public class TagModel : IBaseModel<string>
    {
        [Key]
        [Column("TagId")]
        public string Id { get; set; }

        public List<PostTagModel> PostTags { get; set; }
    }

    public class PostTagModel
    {
        public int PostId { get; set; }

        [ForeignKey("PostId")]
        public virtual PostModel Post { get; set; }

        public string TagId { get; set; }

        [ForeignKey("TagId")]
        public virtual TagModel Tag { get; set; }
    }
}
