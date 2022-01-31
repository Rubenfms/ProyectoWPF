﻿using ProyectoParking.vistamodelo;
using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProyectoParking.vistas
{
    /// <summary>
    /// Lógica de interacción para Estacionamientos.xaml
    /// </summary>
    public partial class Estacionamientos : UserControl
    {
        private EstacionamientosVM vm = new EstacionamientosVM();
        public Estacionamientos()
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}