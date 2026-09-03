namespace FinTrackCore.Application.Common.Configuration;

public class MessageSettings
{
    public string ValidationFailed { get; set; } = "One or more validation errors occurred.";
    public string NotFound { get; set; } = "The requested resource was not found.";
    public string Unauthorized { get; set; } = "You are not authorized to perform this action.";
    public string Forbidden { get; set; } = "Access to this resource is forbidden.";
    public string Conflict { get; set; } = "This action cannot be completed because it conflicts with existing data.";
    public string DomainError { get; set; } = "A business rule was violated.";
    public string InternalError { get; set; } = "An unexpected error occurred. Please try again later.";
    public string RequestCanceled { get; set; } = "The request was canceled.";

    public string InsertSuccess { get; set; } = "Data saved successfully.";
    public string UpdateSuccess { get; set; } = "Data updated successfully.";
    public string DeleteSuccess { get; set; } = "Data deleted successfully.";

    public string LoginSuccess { get; set; } = "Login successful.";
    public string LoginFailed { get; set; } = "Invalid email or password.";
    public string DuplicateEmail { get; set; } = "An account with this email already exists. Please sign in or use a different email.";
    public string LogoutSuccess { get; set; } = "Logged out successfully.";
    public string TokenRefreshed { get; set; } = "Token refreshed successfully.";
    public string InvalidRefreshToken { get; set; } = "Invalid or expired refresh token.";
    public string GoogleAuthSuccess { get; set; } = "Signed in with Google successfully.";
    public string GoogleAuthFailed { get; set; } = "Google authentication failed.";
    public string GoogleEmailLinkedToOtherAccount { get; set; } =
        "This email is already linked to a different Google account. Sign in with your existing account instead.";

    public string SystemAccountDeleteForbidden { get; set; } = "System default accounts cannot be deleted.";
    public string SystemAccountUpdateForbidden { get; set; } = "System default accounts cannot be edited.";
    public string CoaInUseDeleteForbidden { get; set; } = "This account is used in transactions and cannot be deleted.";
    public string CoaHasChildrenDeleteForbidden { get; set; } =
        "This account has child accounts and cannot be deleted. Delete or move the child accounts first.";
    public string InvalidAccountType { get; set; } = "Invalid account type.";
    public string InvalidParentAccount { get; set; } = "Parent account is invalid for this user.";
    public string DuplicateAccountHeadName { get; set; } = "An account head with this name already exists for the selected account type.";

    public string InvalidFinancialYear { get; set; } = "Financial year is invalid for this user.";
    public string FinancialYearClosed { get; set; } = "Cannot post to a closed financial year.";
    public string InvalidTransactionType { get; set; } = "Invalid transaction type.";
    public string InvalidCoa { get; set; } = "One or more accounts are invalid for this user.";
    public string InvalidCoaForTransactionType { get; set; } = "Accounts do not match the transaction type.";
    public string InvalidAmount { get; set; } = "Amount must be greater than zero.";
    public string TransactionDateOutOfRange { get; set; } = "Transaction date is outside the financial year.";
    public string SameDebitCreditCoa { get; set; } = "Debit and credit accounts must be different.";
}
