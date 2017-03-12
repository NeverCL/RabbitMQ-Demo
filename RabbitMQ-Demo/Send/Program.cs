using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace Send
{
    /// <summary>
    /// 生产者
    /// </summary>
    class Program
    {
        private static string hostName = "test.youlijinfu.com";

        static void Main(string[] args)
        {
            HelloWorld();
        }

        /// <summary>
        /// Routing
        /// </summary>
        private static void Routing()
        {
            var factory = new ConnectionFactory() {HostName = "localhost"};
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "direct_logs", type: "direct"); // 此处为direct

                while (true)
                {
                    var severity = Console.ReadLine();
                    var message = "Hello World:" + severity;
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(
                        exchange: "direct_logs",
                        routingKey: severity, // 发送到指定的路由键
                        basicProperties: null,
                        body: body);
                    Console.WriteLine(" [x] Sent '{0}':'{1}'", severity, message);
                    Console.ReadLine();
                }
            }
        }

        /// <summary>
        /// Publish发布
        /// </summary>
        private static void PublishSubscribe()
        {
            Console.WriteLine(" Press [Ctrl + C] to exit.");
            var factory = new ConnectionFactory() {HostName = "localhost"};
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout); // 创建exchange,type内置不可任意

                while (true)
                {
                    var message = "hello world";
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: "logs", // 根据exchange发送
                        routingKey: "",
                        basicProperties: null,
                        body: body);
                    Console.WriteLine(" [x] Sent {0}", message);
                    Console.ReadLine();
                }
            }
        }

        /// <summary>
        /// 持久化
        /// </summary>
        private static void WorkQueue()
        {
            var factory = new ConnectionFactory() {HostName = "localhost"};
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "workQueue",
                        durable: true, // 持久化,不能直接更改已经存在的队列
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    var prop = channel.CreateBasicProperties();
                    prop.Persistent = true; // 持久化

                    while (true)
                    {
                        string message = "Hello World!" + DateTime.Now;
                        var body = Encoding.UTF8.GetBytes(message);

                        channel.BasicPublish(exchange: "",
                            routingKey: "workQueue",
                            basicProperties: null,
                            body: body);
                        Console.WriteLine(" [x] Sent {0}", message);
                        Console.ReadLine();
                    }
                }
            }
        }

        private static void HelloWorld()
        {
            var factory = new ConnectionFactory
            {
                HostName = hostName,
                UserName = "admin",
                Password = "admin",
                Port = 5672,
                VirtualHost = "/"            // 虚拟Host,需提前配置
            };

            using (var connection = factory.CreateConnection()) // 创建与服务器的连接
            {
                using (var channel = connection.CreateModel()) // 创建1个通道,大部分API在该channel中
                {
                    // 定义1个队列
                    channel.QueueDeclare(queue: "hello",    // 队列名称
                        durable: false,                     // 是否持久化
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    while (true)
                    {
                        string message = "Hello World!" + DateTime.Now;
                        var body = Encoding.UTF8.GetBytes(message);

                        channel.BasicPublish(exchange: "", // 发送消息到队列中
                            routingKey: "hello",
                            basicProperties: properties,
                            body: body); // 发送的是bytes 可以任意编码

                        Console.WriteLine(" [x] Sent {0}", message);
                        Thread.Sleep(1000);
                    }
                }
            }
        }
    }
}