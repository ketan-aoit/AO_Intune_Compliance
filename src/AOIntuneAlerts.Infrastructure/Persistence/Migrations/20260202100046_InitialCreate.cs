using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AOIntuneAlerts.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertCooldowns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlertType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastAlertSent = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CooldownExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertCooldowns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AlertRecipients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    MinimumSeverity = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertRecipients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WasSent = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    RuleType = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Configuration = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplicableOs = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IntuneDeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeviceName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UserPrincipalName = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    UserDisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DeviceType = table.Column<int>(type: "int", nullable: false),
                    OsType = table.Column<int>(type: "int", nullable: false),
                    OsName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OsVersionMajor = table.Column<int>(type: "int", nullable: false),
                    OsVersionMinor = table.Column<int>(type: "int", nullable: false),
                    OsVersionPatch = table.Column<int>(type: "int", nullable: false),
                    OsVersionPreRelease = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OsVersionBuild = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OsEdition = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OsBuildNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ComplianceState = table.Column<int>(type: "int", nullable: false),
                    IntuneComplianceState = table.Column<int>(type: "int", nullable: false),
                    LastSyncDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastComplianceEvaluationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndOfSupportDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsEncrypted = table.Column<bool>(type: "bit", nullable: false),
                    IsManaged = table.Column<bool>(type: "bit", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PortalUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AzureAdObjectId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserPrincipalName = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortalUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VendorSupportDates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperatingSystemType = table.Column<int>(type: "int", nullable: false),
                    VersionPattern = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MinVersionMajor = table.Column<int>(type: "int", nullable: false),
                    MinVersionMinor = table.Column<int>(type: "int", nullable: false),
                    MinVersionPatch = table.Column<int>(type: "int", nullable: false),
                    MinVersionPreRelease = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MinVersionBuild = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EndOfSupportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorSupportDates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AlertAlertRecipient",
                columns: table => new
                {
                    AlertId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlertRecipientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertAlertRecipient", x => new { x.AlertId, x.AlertRecipientId });
                    table.ForeignKey(
                        name: "FK_AlertAlertRecipient_AlertRecipients_AlertRecipientId",
                        column: x => x.AlertRecipientId,
                        principalTable: "AlertRecipients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlertAlertRecipient_Alerts_AlertId",
                        column: x => x.AlertId,
                        principalTable: "Alerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceBrowsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BrowserType = table.Column<int>(type: "int", nullable: false),
                    BrowserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BrowserVersionMajor = table.Column<int>(type: "int", nullable: false),
                    BrowserVersionMinor = table.Column<int>(type: "int", nullable: false),
                    BrowserVersionPatch = table.Column<int>(type: "int", nullable: false),
                    BrowserVersionPreRelease = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BrowserVersionBuild = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsCompliant = table.Column<bool>(type: "bit", nullable: false),
                    LastCheckedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceBrowsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceBrowsers_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceComplianceIssues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RuleId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RuleName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    DetectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceComplianceIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceComplianceIssues_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertAlertRecipient_AlertRecipientId",
                table: "AlertAlertRecipient",
                column: "AlertRecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertCooldowns_DeviceId_AlertType",
                table: "AlertCooldowns",
                columns: new[] { "DeviceId", "AlertType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlertRecipients_IsEnabled",
                table: "AlertRecipients",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_CreatedAt",
                table: "Alerts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_DeviceId",
                table: "Alerts",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_SentAt",
                table: "Alerts",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRules_IsEnabled",
                table: "ComplianceRules",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRules_RuleType",
                table: "ComplianceRules",
                column: "RuleType");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceBrowsers_DeviceId",
                table: "DeviceBrowsers",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceComplianceIssues_DeviceId",
                table: "DeviceComplianceIssues",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_IntuneDeviceId",
                table: "Devices",
                column: "IntuneDeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortalUsers_AzureAdObjectId",
                table: "PortalUsers",
                column: "AzureAdObjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorSupportDates_OperatingSystemType_VersionPattern",
                table: "VendorSupportDates",
                columns: new[] { "OperatingSystemType", "VersionPattern" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertAlertRecipient");

            migrationBuilder.DropTable(
                name: "AlertCooldowns");

            migrationBuilder.DropTable(
                name: "ComplianceRules");

            migrationBuilder.DropTable(
                name: "DeviceBrowsers");

            migrationBuilder.DropTable(
                name: "DeviceComplianceIssues");

            migrationBuilder.DropTable(
                name: "PortalUsers");

            migrationBuilder.DropTable(
                name: "VendorSupportDates");

            migrationBuilder.DropTable(
                name: "AlertRecipients");

            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "Devices");
        }
    }
}
