﻿// <auto-generated />
using IELDiscordBot.Classes.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace IELDiscordBot.Migrations
{
    [DbContext(typeof(IELContext))]
    partial class IELContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("IELDiscordBot.Classes.Models.CustomCommand", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Command")
                        .IsRequired()
                        .HasColumnType("varchar(30)")
                        .HasMaxLength(30);

                    b.Property<string>("ReturnValue")
                        .IsRequired()
                        .HasColumnType("varchar(5000)")
                        .HasMaxLength(5000);

                    b.HasKey("ID");

                    b.ToTable("CustomCommands");
                });

            modelBuilder.Entity("IELDiscordBot.Classes.Models.ManualPeakOverride", b =>
                {
                    b.Property<string>("Platform")
                        .HasColumnType("varchar(10)")
                        .HasMaxLength(10);

                    b.Property<string>("User")
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.Property<int>("Season")
                        .HasColumnType("int");

                    b.Property<int>("Peak")
                        .HasColumnType("int");

                    b.HasKey("Platform", "User", "Season");

                    b.ToTable("ManualPeakOverrides");
                });

            modelBuilder.Entity("IELDiscordBot.Classes.Models.DBConfigSettings", b =>
                {
                    b.Property<string>("Subsection")
                        .HasColumnType("varchar(100)")
                        .HasMaxLength(100);

                    b.Property<string>("Key")
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.HasKey("Subsection", "Key");

                    b.ToTable("ConfigSettings");
                });

            modelBuilder.Entity("IELDiscordBot.Classes.Models.Team", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Abbreviation")
                        .IsRequired()
                        .HasColumnType("varchar(10)")
                        .HasMaxLength(10);

                    b.Property<string>("CaptainID")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(100)")
                        .HasMaxLength(100);

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.ToTable("Teams");
                });
#pragma warning restore 612, 618
        }
    }
}
