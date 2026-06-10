namespace SCM3.Web.Components.Shared;

internal enum RequestTypeKind
{
    System,
    Segment,
    CsciGaAsi,
    CsciSupplier,
    EeRequest,
    TeRequest,
    ThirdPartySoftware
}

internal sealed record RequestTypePresentation(
    RequestTypeKind Kind,
    string Name,
    string Abbreviation,
    string CssKey);

internal enum RequestStatusKind
{
    Draft,
    Pending,
    InReview,
    Released,
    Rejected,
    Terminated
}

internal sealed record RequestStatusPresentation(
    RequestStatusKind Kind,
    string Name,
    string CssKey);

// Shared lookup for rendering requests consistently across the master list and detail
// panel — keeps the status-color mapping (root CLAUDE.md §7-8) in one place.
internal static class RequestDisplay
{
    public static readonly IReadOnlyList<RequestTypePresentation> RequestTypes =
    [
        new(RequestTypeKind.System, "System", "Sy", "system"),
        new(RequestTypeKind.Segment, "Segment", "Sg", "segment"),
        new(RequestTypeKind.CsciGaAsi, "CSCI GA-ASI", "CsGA", "csci-gaasi"),
        new(RequestTypeKind.CsciSupplier, "CSCI Supplier", "CsSup", "csci-supplier"),
        new(RequestTypeKind.EeRequest, "EE Request", "EE", "ee"),
        new(RequestTypeKind.TeRequest, "TE Request", "TE", "te"),
        new(RequestTypeKind.ThirdPartySoftware, "Third Party SW", "TPS", "third-party")
    ];

    public static RequestTypePresentation RequestType(string? name)
        => RequestTypes.FirstOrDefault(type =>
               string.Equals(type.Name, name, StringComparison.OrdinalIgnoreCase))
           ?? new(RequestTypeKind.System, name ?? "Unknown", "?", "unknown");

    public static string RequestTypeClass(string? name)
        => $"request-type-{RequestType(name).CssKey}";

    public static readonly IReadOnlyList<RequestStatusPresentation> RequestStatuses =
    [
        new(RequestStatusKind.InReview, "In Review", "in-review"),
        new(RequestStatusKind.Pending, "Pending", "pending"),
        new(RequestStatusKind.Released, "Released", "released"),
        new(RequestStatusKind.Draft, "Draft", "draft"),
        new(RequestStatusKind.Rejected, "Rejected", "rejected"),
        new(RequestStatusKind.Terminated, "Terminated", "terminated")
    ];

    public static RequestStatusPresentation RequestStatus(string? name)
        => RequestStatuses.FirstOrDefault(status =>
               string.Equals(status.Name, name, StringComparison.OrdinalIgnoreCase))
           ?? new(RequestStatusKind.Draft, name ?? "Unknown", "unknown");

    public static string RequestStatusClass(string? name)
        => $"request-status-chip request-status-{RequestStatus(name).CssKey}";
}
