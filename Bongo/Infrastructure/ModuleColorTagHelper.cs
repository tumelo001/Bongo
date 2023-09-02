using Bongo.Data;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Bongo.Infrastructure
{
    [HtmlTargetElement("td", Attributes = "module-color")]
    public class ModuleColorTagHelper : TagHelper
    {
        private readonly IRepositoryWrapper _repo;
        public ModuleColorTagHelper(IRepositoryWrapper repo) 
        {
            _repo = repo;
        }


        [HtmlAttributeName("module-color")]
        public string ModuleCode { get; set; }

        [HtmlAttributeName("module-color-username")]
        public string Username { get; set; }

        public override void Process(TagHelperContext context,
            TagHelperOutput output)
        {
            var module = _repo.ModuleColor.GetModuleColorWithColorDetails(Username, ModuleCode);
            if (module.Color.ColorName != "No-color")
            {
                output.Attributes.SetAttribute("style", $"background-color: {module.Color.ColorValue}; color: white");
            }
            else
            {
                output.Attributes.SetAttribute("style", "color: black");
            }
        }
    }
}
