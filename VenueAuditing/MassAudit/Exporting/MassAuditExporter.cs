using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Utils.Broadcasting;
using FFXIVVenues.Veni.VenueAuditing.MassAudit.Models;
using FFXIVVenues.VenueModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ScottPlot;

namespace FFXIVVenues.Veni.VenueAuditing.MassAudit.Exporting;

public class MassAuditExporter : IMassAuditExporter
{

    public MassAuditStatusSummary GetSummaryForMassAudit(MassAuditRecord massAudit, IEnumerable<Venue> allVenues,
        IQueryable<VenueAuditRecord> audits)
    {
        var statusSummary = new MassAuditStatusSummary
        {
            id = massAudit.id,
            Status = massAudit.Status,
            StartedAt = massAudit.StartedAt,
            PausedAt = massAudit.PausedAt,
            CompletedAt = massAudit.CompletedAt,
            RequestedBy = massAudit.RequestedBy,
            TotalVenues = allVenues.Count(),
            AuditsSent = 0,
            AuditsAnswered = 0,
            VenuesConfirmed = 0,
            VenuesEdited = 0,
            VenuesClosed = 0,
            VenuesDeleted = 0,
            AuditsAwaitingAnswer = 0,
            AuditsInProgress = 0,
            AuditsSkipped = 0,
            AuditsFailed = 0
        };

        foreach (var audit in audits)
        {
            statusSummary.AuditsSent++;
            switch (audit.Status)
            {
                case VenueAuditStatus.Pending:
                    statusSummary.AuditsInProgress++;
                    break;
                case VenueAuditStatus.AwaitingResponse:
                    statusSummary.AuditsAwaitingAnswer++;
                    break;
                case VenueAuditStatus.RespondedEdit:
                    statusSummary.AuditsAnswered++;
                    statusSummary.VenuesEdited++;
                    break;
                case VenueAuditStatus.RespondedConfirmed:
                    statusSummary.AuditsAnswered++;
                    statusSummary.VenuesConfirmed++;
                    break;
                case VenueAuditStatus.RespondedDelete:
                    statusSummary.AuditsAnswered++;
                    statusSummary.VenuesDeleted++;
                    break;
                case VenueAuditStatus.RespondedClose:
                    statusSummary.AuditsAnswered++;
                    statusSummary.VenuesClosed++;
                    break;
                case VenueAuditStatus.Skipped:
                    statusSummary.AuditsSkipped++;
                    break;
                case VenueAuditStatus.Failed:
                    statusSummary.AuditsFailed++;
                    break;
            }
        }

        return statusSummary;
    }
    
    
    public async Task<MassAuditStatusReport> GetExportForMassAuditAsync(MassAuditRecord massAudit, 
                                               IEnumerable<Venue> venues, 
                                               IQueryable<VenueAuditRecord> audits)
    {

        var summary = this.GetSummaryForMassAudit(massAudit, venues, audits);
        using var package = new ExcelPackage();
        AddOverviewWorksheet(summary, venues, package);
        AddLogsWorksheet(massAudit.Logs, audits, package);
        AddVenuesWorksheet(venues, audits, package);
        
        return new()
        {
            FileName = "MassAuditReport.xlsx",
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ContentStream = new MemoryStream(await package.GetAsByteArrayAsync())
        };
    }

