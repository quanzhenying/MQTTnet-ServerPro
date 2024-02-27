
using MQTTnet;
using MQTTnet.Diagnostics;
using MQTTnet.Implementations;
using MQTTnet.Server;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

var sc = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cert", "server-cert.pem");
var sk = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cert", "server-key.pem");
var serverCert = X509Certificate2.CreateFromPemFile(sc, sk);

//1883
var options = new MqttServerOptionsBuilder()
               .WithDefaultEndpoint()
               .WithDefaultEndpointPort(1883)
               .WithDefaultEndpointBoundIPAddress(IPAddress.Any)

               .WithEncryptedEndpoint()
               .WithEncryptedEndpointPort(16903)
               .WithEncryptedEndpointBoundIPAddress(IPAddress.Any)

               .WithEncryptionCertificate(serverCert.Export(X509ContentType.Pfx))
               .WithEncryptionSslProtocol(SslProtocols.Tls12)

               .WithTcpKeepAliveTime(60)
               .WithTcpKeepAliveRetryCount(3)
               .WithTcpKeepAliveInterval(30)

               .WithPersistentSessions() //持续会话 支持QoS 2实现掉线缓冲（并没有持久化）
               .WithMaxPendingMessagesPerClient(10000000) //每终端主题最大缓冲1000万
               .WithConnectionBacklog(10000000) //单机最大连接数1000万
               .Build();

var factory = new MqttFactory();
using var server = factory.CreateMqttServer(options, new MyLogger());

server.ValidatingConnectionAsync += (async arg =>
{
    var sessions = server.ServerSessionItems;
    if (arg.UserName == "IMA_OAUTH_ACCESS_TOKEN")
    {
        var token = arg.Password;
        await Task.Delay(1000);
    }
});
server.ClientAcknowledgedPublishPacketAsync += (async arg =>
{
    var sessions = await server.GetSessionsAsync();

});

await server.StartAsync();

Console.ReadLine();
Console.ReadLine();
Console.ReadLine();



public class MyLogger : IMqttNetLogger
{
    public bool IsEnabled => true;

    public void Publish(MqttNetLogLevel logLevel, string source, string message, object[] parameters, Exception exception)
    {
        Console.WriteLine(message, parameters);
    }
}