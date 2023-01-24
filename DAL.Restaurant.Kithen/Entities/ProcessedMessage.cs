using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Restaurant.Kithen.Entities;

[Table("ProcessedMessages")]
public sealed class ProcessedMessage
{
    [Key, Required]
    public Guid OrderId { get; set; }
    [Required]
    public Guid MessageId { get; set; }
}