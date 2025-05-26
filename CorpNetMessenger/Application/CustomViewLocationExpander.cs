using Microsoft.AspNetCore.Mvc.Razor;

namespace CorpNetMessenger.Application
{
    public class CustomViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            var additionalLocations = new[]
            {
                "~/Web/Areas/" + context.AreaName + "/Views/" + context.ControllerName + "/" + context.ViewName + ".cshtml",
                "~/Web/Areas/" + context.AreaName + "/Views/Shared/" + context.ViewName + ".cshtml",
                "~/Web/Views/" + context.ControllerName + "/" + context.ViewName + ".cshtml",
                "~/Web/Views/Shared/" + context.ViewName + ".cshtml"
        };

            return additionalLocations.Concat(viewLocations);
        }
    }
}
