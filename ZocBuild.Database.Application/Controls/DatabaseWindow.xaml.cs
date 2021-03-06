﻿using System;
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
using ZocBuild.Database.Application.ViewModels;
using ZocBuild.Database.SqlParser;

namespace ZocBuild.Database.Application.Controls
{
    /// <summary>
    /// Interaction logic for DatabaseWindow.xaml
    /// </summary>
    public partial class DatabaseWindow : Window
    {
        public DatabaseWindow()
        {
            InitializeComponent();
        }

        private void add_DatabaseAdded(object sender, EventArgs e)
        {
            var dbSetting = add.Result;
            ((MainWindowViewModel)DataContext).Databases.Add(dbSetting);
            ((MainWindowViewModel)DataContext).SelectedDatabase = dbSetting;
            Properties.Settings.Default.Databases.Add(dbSetting);
            Properties.Settings.Default.Save();
            DialogResult = true;
        }

        private void add_Cancelled(object sender, EventArgs e)
        {
            DialogResult = false;
        }

        private void remove_Closed(object sender, EventArgs e)
        {
            DialogResult = true;
        }
    }
}
