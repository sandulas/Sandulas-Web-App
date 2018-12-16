using System;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;

namespace SandulasWebApp
{
	public class Startup
	{
		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
			Configuration = builder.Build();
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddBot<SandulasBot>(options =>
			{
				var secretKey = Configuration.GetSection("botFileSecret")?.Value;
				var botFilePath = Configuration.GetSection("botFilePath")?.Value;
				if (!File.Exists(botFilePath))
				{
					throw new FileNotFoundException($"The .bot configuration file was not found. botFilePath: {botFilePath}");
				}

				// Loads .bot configuration file and adds a singleton that your Bot can access through dependency injection.
				BotConfiguration botConfig = null;
				try
				{
					botConfig = BotConfiguration.Load(botFilePath ?? @".\BotConfiguration.bot", secretKey);
				}
				catch
				{
					var msg = @"Error reading bot file. Please ensure you have valid botFilePath and botFileSecret set for your environment.
    - You can find the botFilePath and botFileSecret in the Azure App Service application settings.
    - If you are running this bot locally, consider adding a appsettings.json file with botFilePath and botFileSecret.
    - See https://aka.ms/about-bot-file to learn more about .bot file its use and bot configuration.
    ";
					throw new InvalidOperationException(msg);
				}

				services.AddSingleton(sp => botConfig);

				// Retrieve current endpoint.
				var service = botConfig.Services.Where(s => s.Type == "endpoint" && s.Name == "development").FirstOrDefault();
				if (!(service is EndpointService endpointService))
				{
					throw new InvalidOperationException($"The .bot file does not contain a development endpoint.");
				}

				options.CredentialProvider = new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);

				// Catches any errors that occur during a conversation turn and logs them.
				options.OnTurnError = async (context, exception) =>
				{
					await context.SendActivityAsync("Sorry, it looks like something went wrong.");
				};
			});

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			//app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
			app.UseBotFramework();
		}
	}
}
