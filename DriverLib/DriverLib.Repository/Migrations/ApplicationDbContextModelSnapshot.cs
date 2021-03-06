﻿// <auto-generated />
using System;
using DriverLib.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DriverLib.Infra.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.2-servicing-10034")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("DriverLib.Domain.Address", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("City")
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(30);

                    b.Property<string>("Complement")
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(30);

                    b.Property<string>("Country")
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(30);

                    b.Property<DateTime?>("CreationDate");

                    b.Property<string>("Neighborhood")
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(30);

                    b.Property<string>("Number")
                        .HasColumnType("varchar(10)")
                        .HasMaxLength(10);

                    b.Property<string>("PostalCode")
                        .HasColumnType("varchar(15)")
                        .HasMaxLength(15);

                    b.Property<string>("State")
                        .HasColumnType("varchar(30)")
                        .HasMaxLength(30);

                    b.Property<string>("Street")
                        .HasColumnType("varchar(80)")
                        .HasMaxLength(50);

                    b.Property<Guid>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("Addresses");
                });

            modelBuilder.Entity("DriverLib.Domain.Car", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Brand")
                        .HasColumnType("varchar(200)")
                        .HasMaxLength(200);

                    b.Property<DateTime?>("CreationDate");

                    b.Property<string>("LicensePlate")
                        .HasColumnType("varchar(200)")
                        .HasMaxLength(200);

                    b.Property<string>("Model")
                        .HasColumnType("varchar(200)")
                        .HasMaxLength(200);

                    b.HasKey("Id");

                    b.ToTable("Cars");
                });

            modelBuilder.Entity("DriverLib.Domain.JobHistory", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("CreationDate");

                    b.Property<string>("Details")
                        .HasColumnType("varchar(max)");

                    b.Property<bool>("IsSuccess");

                    b.Property<string>("JobName")
                        .IsRequired()
                        .HasColumnType("varchar(200)")
                        .HasMaxLength(200);

                    b.Property<string>("LastResult")
                        .HasColumnType("varchar(200)")
                        .HasMaxLength(200);

                    b.Property<double>("TimeSpentSeconds");

                    b.HasKey("Id");

                    b.ToTable("JobHistories");
                });

            modelBuilder.Entity("DriverLib.Domain.LogEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("CreationDate");

                    b.Property<Guid>("EntityId");

                    b.Property<string>("EntityName");

                    b.Property<DateTime>("LogDateTime");

                    b.Property<string>("Operation");

                    b.Property<Guid?>("UserId");

                    b.Property<string>("ValuesChanges");

                    b.HasKey("Id");

                    b.ToTable("LogEntries");
                });

            modelBuilder.Entity("DriverLib.Domain.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("CarId");

                    b.Property<DateTime?>("CreationDate");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("varchar(100)")
                        .HasMaxLength(100);

                    b.Property<string>("HashCodePassword")
                        .HasColumnType("varchar(200)")
                        .HasMaxLength(200);

                    b.Property<DateTime>("HashCodePasswordExpiryDate")
                        .HasColumnType("datetime2(7)");

                    b.Property<string>("LastName");

                    b.Property<string>("Linkedin")
                        .HasColumnType("varchar(100)")
                        .HasMaxLength(100);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(200)")
                        .HasMaxLength(100);

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.Property<string>("PasswordSalt")
                        .IsRequired()
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.Property<string>("Phone")
                        .HasColumnType("varchar(30)")
                        .HasMaxLength(30);

                    b.Property<int>("Profile");

                    b.HasKey("Id");

                    b.HasIndex("CarId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("DriverLib.Domain.Address", b =>
                {
                    b.HasOne("DriverLib.Domain.User")
                        .WithOne("Address")
                        .HasForeignKey("DriverLib.Domain.Address", "UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DriverLib.Domain.User", b =>
                {
                    b.HasOne("DriverLib.Domain.Car", "Car")
                        .WithMany()
                        .HasForeignKey("CarId");
                });
#pragma warning restore 612, 618
        }
    }
}
