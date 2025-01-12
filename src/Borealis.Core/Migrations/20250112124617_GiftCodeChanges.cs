using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Borealis.Core.Migrations
{
    /// <inheritdoc />
    public partial class GiftCodeChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GiftCodeRedemption_GiftCodes_GiftCodeId",
                table: "GiftCodeRedemption");

            migrationBuilder.DropForeignKey(
                name: "FK_GiftCodeRedemption_Players_PlayerId",
                table: "GiftCodeRedemption");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GiftCodeRedemption",
                table: "GiftCodeRedemption");

            migrationBuilder.RenameTable(
                name: "GiftCodeRedemption",
                newName: "GiftCodeRedemptions")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "GiftCodeRedemptionHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.RenameIndex(
                name: "IX_GiftCodeRedemption_PlayerId",
                table: "GiftCodeRedemptions",
                newName: "IX_GiftCodeRedemptions_PlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_GiftCodeRedemption_GiftCodeId",
                table: "GiftCodeRedemptions",
                newName: "IX_GiftCodeRedemptions_GiftCodeId");

            migrationBuilder.AlterTable(
                name: "GiftCodeRedemptions")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "GiftCodeRedemptionsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart")
                .OldAnnotation("SqlServer:IsTemporal", true)
                .OldAnnotation("SqlServer:TemporalHistoryTableName", "GiftCodeRedemptionHistory")
                .OldAnnotation("SqlServer:TemporalHistoryTableSchema", null)
                .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GiftCodeRedemptions",
                table: "GiftCodeRedemptions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GiftCodeRedemptions_GiftCodes_GiftCodeId",
                table: "GiftCodeRedemptions",
                column: "GiftCodeId",
                principalTable: "GiftCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GiftCodeRedemptions_Players_PlayerId",
                table: "GiftCodeRedemptions",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GiftCodeRedemptions_GiftCodes_GiftCodeId",
                table: "GiftCodeRedemptions");

            migrationBuilder.DropForeignKey(
                name: "FK_GiftCodeRedemptions_Players_PlayerId",
                table: "GiftCodeRedemptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GiftCodeRedemptions",
                table: "GiftCodeRedemptions");

            migrationBuilder.RenameTable(
                name: "GiftCodeRedemptions",
                newName: "GiftCodeRedemption")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "GiftCodeRedemptionsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.RenameIndex(
                name: "IX_GiftCodeRedemptions_PlayerId",
                table: "GiftCodeRedemption",
                newName: "IX_GiftCodeRedemption_PlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_GiftCodeRedemptions_GiftCodeId",
                table: "GiftCodeRedemption",
                newName: "IX_GiftCodeRedemption_GiftCodeId");

            migrationBuilder.AlterTable(
                name: "GiftCodeRedemption")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "GiftCodeRedemptionHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart")
                .OldAnnotation("SqlServer:IsTemporal", true)
                .OldAnnotation("SqlServer:TemporalHistoryTableName", "GiftCodeRedemptionsHistory")
                .OldAnnotation("SqlServer:TemporalHistoryTableSchema", null)
                .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GiftCodeRedemption",
                table: "GiftCodeRedemption",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GiftCodeRedemption_GiftCodes_GiftCodeId",
                table: "GiftCodeRedemption",
                column: "GiftCodeId",
                principalTable: "GiftCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GiftCodeRedemption_Players_PlayerId",
                table: "GiftCodeRedemption",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
