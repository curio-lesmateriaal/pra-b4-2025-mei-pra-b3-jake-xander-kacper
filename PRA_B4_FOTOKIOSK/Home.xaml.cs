﻿using PRA_B4_FOTOKIOSK.controller;
using PRA_B4_FOTOKIOSK.magie;
using PRA_B4_FOTOKIOSK.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PRA_B4_FOTOKIOSK
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Window
    {
        // Expose controls that need to be accessed by managers
        public Image ImageBig => imgBig;
        public TextBox SearchTextBox => tbZoeken;
        public Label SearchInfoLabel => lbSearchInfo;

        public ShopController ShopController { get; set; }
        public PictureController PictureController { get; set; }
        public SearchController SearchController { get; set; }        public Home()
        {
            // Bouw de UI
            InitializeComponent();

            // Maak de controllers first
            ShopController = new ShopController();
            PictureController = new PictureController();
            SearchController = new SearchController();
            
            // Stel de manager in
            PictureManager.Instance = this;
            ShopManager.Instance = this;
            SearchManager.Instance = this;
            
            // Then set the Window references
            ShopController.Window = this;
            PictureController.Window = this;
            SearchController.Window = this;

            // Start de paginas last
            PictureController.Start();
            ShopController.Start();
            SearchController.Start();
        }

        private void btnShopAdd_Click(object sender, RoutedEventArgs e)
        {
            ShopController.AddButtonClick();
        }

        private void btnShopReset_Click(object sender, RoutedEventArgs e)
        {
            ShopController.ResetButtonClick();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            PictureController.RefreshButtonClick();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            ShopController.SaveButtonClick();
        }        private void btnZoeken_Click(object sender, RoutedEventArgs e)
        {
            SearchController.SearchButtonClick();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            ShopController.ExportButtonClick();
        }
    }
}
