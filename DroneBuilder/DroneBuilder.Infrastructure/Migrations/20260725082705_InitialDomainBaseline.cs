using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DroneBuilder.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialDomainBaseline : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AspNetRoles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_AspNetRoles", x => x.Id));

        migrationBuilder.CreateTable(
            name: "AspNetUsers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                PasswordHash = table.Column<string>(type: "text", nullable: true),
                SecurityStamp = table.Column<string>(type: "text", nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                PhoneNumber = table.Column<string>(type: "text", nullable: true),
                PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_AspNetUsers", x => x.Id));

        migrationBuilder.CreateTable(
            name: "ComponentTypes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ComponentTypes", x => x.Id));

        migrationBuilder.CreateTable(
            name: "ImportSources",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                BaseUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ImportSources", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Messages",
            columns: table => new
            {
                Id = table.Column<string>(type: "text", nullable: false),
                Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Payload = table.Column<string>(type: "text", nullable: false),
                QueueName = table.Column<string>(type: "text", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                Error = table.Column<string>(type: "text", nullable: true),
                RetryCount = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Messages", x => x.Id));

        migrationBuilder.CreateTable(
            name: "ProductCategories",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductCategories", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProductCategories_ProductCategories_ParentId",
                    column: x => x.ParentId,
                    principalTable: "ProductCategories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "UnitDefinitions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Symbol = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                Dimension = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                ConversionFactorToBase = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UnitDefinitions", x => x.Id);
                table.CheckConstraint("CK_UnitDefinitions_ConversionFactor_Positive", "\"ConversionFactorToBase\" > 0");
            });

        migrationBuilder.CreateTable(
            name: "Values",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Text = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                NumericValue = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                BooleanValue = table.Column<bool>(type: "boolean", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Values", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Warehouses",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Warehouses", x => x.Id));

        migrationBuilder.CreateTable(
            name: "AspNetRoleClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                ClaimType = table.Column<string>(type: "text", nullable: true),
                ClaimValue = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                ClaimType = table.Column<string>(type: "text", nullable: true),
                ClaimValue = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserLogins",
            columns: table => new
            {
                LoginProvider = table.Column<string>(type: "text", nullable: false),
                ProviderKey = table.Column<string>(type: "text", nullable: false),
                ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                UserId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                table.ForeignKey(
                    name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserRoles",
            columns: table => new
            {
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                RoleId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserTokens",
            columns: table => new
            {
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                LoginProvider = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Value = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                table.ForeignKey(
                    name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Carts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Carts", x => x.Id);
                table.ForeignKey(
                    name: "FK_Carts_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Orders",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                TotalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                ShippingAddress_FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                ShippingAddress_AddressLine1 = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                ShippingAddress_AddressLine2 = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                ShippingAddress_City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                ShippingAddress_State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                ShippingAddress_PostalCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                ShippingAddress_Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                ShippingAddress_PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Orders", x => x.Id);
                table.CheckConstraint("CK_Orders_TotalPrice_NonNegative", "\"TotalPrice\" >= 0");
                table.ForeignKey(
                    name: "FK_Orders_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ImportBatches",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ImportSourceId = table.Column<Guid>(type: "uuid", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                FinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                TotalItems = table.Column<int>(type: "integer", nullable: false),
                ProcessedItems = table.Column<int>(type: "integer", nullable: false),
                FailedItems = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ImportBatches", x => x.Id);
                table.ForeignKey(
                    name: "FK_ImportBatches_ImportSources_ImportSourceId",
                    column: x => x.ImportSourceId,
                    principalTable: "ImportSources",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Products",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                ProductCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                Kind = table.Column<int>(type: "integer", nullable: false),
                ComponentTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Products", x => x.Id);
                table.ForeignKey(
                    name: "FK_Products_ComponentTypes_ComponentTypeId",
                    column: x => x.ComponentTypeId,
                    principalTable: "ComponentTypes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Products_ProductCategories_ProductCategoryId",
                    column: x => x.ProductCategoryId,
                    principalTable: "ProductCategories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Properties",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                DataType = table.Column<int>(type: "integer", nullable: false),
                UnitDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                IsFilterable = table.Column<bool>(type: "boolean", nullable: false),
                IsCompatibilityRelevant = table.Column<bool>(type: "boolean", nullable: false),
                AllowsMultipleValues = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Properties", x => x.Id);
                table.ForeignKey(
                    name: "FK_Properties_UnitDefinitions_UnitDefinitionId",
                    column: x => x.UnitDefinitionId,
                    principalTable: "UnitDefinitions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "UnitAliases",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UnitDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                Alias = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                NormalizedAlias = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UnitAliases", x => x.Id);
                table.ForeignKey(
                    name: "FK_UnitAliases_UnitDefinitions_UnitDefinitionId",
                    column: x => x.UnitDefinitionId,
                    principalTable: "UnitDefinitions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ValueAliases",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ValueId = table.Column<Guid>(type: "uuid", nullable: false),
                ImportSourceId = table.Column<Guid>(type: "uuid", nullable: true),
                Alias = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                NormalizedAlias = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ValueAliases", x => x.Id);
                table.ForeignKey(
                    name: "FK_ValueAliases_ImportSources_ImportSourceId",
                    column: x => x.ImportSourceId,
                    principalTable: "ImportSources",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ValueAliases_Values_ValueId",
                    column: x => x.ValueId,
                    principalTable: "Values",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Images",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                FileName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                SortOrder = table.Column<int>(type: "integer", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Images", x => x.Id);
                table.ForeignKey(
                    name: "FK_Images_Products_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ImportItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ImportBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                ExternalId = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                RawPayload = table.Column<string>(type: "jsonb", nullable: false),
                ContentHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false),
                Error = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                ProductId = table.Column<Guid>(type: "uuid", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ImportItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_ImportItems_ImportBatches_ImportBatchId",
                    column: x => x.ImportBatchId,
                    principalTable: "ImportBatches",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ImportItems_Products_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "ProductExternalReferences",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ImportSourceId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                ExternalId = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                SourceUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                ContentHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                LastSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductExternalReferences", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProductExternalReferences_ImportSources_ImportSourceId",
                    column: x => x.ImportSourceId,
                    principalTable: "ImportSources",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ProductExternalReferences_Products_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProductVariants",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                Sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductVariants", x => x.Id);
                table.CheckConstraint("CK_ProductVariants_Price_NonNegative", "\"Price\" >= 0");
                table.ForeignKey(
                    name: "FK_ProductVariants_Products_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ComponentTypeProperties",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ComponentTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                IsVariantSpecific = table.Column<bool>(type: "boolean", nullable: false),
                SortOrder = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ComponentTypeProperties", x => x.Id);
                table.CheckConstraint("CK_ComponentTypeProperties_SortOrder_NonNegative", "\"SortOrder\" >= 0");
                table.ForeignKey(
                    name: "FK_ComponentTypeProperties_ComponentTypes_ComponentTypeId",
                    column: x => x.ComponentTypeId,
                    principalTable: "ComponentTypes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ComponentTypeProperties_Properties_PropertyId",
                    column: x => x.PropertyId,
                    principalTable: "Properties",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "PropertyAliases",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                ImportSourceId = table.Column<Guid>(type: "uuid", nullable: true),
                Alias = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                NormalizedAlias = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PropertyAliases", x => x.Id);
                table.ForeignKey(
                    name: "FK_PropertyAliases_ImportSources_ImportSourceId",
                    column: x => x.ImportSourceId,
                    principalTable: "ImportSources",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PropertyAliases_Properties_PropertyId",
                    column: x => x.PropertyId,
                    principalTable: "Properties",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PropertyValues",
            columns: table => new
            {
                PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                ValueId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PropertyValues", x => new { x.PropertyId, x.ValueId });
                table.ForeignKey(
                    name: "FK_PropertyValues_Properties_PropertyId",
                    column: x => x.PropertyId,
                    principalTable: "Properties",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PropertyValues_Values_ValueId",
                    column: x => x.ValueId,
                    principalTable: "Values",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "CartItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Quantity = table.Column<int>(type: "integer", nullable: false),
                ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                CartId = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CartItems", x => x.Id);
                table.CheckConstraint("CK_CartItems_Quantity_Positive", "\"Quantity\" > 0");
                table.ForeignKey(
                    name: "FK_CartItems_Carts_CartId",
                    column: x => x.CartId,
                    principalTable: "Carts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_CartItems_ProductVariants_ProductVariantId",
                    column: x => x.ProductVariantId,
                    principalTable: "ProductVariants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "OrderItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductVariantId = table.Column<Guid>(type: "uuid", nullable: true),
                ProductName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Quantity = table.Column<int>(type: "integer", nullable: false),
                OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                PriceAtPurchase = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderItems", x => x.Id);
                table.CheckConstraint("CK_OrderItems_Price_NonNegative", "\"PriceAtPurchase\" >= 0");
                table.CheckConstraint("CK_OrderItems_Quantity_Positive", "\"Quantity\" > 0");
                table.ForeignKey(
                    name: "FK_OrderItems_Orders_OrderId",
                    column: x => x.OrderId,
                    principalTable: "Orders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_OrderItems_ProductVariants_ProductVariantId",
                    column: x => x.ProductVariantId,
                    principalTable: "ProductVariants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "ProductVariantExternalReferences",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ImportSourceId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                ExternalId = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                SourceUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                ContentHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                LastSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductVariantExternalReferences", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProductVariantExternalReferences_ImportSources_ImportSource~",
                    column: x => x.ImportSourceId,
                    principalTable: "ImportSources",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ProductVariantExternalReferences_ProductVariants_ProductVar~",
                    column: x => x.ProductVariantId,
                    principalTable: "ProductVariants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "WarehouseItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                Quantity = table.Column<int>(type: "integer", nullable: false),
                ReservedQuantity = table.Column<int>(type: "integer", nullable: false),
                Version = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WarehouseItems", x => x.Id);
                table.CheckConstraint("CK_WarehouseItems_Quantity_NonNegative", "\"Quantity\" >= 0");
                table.CheckConstraint("CK_WarehouseItems_ReservedQuantity_NonNegative", "\"ReservedQuantity\" >= 0");
                table.CheckConstraint("CK_WarehouseItems_ReservedWithinQuantity", "\"ReservedQuantity\" <= \"Quantity\"");
                table.ForeignKey(
                    name: "FK_WarehouseItems_ProductVariants_ProductVariantId",
                    column: x => x.ProductVariantId,
                    principalTable: "ProductVariants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_WarehouseItems_Warehouses_WarehouseId",
                    column: x => x.WarehouseId,
                    principalTable: "Warehouses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ProductPropertyValues",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                ValueId = table.Column<Guid>(type: "uuid", nullable: true),
                TextValue = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                NumericValue = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                MinNumericValue = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                MaxNumericValue = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                BooleanValue = table.Column<bool>(type: "boolean", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductPropertyValues", x => x.Id);
                table.CheckConstraint("CK_ProductPropertyValues_CompleteRange", "(\"MinNumericValue\" IS NULL AND \"MaxNumericValue\" IS NULL) OR (\"MinNumericValue\" IS NOT NULL AND \"MaxNumericValue\" IS NOT NULL)");
                table.CheckConstraint("CK_ProductPropertyValues_ExactlyOneValue", "(CASE WHEN \"ValueId\" IS NOT NULL THEN 1 ELSE 0 END + CASE WHEN \"TextValue\" IS NOT NULL THEN 1 ELSE 0 END + CASE WHEN \"NumericValue\" IS NOT NULL THEN 1 ELSE 0 END + CASE WHEN \"MinNumericValue\" IS NOT NULL OR \"MaxNumericValue\" IS NOT NULL THEN 1 ELSE 0 END + CASE WHEN \"BooleanValue\" IS NOT NULL THEN 1 ELSE 0 END) = 1");
                table.CheckConstraint("CK_ProductPropertyValues_ValidRange", "\"MinNumericValue\" IS NULL OR \"MaxNumericValue\" IS NULL OR \"MinNumericValue\" <= \"MaxNumericValue\"");
                table.ForeignKey(
                    name: "FK_ProductPropertyValues_Products_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProductPropertyValues_Properties_PropertyId",
                    column: x => x.PropertyId,
                    principalTable: "Properties",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ProductPropertyValues_PropertyValues_PropertyId_ValueId",
                    columns: x => new { x.PropertyId, x.ValueId },
                    principalTable: "PropertyValues",
                    principalColumns: new[] { "PropertyId", "ValueId" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ProductPropertyValues_Values_ValueId",
                    column: x => x.ValueId,
                    principalTable: "Values",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ProductVariantPropertyValues",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                ValueId = table.Column<Guid>(type: "uuid", nullable: true),
                TextValue = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                NumericValue = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                MinNumericValue = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                MaxNumericValue = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                BooleanValue = table.Column<bool>(type: "boolean", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductVariantPropertyValues", x => x.Id);
                table.CheckConstraint("CK_ProductVariantPropertyValues_CompleteRange", "(\"MinNumericValue\" IS NULL AND \"MaxNumericValue\" IS NULL) OR (\"MinNumericValue\" IS NOT NULL AND \"MaxNumericValue\" IS NOT NULL)");
                table.CheckConstraint("CK_ProductVariantPropertyValues_ExactlyOneValue", "(CASE WHEN \"ValueId\" IS NOT NULL THEN 1 ELSE 0 END + CASE WHEN \"TextValue\" IS NOT NULL THEN 1 ELSE 0 END + CASE WHEN \"NumericValue\" IS NOT NULL THEN 1 ELSE 0 END + CASE WHEN \"MinNumericValue\" IS NOT NULL OR \"MaxNumericValue\" IS NOT NULL THEN 1 ELSE 0 END + CASE WHEN \"BooleanValue\" IS NOT NULL THEN 1 ELSE 0 END) = 1");
                table.CheckConstraint("CK_ProductVariantPropertyValues_ValidRange", "\"MinNumericValue\" IS NULL OR \"MaxNumericValue\" IS NULL OR \"MinNumericValue\" <= \"MaxNumericValue\"");
                table.ForeignKey(
                    name: "FK_ProductVariantPropertyValues_ProductVariants_ProductVariant~",
                    column: x => x.ProductVariantId,
                    principalTable: "ProductVariants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProductVariantPropertyValues_Properties_PropertyId",
                    column: x => x.PropertyId,
                    principalTable: "Properties",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ProductVariantPropertyValues_PropertyValues_PropertyId_Valu~",
                    columns: x => new { x.PropertyId, x.ValueId },
                    principalTable: "PropertyValues",
                    principalColumns: new[] { "PropertyId", "ValueId" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ProductVariantPropertyValues_Values_ValueId",
                    column: x => x.ValueId,
                    principalTable: "Values",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "InventoryReservations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                WarehouseItemId = table.Column<Guid>(type: "uuid", nullable: false),
                CartId = table.Column<Guid>(type: "uuid", nullable: false),
                CartItemId = table.Column<Guid>(type: "uuid", nullable: true),
                Quantity = table.Column<int>(type: "integer", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_InventoryReservations", x => x.Id);
                table.CheckConstraint("CK_InventoryReservations_ActiveCartItem", "\"Status\" <> 0 OR \"CartItemId\" IS NOT NULL");
                table.CheckConstraint("CK_InventoryReservations_CompletionState", "(\"Status\" = 0 AND \"CompletedAt\" IS NULL) OR (\"Status\" <> 0 AND \"CompletedAt\" IS NOT NULL)");
                table.CheckConstraint("CK_InventoryReservations_Quantity_Positive", "\"Quantity\" > 0");
                table.ForeignKey(
                    name: "FK_InventoryReservations_CartItems_CartItemId",
                    column: x => x.CartItemId,
                    principalTable: "CartItems",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_InventoryReservations_Carts_CartId",
                    column: x => x.CartId,
                    principalTable: "Carts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_InventoryReservations_WarehouseItems_WarehouseItemId",
                    column: x => x.WarehouseItemId,
                    principalTable: "WarehouseItems",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.InsertData(
            table: "ComponentTypes",
            columns: new[] { "Id", "Code", "CreatedAt", "IsActive", "Name", "UpdatedAt" },
            values: new object[,]
            {
                { new Guid("00000000-0000-0000-0002-000000000001"), "motor", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Motor", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0002-000000000002"), "frame", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Frame", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0002-000000000003"), "battery", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Battery", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0002-000000000004"), "esc", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Electronic Speed Controller", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0002-000000000005"), "flight-controller", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Flight Controller", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
            });

        migrationBuilder.InsertData(
            table: "Properties",
            columns: new[] { "Id", "AllowsMultipleValues", "Code", "CreatedAt", "DataType", "IsCompatibilityRelevant", "IsFilterable", "Name", "UnitDefinitionId", "UpdatedAt" },
            values: new object[] { new Guid("00000000-0000-0000-0003-000000000011"), false, "connector-type", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, true, true, "Connector Type", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

        migrationBuilder.InsertData(
            table: "UnitDefinitions",
            columns: new[] { "Id", "Code", "ConversionFactorToBase", "CreatedAt", "Dimension", "Name", "Symbol", "UpdatedAt" },
            values: new object[,]
            {
                { new Guid("00000000-0000-0000-0001-000000000001"), "millimeter", 1m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "length", "Millimeter", "mm", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0001-000000000002"), "inch", 25.4m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "length", "Inch", "in", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0001-000000000003"), "volt", 1m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "voltage", "Volt", "V", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0001-000000000004"), "ampere", 1m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "current", "Ampere", "A", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0001-000000000005"), "cell", 1m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "cell_count", "Battery cell count", "S", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0001-000000000006"), "gram", 1m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "mass", "Gram", "g", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0001-000000000007"), "kilogram", 1000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "mass", "Kilogram", "kg", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0001-000000000008"), "watt", 1m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "power", "Watt", "W", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0001-000000000009"), "milliampere-hour", 1m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "electric_charge", "Milliampere-hour", "mAh", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0001-000000000010"), "square-millimeter", 1m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "area", "Square millimeter", "mm²", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0001-000000000011"), "hertz", 1m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "frequency", "Hertz", "Hz", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0001-000000000012"), "megahertz", 1000000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "frequency", "Megahertz", "MHz", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0001-000000000013"), "gigahertz", 1000000000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "frequency", "Gigahertz", "GHz", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0001-000000000014"), "rpm", 1m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "rotational_speed", "Revolutions per minute", "RPM", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0001-000000000015"), "kv", 1m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "motor_velocity_constant", "Motor velocity constant", "KV", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0001-000000000016"), "celsius", 1m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "temperature", "Degree Celsius", "°C", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
            });

        migrationBuilder.InsertData(
            table: "Warehouses",
            columns: new[] { "Id", "Code", "CreatedAt", "IsActive", "Name", "UpdatedAt" },
            values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), "main", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Main Warehouse", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

        migrationBuilder.InsertData(
            table: "ComponentTypeProperties",
            columns: new[] { "Id", "ComponentTypeId", "CreatedAt", "IsRequired", "IsVariantSpecific", "PropertyId", "SortOrder", "UpdatedAt" },
            values: new object[] { new Guid("00000000-0000-0000-0004-000000000011"), new Guid("00000000-0000-0000-0002-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, new Guid("00000000-0000-0000-0003-000000000011"), 30, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

        migrationBuilder.InsertData(
            table: "Properties",
            columns: new[] { "Id", "AllowsMultipleValues", "Code", "CreatedAt", "DataType", "IsCompatibilityRelevant", "IsFilterable", "Name", "UnitDefinitionId", "UpdatedAt" },
            values: new object[,]
            {
                { new Guid("00000000-0000-0000-0003-000000000001"), false, "motor-kv", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, true, "Motor KV", new Guid("00000000-0000-0000-0001-000000000015"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0003-000000000002"), false, "max-current", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, true, "Maximum Current", new Guid("00000000-0000-0000-0001-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0003-000000000003"), false, "input-voltage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, true, true, "Input Voltage", new Guid("00000000-0000-0000-0001-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0003-000000000004"), false, "shaft-diameter", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, true, "Shaft Diameter", new Guid("00000000-0000-0000-0001-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0003-000000000005"), false, "propeller-size", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, true, "Propeller Size", new Guid("00000000-0000-0000-0001-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0003-000000000006"), false, "wheelbase", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, true, "Wheelbase", new Guid("00000000-0000-0000-0001-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0003-000000000007"), false, "mount-width", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, true, "Mount Width", new Guid("00000000-0000-0000-0001-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0003-000000000008"), false, "mount-height", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, true, "Mount Height", new Guid("00000000-0000-0000-0001-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0003-000000000009"), false, "battery-cell-count", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, true, "Battery Cell Count", new Guid("00000000-0000-0000-0001-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0003-000000000010"), false, "battery-capacity", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, true, "Battery Capacity", new Guid("00000000-0000-0000-0001-000000000009"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
            });

        migrationBuilder.InsertData(
            table: "PropertyAliases",
            columns: new[] { "Id", "Alias", "CreatedAt", "ImportSourceId", "NormalizedAlias", "PropertyId", "UpdatedAt" },
            values: new object[,]
            {
                { new Guid("00000000-0000-0000-0006-000000000018"), "connector type", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "connector type", new Guid("00000000-0000-0000-0003-000000000011"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0006-000000000019"), "connector", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "connector", new Guid("00000000-0000-0000-0003-000000000011"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
            });

        migrationBuilder.InsertData(
            table: "UnitAliases",
            columns: new[] { "Id", "Alias", "CreatedAt", "NormalizedAlias", "UnitDefinitionId", "UpdatedAt" },
            values: new object[,]
            {
                { new Guid("00000000-0000-0000-0005-000000000001"), "mm", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "mm", new Guid("00000000-0000-0000-0001-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000002"), "millimeter", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "millimeter", new Guid("00000000-0000-0000-0001-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000003"), "in", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "in", new Guid("00000000-0000-0000-0001-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000004"), "inch", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "inch", new Guid("00000000-0000-0000-0001-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000005"), "\"", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "\"", new Guid("00000000-0000-0000-0001-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000006"), "″", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "″", new Guid("00000000-0000-0000-0001-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000007"), "V", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "v", new Guid("00000000-0000-0000-0001-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000008"), "volt", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "volt", new Guid("00000000-0000-0000-0001-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000009"), "A", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "a", new Guid("00000000-0000-0000-0001-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000010"), "amp", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "amp", new Guid("00000000-0000-0000-0001-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000011"), "S", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "s", new Guid("00000000-0000-0000-0001-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000012"), "cell", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "cell", new Guid("00000000-0000-0000-0001-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000013"), "g", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "g", new Guid("00000000-0000-0000-0001-000000000006"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000014"), "kg", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "kg", new Guid("00000000-0000-0000-0001-000000000007"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000015"), "W", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "w", new Guid("00000000-0000-0000-0001-000000000008"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000016"), "mAh", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "mah", new Guid("00000000-0000-0000-0001-000000000009"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000017"), "mm2", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "mm2", new Guid("00000000-0000-0000-0001-000000000010"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000018"), "mm²", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "mm²", new Guid("00000000-0000-0000-0001-000000000010"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000019"), "Hz", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "hz", new Guid("00000000-0000-0000-0001-000000000011"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000020"), "MHz", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "mhz", new Guid("00000000-0000-0000-0001-000000000012"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000021"), "GHz", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ghz", new Guid("00000000-0000-0000-0001-000000000013"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000022"), "RPM", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "rpm", new Guid("00000000-0000-0000-0001-000000000014"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000023"), "KV", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "kv", new Guid("00000000-0000-0000-0001-000000000015"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000024"), "°C", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "°c", new Guid("00000000-0000-0000-0001-000000000016"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000025"), "millimeters", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "millimeters", new Guid("00000000-0000-0000-0001-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000026"), "millimetre", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "millimetre", new Guid("00000000-0000-0000-0001-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000027"), "millimetres", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "millimetres", new Guid("00000000-0000-0000-0001-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000028"), "inches", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "inches", new Guid("00000000-0000-0000-0001-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000029"), "volts", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "volts", new Guid("00000000-0000-0000-0001-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000030"), "amps", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "amps", new Guid("00000000-0000-0000-0001-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000031"), "ampere", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ampere", new Guid("00000000-0000-0000-0001-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000032"), "cells", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "cells", new Guid("00000000-0000-0000-0001-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000033"), "grams", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "grams", new Guid("00000000-0000-0000-0001-000000000006"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000034"), "watts", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "watts", new Guid("00000000-0000-0000-0001-000000000008"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000035"), "C", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "c", new Guid("00000000-0000-0000-0001-000000000016"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0005-000000000036"), "celsius", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "celsius", new Guid("00000000-0000-0000-0001-000000000016"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
            });

        migrationBuilder.InsertData(
            table: "ComponentTypeProperties",
            columns: new[] { "Id", "ComponentTypeId", "CreatedAt", "IsRequired", "IsVariantSpecific", "PropertyId", "SortOrder", "UpdatedAt" },
            values: new object[,]
            {
                { new Guid("00000000-0000-0000-0004-000000000001"), new Guid("00000000-0000-0000-0002-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, new Guid("00000000-0000-0000-0003-000000000001"), 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0004-000000000002"), new Guid("00000000-0000-0000-0002-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, true, new Guid("00000000-0000-0000-0003-000000000002"), 20, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0004-000000000003"), new Guid("00000000-0000-0000-0002-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, new Guid("00000000-0000-0000-0003-000000000003"), 30, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0004-000000000004"), new Guid("00000000-0000-0000-0002-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, false, new Guid("00000000-0000-0000-0003-000000000004"), 40, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0004-000000000005"), new Guid("00000000-0000-0000-0002-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, new Guid("00000000-0000-0000-0003-000000000005"), 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0004-000000000006"), new Guid("00000000-0000-0000-0002-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, false, new Guid("00000000-0000-0000-0003-000000000006"), 20, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0004-000000000007"), new Guid("00000000-0000-0000-0002-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, new Guid("00000000-0000-0000-0003-000000000007"), 30, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0004-000000000008"), new Guid("00000000-0000-0000-0002-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, new Guid("00000000-0000-0000-0003-000000000008"), 40, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0004-000000000009"), new Guid("00000000-0000-0000-0002-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, new Guid("00000000-0000-0000-0003-000000000009"), 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0004-000000000010"), new Guid("00000000-0000-0000-0002-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, new Guid("00000000-0000-0000-0003-000000000010"), 20, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0004-000000000012"), new Guid("00000000-0000-0000-0002-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, true, new Guid("00000000-0000-0000-0003-000000000002"), 40, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0004-000000000013"), new Guid("00000000-0000-0000-0002-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, new Guid("00000000-0000-0000-0003-000000000002"), 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0004-000000000014"), new Guid("00000000-0000-0000-0002-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, new Guid("00000000-0000-0000-0003-000000000003"), 20, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0004-000000000015"), new Guid("00000000-0000-0000-0002-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, false, new Guid("00000000-0000-0000-0003-000000000007"), 30, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0004-000000000016"), new Guid("00000000-0000-0000-0002-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, false, new Guid("00000000-0000-0000-0003-000000000008"), 40, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0004-000000000017"), new Guid("00000000-0000-0000-0002-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, new Guid("00000000-0000-0000-0003-000000000003"), 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0004-000000000018"), new Guid("00000000-0000-0000-0002-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, new Guid("00000000-0000-0000-0003-000000000007"), 20, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0004-000000000019"), new Guid("00000000-0000-0000-0002-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, new Guid("00000000-0000-0000-0003-000000000008"), 30, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
            });

        migrationBuilder.InsertData(
            table: "PropertyAliases",
            columns: new[] { "Id", "Alias", "CreatedAt", "ImportSourceId", "NormalizedAlias", "PropertyId", "UpdatedAt" },
            values: new object[,]
            {
                { new Guid("00000000-0000-0000-0006-000000000001"), "kv rating", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "kv rating", new Guid("00000000-0000-0000-0003-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0006-000000000002"), "maximum current", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "maximum current", new Guid("00000000-0000-0000-0003-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0006-000000000003"), "max current", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "max current", new Guid("00000000-0000-0000-0003-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0006-000000000004"), "continuous current", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "continuous current", new Guid("00000000-0000-0000-0003-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0006-000000000005"), "input voltage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "input voltage", new Guid("00000000-0000-0000-0003-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0006-000000000006"), "voltage range", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "voltage range", new Guid("00000000-0000-0000-0003-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0006-000000000007"), "supported voltage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "supported voltage", new Guid("00000000-0000-0000-0003-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0006-000000000008"), "shaft diameter", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "shaft diameter", new Guid("00000000-0000-0000-0003-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0006-000000000009"), "propeller size", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "propeller size", new Guid("00000000-0000-0000-0003-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0006-000000000010"), "prop size", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "prop size", new Guid("00000000-0000-0000-0003-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0006-000000000011"), "wheelbase", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "wheelbase", new Guid("00000000-0000-0000-0003-000000000006"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0006-000000000012"), "mount width", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "mount width", new Guid("00000000-0000-0000-0003-000000000007"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0006-000000000013"), "mount height", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "mount height", new Guid("00000000-0000-0000-0003-000000000008"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0006-000000000014"), "cell count", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "cell count", new Guid("00000000-0000-0000-0003-000000000009"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0006-000000000015"), "lipo cells", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "lipo cells", new Guid("00000000-0000-0000-0003-000000000009"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0006-000000000016"), "battery capacity", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "battery capacity", new Guid("00000000-0000-0000-0003-000000000010"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0006-000000000017"), "capacity", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "capacity", new Guid("00000000-0000-0000-0003-000000000010"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
            });

        migrationBuilder.CreateIndex(
            name: "IX_AspNetRoleClaims_RoleId",
            table: "AspNetRoleClaims",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "RoleNameIndex",
            table: "AspNetRoles",
            column: "NormalizedName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserClaims_UserId",
            table: "AspNetUserClaims",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserLogins_UserId",
            table: "AspNetUserLogins",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserRoles_RoleId",
            table: "AspNetUserRoles",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "EmailIndex",
            table: "AspNetUsers",
            column: "NormalizedEmail");

        migrationBuilder.CreateIndex(
            name: "UserNameIndex",
            table: "AspNetUsers",
            column: "NormalizedUserName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CartItems_CartId_ProductVariantId",
            table: "CartItems",
            columns: new[] { "CartId", "ProductVariantId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CartItems_ProductVariantId",
            table: "CartItems",
            column: "ProductVariantId");

        migrationBuilder.CreateIndex(
            name: "IX_Carts_UserId",
            table: "Carts",
            column: "UserId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ComponentTypeProperties_ComponentTypeId_PropertyId",
            table: "ComponentTypeProperties",
            columns: new[] { "ComponentTypeId", "PropertyId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ComponentTypeProperties_PropertyId",
            table: "ComponentTypeProperties",
            column: "PropertyId");

        migrationBuilder.CreateIndex(
            name: "IX_ComponentTypes_Code",
            table: "ComponentTypes",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ComponentTypes_Name",
            table: "ComponentTypes",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Images_ProductId",
            table: "Images",
            column: "ProductId",
            unique: true,
            filter: "\"IsPrimary\" = TRUE");

        migrationBuilder.CreateIndex(
            name: "IX_ImportBatches_ImportSourceId_CreatedAt",
            table: "ImportBatches",
            columns: new[] { "ImportSourceId", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_ImportItems_ImportBatchId_ExternalId",
            table: "ImportItems",
            columns: new[] { "ImportBatchId", "ExternalId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ImportItems_ProductId",
            table: "ImportItems",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_ImportItems_Status",
            table: "ImportItems",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_ImportSources_Code",
            table: "ImportSources",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_InventoryReservations_CartId",
            table: "InventoryReservations",
            column: "CartId");

        migrationBuilder.CreateIndex(
            name: "IX_InventoryReservations_CartItemId",
            table: "InventoryReservations",
            column: "CartItemId",
            unique: true,
            filter: "\"CartItemId\" IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_InventoryReservations_Status_ExpiresAt",
            table: "InventoryReservations",
            columns: new[] { "Status", "ExpiresAt" });

        migrationBuilder.CreateIndex(
            name: "IX_InventoryReservations_WarehouseItemId",
            table: "InventoryReservations",
            column: "WarehouseItemId");

        migrationBuilder.CreateIndex(
            name: "IX_Messages_CreatedAt",
            table: "Messages",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_Messages_ProcessedAt",
            table: "Messages",
            column: "ProcessedAt");

        migrationBuilder.CreateIndex(
            name: "IX_OrderItems_OrderId",
            table: "OrderItems",
            column: "OrderId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderItems_ProductVariantId",
            table: "OrderItems",
            column: "ProductVariantId");

        migrationBuilder.CreateIndex(
            name: "IX_Orders_UserId_CreatedAt",
            table: "Orders",
            columns: new[] { "UserId", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_ProductCategories_Code",
            table: "ProductCategories",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ProductCategories_Name",
            table: "ProductCategories",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ProductCategories_ParentId",
            table: "ProductCategories",
            column: "ParentId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductExternalReferences_ImportSourceId_ExternalId",
            table: "ProductExternalReferences",
            columns: new[] { "ImportSourceId", "ExternalId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ProductExternalReferences_ProductId",
            table: "ProductExternalReferences",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductPropertyValues_ProductId_PropertyId",
            table: "ProductPropertyValues",
            columns: new[] { "ProductId", "PropertyId" });

        migrationBuilder.CreateIndex(
            name: "IX_ProductPropertyValues_PropertyId_ValueId",
            table: "ProductPropertyValues",
            columns: new[] { "PropertyId", "ValueId" });

        migrationBuilder.CreateIndex(
            name: "IX_ProductPropertyValues_ValueId",
            table: "ProductPropertyValues",
            column: "ValueId");

        migrationBuilder.CreateIndex(
            name: "IX_Products_ComponentTypeId",
            table: "Products",
            column: "ComponentTypeId");

        migrationBuilder.CreateIndex(
            name: "IX_Products_ProductCategoryId_IsActive",
            table: "Products",
            columns: new[] { "ProductCategoryId", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_ProductVariantExternalReferences_ImportSourceId_ExternalId",
            table: "ProductVariantExternalReferences",
            columns: new[] { "ImportSourceId", "ExternalId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ProductVariantExternalReferences_ProductVariantId",
            table: "ProductVariantExternalReferences",
            column: "ProductVariantId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductVariantPropertyValues_ProductVariantId_PropertyId",
            table: "ProductVariantPropertyValues",
            columns: new[] { "ProductVariantId", "PropertyId" });

        migrationBuilder.CreateIndex(
            name: "IX_ProductVariantPropertyValues_PropertyId_ValueId",
            table: "ProductVariantPropertyValues",
            columns: new[] { "PropertyId", "ValueId" });

        migrationBuilder.CreateIndex(
            name: "IX_ProductVariantPropertyValues_ValueId",
            table: "ProductVariantPropertyValues",
            column: "ValueId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductVariants_ProductId_IsDefault",
            table: "ProductVariants",
            columns: new[] { "ProductId", "IsDefault" },
            unique: true,
            filter: "\"IsDefault\" = TRUE");

        migrationBuilder.CreateIndex(
            name: "IX_ProductVariants_Sku",
            table: "ProductVariants",
            column: "Sku",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Properties_Code",
            table: "Properties",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Properties_UnitDefinitionId",
            table: "Properties",
            column: "UnitDefinitionId");

        migrationBuilder.CreateIndex(
            name: "IX_PropertyAliases_ImportSourceId_NormalizedAlias",
            table: "PropertyAliases",
            columns: new[] { "ImportSourceId", "NormalizedAlias" },
            unique: true,
            filter: "\"ImportSourceId\" IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_PropertyAliases_NormalizedAlias",
            table: "PropertyAliases",
            column: "NormalizedAlias",
            unique: true,
            filter: "\"ImportSourceId\" IS NULL");

        migrationBuilder.CreateIndex(
            name: "IX_PropertyAliases_PropertyId",
            table: "PropertyAliases",
            column: "PropertyId");

        migrationBuilder.CreateIndex(
            name: "IX_PropertyValues_ValueId",
            table: "PropertyValues",
            column: "ValueId");

        migrationBuilder.CreateIndex(
            name: "IX_UnitAliases_NormalizedAlias",
            table: "UnitAliases",
            column: "NormalizedAlias",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_UnitAliases_UnitDefinitionId",
            table: "UnitAliases",
            column: "UnitDefinitionId");

        migrationBuilder.CreateIndex(
            name: "IX_UnitDefinitions_Code",
            table: "UnitDefinitions",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ValueAliases_ImportSourceId_NormalizedAlias",
            table: "ValueAliases",
            columns: new[] { "ImportSourceId", "NormalizedAlias" },
            unique: true,
            filter: "\"ImportSourceId\" IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_ValueAliases_NormalizedAlias",
            table: "ValueAliases",
            column: "NormalizedAlias",
            unique: true,
            filter: "\"ImportSourceId\" IS NULL");

        migrationBuilder.CreateIndex(
            name: "IX_ValueAliases_ValueId",
            table: "ValueAliases",
            column: "ValueId");

        migrationBuilder.CreateIndex(
            name: "IX_Values_Code",
            table: "Values",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_WarehouseItems_ProductVariantId",
            table: "WarehouseItems",
            column: "ProductVariantId");

        migrationBuilder.CreateIndex(
            name: "IX_WarehouseItems_WarehouseId_ProductVariantId",
            table: "WarehouseItems",
            columns: new[] { "WarehouseId", "ProductVariantId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Warehouses_Code",
            table: "Warehouses",
            column: "Code",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AspNetRoleClaims");

        migrationBuilder.DropTable(
            name: "AspNetUserClaims");

        migrationBuilder.DropTable(
            name: "AspNetUserLogins");

        migrationBuilder.DropTable(
            name: "AspNetUserRoles");

        migrationBuilder.DropTable(
            name: "AspNetUserTokens");

        migrationBuilder.DropTable(
            name: "ComponentTypeProperties");

        migrationBuilder.DropTable(
            name: "Images");

        migrationBuilder.DropTable(
            name: "ImportItems");

        migrationBuilder.DropTable(
            name: "InventoryReservations");

        migrationBuilder.DropTable(
            name: "Messages");

        migrationBuilder.DropTable(
            name: "OrderItems");

        migrationBuilder.DropTable(
            name: "ProductExternalReferences");

        migrationBuilder.DropTable(
            name: "ProductPropertyValues");

        migrationBuilder.DropTable(
            name: "ProductVariantExternalReferences");

        migrationBuilder.DropTable(
            name: "ProductVariantPropertyValues");

        migrationBuilder.DropTable(
            name: "PropertyAliases");

        migrationBuilder.DropTable(
            name: "UnitAliases");

        migrationBuilder.DropTable(
            name: "ValueAliases");

        migrationBuilder.DropTable(
            name: "AspNetRoles");

        migrationBuilder.DropTable(
            name: "ImportBatches");

        migrationBuilder.DropTable(
            name: "CartItems");

        migrationBuilder.DropTable(
            name: "WarehouseItems");

        migrationBuilder.DropTable(
            name: "Orders");

        migrationBuilder.DropTable(
            name: "PropertyValues");

        migrationBuilder.DropTable(
            name: "ImportSources");

        migrationBuilder.DropTable(
            name: "Carts");

        migrationBuilder.DropTable(
            name: "ProductVariants");

        migrationBuilder.DropTable(
            name: "Warehouses");

        migrationBuilder.DropTable(
            name: "Properties");

        migrationBuilder.DropTable(
            name: "Values");

        migrationBuilder.DropTable(
            name: "AspNetUsers");

        migrationBuilder.DropTable(
            name: "Products");

        migrationBuilder.DropTable(
            name: "UnitDefinitions");

        migrationBuilder.DropTable(
            name: "ComponentTypes");

        migrationBuilder.DropTable(
            name: "ProductCategories");
    }
}
