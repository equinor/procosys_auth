using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Equinor.ProCoSys.Common.Email;


public class EmailValidator
{
    public static bool IsValid(string email)
    {
        var valid = true;

        try
        {
            var emailAddress = new MailAddress(email);
        }
        catch
        {
            valid = false;
        }

        return valid;
    }

    public static void ValidateEmail(string email)
    {
        if (!(IsValid(email)))
        {
            throw new Exception($"Not able to send email because of invalid email-address: {email}");
        }
    }

    public static void ValidateEmails(List<string> emails)
    {
        foreach (var email in emails)
        {
            EmailValidator.ValidateEmail(email);
        }
    }
}
