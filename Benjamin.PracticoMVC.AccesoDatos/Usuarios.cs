﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Data.SqlClient;
using Dapper;
using System.Security.Cryptography;


namespace Benjamin.PracticoMVC.AccesoDatos
{
    public class Usuarios
    {
        string cadenaConexion = Conexiones.ObtenerCadenaConexion();

        public List<Entidades.Join_UsuariosRoles> Listar()
        {
            List<Entidades.Join_UsuariosRoles> listaUsuariosRoles = new List<Entidades.Join_UsuariosRoles>();

            StringBuilder consultaSQL = new StringBuilder();

            /*
             
SELECT  
Usuarios.Id AS ID, 
Usuario AS USUARIO, 
Roles.Descripcion AS ROL, 
Nombre AS NOMBRES, 
Apellido AS APELLIDOS, 
FechaCreacion AS FECHA_ALTA,
Activo AS ESTADO
FROM Usuarios
INNER JOIN Roles ON 
Usuarios.IdRol = Roles.Id


             */

            consultaSQL.Append("SELECT ");
            consultaSQL.Append("Usuarios.Id AS ID, ");
            consultaSQL.Append("Usuario AS USUARIO, ");
            consultaSQL.Append("Roles.Descripcion AS ROL, ");
            consultaSQL.Append("Nombre AS NOMBRES, ");
            consultaSQL.Append("Apellido AS APELLIDOS, ");
            consultaSQL.Append("FechaCreacion AS FECHA_ALTA, ");
            consultaSQL.Append("Activo AS ESTADO ");
            consultaSQL.Append("FROM Usuarios ");
            consultaSQL.Append("INNER JOIN Roles ON  ");
            consultaSQL.Append("Usuarios.IdRol = Roles.Id ");


            using (var connection = new SqlConnection(cadenaConexion))
            {
                listaUsuariosRoles = connection.Query<Entidades.Join_UsuariosRoles>(consultaSQL.ToString()).ToList();
            }

            return listaUsuariosRoles;
        }

        public Entidades.Usuarios Detalle(int id)
        {
            StringBuilder consultaSQL = new StringBuilder();

            consultaSQL.Append("SELECT ");
            consultaSQL.Append("Id, IdRol, Usuario, Nombre, Apellido, Password, PasswordSalt, FechaCreacion, Activo ");
            consultaSQL.Append("FROM Usuarios ");
            consultaSQL.Append("WHERE Id = @idParametro ");


            using (var connection = new SqlConnection(cadenaConexion))
            {
                var objUsuario = connection.QuerySingleOrDefault<Entidades.Usuarios>(consultaSQL.ToString(), new { idParametro = id });


                return objUsuario;
            }

        }

        public void Crear(Entidades.Usuarios objEntidad)
        {
            //INSERT INTO Usuarios(IdRol, Usuario, Nombre, Apellido, Password, PasswordSalt, FechaCreacion, Activo)
            //VALUES('CLI', 'drodriguez', 'David', 'Rodriguez', 'password', 'passwordSalt', GETDATE(), 0);

            //cuando creamos el usuario, la contraseña es la misma que el nombre de usuario. 
            // En el primer logueo le pedira el cambio de la misma
            objEntidad.Password = objEntidad.Usuario;

            //generamos password salt para guardar en la base
            objEntidad.PasswordSalt = GenerarPasswordSalt(objEntidad.Password);

            //generamos Password hash ya encriptada, para que solo el usuario sepa la password
            objEntidad.Password = GenerarPasswordHash(objEntidad.Password, objEntidad.PasswordSalt);


            StringBuilder consultaSQL = new StringBuilder();

            consultaSQL.Append("INSERT INTO Usuarios(IdRol, Usuario, Nombre, Apellido, Password, PasswordSalt, FechaCreacion, Activo)  ");
            consultaSQL.Append("VALUES(@IdRol, @Usuario, @Nombre, @Apellido, @Password, @PasswordSalt, @FechaCreacion, @Activo); ");

            using (var connection = new SqlConnection(cadenaConexion))
            {
                var filasAfectadas = connection.Execute(consultaSQL.ToString(),
                    new
                    {
                        IdRol = objEntidad.IdRol,
                        Usuario = objEntidad.Usuario,
                        Nombre = objEntidad.Nombre,
                        Apellido = objEntidad.Apellido,
                        Password = objEntidad.Password,
                        PasswordSalt = objEntidad.PasswordSalt,
                        FechaCreacion = DateTime.Now,
                        Activo = objEntidad.Activo
                    });


            }



        }


