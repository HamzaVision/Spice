using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Spice.Models;

namespace Spice.TagHelpers
{
	[HtmlTargetElement("div",Attributes ="page-model")]
	public class Page_LinkTagHelper : TagHelper
	{
		private IUrlHelperFactory urlHelpFactory;
		public Page_LinkTagHelper(IUrlHelperFactory urlhelp)
		{
			urlHelpFactory = urlhelp;
		}
		[ViewContext]
		[HtmlAttributeNotBound]
		public ViewContext viewContext { get; set; }
		public PagingInfo pageModel { get; set; }
		public string PageAction { get; set; }
		public  bool pageClassEnabled { get; set; }
		public string pageClass { get; set; }
		public string pageNormal { get; set; }
		public string pageSelected { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			Microsoft.AspNetCore.Mvc.IUrlHelper urlHelper = urlHelpFactory.GetUrlHelper(viewContext);
			TagBuilder result = new TagBuilder("div");
			for (int i=1; i < pageModel.TotalPage; i++)
			{
				TagBuilder tag = new TagBuilder("a");
				string url = pageModel.urlParam.Replace(":", i.ToString());
				tag.Attributes["href"] = url;
				if (pageClassEnabled)
				{
					tag.AddCssClass(pageClass);
					tag.AddCssClass(i == pageModel.CurrentPage ? pageSelected : pageNormal);

				}
				tag.InnerHtml.Append(i.ToString());
				result.InnerHtml.AppendHtml(result.InnerHtml);

			}
			output.Content.AppendHtml(result.InnerHtml);
		}

	}
}
