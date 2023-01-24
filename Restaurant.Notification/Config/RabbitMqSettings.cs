namespace Restaurant.Notification;

internal sealed class RabbitMqSettings
{
    public string Host { get; set; } = null!;
    public string VirtualHost { get; set; } = null!;
    public string User { get; set; } = null!;
    public string Password { get; set; } = null!;
    public ushort Port { get; set; }
}