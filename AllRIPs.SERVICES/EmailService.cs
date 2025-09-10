using System.Net.Mail;
using System.Net.Mime;
using Microsoft.Extensions.Configuration;
using AllRIPs.DTOS;

namespace AllRIPs.SERVICES
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<bool> SendEmail(EmailDTO datosemail)
        {
            try
            {
                var correo = new MailMessage();
                correo.Subject = System.Text.Encoding.UTF8.GetString(System.Text.Encoding.Default.GetBytes(datosemail.Asunto!));
                correo.Body = datosemail.cuerpo;
                correo.Priority = MailPriority.Normal;
                correo.IsBodyHtml = true;
                if (datosemail.Destinatarios != null)
                    foreach (var item in datosemail.Destinatarios)
                    {
                        correo.To.Add(item);
                    }
                if (datosemail.CCDestinatarios != null)
                    foreach (var item in datosemail.CCDestinatarios)
                    {
                        correo.CC.Add(item);
                    }
                correo.From = new MailAddress(_config.GetValue<string>("Email:UserEmail"));

                if (!string.IsNullOrEmpty(datosemail.Adjunto))
                {
                    byte[] pdfBytes = Convert.FromBase64String(datosemail.Adjunto);
                    Attachment attachment = new Attachment(new MemoryStream(pdfBytes), datosemail.nombreadjunto, "application/zip");
                    attachment.ContentDisposition.Inline = false;
                    attachment.ContentDisposition.DispositionType = DispositionTypeNames.Attachment;
                    correo.Attachments.Add(attachment);
                }

                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Host = _config.GetValue<string>("Email:HostEmail");
                    smtpClient.Port = Convert.ToInt32(_config.GetValue<string>("Email:PuertoEmail"));
                    smtpClient.Credentials = new System.Net.NetworkCredential(_config.GetValue<string>("Email:UserEmail"), _config.GetValue<string>("Email:PassEmail"));
                    smtpClient.EnableSsl = true;
                    await smtpClient.SendMailAsync(correo);
                }
                return true;
            }
            catch (Exception)
            {
                try
                {
                    var correo = new MailMessage();
                    correo.Subject = datosemail.Asunto;
                    correo.Body = datosemail.cuerpo;
                    correo.Priority = MailPriority.Normal;
                    correo.IsBodyHtml = true;
                    foreach (var item in datosemail.Destinatarios)
                    {
                        correo.To.Add(item);
                    }
                    foreach (var item in datosemail.CCDestinatarios)
                    {
                        correo.CC.Add(item);
                    }
                    correo.From = new MailAddress(_config.GetValue<string>("Email:UserEmail"));

                    if (!string.IsNullOrEmpty(datosemail.Adjunto))
                    {
                        byte[] pdfBytes = Convert.FromBase64String(datosemail.Adjunto);
                        Attachment attachment = new Attachment(new MemoryStream(pdfBytes), datosemail.nombreadjunto, "application/pdf");
                        attachment.ContentDisposition.Inline = false;
                        attachment.ContentDisposition.DispositionType = DispositionTypeNames.Attachment;
                        correo.Attachments.Add(attachment);
                    }

                    using (var smtpClient = new SmtpClient())
                    {
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.Host = _config.GetValue<string>("Email:HostEmail");
                        smtpClient.Port = Convert.ToInt32(_config.GetValue<string>("Email:PuertoEmail"));
                        smtpClient.Credentials = new System.Net.NetworkCredential(_config.GetValue<string>("Email:UserEmail"), _config.GetValue<string>("Email:PassEmail"));
                        smtpClient.EnableSsl = true;
                        await smtpClient.SendMailAsync(correo);

                    }
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}
