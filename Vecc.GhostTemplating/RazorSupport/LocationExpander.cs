using Microsoft.AspNetCore.Mvc.Razor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vecc.GhostTemplating.RazorSupport
{
    public class LocationExpander : IViewLocationExpander
    {
        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            Console.WriteLine(context.ViewName);
            return new string[] { @"C:\Users\edward\source\repos\Vecc.GhostTemplating\Vecc.GhostTemplating\RazorSupport\" };
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            //throw new NotImplementedException();
        }
    }
}
