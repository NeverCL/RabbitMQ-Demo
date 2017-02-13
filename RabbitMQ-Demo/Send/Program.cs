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
            PublishSubscribe();
        }

        private static void PublishSubscribe()
        {
            Console.WriteLine(" Press [Ctrl + C] to exit.");
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "logs", type: "fanout");  // 创建exchange

                while (true)
                {
                    var message = "hello world";
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: "logs",      // 根据exchange发送
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
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "workQueue",
                                durable: true,              // 持久化,不能直接更改已经存在的队列
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);

                    var prop = channel.CreateBasicProperties();
                    prop.Persistent = true;                 // 持久化

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
