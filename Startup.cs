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
using System.Collections.Generic;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.AspNetCore.HttpOverrides;

namespace SandulasWebApp
{
	public class Startup
	{
		public IConfiguration Configuration { get; }
		string hostingEnvironment;

		public Startup(IHostingEnvironment env)
		{
			hostingEnvironment = env.EnvironmentName;

			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
			Configuration = builder.Build();
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			#region Load bot configuration and QnA services

			// Loads .bot configuration file and adds a singleton that your Bot can access through dependency injection.
			var secretKey = Configuration.GetSection("botFileSecret")?.Value;
			var botFilePath = Configuration.GetSection("botFilePath")?.Value;
			if (!File.Exists(botFilePath))
			{
				throw new FileNotFoundException($"The .bot configuration file was not found. botFilePath: { botFilePath }");
			}

			BotConfiguration botConfig = null;
			try
			{
				botConfig = BotConfiguration.Load(botFilePath, secretKey);
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
			services.AddSingleton(sp => initQnABotServices(botConfig));

			#endregion

			#region Add bot service

			services.AddBot<SandulasBot>(options =>
			{
				// Retrieve current endpoint.
				var service = botConfig.Services.Where(s => s.Type == "endpoint" && s.Name.ToLower() == hostingEnvironment.ToLower()).FirstOrDefault();
				if (!(service is EndpointService endpointService))
				{
					throw new InvalidOperationException($"The .bot file does not contain a { hostingEnvironment } endpoint.");
				}

				options.CredentialProvider = new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);
				//options.ChannelProvider = new ConfigurationChannelProvider(Configuration);

				// Catches any errors that occur during a conversation turn
				options.OnTurnError = async (context, exception) =>
				{
					await context.SendActivityAsync("Sorry, it looks like something went wrong.");
				};
			});

			#endregion

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
				// used to access the app through a reverse proxy
				app.UseForwardedHeaders(new ForwardedHeadersOptions
				{
					ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
				});

				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			//app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "Main",
					template: "{action}/{Id?}",
					defaults: new {controller="Main", action="About"});
			});
			app.UseBotFramework();
		}

		private QnAServices initQnABotServices(BotConfiguration botConfig)
		{
			var qnaServices = new Dictionary<string, QnAMaker>();

			foreach (var service in botConfig.Services)
			{
				if (service.Type == ServiceTypes.QnA)
				{
					var qnaService = (QnAMakerService)service;
					var qnaEndPoint = new QnAMakerEndpoint()
					{
						KnowledgeBaseId = qnaService.KbId,
						EndpointKey = qnaService.EndpointKey,
						Host = qnaService.Hostname
					};

					var qnaMaker = new QnAMaker(qnaEndPoint);
					qnaServices.Add(qnaService.Name, qnaMaker);
				}
			}

			return new QnAServices(qnaServices);
		}
	}
}
