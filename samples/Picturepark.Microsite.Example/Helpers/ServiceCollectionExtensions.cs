using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Picturepark.Microsite.Example.Helpers
{
    public static class ServiceCollectionExtensions
    {
        public static T AddConfiguration<T>(this IServiceCollection services, IConfiguration configuration)
            where T : class, new()
        {
            var className = typeof(T).Name;
            var sectionName = className.EndsWith("Configuration")
                ? className.Substring(0, className.Length - 13)
                : className;

            var config = new T();
            configuration.GetSection(sectionName).Bind(config);
            services.AddSingleton(config);
            return config;
        }
    }
}