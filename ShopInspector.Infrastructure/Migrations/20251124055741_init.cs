using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopInspector.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetTypes",
                columns: table => new
                {
                    AssetTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetTypeName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetTypes", x => x.AssetTypeID);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    CompanyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CompanyAdminEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CompanyContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.CompanyID);
                });

            migrationBuilder.CreateTable(
                name: "InspectionCheckLists",
                columns: table => new
                {
                    InspectionCheckListID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectionCheckListName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InspectionCheckListDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InspectionCheckListTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionCheckLists", x => x.InspectionCheckListID);
                });

            migrationBuilder.CreateTable(
                name: "InspectionFrequencies",
                columns: table => new
                {
                    InspectionFrequencyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FrequencyName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionFrequencies", x => x.InspectionFrequencyID);
                });

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    AssetID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssetLocation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssetTypeID = table.Column<int>(type: "int", nullable: false),
                    AssetCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.AssetID);
                    table.ForeignKey(
                        name: "FK_Assets_AssetTypes_AssetTypeID",
                        column: x => x.AssetTypeID,
                        principalTable: "AssetTypes",
                        principalColumn: "AssetTypeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyID = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeID);
                    table.ForeignKey(
                        name: "FK_Employees_Companies_CompanyID",
                        column: x => x.CompanyID,
                        principalTable: "Companies",
                        principalColumn: "CompanyID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssetCheckLists",
                columns: table => new
                {
                    AssetCheckListID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetID = table.Column<int>(type: "int", nullable: false),
                    InspectionCheckListID = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetCheckLists", x => x.AssetCheckListID);
                    table.ForeignKey(
                        name: "FK_AssetCheckLists_Assets_AssetID",
                        column: x => x.AssetID,
                        principalTable: "Assets",
                        principalColumn: "AssetID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetCheckLists_InspectionCheckLists_InspectionCheckListID",
                        column: x => x.InspectionCheckListID,
                        principalTable: "InspectionCheckLists",
                        principalColumn: "InspectionCheckListID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssetInspections",
                columns: table => new
                {
                    AssetInspectionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetID = table.Column<int>(type: "int", nullable: false),
                    InspectorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Attachment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InspectionFrequencyID = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    ThirdParty = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetInspections", x => x.AssetInspectionID);
                    table.ForeignKey(
                        name: "FK_AssetInspections_Assets_AssetID",
                        column: x => x.AssetID,
                        principalTable: "Assets",
                        principalColumn: "AssetID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetInspections_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetInspections_InspectionFrequencies_InspectionFrequencyID",
                        column: x => x.InspectionFrequencyID,
                        principalTable: "InspectionFrequencies",
                        principalColumn: "InspectionFrequencyID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssetInspectionCheckLists",
                columns: table => new
                {
                    AssetInspectionCheckListID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetInspectionID = table.Column<int>(type: "int", nullable: false),
                    AssetCheckListID = table.Column<int>(type: "int", nullable: false),
                    IsChecked = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetInspectionCheckLists", x => x.AssetInspectionCheckListID);
                    table.ForeignKey(
                        name: "FK_AssetInspectionCheckLists_AssetCheckLists_AssetCheckListID",
                        column: x => x.AssetCheckListID,
                        principalTable: "AssetCheckLists",
                        principalColumn: "AssetCheckListID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetInspectionCheckLists_AssetInspections_AssetInspectionID",
                        column: x => x.AssetInspectionID,
                        principalTable: "AssetInspections",
                        principalColumn: "AssetInspectionID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InspectionPhotos",
                columns: table => new
                {
                    InspectionPhotoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetInspectionID = table.Column<int>(type: "int", nullable: false),
                    PhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionPhotos", x => x.InspectionPhotoID);
                    table.ForeignKey(
                        name: "FK_InspectionPhotos_AssetInspections_AssetInspectionID",
                        column: x => x.AssetInspectionID,
                        principalTable: "AssetInspections",
                        principalColumn: "AssetInspectionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetCheckLists_AssetID",
                table: "AssetCheckLists",
                column: "AssetID");

            migrationBuilder.CreateIndex(
                name: "IX_AssetCheckLists_InspectionCheckListID",
                table: "AssetCheckLists",
                column: "InspectionCheckListID");

            migrationBuilder.CreateIndex(
                name: "IX_AssetInspectionCheckLists_AssetCheckListID",
                table: "AssetInspectionCheckLists",
                column: "AssetCheckListID");

            migrationBuilder.CreateIndex(
                name: "IX_AssetInspectionCheckLists_AssetInspectionID",
                table: "AssetInspectionCheckLists",
                column: "AssetInspectionID");

            migrationBuilder.CreateIndex(
                name: "IX_AssetInspections_AssetID",
                table: "AssetInspections",
                column: "AssetID");

            migrationBuilder.CreateIndex(
                name: "IX_AssetInspections_EmployeeID",
                table: "AssetInspections",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_AssetInspections_InspectionFrequencyID",
                table: "AssetInspections",
                column: "InspectionFrequencyID");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_AssetTypeID",
                table: "Assets",
                column: "AssetTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_CompanyID",
                table: "Employees",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionPhotos_AssetInspectionID",
                table: "InspectionPhotos",
                column: "AssetInspectionID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetInspectionCheckLists");

            migrationBuilder.DropTable(
                name: "InspectionPhotos");

            migrationBuilder.DropTable(
                name: "AssetCheckLists");

            migrationBuilder.DropTable(
                name: "AssetInspections");

            migrationBuilder.DropTable(
                name: "InspectionCheckLists");

            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "InspectionFrequencies");

            migrationBuilder.DropTable(
                name: "AssetTypes");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
