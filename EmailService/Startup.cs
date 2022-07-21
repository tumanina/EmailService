using EmailService.Configuration;
using EmailService.Interfaces;
using EmailService.MessageBroker;
using EmailService.Services;
using Microsoft.OpenApi.Models;

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
            services.Configure<ListenerConfiguration>(Configuration.GetSection("Listener"));
            services.AddScoped<IConsumerFactory, ConsumerFactory>();
            services.AddScoped<IEmailService, SendGridEmailService>();
            services.AddScoped<IEmailService, ElasticEmailService>();
            services.AddScoped<IEmailService, SendInBlueEmailService>();
            services.AddScoped<IListener, Listener>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Email service v1", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
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