﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoParking.ClasesModelo
{
    class Cliente: ObservableObject
    {
        private int idCliente;

        public int IdCliente
        {
            get { return idCliente; }
            set { idCliente = value; }
        }

    }
}
