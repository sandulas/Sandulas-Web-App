using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.AI.QnA;

namespace SandulasWebApp
{
	public class QnAServices
	{
		public Dictionary<string, QnAMaker> QnAMakers { get; } = new Dictionary<string, QnAMaker>();

		public QnAServices(Dictionary<string, QnAMaker> qnaServices)
		{
			QnAMakers = qnaServices ?? throw new ArgumentNullException(nameof(qnaServices)); ;
		}
	}
}