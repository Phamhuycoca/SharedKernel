using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Common.Constants;

/// <summary>
/// Tập hợp các message chuẩn cho API response (Tiếng Việt).
/// </summary>
public static class ResponseMessage
{
    // -------------------------------------------------------
    // CRUD Success
    // -------------------------------------------------------
    public const string CreateSuccess = "Created successfully.";
    public const string UpdateSuccess = "Updated successfully.";
    public const string DeleteSuccess = "Deleted successfully.";
    public const string RestoreSuccess = "Restored successfully.";
    public const string GetSuccess = "Retrieved successfully.";
    public const string ListSuccess = "Retrieved list successfully.";

    // -------------------------------------------------------
    // CRUD Error
    // -------------------------------------------------------
    public const string CreateFailed = "Failed to create.";
    public const string UpdateFailed = "Failed to update.";
    public const string DeleteFailed = "Failed to delete.";
    public const string GetFailed = "Failed to retrieve data.";

    // -------------------------------------------------------
    // Not Found / Conflict / Validation
    // -------------------------------------------------------
    public const string NotFound = "Data not found.";
    public const string AlreadyExists = "Data already exists.";
    public const string Conflict = "The data conflicts with an existing record.";
    public const string InvalidField = "Invalid data.";
    public const string RequiredField = "Please provide all required information.";
    public const string OutOfRange = "The value is out of the allowed range.";
    public const string MaxLength = "The data exceeds the maximum allowed length.";

    // -------------------------------------------------------
    // Auth
    // -------------------------------------------------------
    public const string Unauthorized = "You are not authorized to perform this action.";
    public const string Forbidden = "You do not have permission to access this resource.";
    public const string TokenExpired = "Your session has expired. Please log in again.";
    public const string TokenInvalid = "Invalid token.";
    public const string LoginSuccess = "Login successful.";
    public const string LoginFailed = "Incorrect email or password.";
    public const string LogoutSuccess = "Logout successful.";
    public const string PasswordChanged = "Password changed successfully.";
    public const string PasswordMismatch = "The current password is incorrect.";
    public const string AccountLocked = "Your account has been locked. Please contact support.";
    public const string AccountInactive = "Your account has not been activated.";

    // -------------------------------------------------------
    // General
    // -------------------------------------------------------
    public const string Success = "Operation completed successfully.";
    public const string BadRequest = "Invalid request.";
    public const string InternalError = "An unexpected error occurred. Please try again later.";
    public const string ServiceUnavailable = "The service is temporarily unavailable.";
    public const string NoContent = "No data available.";
    public const string DuplicateRequest = "Duplicate request detected. Please wait before trying again.";
    public const string Validator = "Validation failed";
}