        public static string GenerarPasswordSalt(string password)
        {
            string passwordSalt;

            // generar un salt de 128-bit usando PRNG seguro
            byte[] salt = new byte[128 / 8];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            //convertir en string base 64 bits
            passwordSalt = Convert.ToBase64String(salt);

            return passwordSalt;
        }


        public static string GenerarPasswordHash(string password, string salt, string hashingAlgorithm = "HMACSHA256")
        {
            byte[] passwordBytes = Encoding.Unicode.GetBytes(password);
            byte[] saltBytes = Convert.FromBase64String(salt);
            var saltyPasswordBytes = new byte[saltBytes.Length + passwordBytes.Length];

            Buffer.BlockCopy(saltBytes, 0, saltyPasswordBytes, 0, saltBytes.Length);
            Buffer.BlockCopy(passwordBytes, 0, saltyPasswordBytes, saltBytes.Length, passwordBytes.Length);

            switch (hashingAlgorithm)
            {
                case "HMACSHA256":
                    return Convert.ToBase64String(new HMACSHA256(saltBytes).ComputeHash(saltyPasswordBytes));
                default:
                    // Supported types include: SHA1, MD5, SHA256, SHA384, SHA512
                    HashAlgorithm algorithm = HashAlgorithm.Create(hashingAlgorithm);

                    if (algorithm != null)
                    {
                        return Convert.ToBase64String(algorithm.ComputeHash(saltyPasswordBytes));
                    }

                    throw new CryptographicException("Unknown hash algorithm");
            }
        }


        public void ActualizarPassword(Entidades.Usuarios objUsuario)
        {
            /*
               UPDATE Usuarios
               SET PasswordSalt = 'CLAVE SALADA', Password = 'CLAVE'
               WHERE Usuario LIKE 'mperez';
            */

            //generamos password salt para guardar en la base
            objUsuario.PasswordSalt = GenerarPasswordSalt(objUsuario.Password);

            //generamos Password hash ya encriptada, para que solo el usuario sepa la password
            objUsuario.Password = GenerarPasswordHash(objUsuario.Password, objUsuario.PasswordSalt);


            StringBuilder consultaSQL = new StringBuilder();

            consultaSQL.Append("UPDATE Usuarios ");
            consultaSQL.Append("SET PasswordSalt = @PasswordSalt, Password = @Password ");
            consultaSQL.Append("WHERE Usuario LIKE @Usuario ;");

            using (var connection = new SqlConnection(cadenaConexion))
            {
                var filasAfectadas = connection.Execute(consultaSQL.ToString(),
                    new
                    {
                        Usuario = objUsuario.Usuario,
                        Password = objUsuario.Password,
                        PasswordSalt = objUsuario.PasswordSalt
                    });


            }
        }


























        public bool ConfirmarEliminacion(object id)
        {
            throw new NotImplementedException();
        }

        
        public void Desechar()
        {
            throw new NotImplementedException();
        }

        public void Deshabilitar(object id)
        {
            throw new NotImplementedException();
        }


        public void Editar(Entidades.Usuarios objEntidad)
        {
            throw new NotImplementedException();
        }

        public void Eliminar(object id)
        {
            throw new NotImplementedException();
        }

        public void Guardar()
        {
            throw new NotImplementedException();
        }


    }
}
