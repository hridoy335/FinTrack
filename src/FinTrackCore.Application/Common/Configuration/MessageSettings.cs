namespace FinTrackCore.Application.Common.Configuration;

public class MessageSettings
{
    public const string SectionName = "Messages";

    public string ValidationFailed { get; set; } = "One or more validation errors occurred.";
    public string NotFound { get; set; } = "The requested resource was not found.";
    public string Unauthorized { get; set; } = "You are not authorized to perform this action.";
    public string Forbidden { get; set; } = "Access to this resource is forbidden.";
    public string Conflict { get; set; } = "The request conflicts with the current state.";
    public string DomainError { get; set; } = "A business rule was violated.";
    public string InternalError { get; set; } = "An unexpected error occurred. Please try again later.";

    public string InsertSuccess { get; set; } = "Data saved successfully.";
    public string UpdateSuccess { get; set; } = "Data updated successfully.";
    public string DeleteSuccess { get; set; } = "Data deleted successfully.";
}
