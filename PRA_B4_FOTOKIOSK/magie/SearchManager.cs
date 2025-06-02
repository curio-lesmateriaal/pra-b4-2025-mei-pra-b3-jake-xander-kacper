using PRA_B4_FOTOKIOSK.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PRA_B4_FOTOKIOSK;

namespace PRA_B4_FOTOKIOSK.magie
{
    /// <summary>
    /// Manages search functionality and photo display in the application
    /// </summary>
    public class SearchManager
    {
        private static Home? _instance;
        public static Home Instance 
        { 
            get => _instance ?? throw new InvalidOperationException("Instance not initialized");
            set => _instance = value;
        }

        /// <summary>
        /// Clears the current photo display and resets it to its original state
        /// </summary>
        public static void ClearPictureDisplay()
        {
            // Clear the image source
            Instance.ImageBig.Source = null;

            // Get the parent container
            var parent = Instance.ImageBig.Parent as FrameworkElement;
            
            if (parent is Border border)
            {
                // Reset the border's child back to the image
                border.Child = Instance.ImageBig;
            }
            else if (parent is Grid grid)
            {
                // Clear any grid content
                grid.Children.Clear();
                grid.Children.Add(Instance.ImageBig);
            }
            else if (parent is Panel panel)
            {
                // Clear any panel content
                panel.Children.Clear();
                panel.Children.Add(Instance.ImageBig);
            }

            // Ensure image is visible and properly configured
            Instance.ImageBig.Visibility = Visibility.Visible;
            Instance.ImageBig.Stretch = Stretch.Uniform;
            Instance.ImageBig.HorizontalAlignment = HorizontalAlignment.Center;
            Instance.ImageBig.VerticalAlignment = VerticalAlignment.Center;
        }

        /// <summary>
        /// Sets the main image display to show the image at the given path
        /// </summary>
        /// <param name="path">Path to the image file</param>
        public static void SetPicture(string path)
        {
            ClearPictureDisplay(); // Always clear first
            Instance.ImageBig.Source = PathToImage(path);
        }

        /// <summary>
        /// Converts a file path to a BitmapImage
        /// </summary>
        public static BitmapImage PathToImage(string path)
        {
            var stream = new MemoryStream(File.ReadAllBytes(path));
            var img = new BitmapImage();
            img.BeginInit();
            img.StreamSource = stream;
            img.EndInit();
            return img;
        }

        /// <summary>
        /// Sets the display to show a grid of photos or other UI elements
        /// </summary>
        public static void SetPictureGrid(UIElement gridElement)
        {
            // First clear any existing content
            ClearPictureDisplay();
            
            // Get the parent container
            var parent = Instance.ImageBig.Parent as FrameworkElement;
            
            if (parent is Border border)
            {
                border.Child = gridElement;
            }
            else if (parent is Panel panel)
            {
                panel.Children.Clear();
                panel.Children.Add(gridElement);
            }

            // Hide the original image when showing grid
            Instance.ImageBig.Visibility = Visibility.Collapsed;
        }

        // Helper methods for search interface

        public static string GetSearchInput()
        {
            return Instance.SearchTextBox.Text ?? string.Empty;
        }

        public static void SetSearchImageInfo(string text)
        {
            Instance.SearchInfoLabel.Content = text;
        }

        public static string GetSearchImageInfo()
        {
            return (string)(Instance.SearchInfoLabel.Content ?? string.Empty);
        }

        public static void AddSearchImageInfo(string text)
        {
            SetSearchImageInfo(GetSearchImageInfo() + text);
        }
    }
}
