using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Configuration;
using Sitecore.Framework.Pipelines.Definitions.Extensions;


namespace Plugin.Sample.Inventory
{
    public class ConfigureSitecore : IConfigureSitecore
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllCommands(assembly);
            services.RegisterAllPipelineBlocks(assembly);

            services.Sitecore().Pipelines(
                config =>
                    config
                        .ConfigurePipeline<IConfigureServiceApiPipeline>(c =>
                            c.Add<ConfigureServiceApiBlock>()
                        )
                        .ConfigurePipeline<IRunningPluginsPipeline>(c =>
                        {
                            c.Add<Pipelines.Blocks.RegisteredPluginBlock>().After<RunningPluginsBlock>();
                        })
            );
        }
    }
}