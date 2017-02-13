using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Send
{
    class Program
    {
        static void Main(string[] args)
        {
            HelloWorld();
        }

        private static void HelloWorld()
        {
            Console.WriteLine(" Press [Ctrl + C] to exit.");
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection()) // 创建与服务器的连接
            {
                using (var channel = connection.CreateModel()) // 创建1个通道,大部分API在该类中
                {
                    channel.QueueDeclare(queue: "hello",    // 定义1个队列
                                durable: false,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);

                    while (true)
                    {
                        string message = "Hello World!" + DateTime.Now;
                        var body = Encoding.UTF8.GetBytes(message);

                        channel.BasicPublish(exchange: "",      // 发送消息到队列中
                                             routingKey: "hello",
                                             basicProperties: null,
                                             body: body);       // 发送的是bytes 可以任意编码
                        Console.WriteLine(" [x] Sent {0}", message);
                        Console.ReadLine();
                    }

                }
            }

        }
    }
}
