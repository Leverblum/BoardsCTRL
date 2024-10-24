﻿// <auto-generated />
using System;
using BoardsProject.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BoardsCTRL.Migrations
{
    [DbContext(typeof(BoardsContext))]
    [Migration("20241024163427_NuevaMigracion")]
    partial class NuevaMigracion
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("BoardsProject.Models.Board", b =>
                {
                    b.Property<int>("boardId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("boardId"));

                    b.Property<string>("boardDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("boardStatus")
                        .HasColumnType("bit");

                    b.Property<string>("boardTitle")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("categoryId")
                        .HasColumnType("int");

                    b.Property<int?>("createdBoardById")
                        .HasColumnType("int");

                    b.Property<DateTime>("createdBoardDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("modifiedBoardById")
                        .HasColumnType("int");

                    b.Property<DateTime?>("modifiedBoardDate")
                        .HasColumnType("datetime2");

                    b.HasKey("boardId");

                    b.HasIndex("categoryId");

                    b.ToTable("Boards");
                });

            modelBuilder.Entity("BoardsProject.Models.Category", b =>
                {
                    b.Property<int>("categoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("categoryId"));

                    b.Property<bool>("categoryStatus")
                        .HasColumnType("bit");

                    b.Property<string>("categoryTitle")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("createdCategoryById")
                        .HasColumnType("int");

                    b.Property<DateTime>("createdCategoryDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("modifiedCategoryById")
                        .HasColumnType("int");

                    b.Property<DateTime?>("modifiedCategoryDate")
                        .HasColumnType("datetime2");

                    b.HasKey("categoryId");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("BoardsProject.Models.Role", b =>
                {
                    b.Property<int>("roleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("roleId"));

                    b.Property<int>("createdRoleById")
                        .HasColumnType("int");

                    b.Property<DateTime>("createdRoleDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("modifiedRoleById")
                        .HasColumnType("int");

                    b.Property<DateTime?>("modifiedRoleDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("roleName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("roleStatus")
                        .HasColumnType("bit");

                    b.HasKey("roleId");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("BoardsProject.Models.Slide", b =>
                {
                    b.Property<int>("slideId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("slideId"));

                    b.Property<string>("URL")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("boardId")
                        .HasColumnType("int");

                    b.Property<int>("createdSlideById")
                        .HasColumnType("int");

                    b.Property<DateTime>("createdSlideDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("modifiedSlideById")
                        .HasColumnType("int");

                    b.Property<DateTime?>("modifiedSlideDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("slideStatus")
                        .HasColumnType("bit");

                    b.Property<string>("slideTitle")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("time")
                        .HasColumnType("int");

                    b.HasKey("slideId");

                    b.HasIndex("boardId");

                    b.ToTable("Slides");
                });

            modelBuilder.Entity("BoardsProject.Models.User", b =>
                {
                    b.Property<int>("userId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("userId"));

                    b.Property<string>("createdUserBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("createdUserDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("modifiedUserById")
                        .HasColumnType("int");

                    b.Property<DateTime?>("modifiedUserDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("passwordHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("roleId")
                        .HasColumnType("int");

                    b.Property<bool>("userStatus")
                        .HasColumnType("bit");

                    b.Property<string>("username")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("userId");

                    b.HasIndex("roleId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("BoardsProject.Models.Board", b =>
                {
                    b.HasOne("BoardsProject.Models.Category", "Category")
                        .WithMany("Boards")
                        .HasForeignKey("categoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");
                });

            modelBuilder.Entity("BoardsProject.Models.Slide", b =>
                {
                    b.HasOne("BoardsProject.Models.Board", "Board")
                        .WithMany("Slides")
                        .HasForeignKey("boardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Board");
                });

            modelBuilder.Entity("BoardsProject.Models.User", b =>
                {
                    b.HasOne("BoardsProject.Models.Role", "Role")
                        .WithMany("Users")
                        .HasForeignKey("roleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");
                });

            modelBuilder.Entity("BoardsProject.Models.Board", b =>
                {
                    b.Navigation("Slides");
                });

            modelBuilder.Entity("BoardsProject.Models.Category", b =>
                {
                    b.Navigation("Boards");
                });

            modelBuilder.Entity("BoardsProject.Models.Role", b =>
                {
                    b.Navigation("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
