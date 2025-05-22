using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRA_B4_FOTOKIOSK.models
{
    public class OrderedProduct
    {
        public int? PhotoId { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        
        // Constructor to make it easier to create new ordered products
        public OrderedProduct(int? photoId, string productName, int quantity, decimal unitPrice)
        {
            PhotoId = photoId;
            ProductName = productName;
            Quantity = quantity;
            UnitPrice = unitPrice;
            TotalPrice = unitPrice * quantity;
        }
        
        // Default constructor
        public OrderedProduct() { }
        
        // Returns a formatted string representation of this ordered product for the receipt
        public string ToReceiptString()
        {
            return $"Foto ID: {PhotoId}\n{Quantity}x {ProductName} à €{UnitPrice:F2}\nSubtotaal: €{TotalPrice:F2}\n";
        }
    }
}