using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityDelta.Data
{
    public class Tag
    {
        public int Id { get; set; }

        [Required] public string Name { get; set; }

        [ForeignKey("BlogId")] public Blog Blog { get; set; }

        public int BlogId { get; set; }
    }
}