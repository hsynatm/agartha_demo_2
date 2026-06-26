using System.Net.Mail;

namespace UserManagement.Application.Validation;

public static class UserInputValidator
{
    public static bool TryValidateUsername(string username, out string? error)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            error = "Username is required.";
            return false;
        }

        if (username.Any(char.IsWhiteSpace))
        {
            error = "Username cannot contain spaces.";
            return false;
        }

        error = null;
        return true;
    }

    public static bool TryValidateEmail(string email, out string? error)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            error = "Email is required.";
            return false;
        }

        if (!MailAddress.TryCreate(email, out _))
        {
            error = "Email format is invalid.";
            return false;
        }

        error = null;
        return true;
    }
}
