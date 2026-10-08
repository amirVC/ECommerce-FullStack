namespace ECommerceAPI.Options;

public class GmailSmtpOptions
{
    public string Host { get; set; } = "smtp.gmail.com";

    public int Port { get; set; } = 587;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FromEmail { get; set; } = string.Empty;

    public string FromName { get; set; } = "ECommerce";

    public bool UseStartTls { get; set; } = true;
}