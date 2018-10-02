
namespace IDS.UI.SPA.Routing
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.Routing.Template;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;

    public class DomainTemplateRoute : INamedRouter, IRouter
    {
        //private readonly TemplateRoute _innerRoute;
        private readonly RouteBase _innerRoute;

        private readonly IRouter _target;

        private readonly string _domainTemplate;

        private readonly TemplateMatcher _matcher;

        private ILogger _logger;

        public DomainTemplateRoute(
            IRouter target,
            string domainTemplate,
            string routeTemplate,
            bool ignorePort,
            IInlineConstraintResolver inlineConstraintResolver)
            : this(target, domainTemplate, routeTemplate, null, null, null, ignorePort, inlineConstraintResolver)
        {
        }

        public DomainTemplateRoute(
            IRouter target,
            string domainTemplate,
            string routeTemplate,
            IDictionary<string, object> defaults,
            IDictionary<string, object> constraints,
            IDictionary<string, object> dataTokens,
            bool ignorePort,
            IInlineConstraintResolver inlineConstraintResolver)
            : this(target, null, domainTemplate, routeTemplate, defaults, constraints, dataTokens, ignorePort, inlineConstraintResolver)
        {
        }

        public DomainTemplateRoute(
            IRouter target,
            string routeName,
            string domainTemplate,
            string routeTemplate,
            IDictionary<string, object> defaults,
            IDictionary<string, object> constraints,
            IDictionary<string, object> dataTokens,
            bool ignorePort,
            IInlineConstraintResolver inlineConstraintResolver)
        {
            //_innerRoute = new TemplateRoute(target, routeName, routeTemplate, defaults, constraints, dataTokens, inlineConstraintResolver);
            _innerRoute = new Route(target, routeName, routeTemplate, new RouteValueDictionary(defaults), constraints, new RouteValueDictionary(dataTokens), inlineConstraintResolver);

            _target = target;
            _domainTemplate = domainTemplate;

            _matcher = new TemplateMatcher(
                TemplateParser.Parse(DomainTemplate), new RouteValueDictionary(Defaults));

            Name = routeName;
            IgnorePort = ignorePort;
        }

        public string Name { get; private set; }

        public IReadOnlyDictionary<string, object> Defaults
        {
            get
            {
                return _innerRoute.Defaults;
            }
        }

        public IReadOnlyDictionary<string, object> DataTokens
        {
            get
            {
                return _innerRoute.DataTokens;
            }
        }

        public string RouteTemplate
        {
            get
            {
                return ((Route)_innerRoute).RouteTemplate;
            }
        }

        public IReadOnlyDictionary<string, IRouteConstraint> Constraints
        {
            get
            {
                // return _innerRoute.Constraints;
                return new ReadOnlyDictionary<string, IRouteConstraint>(_innerRoute.Constraints.ToDictionary(k => k.Key, v => v.Value));
            }
        }

        public string DomainTemplate
        {
            get
            {
                return _domainTemplate;
            }
        }

        public bool IgnorePort { get; set; }

        public async Task RouteAsync(RouteContext context)
        {
            EnsureLoggers(context.HttpContext);
            using (_logger.BeginScope("DomainTemplateRoute.RouteAsync"))
            {
                var requestHost = context.HttpContext.Request.Host.Value;
                if (IgnorePort && requestHost.Contains(":"))
                {
                    requestHost = requestHost.Substring(0, requestHost.IndexOf(":"));
                }
                Console.WriteLine($"Subdomain name: {requestHost}");

                // var routeValues = new RouteValueDictionary();
                // var routeData = context.HttpContext.GetRouteData();
                // var values = _matcher.Match(requestHost);
                var values = _matcher.TryMatch(context.HttpContext.Request.Path, new RouteValueDictionary(context.RouteData));
                //if (values == null)
                if (!values)
                {
                    if (_logger.IsEnabled(LogLevel.Trace))
                    {
                        _logger.LogTrace("DomainTemplateRoute " + Name + " - Host \"" + context.HttpContext.Request.Host + "\" did not match.");
                    }

                    // If we got back a null value set, that means the URI did not match
                    return;
                }

                var oldRouteData = context.RouteData;

                var newRouteData = new RouteData(oldRouteData);
                MergeValues(newRouteData.DataTokens, DataTokens);
                newRouteData.Routers.Add(_target);

                //MergeValues(newRouteData.Values, values.ToImmutableDictionary());
                MergeValues(newRouteData.Values, new RouteValueDictionary(context.RouteData).ToImmutableDictionary());

                try
                {
                    context.RouteData = newRouteData;

                    // delegate further processing to inner route
                    await _innerRoute.RouteAsync(context);
                }
                finally
                {
                    // Restore the original values to prevent polluting the route data.
                    // if (!context.IsHandled)
                    // {
                    //     context.RouteData = oldRouteData;
                    // }
                }
            }
        }

        private static void MergeValues(IDictionary<string, object> destination, IReadOnlyDictionary<string, object> values)
        {
            foreach (var kvp in values)
            {
                // This will replace the original value for the specified key.
                // Values from the matched route will take preference over previous
                // data in the route context.
                destination[kvp.Key] = kvp.Value;
            }
        }

        private void EnsureLoggers(HttpContext context)
        {
            if (_logger == null)
            {
                var factory = context.RequestServices.GetRequiredService<ILoggerFactory>();

                _logger = factory.CreateLogger<RouteBase>();
            }
        }

        public override string ToString()
        {
            return _domainTemplate + "/" + RouteTemplate;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            foreach (var matcherParameter in _matcher.Template.Parameters)
            {
                context.Values.Remove(matcherParameter.Name); // make sure none of the domain-placeholders are appended as query string parameters
            }

            return _innerRoute.GetVirtualPath(context);
        }
    }
}
