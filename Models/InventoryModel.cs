using System;
using Sitecore.Commerce.Core;

namespace Plugin.Sample.Inventory.Models
{
    public class InventoryModel : Model
    {
        public string InventorySet { get; set; }
        public string SellableItemId { get; set; }
        public int Quantity { get; set; }
        public Money InvoiceUnitPrice { get; set; }
        
        public bool Backorderable { get; set; }
        public DateTime BackorderAvailabilityDate { get; set; }
        public int BackorderedQuantity { get; set; }
        public int BackorderLimit { get; set; }
        
        public bool Preorderable { get; set; }
        public DateTime PreorderAvailabilityDate { get; set; }
        public int PreorderedQuantity { get; set; }
        public int PreorderLimit { get; set; }
    }
}