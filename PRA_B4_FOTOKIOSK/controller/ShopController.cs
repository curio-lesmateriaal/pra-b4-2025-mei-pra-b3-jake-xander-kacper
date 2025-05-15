using PRA_B4_FOTOKIOSK.magie;
using PRA_B4_FOTOKIOSK.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace PRA_B4_FOTOKIOSK.controller
{
    public class ShopController
    {
        public static Home? Window { get; set; }
        
        // Total price of all products in the receipt
        private decimal _totalPrice = 0;

        public void Start()
        {
            // Reset the total price
            _totalPrice = 0;
            
            // Initialize products with prices and descriptions
            ShopManager.Products.Clear();
            
            ShopManager.Products.Add(new KioskProduct() { 
                Name = "Foto 10x15", 
                Price = 2.55m,
                Description = "Standaard formaat foto, perfect voor albums"
            });
            
            ShopManager.Products.Add(new KioskProduct() { 
                Name = "Foto 13x18", 
                Price = 3.50m,
                Description = "Middelgroot formaat, ideaal voor inlijsten"
            });
            
            ShopManager.Products.Add(new KioskProduct() { 
                Name = "Foto 20x30", 
                Price = 5.95m,
                Description = "Groot formaat voor aan de muur"
            });

            // Update product dropdown
            ShopManager.UpdateDropDownProducts();

            // Build and display price list
            StringBuilder priceList = new StringBuilder();
            priceList.AppendLine("Prijslijst:\n");
            
            foreach (KioskProduct product in ShopManager.Products)
            {
                priceList.AppendLine($"{product.Name}: €{product.Price:F2}");
                priceList.AppendLine($"  {product.Description}\n");
            }
            
            // Display the price list
            ShopManager.SetShopPriceList(priceList.ToString());
            
            // Initialize receipt with header
            ShopManager.SetShopReceipt("Bon:\n");
        }

        // Wordt uitgevoerd wanneer er op de Toevoegen knop is geklikt
        public void AddButtonClick()
        {
            // Get the selected product
            KioskProduct selectedProduct = ShopManager.GetSelectedProduct();
            
            // Get the quantity
            int? amount = ShopManager.GetAmount();
            
            // Get the photo ID
            int? photoId = ShopManager.GetFotoId();
            
            // Check if all required fields are filled in
            if (selectedProduct == null || amount == null || photoId == null)
            {
                MessageBox.Show("Vul alle velden in a.u.b.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // Calculate total price for this product
            decimal totalPrice = selectedProduct.Price * (decimal)amount;
            
            // Remove any existing total line from the receipt before adding the new item
            string receipt = ShopManager.GetShopReceipt();
            int totalLineIndex = receipt.LastIndexOf("Totaal:");
            if (totalLineIndex >= 0)
            {
                // Remove the total line and the separator
                int separatorIndex = receipt.LastIndexOf("-----");
                if (separatorIndex >= 0)
                {
                    string receiptWithoutTotal = receipt.Substring(0, separatorIndex);
                    ShopManager.SetShopReceipt(receiptWithoutTotal);
                }
            }
            else if (receipt.StartsWith("Bon:"))
            {
                // This is the first item, clear the initial receipt text
                ShopManager.SetShopReceipt("");
            }
            
            // Add the new item to the receipt
            ShopManager.AddShopReceipt($"\nFoto ID: {photoId}\n");
            ShopManager.AddShopReceipt($"{amount}x {selectedProduct.Name} à €{selectedProduct.Price:F2}\n");
            ShopManager.AddShopReceipt($"Subtotaal: €{totalPrice:F2}\n\n");
            
            // Calculate and update the total
            UpdateTotalPrice();
        }

        // Updates the total price on the receipt
        private void UpdateTotalPrice()
        {
            // Get current receipt text
            string receipt = ShopManager.GetShopReceipt();
            
            // Extract all prices from the receipt
            decimal totalPrice = 0;
            string pattern = @"Subtotaal: €(\d+,\d+)";
            
            foreach (Match match in Regex.Matches(receipt, pattern))
            {
                if (match.Groups.Count > 1)
                {
                    string priceText = match.Groups[1].Value;
                    if (decimal.TryParse(priceText, out decimal subtotal))
                    {
                        totalPrice += subtotal;
                    }
                }
            }
            
            _totalPrice = totalPrice;
            
            // Update the receipt with the new total
            // Find the position where the total is displayed (at beginning or end)
            int totalLineIndex = receipt.LastIndexOf("Totaal:");
            
            if (totalLineIndex >= 0)
            {
                // Remove the old total line and everything after it
                string receiptWithoutTotal = receipt.Substring(0, totalLineIndex);
                
                // Append a separator line and the new total
                ShopManager.SetShopReceipt($"{receiptWithoutTotal}--------------------------------\nTotaal: €{_totalPrice:F2}");
            }
            else
            {
                // No total line found, this might be the first item
                // Just append the total at the end
                ShopManager.AddShopReceipt($"--------------------------------\nTotaal: €{_totalPrice:F2}");
            }
        }

        // Wordt uitgevoerd wanneer er op de Resetten knop is geklikt
        public void ResetButtonClick()
        {
            // Reset the receipt and total price
            _totalPrice = 0;
            ShopManager.SetShopReceipt("Bon:\n");
        }

        // Wordt uitgevoerd wanneer er op de Save knop is geklikt
        public void SaveButtonClick()
        {
            try
            {
                // Create a timestamp for the filename
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string directory = "../../../receipts";
                
                // Create directory if it doesn't exist
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // Create the filename
                string filename = $"{directory}/receipt_{timestamp}.txt";
                
                // Get the receipt content
                string receiptContent = ShopManager.GetShopReceipt();
                
                // Write to file
                File.WriteAllText(filename, receiptContent);
                
                MessageBox.Show($"Bon opgeslagen als {filename}", "Opgeslagen", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij opslaan: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
