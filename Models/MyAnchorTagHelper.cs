using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SandulasWebApp.Models
{
	//https://damienbod.com/2018/08/13/is-active-route-tag-helper-for-asp-net-mvc-core-with-razor-page-support/

	[HtmlTargetElement("a", Attributes = "select-current-route")]
	public class MyAnchorTagHelper : AnchorTagHelper
	{
		public MyAnchorTagHelper(IHtmlGenerator generator) : base(generator) { }

		public override int Order { get { return 1; } }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.Attributes.RemoveAll("select-current-route");

			if (!isCurrentRoute()) return;

			//add "selected" value to the "class" attribute
			var classAttr = output.Attributes.FirstOrDefault(a => a.Name == "class");
			if (classAttr == null)
			{
				output.Attributes.Add("class", "selected");
			}
			else if (classAttr.Value == null)
			{
				output.Attributes.SetAttribute("class", "selected");
			}
			else if (!classAttr.Value.ToString().ToLower().Contains("selected"))
			{
				output.Attributes.SetAttribute("class", classAttr.Value.ToString() + " selected");
			}

			//remove the "href" attribute
			var hrefIndex = output.Attributes.IndexOfName("href");
			if (hrefIndex > - 1) output.Attributes.RemoveAt(hrefIndex);
		}

		private bool isCurrentRoute()
		{
			string currentController = ViewContext.RouteData.Values["Controller"].ToString();
			string currentAction = ViewContext.RouteData.Values["Action"].ToString();

			if (!string.IsNullOrWhiteSpace(Controller) && Controller.ToLower() != currentController.ToLower())
			{
				return false;
			}

			if (!string.IsNullOrWhiteSpace(Action) && Action.ToLower() != currentAction.ToLower())
			{
				return false;
			}

			return true;
		}
	}
}
