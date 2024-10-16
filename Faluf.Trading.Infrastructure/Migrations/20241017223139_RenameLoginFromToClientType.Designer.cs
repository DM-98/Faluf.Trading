﻿// <auto-generated />
using System;
using Faluf.Trading.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Faluf.Trading.Infrastructure.Migrations
{
    [DbContext(typeof(TradingDbContext))]
    [Migration("20241017223139_RenameLoginFromToClientType")]
    partial class RenameLoginFromToClientType
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0-rc.2.24474.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Faluf.Trading.Core.Domain.NewsArticle", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AuthorName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("BodyText")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAtUTC")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DeletedAtUTC")
                        .HasColumnType("datetime2");

                    b.Property<string>("Heading")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("PublishedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Subheading")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UpdatedAtUTC")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("NewsArticles");
                });

            modelBuilder.Entity("Faluf.Trading.Core.Domain.RefreshToken", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("ClientType")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAtUTC")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DeletedAtUTC")
                        .HasColumnType("datetime2");

                    b.Property<DateTimeOffset>("ExpiresAtUTC")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("LockoutEndUTC")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UpdatedAtUTC")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("RefreshTokens");
                });

            modelBuilder.Entity("Faluf.Trading.Core.Domain.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAtUTC")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DeletedAtUTC")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HashedPassword")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset?>("LockoutEndUTC")
                        .HasColumnType("datetimeoffset");

                    b.PrimitiveCollection<string>("Roles")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UpdatedAtUTC")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("FriendlyName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Xml")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("DataProtectionKeys");
                });

            modelBuilder.Entity("Faluf.Trading.Core.Domain.RefreshToken", b =>
                {
                    b.HasOne("Faluf.Trading.Core.Domain.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });
#pragma warning restore 612, 618
        }
    }
}
