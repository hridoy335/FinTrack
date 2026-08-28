namespace FinTrackCore.Application.Common.Configuration;

public class MessageSettings
{
    public string ValidationFailed { get; set; } = "One or more validation errors occurred.";
    public string NotFound { get; set; } = "The requested resource was not found.";
    public string Unauthorized { get; set; } = "You are not authorized to perform this action.";
    public string Forbidden { get; set; } = "Access to this resource is forbidden.";
    public string Conflict { get; set; } = "The request conflicts with the current state.";
    public string DomainError { get; set; } = "A business rule was violated.";
    public string InternalError { get; set; } = "An unexpected error occurred. Please try again later.";
    public string RequestCanceled { get; set; } = "The request was canceled.";

    public string InsertSuccess { get; set; } = "Data saved successfully.";
    public string UpdateSuccess { get; set; } = "Data updated successfully.";
    public string DeleteSuccess { get; set; } = "Data deleted successfully.";

    public string LoginSuccess { get; set; } = "Login successful.";
    public string LoginFailed { get; set; } = "Invalid username or password.";
    public string LogoutSuccess { get; set; } = "Logged out successfully.";
    public string TokenRefreshed { get; set; } = "Token refreshed successfully.";
    public string InvalidRefreshToken { get; set; } = "Invalid or expired refresh token.";
    public string GoogleAuthSuccess { get; set; } = "Signed in with Google successfully.";
    public string GoogleAuthFailed { get; set; } = "Google authentication failed.";

    public string SystemAccountDeleteForbidden { get; set; } = "System default accounts cannot be deleted.";
    public string InvalidAccountType { get; set; } = "Invalid account type.";
    public string InvalidParentAccount { get; set; } = "Parent account is invalid for this user.";

    public string InvalidFinancialYear { get; set; } = "Financial year is invalid for this user.";
    public string FinancialYearClosed { get; set; } = "Cannot post to a closed financial year.";
    public string InvalidTransactionType { get; set; } = "Invalid transaction type.";
    public string InvalidCoa { get; set; } = "One or more accounts are invalid for this user.";
    public string InvalidCoaForTransactionType { get; set; } = "Accounts do not match the transaction type.";
    public string InvalidAmount { get; set; } = "Amount must be greater than zero.";
    public string TransactionDateOutOfRange { get; set; } = "Transaction date is outside the financial year.";
    public string SameDebitCreditCoa { get; set; } = "Debit and credit accounts must be different.";
}
