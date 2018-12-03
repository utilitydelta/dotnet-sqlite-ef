using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityDelta.Data
{
    public class Blog
    {
        public Blog()
        {
            Tags = new List<Tag>();
        }

        public int Id { get; set; }

        [Required] public string Url { get; set; }

        [InverseProperty("Blog")] public ICollection<Tag> Tags { get; set; }
    }
}