    private static ExcelWorksheet AddVenuesWorksheet(IEnumerable<Venue> venues, IEnumerable<VenueAuditRecord> audits, ExcelPackage package)
    {
        var worksheet = package.Workbook.Worksheets.Add("Venues");
        worksheet.Cells[1, 1, 10_000, 15].AutoFilter = true;
        worksheet.View.FreezePanes(2, 1);
        worksheet.Rows.Style.Font.Size = 12;
        worksheet.Row(1).Style.Font.Bold = true;
        worksheet.Row(1).Style.Fill.PatternType = ExcelFillStyle.Solid;
        worksheet.Row(1).Style.Fill.BackgroundColor.SetColor(0, 64, 64, 64);
        worksheet.Row(1).Style.Font.Color.SetColor(Color.White);
        worksheet.Cells[1, 1].Value = "Id";
        worksheet.Column(1).Width = 17;
        worksheet.Cells[1, 2].Value = "Name";
        worksheet.Column(2).Width = 35;
        worksheet.Cells[1, 3].Value = "Added";
        worksheet.Column(3).Width = 25;
        worksheet.Cells[1, 4].Value = "Data Center";
        worksheet.Column(4).Width = 15;
        worksheet.Cells[1, 5].Value = "Managers";
        worksheet.Column(5).Width = 28;
        worksheet.Cells[1, 6].Value = "Managers Count";
        worksheet.Column(6).Width = 18.5;
        worksheet.Column(6).Style.Numberformat.Format = "@";
        worksheet.Column(6).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        worksheet.Cells[1, 7].Value = "Scheduled Openings";
        worksheet.Column(7).Width = 21.5;
        worksheet.Column(7).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        worksheet.Cells[1, 8].Value = "Uri";
        worksheet.Column(8).Width = 35;
        worksheet.Cells[1, 9].Value = "Audit Id";
        worksheet.Column(9).Width = 15;
        worksheet.Cells[1, 10].Value = "Audit Status";
        worksheet.Column(10).Width = 25;
        worksheet.Cells[1, 11].Value = "Audit Sent At";
        worksheet.Column(11).Width = 23;
        worksheet.Cells[1, 12].Value = "Audit Sent To";
        worksheet.Column(12).Width = 28;
        worksheet.Cells[1, 13].Value = "Audit Sent To Count";
        worksheet.Column(13).Width = 21;
        worksheet.Column(13).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        worksheet.Cells[1, 14].Value = "Audit Answered At";
        worksheet.Column(14).Width = 23;
        worksheet.Cells[1, 15].Value = "Audit Answered By";
        worksheet.Column(15).Width = 25;

        var row = 2;
        foreach (var venue in venues)
        {
            var audit = audits.FirstOrDefault(a => a.VenueId == venue.Id);
            
            worksheet.Cells[row, 1].Value = venue.Id;
            worksheet.Cells[row, 2].Value = venue.Name;
            worksheet.Cells[row, 3].Value = venue.Added;
            worksheet.Cells[row, 4].Value = venue.Location?.DataCenter ?? "";
            worksheet.Cells[row, 5].Value = venue.Managers != null ? string.Join(",", venue.Managers) : "";
            worksheet.Cells[row, 5].Style.Numberformat.Format = "@";
            worksheet.Cells[row, 6].Value = venue.Managers?.Count ?? 0;
            worksheet.Cells[row, 7].Value = venue.Openings?.Count ?? 0;
            worksheet.Cells[row, 8].Value = $"https://ffxivvenues.com/#{venue.Id}";
            worksheet.Cells[row, 8].Hyperlink = new Uri($"https://ffxivvenues.com/#{venue.Id}");
            worksheet.Cells[row, 8].Style.Font.Color.SetColor(Color.CornflowerBlue);
            worksheet.Cells[row, 9].Value = audit?.id ?? "";
            worksheet.Cells[row, 10].Value = audit?.Status;
            worksheet.Cells[row, 11].Value = audit?.SentTime.ToString("g") ?? "";
            worksheet.Cells[row, 12].Value = audit?.Messages != null
                ? string.Join(",", audit.Messages.Where(m => m.Status == MessageStatus.Sent).Select(m => m.UserId))
                : "";
            worksheet.Cells[row, 13].Value = audit?.Messages?.Count(m => m.Status == MessageStatus.Sent) ?? 0;
            worksheet.Cells[row, 14].Value = audit?.CompletedAt?.ToString("g") ?? "";
            worksheet.Cells[row, 15].Value = audit?.CompletedBy == 0 ?  "" : audit?.CompletedBy.ToString();
            worksheet.Cells[row, 15].Style.Numberformat.Format = "@";

            row++;
        }

        return worksheet;
    }
    
    private static ExcelWorksheet AddLogsWorksheet(List<VenueAuditLog> roundLogs, IEnumerable<VenueAuditRecord> audits, ExcelPackage package)
    {
        List<(DateTime Date, string VenueId, string Message)> logs =
            roundLogs.Select(l => (l.Date, (string)null, l.Message)).ToList();
        foreach (var audit in audits)
        foreach (var log in audit.Logs)
            logs.Add((log.Date, audit.VenueId, log.Message));

        var worksheet = package.Workbook.Worksheets.Add("Logs");
        worksheet.View.FreezePanes(2, 1);
        worksheet.Rows.Style.Font.Size = 12;
        worksheet.Row(1).Style.Font.Bold = true;
        worksheet.Row(1).Style.Fill.PatternType = ExcelFillStyle.Solid;
        worksheet.Row(1).Style.Fill.BackgroundColor.SetColor(0, 64, 64, 64);
        worksheet.Row(1).Style.Font.Color.SetColor(Color.White);
        worksheet.Column(1).Width = 25;
        worksheet.Column(2).Width = 17;
        worksheet.Column(3).Width = 160;
        worksheet.Cells[1, 1].Value = "Date";
        worksheet.Cells[1, 2].Value = "Venue Id";
        worksheet.Cells[1, 3].Value = "Log Message";
        worksheet.Cells[1, 1, 100_000, 3].AutoFilter = true;
        
        var row = 2;
        foreach (var log in logs.OrderBy(l => l.Date))
        {
            worksheet.Cells[row, 1].Value = log.Date;
            worksheet.Cells[row, 2].Value = log.VenueId ?? "";
            worksheet.Cells[row, 3].Value = log.Message;

            row++;
        }

        return worksheet;
    }

