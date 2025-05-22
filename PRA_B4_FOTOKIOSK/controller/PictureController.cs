using PRA_B4_FOTOKIOSK.magie;
using PRA_B4_FOTOKIOSK.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRA_B4_FOTOKIOSK.controller
{
    public class PictureController
    {
        // De window die we laten zien op het scherm
        public static Home Window { get; set; }


        // De lijst met fotos die we laten zien
        public List<KioskPhoto> PicturesToDisplay = new List<KioskPhoto>();
        
        
        // Start methode die wordt aangeroepen wanneer de foto pagina opent.
        public void Start()
        {
            // Clear the previous list of photos
            PicturesToDisplay.Clear();
            
            // Get current time
            var now = DateTime.Now;
            
            // Calculate time boundaries: between 2 and 30 minutes ago
            DateTime minTime = now.AddMinutes(-30);
            DateTime maxTime = now.AddMinutes(-2);
            
            // Current day number (0 = Sunday through 6 = Saturday)
            int day = (int)now.DayOfWeek;

            Console.WriteLine($"Current time: {now}, Showing photos between {minTime} and {maxTime}");
            
            // Initializeer de lijst met fotos
            foreach (string dir in Directory.GetDirectories(@"../../../fotos"))
            {
                // Extract the day number from directory name (e.g., "0_Zondag" -> 0)
                string dayNumber = Path.GetFileName(dir).Split('_')[0];
                if (int.Parse(dayNumber) == day)
                {
                    foreach (string file in Directory.GetFiles(dir))
                    {
                        // Extract time components from filename (format: hour_minute_second_id.jpg)
                        string filename = Path.GetFileName(file);
                        string[] parts = filename.Split('_');
                        
                        if (parts.Length >= 3)
                        {
                            // Parse hour, minute, second from filename
                            if (int.TryParse(parts[0], out int hour) && 
                                int.TryParse(parts[1], out int minute) && 
                                int.TryParse(parts[2], out int second))
                            {
                                // Create a datetime for the photo
                                DateTime photoTime = new DateTime(
                                    now.Year, now.Month, now.Day, 
                                    hour, minute, second
                                );
                                
                                // Check if the photo time is between 2 and 30 minutes ago
                                if (photoTime >= minTime && photoTime <= maxTime)
                                {
                                    Console.WriteLine($"Including photo: {filename}, time: {photoTime}");
                                    PicturesToDisplay.Add(new KioskPhoto() { Id = 0, Source = file });
                                }
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"Total photos displayed: {PicturesToDisplay.Count}");
            
            // Update de fotos
            PictureManager.UpdatePictures(PicturesToDisplay);
        }

        // Wordt uitgevoerd wanneer er op de Refresh knop is geklikt
        public void RefreshButtonClick()
        {
            // Simply call Start again to refresh the photos with the current time
            Start();
        }

    }
}
