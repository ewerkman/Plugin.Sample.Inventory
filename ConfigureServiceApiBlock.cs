using System;
using Microsoft.AspNet.OData.Builder;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sample.Inventory
{
    public class ConfigureServiceApiBlock : SyncPipelineBlock<ODataConventionModelBuilder, ODataConventionModelBuilder, CommercePipelineExecutionContext>
    {
        public override ODataConventionModelBuilder Run(ODataConventionModelBuilder arg, CommercePipelineExecutionContext context)
        {
            var createInventoryInformationAction = arg.Function("CreateInventoryInformation");
            createInventoryInformationAction.Parameter<string>("sellableItemId");
            createInventoryInformationAction.Parameter<int>("quantity");
            createInventoryInformationAction.ReturnsFromEntitySet<CommerceCommand>("Commands");
            
            return arg;
        }
    }
}