    private static ExcelWorksheet AddOverviewWorksheet(MassAuditStatusSummary massAudit,
                                                       IEnumerable<Venue> venues,
                                                       ExcelPackage package)
    {
        var worksheet = package.Workbook.Worksheets.Add("Overview");
        worksheet.View.FreezePanes(1, 2);
        worksheet.Rows.Style.Font.Size = 12;
        worksheet.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
        worksheet.Column(1).Style.Font.Bold = true;
        worksheet.Column(1).Style.Fill.PatternType = ExcelFillStyle.Solid;
        worksheet.Column(1).Style.Fill.BackgroundColor.SetColor(0, 64, 64, 64);
        worksheet.Column(1).Style.Font.Color.SetColor(Color.White);
        worksheet.Column(1).Style.Font.Bold = true;
        worksheet.Column(1).Width = 40;
        worksheet.Column(2).Width = 25;
        worksheet.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        worksheet.Column(2).Style.Indent = 1;

        worksheet.Cells[2, 1].Style.Font.Bold = true;
        worksheet.Cells[2, 1].Value = "Mass Audit Id";
        worksheet.Cells[2, 2].Value = massAudit.id;
        worksheet.Cells[3, 1].Value = "Status";
        worksheet.Cells[3, 2].Value = massAudit.Status;
        worksheet.Cells[4, 1].Value = "Requested by";
        worksheet.Cells[4, 2].Value = massAudit.RequestedBy.ToString();
        worksheet.Cells[5, 1].Value = "Started At";
        worksheet.Cells[5, 2].Value = massAudit.StartedAt?.ToString("g") ?? "";
        worksheet.Cells[6, 1].Value = "Paused At";
        worksheet.Cells[6, 2].Value = massAudit.PausedAt?.ToString("g") ?? "";
        
        worksheet.Cells[7, 1].Value = "Completed At";
        worksheet.Cells[7, 2].Value = massAudit.CompletedAt?.ToString("g") ?? ""; 

        worksheet.Cells[9, 1].Value = "Total Venues";
        worksheet.Cells[9, 2].Value = venues.Count();
        worksheet.Cells[10, 1].Value = "Audits Sent";
        worksheet.Cells[10, 2].Value = massAudit.AuditsSent;
        worksheet.Cells[11, 1].Value = "Audits Answered";
        worksheet.Cells[11, 2].Value = massAudit.AuditsAnswered;

        worksheet.Cells[13, 1].Value = "Venues Confirmed";
        worksheet.Cells[13, 2].Value = massAudit.VenuesConfirmed;
        worksheet.Cells[14, 1].Value = "Venues Edited";
        worksheet.Cells[14, 2].Value = massAudit.VenuesEdited;
        worksheet.Cells[15, 1].Value = "Venues Closed";
        worksheet.Cells[15, 2].Value = massAudit.VenuesClosed;
        worksheet.Cells[16, 1].Value = "Venues Deleted";
        worksheet.Cells[16, 2].Value = massAudit.VenuesDeleted;

        worksheet.Cells[18, 1].Value = "Audits Awaiting Answer";
        worksheet.Cells[18, 2].Value = massAudit.AuditsAwaitingAnswer;
        worksheet.Cells[19, 1].Value = "Audits Skipped";
        worksheet.Cells[19, 2].Value = massAudit.AuditsSkipped;
        worksheet.Cells[20, 1].Value = "Audits Failed";
        worksheet.Cells[20, 2].Value = massAudit.AuditsFailed;
        worksheet.Cells[21, 1].Value = "Audits In Progress";
        worksheet.Cells[21, 2].Value = massAudit.AuditsInProgress;
        return worksheet;
    }
}