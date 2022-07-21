using EmailService.Configuration;
using EmailService.Interfaces;
using EmailService.MessageBroker;
using EmailService.Models;
using EmailService.Services;
using Microsoft.OpenApi.Models;
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

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.Configure<SendGridConfiguration>(Configuration.GetSection("SendGrid"));
            services.Configure<ElasticConfiguration>(Configuration.GetSection("Elastic"));
            services.Configure<SendInBlueConfiguration>(Configuration.GetSection("SendInBlue"));
            services.AddScoped<IConsumerFactory, ConsumerFactory>();
            services.AddScoped<IEmailService, SendGridEmailService>();
            services.AddScoped<IEmailService, ElasticEmailService>();
            services.AddScoped<IEmailService, SendInBlueEmailService>();

            ConfigureListeners(services);

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Email service v1", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Email service v1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void ConfigureListeners(IServiceCollection services)
        {
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
                    serviceProvider.GetServices<IEmailService>()));
                }
            }
        }
    }
}