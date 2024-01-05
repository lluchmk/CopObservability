﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OrdersAPI;

#nullable disable

namespace OrdersAPI.Migrations
{
    [DbContext(typeof(OrdersContext))]
    partial class OrdersContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("OrdersAPI.Order", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Amount")
                        .HasColumnType("int");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.ToTable("Orders");

                    b.HasData(
                        new
                        {
                            Id = new Guid("52dd8301-69d7-4040-b0bf-3695625a15e2"),
                            Amount = 15,
                            CustomerId = new Guid("87c77822-d53d-4db1-8e66-468e28102456"),
                            ProductId = new Guid("5ba58a44-ead2-4efa-96dd-b789101953e6")
                        },
                        new
                        {
                            Id = new Guid("86356385-d6cd-48a7-b185-9187e10e9a2b"),
                            Amount = 8,
                            CustomerId = new Guid("3f617303-3844-4403-9017-4fb0bd0ac827"),
                            ProductId = new Guid("e7e45871-5885-462f-b6e7-85dec42e037e")
                        },
                        new
                        {
                            Id = new Guid("a0140d4d-ca10-4225-8182-7a2defb890a1"),
                            Amount = 13,
                            CustomerId = new Guid("3f617303-3844-4403-9017-4fb0bd0ac827"),
                            ProductId = new Guid("e7e45871-5885-462f-b6e7-85dec42e037e")
                        },
                        new
                        {
                            Id = new Guid("60fd41e5-8b52-4923-b72e-ad9b98f38aed"),
                            Amount = 19,
                            CustomerId = new Guid("87c77822-d53d-4db1-8e66-468e28102456"),
                            ProductId = new Guid("e7e45871-5885-462f-b6e7-85dec42e037e")
                        },
                        new
                        {
                            Id = new Guid("d543a9db-ced4-4d9c-b5ba-06f27f447087"),
                            Amount = 64,
                            CustomerId = new Guid("87c77822-d53d-4db1-8e66-468e28102456"),
                            ProductId = new Guid("e7e45871-5885-462f-b6e7-85dec42e037e")
                        },
                        new
                        {
                            Id = new Guid("5497896b-485d-4951-a720-e52826460705"),
                            Amount = 2,
                            CustomerId = new Guid("c654b145-1a4a-43b4-a741-87b186554edc"),
                            ProductId = new Guid("9323c4f1-8a0b-4dda-9272-a96b4c59313f")
                        },
                        new
                        {
                            Id = new Guid("fde052ba-6d8c-4d6b-ba9a-121d5e9dde8f"),
                            Amount = 98,
                            CustomerId = new Guid("c654b145-1a4a-43b4-a741-87b186554edc"),
                            ProductId = new Guid("e7e45871-5885-462f-b6e7-85dec42e037e")
                        },
                        new
                        {
                            Id = new Guid("0eb4b3a1-080f-430b-a42b-527e2c5607a6"),
                            Amount = 13,
                            CustomerId = new Guid("87c77822-d53d-4db1-8e66-468e28102456"),
                            ProductId = new Guid("e7e45871-5885-462f-b6e7-85dec42e037e")
                        });
                });
#pragma warning restore 612, 618
        }
    }
}