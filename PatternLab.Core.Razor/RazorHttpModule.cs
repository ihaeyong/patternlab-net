﻿using System.Web;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using PatternLab.Core.Razor;

// Module auto registers itself without the need for web.config
[assembly: PreApplicationStartMethod(typeof(RazorHttpModule), "LoadModule")]

namespace PatternLab.Core.Razor
{
    /// <summary>
    /// The Pattern Lab razor enabling HTTP module
    /// </summary>
    public class RazorHttpModule : IHttpModule
    {
        /// <summary>
        /// Disposes of the Pattern Lab HTTP module
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Initialises the Pattern Lab HTTP module
        /// </summary>
        /// <param name="context">The current context</param>
        public void Init(HttpApplication context)
        {
            // Register razor pattern engine
            context.Context.Application["patternEngine"] = new RazorPatternEngine();
        }

        /// <summary>
        /// Fires when the HTTP module dynamically loads
        /// </summary>
        public static void LoadModule()
        {
            // Register the module
            DynamicModuleUtility.RegisterModule(typeof(RazorHttpModule));
        }
    }
}