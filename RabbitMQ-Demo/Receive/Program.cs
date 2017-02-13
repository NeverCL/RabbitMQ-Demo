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
        static void Main(string[] args)
        {
           
        }

        /// <summary>
        /// 消息确认 消息分配
        /// </summary>
        private static void WorkQueue()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "workQueue",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine(" [x] Received {0}", message);
                        Thread.Sleep(3000);
                        Console.WriteLine(" [x] Done!");
                        channel?.BasicAck(ea.DeliveryTag, false);   // 发送消息确认,队列收到确认才移除消息
                    };
                    channel.BasicQos(0, 1, false);              // 直到队列完成才接受新的,默认为轮流接受

                    channel.BasicConsume(queue: "workQueue",
                                         //noAck: true,         // Message acknowledgment 消息确认
                                         consumer: consumer);

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
        }

        private static void HelloWorld()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection()) // 与Producer Send配置一样
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "hello",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

                    var consumer = new EventingBasicConsumer(channel);  // 创建Consumer
                    consumer.Received += (model, ea) =>         // 通过回调函数异步推送我们的消息
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine(" [x] Received {0}", message);
                    };

                    channel.BasicConsume(queue: "hello",
                                         noAck: true,
                                         consumer: consumer);

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
        }
    }
}
