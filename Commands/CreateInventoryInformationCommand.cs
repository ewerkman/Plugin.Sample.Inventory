using System;
using System.Linq;
using System.Threading.Tasks;
using Plugin.Sample.Inventory.Models;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Inventory;
using Sitecore.Commerce.Plugin.ManagedLists;

namespace Plugin.Sample.Inventory.Commands
{
    public class CreateInventoryInformationCommand : CommerceCommand
    {
        private readonly InventoryCommander _inventoryCommander;

        public CreateInventoryInformationCommand(InventoryCommander inventoryCommander)
        {
            _inventoryCommander = inventoryCommander;
        }

        public virtual async Task<InventoryInformation> Process(CommerceContext commerceContext,
            InventoryModel inventory)
        {
            var inventorySetId = $"{CommerceEntity.IdPrefix<InventorySet>()}{inventory.InventorySet}";
            string inventorySetIdPrefix = CommerceEntity.IdPrefix<InventorySet>();

            var productArgument = ProductArgument.FromItemId(inventory.SellableItemId);
            string inventoryIdPrefix = CommerceEntity.IdPrefix<InventoryInformation>();
            string inventorySetName =
                inventorySetId.Replace(inventorySetIdPrefix, string.Empty, StringComparison.OrdinalIgnoreCase);
            string friendlyId = string.IsNullOrWhiteSpace(productArgument.VariantId)
                ? $"{inventorySetName}-{productArgument.ProductId}"
                : $"{inventorySetName}-{productArgument.ProductId}-{productArgument.VariantId}";
            string inventoryId = $"{inventoryIdPrefix}{friendlyId}";

            string sellableItemId = $"{CommerceEntity.IdPrefix<SellableItem>()}{productArgument.ProductId}";
            string variationId = productArgument.VariantId;

            var inventoryInformation = CreateInventoryInformation(inventory, inventoryId, friendlyId, inventorySetId);

            await AssociateInventoryInformationToSellableItem(commerceContext, inventorySetId, inventoryInformation);

            await UpdateSellableItem(commerceContext, sellableItemId, variationId, inventoryInformation, inventorySetId);

            await _inventoryCommander.PersistEntity(commerceContext, inventoryInformation);


            return inventoryInformation;
        }

        private static InventoryInformation CreateInventoryInformation(InventoryModel inventory, string inventoryId,
            string friendlyId, string inventorySetId)
        {
            // Create an InventoryInformationObject
            var inventoryInformation = new InventoryInformation();

            inventoryInformation.Id = inventoryId;
            inventoryInformation.FriendlyId = friendlyId;

            inventoryInformation.SellableItem = new EntityReference(inventory.SellableItemId);
            inventoryInformation.InventorySet = new EntityReference(inventorySetId);

            inventoryInformation.Quantity = inventory.Quantity;
            inventoryInformation.InvoiceUnitPrice = inventory.InvoiceUnitPrice;

            if (inventory.Backorderable)
            {
                var backorderableComponent = inventoryInformation.GetComponent<BackorderableComponent>();
                backorderableComponent.Backorderable = inventory.Backorderable;
                backorderableComponent.BackorderAvailabilityDate = inventory.BackorderAvailabilityDate;
                backorderableComponent.BackorderedQuantity = inventory.BackorderedQuantity;
                backorderableComponent.BackorderLimit = inventory.BackorderLimit;
            }

            if (inventory.Preorderable)
            {
                var preorderableComponent = inventoryInformation.GetComponent<PreorderableComponent>();
                preorderableComponent.Preorderable = inventory.Preorderable;
                preorderableComponent.PreorderAvailabilityDate = inventory.PreorderAvailabilityDate;
                preorderableComponent.PreorderedQuantity = inventory.PreorderedQuantity;
                preorderableComponent.PreorderLimit = inventory.PreorderLimit;
            }

            // Add to list of Inventory Informations
            inventoryInformation.GetComponent<TransientListMembershipsComponent>().Memberships.Add(
                CommerceEntity.ListName<InventoryInformation>());
            return inventoryInformation;
        }

        private async Task AssociateInventoryInformationToSellableItem(CommerceContext commerceContext, string inventorySetId,
            InventoryInformation inventoryInformation)
        {
            // Associate inventory information to sellable item
            // Establish relationship between inventory set and inventory information
            await _inventoryCommander.Pipeline<ICreateRelationshipPipeline>().RunAsync(
                    new RelationshipArgument(
                            inventorySetId,
                            inventoryInformation.Id,
                            InventoryConstants.InventorySetToInventoryInformation)
                        {TargetType = typeof(InventoryInformation)},
                    commerceContext.PipelineContext)
                .ConfigureAwait(false);
        }

        private async Task UpdateSellableItem(CommerceContext commerceContext, string sellableItemId, string variationId,
            InventoryInformation inventoryInformation, string inventorySetId)
        {
            // Update all sellable item versions.
            var versions =
                await _inventoryCommander.Pipeline<IFindEntityVersionsPipeline>()
                    .RunAsync(new FindEntityArgument(typeof(SellableItem), sellableItemId),
                        commerceContext.PipelineContext)
                    .ConfigureAwait(false);

            foreach (var sellableItemVersion in versions.Cast<SellableItem>())
            {
                var sellableItemVersionVariation = sellableItemVersion.GetVariation(variationId);

                if (sellableItemVersionVariation == null)
                {
                    // Check if this variation exists in any other variation.
                    if (versions.Cast<SellableItem>().Any(version => version.GetVariation(variationId) != null))
                    {
                        continue;
                    }
                }

                var inventoryComponent =
                    sellableItemVersionVariation != null
                        ? sellableItemVersionVariation.GetComponent<InventoryComponent>()
                        : sellableItemVersion.GetComponent<InventoryComponent>();

                inventoryComponent.InventoryAssociations.Add(new InventoryAssociation
                {
                    InventoryInformation = new EntityReference(inventoryInformation.Id),
                    InventorySet = new EntityReference(inventorySetId)
                });

                await _inventoryCommander.Pipeline<IPersistEntityPipeline>()
                    .RunAsync(new PersistEntityArgument(sellableItemVersion), commerceContext.PipelineContext)
                    .ConfigureAwait(false);
            }
        }
    }
}