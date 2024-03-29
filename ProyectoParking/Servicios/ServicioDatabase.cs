﻿using Microsoft.Data.Sqlite;
using ProyectoParking.ClasesModelo;
using ProyectoParking.servicios;
using ProyectoParking.vistamodelo;
using ProyectoParking.vistas;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProyectoParking.Servicios
{
    /// <summary>
    /// Clase que gestiona la base de datos en SQLite
    /// </summary>
    static class ServicioDatabase
    {
        //Instalar el paquete NuGet Microsoft.Data.Sqlite

        //Crea una conexión al fichero de base de datos parking.db
        //Si no existe, lo creará
        static readonly SqliteConnection conexion = new SqliteConnection(Properties.Settings.Default.RutaConexionDatabase);
 
        /// <summary>
        /// Crea la conexión con la base de datos y la crea si no existe y si la crea añade unos datos de ejemplo
        /// </summary>
        public static void ConnectDatabase()
        {
            //Abre la conexión con la base de datos
            conexion.Open();

            //Creamos una tabla utilizando un comando
            SqliteCommand comando = conexion.CreateCommand();

            // Creación de tablas si no existen
            comando.CommandText = @"CREATE TABLE IF NOT EXISTS clientes (
                                        id_cliente INTEGER PRIMARY KEY AUTOINCREMENT,
                                        nombre     TEXT,
                                        documento  TEXT    NOT NULL,
                                        foto       TEXT,
                                        edad       INTEGER,
                                        genero     TEXT,
                                        telefono   TEXT)";
            comando.ExecuteNonQuery();

            comando.CommandText = @"CREATE TABLE IF NOT EXISTS vehiculos (
                                        id_vehiculo INTEGER PRIMARY KEY AUTOINCREMENT,
                                        id_cliente  INTEGER NOT NULL REFERENCES clientes (id_cliente),
                                        matricula   TEXT    NOT NULL,
                                        id_marca    INTEGER REFERENCES marcas (id_marca),
                                        modelo      TEXT,
                                        tipo        TEXT    NOT NULL)";
            comando.ExecuteNonQuery();

            comando.CommandText = @"CREATE TABLE IF NOT EXISTS marcas (
                                        id_marca INTEGER PRIMARY KEY AUTOINCREMENT,
                                        marca TEXT NOT NULL)";
            comando.ExecuteNonQuery();

            comando.CommandText = @"CREATE TABLE IF NOT EXISTS estacionamientos (
                                        id_estacionamiento INTEGER PRIMARY KEY AUTOINCREMENT,
                                        id_vehiculo INTEGER REFERENCES vehiculos (id_vehiculo),
                                        matricula TEXT NOT NULL,
                                        entrada TEXT NOT NULL,
                                        salida TEXT, 
                                        importe REAL, 
                                        tipo TEXT NOT NULL)";

            comando.ExecuteNonQuery();

            // Comprobamos si las tablas están vacías y si lo están les añadimos datos de ejemplo
            comando.CommandText = "SELECT COUNT(*) FROM clientes";
            int resultado = Convert.ToInt32(comando.ExecuteScalar());
            if (resultado < 0) InsertarDatosClientes();

            comando.CommandText = "SELECT COUNT(*) FROM vehiculos";
            int resultado2 = Convert.ToInt32(comando.ExecuteScalar());
            if (resultado2 < 0) InsertarDatosVehiculos();

            //Cerramos la conexión
            conexion.Close();
        }

        #region MetodosVehiculos
        // Métodos para Insertar, Editar y Borrar un vehículo

        /// <summary>
        /// Método para insertar un vehiculo
        /// </summary>
        /// <param name="vehiculo">
        /// El vehiculo a insertar
        /// </param>
        public static void InsertarVehiculo(Vehiculo vehiculo)
        {
            conexion.Open();
            SqliteCommand comando = conexion.CreateCommand();
            comando.CommandText = "insert into vehiculos (id_cliente, matricula, id_marca, modelo, tipo) VALUES (@id_cliente, @matricula, @id_marca, @modelo, @tipo);";
            comando.Parameters.Add("@id_cliente", SqliteType.Integer);
            comando.Parameters.Add("@matricula", SqliteType.Text);
            comando.Parameters.Add("@id_marca", SqliteType.Integer);
            comando.Parameters.Add("@modelo", SqliteType.Text);
            comando.Parameters.Add("@tipo", SqliteType.Text);

            comando.Parameters["@id_cliente"].Value = vehiculo.IdCliente;
            comando.Parameters["@matricula"].Value = vehiculo.Matricula;
            comando.Parameters["@id_marca"].Value = vehiculo.IdMarca;
            comando.Parameters["@modelo"].Value = vehiculo.Modelo;
            comando.Parameters["@tipo"].Value = vehiculo.Tipo;

            comando.ExecuteNonQuery();

            //Cerramos la conexión
            conexion.Close();
        }

        /// <summary>
        /// Método para editar un vehiculo
        /// </summary>
        /// <param name="vehiculo">
        /// El vehiculo a editar
        /// </param>
        public static void EditarVehiculo(Vehiculo vehiculo)
        {
            conexion.Open();
            SqliteCommand comando = conexion.CreateCommand();

            comando.CommandText = "UPDATE vehiculos" +
                                  "SET id_cliente = '" + vehiculo.IdCliente + "'," +
                                       "matricula = '" + vehiculo.Matricula + "'," +
                                       "id_marca = '" + vehiculo.IdMarca + "'," +
                                       "modelo = '" + vehiculo.Modelo + "'," +
                                       "tipo = '" + vehiculo.Tipo + "'" +
                                   "WHERE id_vehiculo =" + vehiculo.IdVehiculo;
            comando.ExecuteNonQuery();

            //Cerramos la conexión
            conexion.Close();
        }

        /// <summary>
        /// Método para eliminar un vehiculo
        /// </summary>
        /// <param name="vehiculo"></param>
        public static void EliminarVehiculo(Vehiculo vehiculo)
        {
            conexion.Open();

            try
            {
                SqliteCommand comando = conexion.CreateCommand();

                comando.CommandText = "DELETE FROM vehiculos " +
                   "WHERE id_vehiculo =" + vehiculo.IdVehiculo;
                comando.ExecuteNonQuery();
            }
            catch (NullReferenceException)
            {
                ServicioDialogos.ServicioMessageBox("Tienes que seleccionar un vehículo para eliminar", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //Cerramos la conexión
            conexion.Close();
        }

        /// <summary>
        /// Método para recoger todos los vehiculos de la base de datos
        /// </summary>
        /// <returns>
        /// Devuelve una ObservableCollection con todos los vehiulos
        /// </returns>
        public static ObservableCollection<Vehiculo> GetVehiculos()
        {
            conexion.Open();
            SqliteCommand comando = conexion.CreateCommand();
            comando.CommandText = "SELECT * FROM vehiculos";
            SqliteDataReader lector = comando.ExecuteReader();
            ObservableCollection<Vehiculo> listaVehiculos = new ObservableCollection<Vehiculo>();
            if (lector.HasRows)
            {
                while (lector.Read())
                {
                    int idVehiculo = lector.GetInt32(0);
                    int idCliente = lector.GetInt32(1);
                    string matricula = lector.GetString(2);
                    int idMarca = lector.GetInt32(3);
                    string modelo = lector.GetString(4);
                    string tipo = lector.GetString(5);
                    listaVehiculos.Add(new Vehiculo(idVehiculo, idCliente, matricula, idMarca, modelo, tipo));
                }
            }

            lector.Close();

            //Cerramos la conexión
            conexion.Close();

            return listaVehiculos;
        }


        /// <summary>
        /// Inserta unos datos de ejemplo en la tabla de vehiculos
        /// </summary>
        public static void InsertarDatosVehiculos()
        {
            conexion.Open();
            SqliteCommand comando = conexion.CreateCommand();
            comando.CommandText = "insert into vehiculos (id_cliente, matricula, id_marca, modelo, tipo) values (1, 'WBAPK5G59BN204645', 1, 'Avalon', 'Female');" +
                                  "insert into vehiculos (id_cliente, matricula, id_marca, modelo, tipo) values (2, 'WAUDF78E88A934340', 2, 'Eclipse', 'Female');" +
                                  "insert into vehiculos (id_cliente, matricula, id_marca, modelo, tipo) values (2, 'WAUCFAFH3BN051266', 3, 'Continental GT', 'Female');" +
                                  "insert into vehiculos (id_cliente, matricula, id_marca, modelo, tipo) values (3, 'WBA3N3C54FF518798', 4, 'Sierra 3500', 'Male');" +
                                  "insert into vehiculos (id_cliente, matricula, id_marca, modelo, tipo) values (5, '1FAHP2DW5BG954722', 5, 'Camry Solara', 'Female');";
            comando.ExecuteNonQuery();
            conexion.Close();
        }
        #endregion

        #region MetodosClientes

        /// <summary>
        /// Inserta datos de ejemplo en la tabla de Clientes
        /// </summary>
        public static void InsertarDatosClientes()
        {
            conexion.Open();
            SqliteCommand comando = conexion.CreateCommand();
            comando.CommandText = "insert into clientes (nombre, documento, foto, edad, genero, telefono) values ('Harriott', '106-92-6159', 'hcartin13@networkadvertising.org', 'Progressive tangible interface', 'Female', '2452140252');" +
                                  "insert into clientes (nombre, documento, foto, edad, genero, telefono) values ('Jocko', '271-78-8979', 'jlumm12@vimeo.com', 'Horizontal grid-enabled hierarchy', 'Male', '4863414850');" +
                                  "insert into clientes (nombre, documento, foto, edad, genero, telefono) values ('Ofilia', '642-61-2726', 'ocords11@so-net.ne.jp', 'Universal human-resource adapter', 'Genderqueer', '8296305047');" +
                                  "insert into clientes (nombre, documento, foto, edad, genero, telefono) values ('Rubie', '446-86-1623', 'rplevey10@bloomberg.com', 'Cloned full-range Graphic Interface', 'Genderfluid', '2009434005');" +
                                  "insert into clientes (nombre, documento, foto, edad, genero, telefono) values ('Pollyanna', '722-55-9858', 'pshippeyz@fotki.com', 'Profound motivating knowledge user', 'Female', '9252836688');";
            comando.ExecuteNonQuery();
            conexion.Close();
        }



        //metodos insertar, editar y borrrar un cliente
        /// <summary>
        /// Inserta un cliente en la BD
        /// </summary>
        /// <param name="cliente">
        /// El cliente a insertar
        /// </param>
        public static void InsertarCliente(Cliente cliente)
        {
            conexion.Open();
            SqliteCommand comando = conexion.CreateCommand();
            comando.CommandText = "insert into clientes (nombre, documento, foto, edad, genero, telefono) VALUES (@nombre, @documento, @foto, @edad, @genero, @telefono);";
            comando.Parameters.Add("@nombre", SqliteType.Text);
            comando.Parameters.Add("@documento", SqliteType.Text);
            comando.Parameters.Add("@foto", SqliteType.Text);
            comando.Parameters.Add("@edad", SqliteType.Integer);
            comando.Parameters.Add("@genero", SqliteType.Text);
            comando.Parameters.Add("@telefono", SqliteType.Text);

            comando.Parameters["@nombre"].Value = cliente.Nombre;
            comando.Parameters["@documento"].Value = cliente.Documento;
            comando.Parameters["@foto"].Value = cliente.Foto;
            comando.Parameters["@edad"].Value = cliente.Edad;
            comando.Parameters["@genero"].Value = cliente.Genero;
            comando.Parameters["@telefono"].Value = cliente.Telefono;

            comando.ExecuteNonQuery();

            //Cerramos la conexión
            conexion.Close();
        }

        /// <summary>
        /// Edita un cliente de la BD
        /// </summary>
        /// <param name="cliente">
        /// El cliente a editar
        /// </param>
        public static void EditarCliente(Cliente cliente)
        {
            conexion.Open();

            SqliteCommand comando = conexion.CreateCommand();

            comando.CommandText = "UPDATE clientes " +
                                  "SET id_cliente = '" + cliente.IdCliente + "'," +
                                       "nombre = '" + cliente.Nombre + "'," +
                                       "documento = '" + cliente.Documento + "'," +
                                       "foto = '" + cliente.Foto + "'," +
                                       "edad = '" + cliente.Edad + "'," +
                                       "genero = '" + cliente.Genero + "'," +
                                       "telefono = '" + cliente.Telefono + "'" +
                                   "WHERE id_cliente =" + cliente.IdCliente;
            comando.ExecuteNonQuery();

            //Cerramos la conexión
            conexion.Close();
        }


        /// <summary>
        /// Elimina in cliente de la BD
        /// </summary>
        /// <param name="cliente">
        /// El cliente a eliminar
        /// </param>
        public static void EliminarCliente(Cliente cliente)
        {
            conexion.Open();
            try
            {
                SqliteCommand comando = conexion.CreateCommand();

                comando.CommandText = "DELETE FROM clientes WHERE id_cliente = " + cliente.IdCliente;
                comando.ExecuteNonQuery();

            }
            catch (NullReferenceException)
            {
                ServicioDialogos.ServicioMessageBox("Selecciona un cliente para poder borrarlo", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //Cerramos la conexión
            conexion.Close();
        }


        /// <summary>
        /// Método que recoge todos los clientes de una BD
        /// </summary>
        /// <returns>
        /// Una ObservableCollection de clientes con todos los clientes de la BD
        /// </returns>
        public static ObservableCollection<Cliente> GetClientes()
        {
            conexion.Open();
            SqliteCommand comando = conexion.CreateCommand();
            comando.CommandText = "SELECT * FROM clientes";
            SqliteDataReader lector = comando.ExecuteReader();
            ObservableCollection<Cliente> listaClientes = new ObservableCollection<Cliente>();
            if (lector.HasRows)
            {
                while (lector.Read())
                {
                    int id = lector.GetInt32(0);
                    string nombre = lector.GetString(1);
                    string documento = lector.GetString(2);
                    string foto = lector.GetString(3);
                    int edad = lector.GetInt32(4);
                    string genero = lector.GetString(5);
                    string telefono = lector.GetString(6);
                    listaClientes.Add(new Cliente(id, nombre, documento, foto, edad, genero, telefono));
                }
            }

            lector.Close();

            //Cerramos la conexión
            conexion.Close();

            return listaClientes;
        }
        #endregion

        #region MetodosEstacionamientos

        //Metodos para gestionar la tabla de estacionamientos

        /// <summary>
        /// Método que recoge los estacionamientos de la BD
        /// </summary>
        /// <returns>
        /// Una ObservableCollection con todos los estacionamientos de la BD
        /// </returns>
        public static ObservableCollection<Estacionamiento> GetEstacionamientos()
        {
            conexion.Open();
            SqliteCommand comando = conexion.CreateCommand();
            comando.CommandText = "SELECT * FROM estacionamientos";
            SqliteDataReader lector = comando.ExecuteReader();
            ObservableCollection<Estacionamiento> listaEstacionamientos = new ObservableCollection<Estacionamiento>();
            if (lector.HasRows)
            {
                while (lector.Read())
                {
                    int id_estacionamiento = lector.GetInt32(0);
                    int id_vehiculo;
                    if (lector["id_vehiculo"] is DBNull)
                    {
                        id_vehiculo = 0;
                    }
                    else
                    {
                        id_vehiculo = lector.GetInt32(1);
                    }
                    string matricula = lector.GetString(2);
                    string entrada = lector.GetString(3);
                    string salida = lector.GetString(4);
                    double importe = lector.GetDouble(5);
                    string tipo = lector.GetString(6);
                    listaEstacionamientos.Add(new Estacionamiento(id_estacionamiento, id_vehiculo, matricula, entrada, salida, importe, tipo));
                }
            }

            lector.Close();

            //Cerramos la conexión
            conexion.Close();

            return listaEstacionamientos;
        }
        

        /// <summary>
        /// Método para eliminar un estacionamiento de la BD
        /// </summary>
        /// <param name="estacionamientos">
        /// El estacionamiento a eliminar
        /// </param>
        public static void EliminarEstacionamiento(Estacionamiento estacionamientos)
        {
            conexion.Open();
            SqliteCommand comando = conexion.CreateCommand();

            comando.CommandText = "DELETE FROM estacionamientos WHERE id_estacionamientos = " + estacionamientos.Id_estacionamientos;
            comando.ExecuteNonQuery();

            //Cerramos la conexion
            conexion.Close();
        }


        /// <summary>
        /// Método para editar un estacionamiento
        /// </summary>
        /// <param name="estacionamiento">
        /// El estacionamiento a editar
        /// </param>
        public static void EditarEstacionamiento(Estacionamiento estacionamiento)
        {
            conexion.Open();
            SqliteCommand comando = conexion.CreateCommand();

            comando.CommandText = "UPDATE estacionamientos " +
                                  "SET id_estacionamiento = '" + estacionamiento.Id_estacionamientos + "'," +
                                       "id_vehiculo = '" + estacionamiento.Id_vehiculo + "'," +
                                       "matricula = '" + estacionamiento.Matricula + "'," +
                                       "entrada = '" + estacionamiento.Entrada + "'," +
                                       "salida = '" + estacionamiento.Salida + "'," +
                                       "importe = '" + estacionamiento.Importe + "'," +
                                       "tipo = '" + estacionamiento.Tipo + "'" +
                                   "WHERE id_estacionamiento =" + estacionamiento.Id_estacionamientos;
            comando.ExecuteNonQuery();

            //Cerramos la conexión
            conexion.Close();
        }


        /// <summary>
        /// Método para añadir un estacionamiento a la BD
        /// </summary>
        /// <param name="estacionamiento">
        /// El estacionamiento a añadir
        /// </param>
        public static void InsertarEstacionamiento(Estacionamiento estacionamiento)
        {
            conexion.Open();
            SqliteCommand comando = conexion.CreateCommand();
            comando.CommandText = "insert into estacionamientos (id_estacionamiento, id_vehiculo, matricula, entrada, salida, importe, tipo) VALUES (@id_estacionamiento, @id_vehiculo, @matricula, @entrada, @salida, importe, tipo);";
            comando.Parameters.Add("@id_estacionamiento", SqliteType.Integer);
            comando.Parameters.Add("@id_vehiculo", SqliteType.Integer);
            comando.Parameters.Add("@matricula", SqliteType.Text);
            comando.Parameters.Add("@entrada", SqliteType.Text);
            comando.Parameters.Add("@salida", SqliteType.Text);
            comando.Parameters.Add("@importe", SqliteType.Real);
            comando.Parameters.Add("@tipo", SqliteType.Text);

            comando.Parameters["@id_estacionamiento"].Value = estacionamiento.Id_estacionamientos;
            comando.Parameters["@id_vehiculo"].Value = estacionamiento.Id_vehiculo;
            comando.Parameters["@matricula"].Value = estacionamiento.Matricula;
            comando.Parameters["@entrada"].Value = estacionamiento.Entrada;
            comando.Parameters["@salida"].Value = estacionamiento.Salida;
            comando.Parameters["@importe"].Value = estacionamiento.Importe;
            comando.Parameters["@tipo"].Value = estacionamiento.Tipo;

            comando.ExecuteNonQuery();

            //Cerramos la conexión
            conexion.Close();
        }

        #endregion
    }
}
