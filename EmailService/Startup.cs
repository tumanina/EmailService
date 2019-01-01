using System;
using System.Collections.Generic;
using EmailService.Configuration;
using EmailService.MessageBroker;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using RabbitMQ.Client;

namespace EmailService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var sendGridConfiguration = Configuration.GetSection("SendGrid").Get<SendGridConfiguration>();
            var elasticConfiguration = Configuration.GetSection("Elastic").Get<ElasticConfiguration>();
            services.AddSingleton<IConsumerFactory, ConsumerFactory>();
            services.AddSingleton<IEmailService>(t => new SendGridEmailService(sendGridConfiguration));
            services.AddSingleton<IEmailService>(t => new ElasticEmailService(elasticConfiguration));

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });
            });
            NLog.LogManager.LoadConfiguration("nlog.config");

            var listeners = Configuration.GetSection("Listeners").Get<IEnumerable<ListenerConfiguration>>();

            var serviceProvider = services.BuildServiceProvider();

            if (listeners != null)
            {
                foreach (var listener in listeners)
                {
                    services.AddSingleton<IListener>(t => new Listener(
                    EmailServiceType.Elastic,
                    new ConnectionFactory
                    {
                        HostName = listener.Server.Host,
                        UserName = listener.Server.UserName,
                        Password = listener.Server.Password
                    },
                    serviceProvider.GetService<IConsumerFactory>(),
                    listener.QueueName,
                    listener.ExchangeName,
                    serviceProvider.GetServices<IEmailService>(),
                    serviceProvider.GetService<ILogger<Listener>>()));
                }
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            RunListeners(serviceProvider);
        }

        private void RunListeners(IServiceProvider serviceProvider)
        {
            foreach (var listener in serviceProvider.GetServices<IListener>())
            {
                listener.Run();
            }
        }
    }
}
