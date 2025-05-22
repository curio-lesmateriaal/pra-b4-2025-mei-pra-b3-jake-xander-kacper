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
        
        // List to keep track of all ordered products
        private List<OrderedProduct> _orderedProducts = new List<OrderedProduct>();
        public void Start()
        {
            // Reset the total price and clear ordered products
            _totalPrice = 0;
            _orderedProducts.Clear();

            // Initialize products with prices and descriptions
            ShopManager.Products.Clear();

            ShopManager.Products.Add(new KioskProduct()
            {
                Name = "Foto 10x15",
                Price = 2.55m,
                Description = "Standaard formaat foto, perfect voor albums"
            });

            ShopManager.Products.Add(new KioskProduct()
            {
                Name = "Foto 13x18",
                Price = 3.50m,
                Description = "Middelgroot formaat, ideaal voor inlijsten"
            });

            ShopManager.Products.Add(new KioskProduct()
            {
                Name = "Foto 20x30",
                Price = 5.95m,
                Description = "Groot formaat voor aan de muur"
            });

            // Update product dropdown
            ShopManager.UpdateDropDownProducts();            // Build and display price list with improved styling
            StringBuilder priceList = new StringBuilder();
            
            // Create a styled header
            priceList.AppendLine("✦✦✦ THEMEPARK FOTOKIOSK PRIJSLIJST ✦✦✦\n");
            
            // Add each product with better formatting
            foreach (KioskProduct product in ShopManager.Products)
            {
                priceList.AppendLine($"◉ {product.Name}");
                priceList.AppendLine($"  Prijs: €{product.Price:F2}");
                priceList.AppendLine($"  {product.Description}");
                priceList.AppendLine("  ----------------------------------------\n");
            }            
            
            // Display the price list
            ShopManager.SetShopPriceList(priceList.ToString());

            // Initialize receipt with header
            ShopManager.SetShopReceipt("=== THEMEPARK FOTOKIOSK ===\n\n(Voeg producten toe om de bon te zien)");
        }// Wordt uitgevoerd wanneer er op de Toevoegen knop is geklikt
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
            
            // Create a new ordered product
            OrderedProduct orderedProduct = new OrderedProduct(
                photoId,
                selectedProduct.Name ?? "Onbekend product",
                amount.Value,
                selectedProduct.Price
            );
            
            // Add to our ordered products list
            _orderedProducts.Add(orderedProduct);
            
            // Regenerate the receipt with all ordered products
            RegenerateReceipt();
        }        // Regenerates the complete receipt from ordered products
        private void RegenerateReceipt()
        {
            // Start with an empty receipt and add a header
            StringBuilder displayReceipt = new StringBuilder();
            displayReceipt.AppendLine("=== THEMEPARK FOTOKIOSK ===\n");
            
            // Reset the total price
            _totalPrice = 0;
            
            // Add each ordered product to the receipt with better formatting
            foreach (OrderedProduct product in _orderedProducts)
            {
                displayReceipt.AppendLine($"Foto ID: {product.PhotoId}");
                displayReceipt.AppendLine($"{product.Quantity}x {product.ProductName}");
                displayReceipt.AppendLine($"Prijs per stuk: €{product.UnitPrice:F2}");
                displayReceipt.AppendLine($"Subtotaal: €{product.TotalPrice:F2}");
                displayReceipt.AppendLine("------------------------\n");
                
                _totalPrice += product.TotalPrice;
            }
            
            // Add the total with better styling
            displayReceipt.AppendLine("========================");
            displayReceipt.AppendLine($"TOTAAL: €{_totalPrice:F2}");
            displayReceipt.AppendLine("========================");
            
            // Update the receipt display
            ShopManager.SetShopReceipt(displayReceipt.ToString());        }        // Wordt uitgevoerd wanneer er op de Resetten knop is geklikt
        public void ResetButtonClick()
        {
            // Reset the receipt and total price
            _totalPrice = 0;
            _orderedProducts.Clear();
            ShopManager.SetShopReceipt("=== THEMEPARK FOTOKIOSK ===\n\n(Voeg producten toe om de bon te zien)");
        }// Wordt uitgevoerd wanneer er op de Save knop is geklikt
        public void SaveButtonClick()
        {
            try
            {
                // Check if there are any items on the receipt
                if (_orderedProducts.Count == 0)
                {
                    MessageBox.Show("Er zijn geen producten op de bon. Voeg eerst producten toe.", 
                        "Lege bon", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
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
                
                // Build a receipt with styled header information
                StringBuilder receiptBuilder = new StringBuilder();
                receiptBuilder.AppendLine("****************************************************");
                receiptBuilder.AppendLine("*                  THEMEPARK FOTOKIOSK            *");
                receiptBuilder.AppendLine("****************************************************");
                receiptBuilder.AppendLine();
                receiptBuilder.AppendLine($"Datum: {DateTime.Now.ToString("dd-MM-yyyy")}");
                receiptBuilder.AppendLine($"Tijd:  {DateTime.Now.ToString("HH:mm:ss")}");
                receiptBuilder.AppendLine();
                receiptBuilder.AppendLine("====================================================");
                receiptBuilder.AppendLine("  PRODUCT DETAILS                                  ");
                receiptBuilder.AppendLine("====================================================");
                receiptBuilder.AppendLine();
                  // Add each product with improved formatting
                foreach (OrderedProduct product in _orderedProducts)
                {
                    receiptBuilder.AppendLine($"  Foto ID: {product.PhotoId}");
                    receiptBuilder.AppendLine($"  {product.Quantity}x {product.ProductName}");
                    receiptBuilder.AppendLine($"  Prijs per stuk: €{product.UnitPrice:F2}");
                    receiptBuilder.AppendLine($"  Subtotaal: €{product.TotalPrice:F2}");
                    receiptBuilder.AppendLine("  --------------------------------------------------");
                }
                
                // Add the total with better styling
                receiptBuilder.AppendLine();
                receiptBuilder.AppendLine("====================================================");
                receiptBuilder.AppendLine($"  TOTAAL: €{_totalPrice:F2}");
                receiptBuilder.AppendLine("====================================================");
                receiptBuilder.AppendLine();
                receiptBuilder.AppendLine("  Bedankt voor uw bestelling bij ons themepark!");
                receiptBuilder.AppendLine("  Bewaar deze bon als aankoopbewijs.");
                receiptBuilder.AppendLine();
                receiptBuilder.AppendLine("****************************************************");
                
                // Write to file
                File.WriteAllText(filename, receiptBuilder.ToString());
                
                // Get absolute path for display
                string absolutePath = Path.GetFullPath(filename);
                
                MessageBox.Show($"Bon opgeslagen als:\n{absolutePath}", 
                    "Bon opgeslagen", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {                MessageBox.Show($"Fout bij opslaan: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // Wordt uitgevoerd wanneer er op de Export knop is geklikt
        public void ExportButtonClick()
        {
            try
            {
                // Check if there are any items on the receipt
                if (_orderedProducts.Count == 0)
                {
                    MessageBox.Show("Er zijn geen producten op de bon. Voeg eerst producten toe.", 
                        "Lege bon", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // Create a Save File Dialog
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.Filter = "Text bestanden (*.txt)|*.txt|CSV bestanden (*.csv)|*.csv|Alle bestanden (*.*)|*.*";
                saveFileDialog.Title = "Exporteer Bon";
                saveFileDialog.DefaultExt = "txt";
                saveFileDialog.FileName = $"bon_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}";
                
                // Show the dialog and get result
                bool? result = saveFileDialog.ShowDialog();
                  // If user clicked OK, proceed with saving
                if (result == true)
                {
                    string fileExtension = System.IO.Path.GetExtension(saveFileDialog.FileName).ToLower();
                    
                    // Check if it's a TXT file
                    if (fileExtension == ".txt")
                    {
                        // Build a receipt with styled header information
                        StringBuilder receiptBuilder = new StringBuilder();
                        receiptBuilder.AppendLine("****************************************************");
                        receiptBuilder.AppendLine("*                  THEMEPARK FOTOKIOSK            *");
                        receiptBuilder.AppendLine("****************************************************");
                        receiptBuilder.AppendLine();
                        receiptBuilder.AppendLine($"Datum: {DateTime.Now.ToString("dd-MM-yyyy")}");
                        receiptBuilder.AppendLine($"Tijd:  {DateTime.Now.ToString("HH:mm:ss")}");
                        receiptBuilder.AppendLine();
                        receiptBuilder.AppendLine("====================================================");
                        receiptBuilder.AppendLine("  PRODUCT DETAILS                                  ");
                        receiptBuilder.AppendLine("====================================================");
                        receiptBuilder.AppendLine();
                        
                        // Add each product with improved formatting
                        foreach (OrderedProduct product in _orderedProducts)
                        {
                            receiptBuilder.AppendLine($"  Foto ID: {product.PhotoId}");
                            receiptBuilder.AppendLine($"  {product.Quantity}x {product.ProductName}");
                            receiptBuilder.AppendLine($"  Prijs per stuk: €{product.UnitPrice:F2}");
                            receiptBuilder.AppendLine($"  Subtotaal: €{product.TotalPrice:F2}");
                            receiptBuilder.AppendLine("  --------------------------------------------------");
                        }
                        
                        // Add the total with better styling
                        receiptBuilder.AppendLine();
                        receiptBuilder.AppendLine("====================================================");
                        receiptBuilder.AppendLine($"  TOTAAL: €{_totalPrice:F2}");
                        receiptBuilder.AppendLine("====================================================");
                        receiptBuilder.AppendLine();
                        receiptBuilder.AppendLine("  Bedankt voor uw bestelling bij ons themepark!");
                        receiptBuilder.AppendLine("  Bewaar deze bon als aankoopbewijs.");
                        receiptBuilder.AppendLine();
                        receiptBuilder.AppendLine("****************************************************");
                        
                        // Write to the selected file
                        File.WriteAllText(saveFileDialog.FileName, receiptBuilder.ToString());
                    }
                    // Check if it's a CSV file
                    else if (fileExtension == ".csv")
                    {
                        // CSV header
                        StringBuilder csvBuilder = new StringBuilder();
                        csvBuilder.AppendLine("FotoID,Product,Aantal,PrijsPerStuk,Subtotaal");
                        
                        // Add each product as a CSV row
                        foreach (OrderedProduct product in _orderedProducts)
                        {
                            csvBuilder.AppendLine($"{product.PhotoId},\"{product.ProductName}\",{product.Quantity},{product.UnitPrice:F2},{product.TotalPrice:F2}");
                        }
                        
                        // Add total as the last row
                        csvBuilder.AppendLine($"TOTAAL,,,{_totalPrice:F2}");
                        
                        // Write to the selected file
                        File.WriteAllText(saveFileDialog.FileName, csvBuilder.ToString());
                    }
                    
                    MessageBox.Show($"Bon geëxporteerd naar:\n{saveFileDialog.FileName}", 
                        "Bon geëxporteerd", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij exporteren: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
