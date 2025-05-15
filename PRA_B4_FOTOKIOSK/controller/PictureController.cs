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
            // Get current day number (0 = Sunday through 6 = Saturday)
            var now = DateTime.Now;
            int day = (int)now.DayOfWeek;

            // Initializeer de lijst met fotos
            foreach (string dir in Directory.GetDirectories(@"../../../fotos"))
            {
                // Extract the day number from directory name (e.g., "0_Zondag" -> 0)
                string dayNumber = Path.GetFileName(dir).Split('_')[0];
                if (int.Parse(dayNumber) == day)
                {
                    foreach (string file in Directory.GetFiles(dir))
                    {
                        PicturesToDisplay.Add(new KioskPhoto() { Id = 0, Source = file });
                    }
                }
            }

            // Update de fotos
            PictureManager.UpdatePictures(PicturesToDisplay);
        }

        // Wordt uitgevoerd wanneer er op de Refresh knop is geklikt
        public void RefreshButtonClick()
        {

        }

    }
}
