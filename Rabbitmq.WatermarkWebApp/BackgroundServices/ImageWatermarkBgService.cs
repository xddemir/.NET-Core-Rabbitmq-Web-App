using System.Drawing;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Rabbitmq.WatermarkWebApp.Services;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Rabbitmq.WatermarkWebApp.BackgroundServices;

public class ImageWatermarkBgService : BackgroundService
{
    private readonly RabbitMQClientService _clientService;
    private readonly ILogger<ImageWatermarkBgService> _logger;
    private IModel _channel;
    
    
    public ImageWatermarkBgService(RabbitMQClientService clientService, ILogger<ImageWatermarkBgService> logger)
    {
        _clientService = clientService;
        _logger = logger;
    }
    
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _channel = _clientService.Connect();
        _channel.BasicQos(0, 1, false);

        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        _channel.BasicConsume(RabbitMQClientService.QueueName, false, consumer);
        consumer.Received += Consumer_Received;

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }
    
    private Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
    {
        try
        {
            var imageCreatedEvent =
                JsonSerializer.Deserialize<ProductImageCreatedEvent>(
                    Encoding.UTF8.GetString(@event.Body.ToArray()));

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images", imageCreatedEvent.ImageName);

            var name = "Dogukan Demir";
        
            using var img = Image.FromFile(path);

            using var graphic = Graphics.FromImage(img);

            var font = new Font(FontFamily.GenericMonospace, 32, FontStyle.Bold, GraphicsUnit.Pixel);

            var textSize = graphic.MeasureString(name, font);

            var color = Color.Red;
            var brush = new SolidBrush(color);

            var position = new Point(img.Width - ((int)textSize.Width + 30), 
                img.Height - ((int)textSize.Height + 30));
        
        
            graphic.DrawString(name, font, brush, position);
        
            img.Save("wwwroot/images/watermarks" + imageCreatedEvent.ImageName);
        
            img.Dispose();
            graphic.Dispose();
        
            _channel.BasicAck(@event.DeliveryTag, false);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return Task.CompletedTask;
    }
}