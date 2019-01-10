using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Schema;

namespace SandulasWebApp
{
	/// <summary>
	/// Represents a bot that processes incoming activities.
	/// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
	/// This is a Transient lifetime service. Transient lifetime services are created
	/// each time they're requested. Objects that are expensive to construct, or have a lifetime
	/// beyond a single turn, should be carefully managed.
	/// For example, the <see cref="MemoryStorage"/> object and associated
	/// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
	/// </summary>
	/// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
	public class SandulasBot : IBot
	{
		private readonly QnAMaker qnaService;

		public SandulasBot(QnAServices qnaServices)
		{
			qnaService = qnaServices.QnAMakers["SandulasQnA"];
		}

		/// <summary>
		/// Every conversation turn calls this method.
		/// </summary>
		/// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
		/// for processing this conversation turn. </param>
		/// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
		/// or threads to receive notice of cancellation.</param>
		/// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
		/// <seealso cref="BotStateSet"/>
		/// <seealso cref="ConversationState"/>
		public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
		{
			/// Handle Message activity type, which is the main activity type for shown within a conversational interface
			/// Message activities may contain text, speech, interactive cards, and binary or unknown attachments.
			/// see https://aka.ms/about-bot-activity-message to learn more about the message and other activity types
			if (turnContext.Activity.Type == ActivityTypes.Message)
			{
				// Get the answers from the QnA maker service
				var responses = await qnaService.GetAnswersAsync(turnContext);

				// Compose the response message
				string message = "";

				if (responses == null || responses.Length == 0)
				{
					message = "No aswers found in the KB";
				}
				else
				{
					foreach (var response in responses)
					{
						message += $@"{ response.Answer }
							-----------------------------
							Score: { response.Score }";
					}
				}

				await turnContext.SendActivityAsync(message, cancellationToken: cancellationToken);
			}
		}
	}
}
