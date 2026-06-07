using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using ClosedXML.Excel;
using cmsOrg.Application.Common;
using cmsOrg.Application.DTO.Export;
using cmsOrg.Application.Interfaces;
using cmsOrg.Infrastructure.Persistence;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace cmsOrg.Infrastructure.Services;

public class ExportImportService(AppDbContext db, IHttpContextAccessor http, IAccessControlService accessControl) : IExportImportService
{
    private Guid CurrentUserId
    {
        get
        {
            var value = http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? http.HttpContext?.User.FindFirst("sub")?.Value;
            return Guid.TryParse(value, out var id) ? id : throw AppException.Unauthorized("User is not authenticated.");
        }
    }

    public async Task<(byte[] Data, string ContentType, string FileName)> ExportOrganisations(string format)
    {
        var userId = CurrentUserId;

        var items = await db.Organisations
            .Where(o => db.UserOrganisationPermissions.Any(uop => uop.OrganisationId == o.Id && uop.UserId == userId))
            .Select(o => new OrganisationExportDto
            {
                Id = o.Id,
                Name = o.Name,
                CreatedAt = o.CreatedAt,
                MemberCount = o.UserPermissions.Count
            })
            .ToListAsync();

        return Serialize(items, format, "organisations");
    }

    public async Task<(byte[] Data, string ContentType, string FileName)> ExportMembers(Guid organisationId, string format)
    {
        var userId = CurrentUserId;
        await accessControl.CheckAccess(userId, organisationId, "Admin");

        var items = await db.UserOrganisationPermissions
            .Where(uop => uop.OrganisationId == organisationId)
            .Include(uop => uop.Permission)
            .Select(uop => new MemberExportDto
            {
                Id = uop.Id,
                UserId = uop.UserId,
                OrganisationId = uop.OrganisationId,
                Role = uop.Permission.Name
            })
            .ToListAsync();

        return Serialize(items, format, "members");
    }

    private static (byte[] Data, string ContentType, string FileName) Serialize<T>(List<T> items, string format, string name)
    {
        return format.ToLowerInvariant() switch
        {
            "csv" => (ToCsv(items), "text/csv", $"{name}.csv"),
            "excel" => (ToExcel(items, name), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{name}.xlsx"),
            _ => (ToJson(items), "application/json", $"{name}.json")
        };
    }

    private static byte[] ToJson<T>(List<T> items)
        => JsonSerializer.SerializeToUtf8Bytes(items, new JsonSerializerOptions { WriteIndented = true });

    private static byte[] ToCsv<T>(List<T> items)
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(items);
        writer.Flush();
        return ms.ToArray();
    }

    private static byte[] ToExcel<T>(List<T> items, string sheetName)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add(sheetName);
        var props = typeof(T).GetProperties();

        for (int c = 0; c < props.Length; c++)
            ws.Cell(1, c + 1).Value = props[c].Name;

        for (int r = 0; r < items.Count; r++)
            for (int c = 0; c < props.Length; c++)
                ws.Cell(r + 2, c + 1).Value = props[c].GetValue(items[r])?.ToString() ?? string.Empty;

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
