using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PRS.Models;


//inherits from DbContext class and calls all tables from SQL database

public partial class PRSDBContext : DbContext
{
    public PRSDBContext()
    {
    }

    public PRSDBContext(DbContextOptions<PRSDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<LineItem> LineItems { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Request> Requests { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vendor> Vendors { get; set; }

}
