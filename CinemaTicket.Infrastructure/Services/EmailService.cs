using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace CinemaTicket.Infrastructure.Services
{
    public class EmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPass;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(IConfiguration configuration)
        {
            _smtpHost = configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
            _smtpUser = configuration["Email:SmtpUser"] ?? "";
            _smtpPass = configuration["Email:SmtpPass"] ?? "";
            _fromEmail = configuration["Email:FromEmail"] ?? "";
            _fromName = configuration["Email:FromName"] ?? "CinemaTicket";
        }

        public async Task SendBookingConfirmationAsync(
            string toEmail, string toName,
            string bookingCode, string movieTitle,
            string cinemaName, string hallName,
            DateTime startTime, List<string> seatNames,
            decimal totalAmount)
        {
            var subject = $"[CinemaTicket] Xác nhận đặt vé - {bookingCode}";
            var body = $@"
                <div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto'>
                    <div style='background:#1a1a2e;padding:20px;text-align:center'>
                        <h1 style='color:#e94560;margin:0'>🎬 CinemaTicket</h1>
                    </div>
                    <div style='padding:30px;background:#f9f9f9'>
                        <h2>Xin chào {toName}!</h2>
                        <p>Đặt vé của bạn đã được xác nhận thành công.</p>
                        <div style='background:white;border-radius:8px;padding:20px;margin:20px 0'>
                            <h3 style='color:#e94560;margin-top:0'>Chi tiết vé</h3>
                            <table style='width:100%;border-collapse:collapse'>
                                <tr><td style='padding:8px 0;color:#666'>Mã đặt vé</td>
                                    <td style='padding:8px 0;font-weight:bold;color:#e94560'>{bookingCode}</td></tr>
                                <tr><td style='padding:8px 0;color:#666'>Phim</td>
                                    <td style='padding:8px 0;font-weight:bold'>{movieTitle}</td></tr>
                                <tr><td style='padding:8px 0;color:#666'>Rạp</td>
                                    <td style='padding:8px 0'>{cinemaName}</td></tr>
                                <tr><td style='padding:8px 0;color:#666'>Phòng</td>
                                    <td style='padding:8px 0'>{hallName}</td></tr>
                                <tr><td style='padding:8px 0;color:#666'>Suất chiếu</td>
                                    <td style='padding:8px 0'>{startTime:HH:mm dd/MM/yyyy}</td></tr>
                                <tr><td style='padding:8px 0;color:#666'>Ghế</td>
                                    <td style='padding:8px 0'>{string.Join(", ", seatNames)}</td></tr>
                                <tr style='border-top:2px solid #eee'>
                                    <td style='padding:12px 0;font-weight:bold'>Tổng tiền</td>
                                    <td style='padding:12px 0;font-weight:bold;color:#e94560;font-size:18px'>
                                        {totalAmount:N0} đ</td></tr>
                            </table>
                        </div>
                        <p style='color:#666;font-size:14px'>
                            Vui lòng xuất trình mã đặt vé khi vào rạp.<br/>
                            Cảm ơn bạn đã sử dụng CinemaTicket!
                        </p>
                    </div>
                    <div style='background:#1a1a2e;padding:15px;text-align:center'>
                        <p style='color:#666;margin:0;font-size:12px'>© 2026 CinemaTicket</p>
                    </div>
                </div>";

            await SendEmailAsync(toEmail, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_smtpHost, _smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_smtpUser, _smtpPass)
                };

                var message = new MailMessage
                {
                    From = new MailAddress(_fromEmail, _fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(toEmail);

                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email error: {ex.Message}");
            }
        }
    }
}