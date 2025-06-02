using PRA_B4_FOTOKIOSK.magie;
using PRA_B4_FOTOKIOSK.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text;

namespace PRA_B4_FOTOKIOSK.controller
{
    public class SearchController
    {
        // Reference to the main window
        public static Home Window { get; set; }
        
        // List of all available photos
        private List<KioskPhoto> _allPhotos = new List<KioskPhoto>();

        // Start method called when the search page opens
        public void Start()
        {
            SearchManager.SetSearchImageInfo("Enter a photo ID and click Search to find your photos.");
            LoadAllPhotos();
        }
        
        // Loads all available photos from the photos directory
        private void LoadAllPhotos()
        {
            _allPhotos.Clear();
            
            try
            {
                string baseDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../fotos"));
                
                foreach (string dir in Directory.GetDirectories(baseDir))
                {
                    foreach (string file in Directory.GetFiles(dir))
                    {
                        // Extract time and ID information from filename
                        // Format: hour_minute_second_id.jpg
                        string filename = Path.GetFileName(file);
                        string[] parts = filename.Split('_');
                        
                        if (parts.Length >= 4)
                        {
                            string idWithExtension = parts[3];
                            string idString = idWithExtension.Split('.')[0];
                            
                            if (idString.StartsWith("id") && int.TryParse(idString.Substring(2), out int photoId))
                            {
                                _allPhotos.Add(new KioskPhoto
                                {
                                    Id = photoId,
                                    Source = file
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading photos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // Called when the Search button is clicked
        public void SearchButtonClick()
        {
            // Reset the display before starting a new search
            SearchManager.ClearPictureDisplay();
            
            string searchInput = SearchManager.GetSearchInput();
            
            if (string.IsNullOrWhiteSpace(searchInput))
            {
                MessageBox.Show("Please enter a valid photo ID to search.", 
                                "No ID", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (!int.TryParse(searchInput, out int searchId))
            {
                MessageBox.Show("Please enter a numeric photo ID.", 
                                "Invalid ID", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            KioskPhoto mainPhoto = _allPhotos.FirstOrDefault(p => p.Id == searchId);
            
            if (mainPhoto == null)
            {
                MessageBox.Show($"No photo found with ID {searchId}.", 
                                "Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            // Find matching photo taken 60 seconds apart
            bool foundPair = FindAndDisplayPhotoPair(mainPhoto);
            if (!foundPair)
            {
                // For single photo display, make sure we clean up first
                SearchManager.ClearPictureDisplay();
                SearchManager.SetPicture(mainPhoto.Source);
                
                // Extract time and date from the photo
                string filename = Path.GetFileName(mainPhoto.Source);
                string directoryName = Path.GetFileName(Path.GetDirectoryName(mainPhoto.Source) ?? "");
                string[] parts = filename.Split('_');
                string timeInfo = "";
                string dayInfo = "";
                
                // Get day information from directory name (e.g., "0_Zondag")
                if (directoryName.Contains("_"))
                {
                    string[] dirParts = directoryName.Split('_');
                    if (dirParts.Length == 2)
                    {
                        dayInfo = dirParts[1]; // Gets "Zondag", "Maandag", etc.
                    }
                }
                
                if (parts.Length >= 3 && 
                    int.TryParse(parts[0], out int hour) && 
                    int.TryParse(parts[1], out int minute) && 
                    int.TryParse(parts[2], out int second))
                {
                    timeInfo = $"{hour:D2}:{minute:D2}:{second:D2}";
                }
                
                var photoInfo = new StringBuilder();
                photoInfo.AppendLine($"Photo Information:");
                photoInfo.AppendLine($"ID: {mainPhoto.Id}");
                photoInfo.AppendLine($"Time: {timeInfo}");
                if (!string.IsNullOrEmpty(dayInfo))
                {
                    photoInfo.AppendLine($"Day: {dayInfo}");
                }
                photoInfo.AppendLine();
                photoInfo.AppendLine("No matching photo found that was taken 60 seconds earlier or later.");
                photoInfo.AppendLine("Try searching for other photo IDs from your ride.");
                
                SearchManager.SetSearchImageInfo(photoInfo.ToString());
            }
        }
        
        // Finds and displays a pair of photos (taken 60 seconds apart)
        private bool FindAndDisplayPhotoPair(KioskPhoto mainPhoto)
        {
            string filename = Path.GetFileName(mainPhoto.Source);
            string[] parts = filename.Split('_');
            
            if (parts.Length < 3)
                return false;
                
            if (!int.TryParse(parts[0], out int hour) || 
                !int.TryParse(parts[1], out int minute) || 
                !int.TryParse(parts[2], out int second))
            {
                return false;
            }
            
            var photoDateTime = new DateTime(2025, 1, 1, hour, minute, second);
            var pairedPhotos = new List<KioskPhoto>();
            
            foreach (var photo in _allPhotos)
            {
                if (photo.Id == mainPhoto.Id)
                    continue;
                    
                string otherFilename = Path.GetFileName(photo.Source);
                string[] otherParts = otherFilename.Split('_');
                
                if (otherParts.Length < 3)
                    continue;
                    
                if (!int.TryParse(otherParts[0], out int otherHour) || 
                    !int.TryParse(otherParts[1], out int otherMinute) || 
                    !int.TryParse(otherParts[2], out int otherSecond))
                {
                    continue;
                }
                
                var otherDateTime = new DateTime(2025, 1, 1, otherHour, otherMinute, otherSecond);
                double diffSeconds = Math.Abs((photoDateTime - otherDateTime).TotalSeconds);
                
                // Check if the photos are exactly 60 seconds apart (with a small margin for error)
                if (Math.Abs(diffSeconds - 60) < 2)
                {
                    pairedPhotos.Add(photo);
                }
            }
              if (pairedPhotos.Count == 0)
                return false;
            
            // If multiple matching photos found, select the one with the closest time difference to 60 seconds
            if (pairedPhotos.Count > 1)
            {
                pairedPhotos = pairedPhotos.OrderBy(photo => {
                    string otherFilename = Path.GetFileName(photo.Source);
                    string[] otherParts = otherFilename.Split('_');
                    
                    int otherHour = int.Parse(otherParts[0]);
                    int otherMinute = int.Parse(otherParts[1]);
                    int otherSecond = int.Parse(otherParts[2]);
                    
                    var otherDateTime = new DateTime(2025, 1, 1, otherHour, otherMinute, otherSecond);
                    return Math.Abs(Math.Abs((photoDateTime - otherDateTime).TotalSeconds) - 60);
                }).ToList();
            }
                
            var pairedPhoto = pairedPhotos.First();
            DisplayPhotosInOrder(mainPhoto, pairedPhoto);
            
            return true;
        }
        
        // Display photos in chronological order (earliest first)
        private void DisplayPhotosInOrder(KioskPhoto photo1, KioskPhoto photo2)
        {
            string[] parts1 = Path.GetFileName(photo1.Source).Split('_');
            string[] parts2 = Path.GetFileName(photo2.Source).Split('_');
            
            int hour1 = int.Parse(parts1[0]);
            int minute1 = int.Parse(parts1[1]);
            int second1 = int.Parse(parts1[2]);
            
            int hour2 = int.Parse(parts2[0]);
            int minute2 = int.Parse(parts2[1]);
            int second2 = int.Parse(parts2[2]);
            
            var time1 = new TimeSpan(hour1, minute1, second1);
            var time2 = new TimeSpan(hour2, minute2, second2);
            
            // Determine which photo comes first chronologically
            KioskPhoto firstPhoto, secondPhoto;
            if (time1.CompareTo(time2) < 0)
            {
                firstPhoto = photo1;
                secondPhoto = photo2;
            }
            else
            {
                firstPhoto = photo2;
                secondPhoto = photo1;
            }
            
            var photoPairContainer = new Grid();
            photoPairContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            photoPairContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            photoPairContainer.ColumnDefinitions.Add(new ColumnDefinition());
            photoPairContainer.ColumnDefinitions.Add(new ColumnDefinition());
            
            // Headers for photos
            var headerLabel1 = new TextBlock
            {
                Text = $"First Photo (ID: {firstPhoto.Id})",
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 200, 0)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };
            
            var headerLabel2 = new TextBlock
            {
                Text = $"Second Photo (ID: {secondPhoto.Id})",
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 200)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };
            
            Grid.SetRow(headerLabel1, 0);
            Grid.SetColumn(headerLabel1, 0);
            Grid.SetRow(headerLabel2, 0);
            Grid.SetColumn(headerLabel2, 1);
            
            photoPairContainer.Children.Add(headerLabel1);
            photoPairContainer.Children.Add(headerLabel2);
            
            var image1 = new Image
            {
                Source = SearchManager.PathToImage(firstPhoto.Source),
                Margin = new Thickness(5),
                Stretch = Stretch.Uniform
            };
            
            // Red border for the first photo
            var border1 = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(0, 255, 0)), // green
                BorderThickness = new Thickness(6),
                Margin = new Thickness(8),
                Child = image1,
                CornerRadius = new CornerRadius(4)
            };
            
            var image2 = new Image
            {
                Source = SearchManager.PathToImage(secondPhoto.Source),
                Margin = new Thickness(5),
                Stretch = Stretch.Uniform
            };
            
            // Blue border for the second photo
            var border2 = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(0, 0, 255)), // Blue
                BorderThickness = new Thickness(6),
                Margin = new Thickness(8),
                Child = image2,
                CornerRadius = new CornerRadius(4)
            };
            
            Grid.SetRow(border1, 1);
            Grid.SetColumn(border1, 0);
            Grid.SetRow(border2, 1);
            Grid.SetColumn(border2, 1);
            
            photoPairContainer.Children.Add(border1);
            photoPairContainer.Children.Add(border2);
              // Calculate exact time difference for message
            double exactTimeDiff = GetExactTimeDifference(firstPhoto, secondPhoto);
              // Get day information
            string dir1 = Path.GetFileName(Path.GetDirectoryName(firstPhoto.Source) ?? "");
            string dir2 = Path.GetFileName(Path.GetDirectoryName(secondPhoto.Source) ?? "");
            string day1 = dir1.Contains("_") ? dir1.Split('_')[1] : "";
            string day2 = dir2.Contains("_") ? dir2.Split('_')[1] : "";

            var infoText = new StringBuilder();
            infoText.AppendLine("Paired photos found!");
            infoText.AppendLine();
            infoText.AppendLine("First Photo (green border):");
            infoText.AppendLine($"- ID: {firstPhoto.Id}");
            infoText.AppendLine($"- Time: {hour1:D2}:{minute1:D2}:{second1:D2}");
            if (!string.IsNullOrEmpty(day1)) infoText.AppendLine($"- Day: {day1}");
            infoText.AppendLine();
            infoText.AppendLine("Second Photo (blue border):");
            infoText.AppendLine($"- ID: {secondPhoto.Id}");
            infoText.AppendLine($"- Time: {hour2:D2}:{minute2:D2}:{second2:D2}");
            if (!string.IsNullOrEmpty(day2)) infoText.AppendLine($"- Day: {day2}");
            infoText.AppendLine();
            infoText.AppendLine($"These photos were taken exactly {exactTimeDiff:F1} seconds apart by the two cameras along the ride.");
            infoText.AppendLine("The photos are displayed in chronological order (earliest on the left).");
            
            SearchManager.SetSearchImageInfo(infoText.ToString());
            SearchManager.SetPictureGrid(photoPairContainer);
        }

        // Helper method to calculate the exact time difference between two photos
        private double GetExactTimeDifference(KioskPhoto photo1, KioskPhoto photo2)
        {
            string[] parts1 = Path.GetFileName(photo1.Source).Split('_');
            string[] parts2 = Path.GetFileName(photo2.Source).Split('_');
            
            int hour1 = int.Parse(parts1[0]);
            int minute1 = int.Parse(parts1[1]);
            int second1 = int.Parse(parts1[2]);
            
            int hour2 = int.Parse(parts2[0]);
            int minute2 = int.Parse(parts2[1]);
            int second2 = int.Parse(parts2[2]);
            
            var time1 = new DateTime(2025, 1, 1, hour1, minute1, second1);
            var time2 = new DateTime(2025, 1, 1, hour2, minute2, second2);
            
            return Math.Abs((time1 - time2).TotalSeconds);
        }
    }
}
