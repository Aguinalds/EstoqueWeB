using EstoqueWeb.Models;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using System;
using System.Threading.Tasks;

namespace EstoqueWeb.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string assunto, string mensagemTexto, string mensagemHtml);
    }

    public class EmailSender : IEmailSender
    {
        private readonly EmailModel _emailModel;

        public EmailSender(IOptions<EmailModel> emailModel)
        {
            _emailModel = emailModel.Value;
        }

        public async Task SendEmailAsync(string email, string assunto, string mensagemTexto, string mensagemHtml)
        {
            var mensagem = new MimeMessage();
            mensagem.From.Add(new MailboxAddress(_emailModel.NomeRemetente, _emailModel.EmailRemetente));
            mensagem.To.Add(MailboxAddress.Parse(email));

            mensagem.Subject = assunto;
            var builder = new BodyBuilder { TextBody = mensagemTexto, HtmlBody = mensagemHtml };
            mensagem.Body = builder.ToMessageBody();

            try
            {
                var smtpClient = new SmtpClient();
                smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await smtpClient.ConnectAsync(_emailModel.EnderecoServidorEmail).ConfigureAwait(false);
                await smtpClient.AuthenticateAsync(_emailModel.EmailRemetente, _emailModel.Senha).ConfigureAwait(false);
                await smtpClient.SendAsync(mensagem).ConfigureAwait(false);
                await smtpClient.DisconnectAsync(true).ConfigureAwait(false);
            }catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }
    }
}
