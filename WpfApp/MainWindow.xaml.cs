﻿using System.Windows;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //DataContext = new MainViewModel(); // ← ViewModel setzen
        }
    }
}
