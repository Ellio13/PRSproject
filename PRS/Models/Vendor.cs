﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


namespace PRS.Models;

//all of these are VARCHAR instead of NVARCHAR in sql so add [Unicode(false)] to prevent
//migration issues

[Table("Vendor")]
public partial class Vendor
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string Code { get; set; } = null!;

    [StringLength(255)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    [StringLength(255)]
    [Unicode(false)]
    public string Address { get; set; } = null!;

    [StringLength(255)]
    [Unicode(false)]
    public string City { get; set; } = null!;

    [StringLength(2)]
    [Unicode(false)]
    public string State { get; set; } = null!;

    [StringLength(5)]
    [Unicode(false)]
    public string Zip { get; set; } = null!;

    [StringLength(12)]
    [Unicode(false)]
    public string? PhoneNumber { get; set; } = null;

    [StringLength(100)]
    [Unicode(false)]
    public string? Email { get; set; } = null;
};



//directives were to delete the following
//[InverseProperty("Vendor")]
//public virtual ICollection<Product> Products { get; set; } = new List<Product>();

