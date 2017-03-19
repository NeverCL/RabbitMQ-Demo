using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Receive
{
    class Program
    {
        private static string hostName = "test.youlijinfu.com";
        static void Main(string[] args)
        {
            ReceiveMsg();
        }

        private static void ReceiveMsg()
        {
            var factory = new ConnectionFactory
            {
                HostName = hostName,                // rabbit server
                UserName = "admin",
                Password = "admin",
                Port = 5672,                        // Broker端口
                VirtualHost = "/"                   // 虚拟Host,需提前配置
            };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var consumer = new EventingBasicConsumer(channel);  // 创建Consumer
                    consumer.Received += (model, ea) =>         // 通过回调函数异步推送我们的消息
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        Thread.Sleep(1000);
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false); // 消息响应
                        Console.WriteLine(" [x] Received {0}", message);
                    };

                    channel.BasicQos(0, 1, false);  // 设置perfetchCount=1 。这样就告诉RabbitMQ 不要在同一时间给一个工作者发送多于1个的消息
                    channel.BasicConsume(queue: "hello",
                                         noAck: false,      // 需要消息响应（Acknowledgments）机制
                                         consumer: consumer);

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
        }
    }
}
