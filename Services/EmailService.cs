using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SrChauferoMVC_AzureIA.Services
{
    public class EmailService
    {

        private readonly IConfiguration _configuration;


        public EmailService(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }



        public void EnviarCodigo(
            string correo,
            string codigo)
        {

            var email =
                new MimeMessage();



            email.From.Add(
                new MailboxAddress(
                    "Sr. Chaufero",
                    _configuration["Email:Usuario"]
                )
            );


            email.To.Add(
                new MailboxAddress(
                    "",
                    correo
                )
            );



            email.Subject =
                "Código de verificación Sr. Chaufero";



            email.Body =
                new TextPart("plain")
                {
                    Text =
                    $"Tu código de verificación es: {codigo}"
                };



            using var smtp =
                new SmtpClient();



            try
            {
                smtp.Connect(
                    "smtp.gmail.com",
                    587,
                    SecureSocketOptions.StartTls);

                smtp.Authenticate(
                    _configuration["Email:Usuario"],
                    _configuration["Email:Password"]);

                smtp.Send(email);

                smtp.Disconnect(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }

        }

    }
}