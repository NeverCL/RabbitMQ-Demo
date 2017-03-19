using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing.Impl;

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
            SendMsg();
        }

        private static void SendMsg()
        {
            var factory = new ConnectionFactory
            {
                HostName = hostName,                // rabbit server
                UserName = "admin",
                Password = "admin",
                Port = 5672,                        // Broker端口
                VirtualHost = "/"                   // 虚拟Host,需提前配置
            };

            using (var connection = factory.CreateConnection()) // 创建与RabbitMQ服务器的连接
            {
                using (var channel = connection.CreateModel())  // 创建1个Channel(大部分API在该Channel中)
                {
                    // 定义1个队列,自动会和默认的exchange 做direct类型绑定
                    channel.QueueDeclare(
                        queue: "hello",                     // 队列名称
                        durable: true,                      // 队列是否持久化
                        exclusive: false,                   // 排他队列:如果一个队列被声明为排他队列，该队列仅对首次声明它的连接可见，并在连接断开时自动删除。(活动在一次连接内)
                        autoDelete: false,                  // 自动删除:当最后一个消费者取消订阅时，队列自动删除。如果您需要仅由一个使用者使用的临时队列，请将自动删除与排除。当消费者断​​开连接时，队列将被删除。(至少消费者能连一次)
                        arguments: null);

                    var randomQueue = channel.QueueDeclare();                   // 定义随机的队列 该队列为临时队列(排他队列 + 自动删除)


                    // 定义Exchange(一般而言,不需要定义exchange,rabbitmq默认创建了所有类型的exchange)
                    //channel.ExchangeDeclare("direct-demo", ExchangeType.Direct);    // 定义direct exchange
                    //channel.ExchangeDeclare("fannout-demo", ExchangeType.Fanout);   // 定义fanout exchange
                    //channel.ExchangeDeclare("topic-demo", ExchangeType.Topic);      // 定义fanout exchange


                    // 定义queue exchange key 关系(在某些业务场景下,会使用该关系做路由功能)
                    //channel.QueueBind(queue: "hello", exchange: "amq.direct", routingKey: "hello"); // 默认绑定的关系和该行代码效果一样
                    //channel.QueueBind("hello", "amq.fanout", "hello");                              // 该类型下的routingKey 实际不需要

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    while (true)
                    {
                        string message = "Hello World!" + DateTime.Now;
                        var body = Encoding.UTF8.GetBytes(message);

                        // 发送消息到队列中
                        channel.BasicPublish(
                            exchange: string.Empty,         // 传递为Empty的时候,通过	`(AMQP default)`传递
                            routingKey: "hello",            // routing key 与 queuebind中的binding key对应
                            basicProperties: properties,    // 消息header
                            body: body);                    // 消息body:发送的是bytes 可以任意编码

                        Console.WriteLine(" [x] Sent {0}", message);
                    }
                }
            }
        }

        /// <summary>
        /// Routing
        /// </summary>
        private static void Routing()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
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
            var factory = new ConnectionFactory() { HostName = "localhost" };
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
            var factory = new ConnectionFactory() { HostName = "localhost" };
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
                HostName = hostName,                // rabbit server
                UserName = "admin",
                Password = "admin",
                Port = 5672,                        // Broker端口
                VirtualHost = "/"                   // 虚拟Host,需提前配置
            };

            using (var connection = factory.CreateConnection()) // 创建与RabbitMQ服务器的连接
            {
                using (var channel = connection.CreateModel())  // 创建1个Channel(大部分API在该Channel中)
                {
                    // 定义1个队列,自动会和默认的exchange 做direct类型绑定
                    channel.QueueDeclare(
                        queue: "hello",                     // 队列名称
                        durable: true,                      // 队列是否持久化
                        exclusive: false,                   // 排他队列:如果一个队列被声明为排他队列，该队列仅对首次声明它的连接可见，并在连接断开时自动删除。(活动在一次连接内)
                        autoDelete: false,                  // 自动删除:当最后一个消费者取消订阅时，队列自动删除。如果您需要仅由一个使用者使用的临时队列，请将自动删除与排除。当消费者断​​开连接时，队列将被删除。(至少消费者能连一次)
                        arguments: null);

                    var randomQueue = channel.QueueDeclare();                   // 定义随机的队列 该队列为临时队列(排他队列 + 自动删除)


                    // 定义Exchange(一般而言,不需要定义exchange,rabbitmq默认创建了所有类型的exchange)
                    //channel.ExchangeDeclare("direct-demo", ExchangeType.Direct);    // 定义direct exchange
                    //channel.ExchangeDeclare("fannout-demo", ExchangeType.Fanout);   // 定义fanout exchange
                    //channel.ExchangeDeclare("topic-demo", ExchangeType.Topic);      // 定义fanout exchange


                    // 定义queue exchange key 关系(在某些业务场景下,会使用该关系做路由功能)
                    //channel.QueueBind(queue: "hello", exchange: "amq.direct", routingKey: "hello"); // 默认绑定的关系和该行代码效果一样
                    //channel.QueueBind("hello", "amq.fanout", "hello");                              // 该类型下的routingKey 实际不需要

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    while (true)
                    {
                        string message = "Hello World!" + DateTime.Now;
                        var body = Encoding.UTF8.GetBytes(message);

                        // 发送消息到队列中
                        channel.BasicPublish(
                            exchange: string.Empty,         // 传递为Empty的时候,通过	`(AMQP default)`传递
                            routingKey: "hello",            // routing key 与 queuebind中的binding key对应
                            basicProperties: properties,    // 消息header
                            body: body);                    // 消息body:发送的是bytes 可以任意编码

                        Console.WriteLine(" [x] Sent {0}", message);
                    }
                }
            }
        }
    }
}