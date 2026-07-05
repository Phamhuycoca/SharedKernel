using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SharedKernel.Utils;

public class EmailHelper
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPassword;
    private readonly bool _enableSsl;

    public EmailHelper(string smtpHost, int smtpPort, string smtpUser, string smtpPassword, bool enableSsl)
    {
        _smtpHost = smtpHost;
        _smtpPort = smtpPort;
        _smtpUser = smtpUser;
        _smtpPassword = smtpPassword;
        _enableSsl = enableSsl;
    }

    /// <summary>
    /// Gửi email đơn giản, không đính kèm.
    /// </summary>
    public async Task SendAsync(
        string fromEmail,
        string fromDisplayName,
        string toEmail,
        string subject,
        string body,
        bool isBodyHtml = true)
    {
        await SendInternalAsync(fromEmail, fromDisplayName, toEmail, subject, body, isBodyHtml, attachments: null);
    }

    /// <summary>
    /// Gửi email kèm 1 file đính kèm (ví dụ PDF).
    /// </summary>
    public async Task SendWithAttachmentAsync(
        string fromEmail,
        string fromDisplayName,
        string toEmail,
        string subject,
        string body,
        byte[] attachmentBytes,
        string attachmentFileName,
        string attachmentContentType = "application/pdf",
        bool isBodyHtml = true)
    {
        using var stream = new MemoryStream(attachmentBytes);
        // QUAN TRỌNG: Name phải được set tường minh, tránh bug "noname"
        var attachment = new Attachment(stream, attachmentFileName, attachmentContentType)
        {
            Name = attachmentFileName
        };

        await SendInternalAsync(fromEmail, fromDisplayName, toEmail, subject, body, isBodyHtml, new[] { attachment });
    }

    /// <summary>
    /// Gửi email kèm nhiều file đính kèm.
    /// </summary>
    public async Task SendWithAttachmentsAsync(
        string fromEmail,
        string fromDisplayName,
        string toEmail,
        string subject,
        string body,
        (byte[] Bytes, string FileName, string ContentType)[] attachmentsData,
        bool isBodyHtml = true)
    {
        var attachments = new Attachment[attachmentsData.Length];
        var streams = new MemoryStream[attachmentsData.Length];

        try
        {
            for (int i = 0; i < attachmentsData.Length; i++)
            {
                streams[i] = new MemoryStream(attachmentsData[i].Bytes);
                attachments[i] = new Attachment(
                    streams[i],
                    attachmentsData[i].FileName,
                    attachmentsData[i].ContentType)
                {
                    Name = attachmentsData[i].FileName
                };
            }

            await SendInternalAsync(fromEmail, fromDisplayName, toEmail, subject, body, isBodyHtml, attachments);
        }
        finally
        {
            foreach (var s in streams)
                s?.Dispose();
        }
    }

    private async Task SendInternalAsync(
        string fromEmail,
        string fromDisplayName,
        string toEmail,
        string subject,
        string body,
        bool isBodyHtml,
        Attachment[]? attachments)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromDisplayName),
            Subject = subject,
            Body = body,
            IsBodyHtml = isBodyHtml
        };

        message.To.Add(toEmail);

        if (attachments != null)
        {
            foreach (var attachment in attachments)
                message.Attachments.Add(attachment);
        }

        using var client = new SmtpClient(_smtpHost, _smtpPort)
        {
            Credentials = new NetworkCredential(_smtpUser, _smtpPassword),
            EnableSsl = _enableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        try
        {
            await client.SendMailAsync(message);
        }
        finally
        {
            if (attachments != null)
            {
                foreach (var attachment in attachments)
                    attachment.Dispose();
            }
        }
    }
}