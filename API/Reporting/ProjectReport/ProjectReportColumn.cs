namespace API.Reporting.ProjectReport;

/// <summary>One printed column of a project-report table: header text, bound field name (on the
/// matching Pdf*Row type), a relative width weight, and an optional Telerik .NET format string.</summary>
public sealed record ProjectReportColumn(string Header, string Field, double Weight, string? Format = null);
