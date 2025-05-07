using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PRS.Models;

[Table("LineItem")]
public partial class LineItem
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("RequestID")]
    public int RequestId { get; set; }

    [Column("ProductID")]
    public int ProductId { get; set; }

    public int Quantity { get; set; }
}

    //directive to delete the following:
    //[ForeignKey("ProductId")]
    //[InverseProperty("LineItems")]
    //public virtual Product Product { get; set; } = null!;

    //[ForeignKey("RequestId")]
    //[InverseProperty("LineItems")]
    //public virtual Request Request { get; set; } = null!;

