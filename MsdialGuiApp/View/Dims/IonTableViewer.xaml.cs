﻿using System;
using System.Collections.Generic;
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

namespace CompMs.App.Msdial.View.Dims
{
    /// <summary>
    /// Interaction logic for IonTableViewer.xaml
    /// </summary>
    public partial class IonTableViewer : Window
    {
        public IonTableViewer() {
            InitializeComponent();
        }

        private void DataGrid_Unloaded(object sender, RoutedEventArgs e) {
            var grid = (DataGrid)sender;
            grid.CommitEdit();
        }
    }
}