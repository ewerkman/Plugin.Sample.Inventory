using System;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Plugin.Sample.Inventory.Commands;
using Plugin.Sample.Inventory.Models;
using Sitecore.Commerce.Core;

namespace Plugin.Sample.Inventory.Controllers
{
    public class CommandsController : CommerceODataController
    {
        public CommandsController(IServiceProvider serviceProvider, CommerceEnvironment globalEnvironment) : base(serviceProvider, globalEnvironment)
        {
        }
        
        [HttpPost]
        [ODataRoute("CreateInventoryInformation()", RouteName = CoreConstants.CommerceApi)]
        public async Task<IActionResult> CreateInventoryInformation([FromBody] InventoryModel model)
        {
            if (!ModelState.IsValid || model == null)
            {
                return new BadRequestObjectResult(ModelState);
            }
            
            var command = Command<CreateInventoryInformationCommand>();
            await command.Process(CurrentContext, model);
            
            return new ObjectResult(command);
        }
    }
}