namespace cmsOrg.Application.Interfaces;

public interface IExportImportService
{
    Task<(byte[] Data, string ContentType, string FileName)> ExportOrganisations(string format);
    Task<(byte[] Data, string ContentType, string FileName)> ExportMembers(Guid organisationId, string format);
}
