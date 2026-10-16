using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OLTPApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDomainModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agency",
                columns: table => new
                {
                    AgencyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Province = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agency", x => x.AgencyID);
                });

            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    AuditLogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserAccountID = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.AuditLogID);
                    table.ForeignKey(
                        name: "FK_AuditLog_AspNetUsers_UserAccountID",
                        column: x => x.UserAccountID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Person",
                columns: table => new
                {
                    PersonID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Province = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Person", x => x.PersonID);
                });

            migrationBuilder.CreateTable(
                name: "Service",
                columns: table => new
                {
                    ServiceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Service", x => x.ServiceID);
                });

            migrationBuilder.CreateTable(
                name: "AgencyManager",
                columns: table => new
                {
                    AgencyManagerID = table.Column<int>(type: "int", nullable: false),
                    AgencyID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencyManager", x => x.AgencyManagerID);
                    table.ForeignKey(
                        name: "FK_AgencyManager_Agency_AgencyID",
                        column: x => x.AgencyID,
                        principalTable: "Agency",
                        principalColumn: "AgencyID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgencyManager_Person_AgencyManagerID",
                        column: x => x.AgencyManagerID,
                        principalTable: "Person",
                        principalColumn: "PersonID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Caregiver",
                columns: table => new
                {
                    CaregiverID = table.Column<int>(type: "int", nullable: false),
                    AgencyID = table.Column<int>(type: "int", nullable: false),
                    IsTrained = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Caregiver", x => x.CaregiverID);
                    table.ForeignKey(
                        name: "FK_Caregiver_Agency_AgencyID",
                        column: x => x.AgencyID,
                        principalTable: "Agency",
                        principalColumn: "AgencyID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Caregiver_Person_CaregiverID",
                        column: x => x.CaregiverID,
                        principalTable: "Person",
                        principalColumn: "PersonID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FamilyMember",
                columns: table => new
                {
                    FamilyMemberID = table.Column<int>(type: "int", nullable: false),
                    WorkPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyMember", x => x.FamilyMemberID);
                    table.ForeignKey(
                        name: "FK_FamilyMember_Person_FamilyMemberID",
                        column: x => x.FamilyMemberID,
                        principalTable: "Person",
                        principalColumn: "PersonID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Senior",
                columns: table => new
                {
                    SeniorID = table.Column<int>(type: "int", nullable: false),
                    SINEncrypted = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    SINLast3 = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    IsLivingAlone = table.Column<bool>(type: "bit", nullable: false),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentDoctorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CurrentDoctorPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    InsuranceCompany = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    InsuranceAccountNoEncrypted = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    MedicalConditions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DeactivationRequestedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeactivatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetainUntil = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Senior", x => x.SeniorID);
                    table.ForeignKey(
                        name: "FK_Senior_Person_SeniorID",
                        column: x => x.SeniorID,
                        principalTable: "Person",
                        principalColumn: "PersonID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CaregiverAvailability",
                columns: table => new
                {
                    AvailabilityID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaregiverID = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaregiverAvailability", x => x.AvailabilityID);
                    table.ForeignKey(
                        name: "FK_CaregiverAvailability_Caregiver_CaregiverID",
                        column: x => x.CaregiverID,
                        principalTable: "Caregiver",
                        principalColumn: "CaregiverID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CaregiverAssignment",
                columns: table => new
                {
                    AssignmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeniorID = table.Column<int>(type: "int", nullable: false),
                    CaregiverID = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaregiverAssignment", x => x.AssignmentID);
                    table.ForeignKey(
                        name: "FK_CaregiverAssignment_Caregiver_CaregiverID",
                        column: x => x.CaregiverID,
                        principalTable: "Caregiver",
                        principalColumn: "CaregiverID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CaregiverAssignment_Senior_SeniorID",
                        column: x => x.SeniorID,
                        principalTable: "Senior",
                        principalColumn: "SeniorID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Medication",
                columns: table => new
                {
                    MedicationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeniorID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Dosage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PrescribedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    RefillDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medication", x => x.MedicationID);
                    table.ForeignKey(
                        name: "FK_Medication_Senior_SeniorID",
                        column: x => x.SeniorID,
                        principalTable: "Senior",
                        principalColumn: "SeniorID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SeniorFamilyMember",
                columns: table => new
                {
                    SeniorID = table.Column<int>(type: "int", nullable: false),
                    FamilyMemberID = table.Column<int>(type: "int", nullable: false),
                    Relationship = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsEmergencyContact = table.Column<bool>(type: "bit", nullable: false),
                    IsPowerOfAttorney = table.Column<bool>(type: "bit", nullable: false),
                    AccessApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CanViewHealth = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeniorFamilyMember", x => new { x.SeniorID, x.FamilyMemberID });
                    table.ForeignKey(
                        name: "FK_SeniorFamilyMember_FamilyMember_FamilyMemberID",
                        column: x => x.FamilyMemberID,
                        principalTable: "FamilyMember",
                        principalColumn: "FamilyMemberID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SeniorFamilyMember_Senior_SeniorID",
                        column: x => x.SeniorID,
                        principalTable: "Senior",
                        principalColumn: "SeniorID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Visit",
                columns: table => new
                {
                    VisitID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeniorID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    CaregiverID = table.Column<int>(type: "int", nullable: false),
                    AgencyID = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserID = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ScheduledStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActualCheckIn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualCheckOut = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CancelReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DestinationAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visit", x => x.VisitID);
                    table.ForeignKey(
                        name: "FK_Visit_Agency_AgencyID",
                        column: x => x.AgencyID,
                        principalTable: "Agency",
                        principalColumn: "AgencyID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visit_AspNetUsers_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visit_Caregiver_CaregiverID",
                        column: x => x.CaregiverID,
                        principalTable: "Caregiver",
                        principalColumn: "CaregiverID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visit_Senior_SeniorID",
                        column: x => x.SeniorID,
                        principalTable: "Senior",
                        principalColumn: "SeniorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visit_Service_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "Service",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VisitNote",
                columns: table => new
                {
                    NoteID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitID = table.Column<int>(type: "int", nullable: false),
                    CaregiverID = table.Column<int>(type: "int", nullable: false),
                    NoteText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitNote", x => x.NoteID);
                    table.ForeignKey(
                        name: "FK_VisitNote_Caregiver_CaregiverID",
                        column: x => x.CaregiverID,
                        principalTable: "Caregiver",
                        principalColumn: "CaregiverID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VisitNote_Visit_VisitID",
                        column: x => x.VisitID,
                        principalTable: "Visit",
                        principalColumn: "VisitID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgencyManager_AgencyID",
                table: "AgencyManager",
                column: "AgencyID");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_UserAccountID",
                table: "AuditLog",
                column: "UserAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_Caregiver_AgencyID",
                table: "Caregiver",
                column: "AgencyID");

            migrationBuilder.CreateIndex(
                name: "IX_CaregiverAssignment_CaregiverID",
                table: "CaregiverAssignment",
                column: "CaregiverID");

            migrationBuilder.CreateIndex(
                name: "IX_CaregiverAssignment_SeniorID",
                table: "CaregiverAssignment",
                column: "SeniorID");

            migrationBuilder.CreateIndex(
                name: "IX_CaregiverAvailability_CaregiverID",
                table: "CaregiverAvailability",
                column: "CaregiverID");

            migrationBuilder.CreateIndex(
                name: "IX_Medication_SeniorID",
                table: "Medication",
                column: "SeniorID");

            migrationBuilder.CreateIndex(
                name: "IX_SeniorFamilyMember_FamilyMemberID",
                table: "SeniorFamilyMember",
                column: "FamilyMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_Visit_AgencyID",
                table: "Visit",
                column: "AgencyID");

            migrationBuilder.CreateIndex(
                name: "IX_Visit_CaregiverID",
                table: "Visit",
                column: "CaregiverID");

            migrationBuilder.CreateIndex(
                name: "IX_Visit_CreatedByUserID",
                table: "Visit",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Visit_SeniorID",
                table: "Visit",
                column: "SeniorID");

            migrationBuilder.CreateIndex(
                name: "IX_Visit_ServiceID",
                table: "Visit",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_VisitNote_CaregiverID",
                table: "VisitNote",
                column: "CaregiverID");

            migrationBuilder.CreateIndex(
                name: "IX_VisitNote_VisitID",
                table: "VisitNote",
                column: "VisitID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgencyManager");

            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "CaregiverAssignment");

            migrationBuilder.DropTable(
                name: "CaregiverAvailability");

            migrationBuilder.DropTable(
                name: "Medication");

            migrationBuilder.DropTable(
                name: "SeniorFamilyMember");

            migrationBuilder.DropTable(
                name: "VisitNote");

            migrationBuilder.DropTable(
                name: "FamilyMember");

            migrationBuilder.DropTable(
                name: "Visit");

            migrationBuilder.DropTable(
                name: "Caregiver");

            migrationBuilder.DropTable(
                name: "Senior");

            migrationBuilder.DropTable(
                name: "Service");

            migrationBuilder.DropTable(
                name: "Agency");

            migrationBuilder.DropTable(
                name: "Person");
        }
    }
}
