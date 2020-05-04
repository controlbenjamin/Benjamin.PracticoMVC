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

        public void Crear3(Entidades.Usuarios objEntidad)
        {

            //INSERT INTO Usuarios(IdRol, Usuario, Nombre, Apellido, Password, PasswordSalt, FechaCreacion, Activo)
            //VALUES('CLI', 'drodriguez', 'David', 'Rodriguez', 'password', 'passwordSalt', GETDATE(), 0);

            //cuando creamos el usuario, la contraseña es la misma que el nombre de usuario. 
            // En el primer logueo si la contraseña es igual al usuario le pedira el cambio de la misma

            //Cuando se crea por primera vez el usuario y la contraseña son las mismas asi en el proximo
            //login pide cambiarla
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

        public int Crear(Entidades.Join_UsuariosClientes obj)
        {


            int filasAfectadas = 0;

            SqlConnection conexion = new SqlConnection(cadenaConexion);


            conexion.Open();

            //como vamos a realizar dos inserciones debemos hacerlo con una transaccion
            var transaccion = conexion.BeginTransaction();


            try
            {
                //Cuando se crea por primera vez el usuario y la contraseña son las mismas 
                //asi en el proximo login pide cambiarla
                string clave = obj.USERNAME;

                //generamos password salt para guardar en la base
                string claveSalt = GenerarPasswordSalt(clave);

                //generamos Password hash ya encriptada, para que solo el usuario sepa la password
                string claveHash = GenerarPasswordHash(clave, claveSalt);


                //primer consulta que inserta un nuevo usuario admin o cliente
                StringBuilder consultaSQL1 = new StringBuilder();
                consultaSQL1.Append("INSERT INTO Usuarios(IdRol, Usuario, Nombre, Apellido, Password, PasswordSalt, FechaCreacion, Activo)  ");
                consultaSQL1.Append("VALUES(@IdRol, @Usuario, @Nombre, @Apellido, @Password, @PasswordSalt, @FechaCreacion, @Activo); ");


                filasAfectadas = conexion.Execute(consultaSQL1.ToString(),
                       new
                       {
                           IdRol = obj.ID_ROL,
                           Usuario = obj.USERNAME,
                           Nombre = obj.NOMBRES,
                           Apellido = obj.APELLIDOS,
                           Password = claveHash,
                           PasswordSalt = claveSalt,
                           FechaCreacion = DateTime.Now,
                           Activo = obj.ACTIVO
                       }
                       , transaction: transaccion);


                //solamente si el usuario es de rol cliente se realiza esta operacion extra
                if (obj.ID_ROL == "CLI")
                {

                    /////////////////////////////

                    StringBuilder consultaSQL2 = new StringBuilder();

                    //obtenemos el id usuario segun el username,
                    //este dato nos servirá luego para insertarlo en la tabla clientes
                    consultaSQL2.Append("SELECT Id FROM Usuarios ");
                    consultaSQL2.Append("WHERE Usuario LIKE @UsernameParametro ");


                    obj.ID_USUARIO = conexion.ExecuteScalar<int>(consultaSQL2.ToString(), new { UsernameParametro = obj.USERNAME }, transaction: transaccion);


                    /////////////////////////////////
                    //insertamos nuevo cliente, relacionado con el idusuario de la tabla usuarios
                    StringBuilder consultaSQL3 = new StringBuilder();
                    consultaSQL3.Append("INSERT INTO Clientes(RazonSocial, FechaCreacion, IdUsuario)  ");
                    consultaSQL3.Append("VALUES (@RazonSocialParametro, @FechaCreacionParametro, @IdUsuarioParametro )  ");


                    filasAfectadas = conexion.Execute(consultaSQL3.ToString(),
                           new
                           {
                               RazonSocialParametro = obj.RAZON_SOCIAL,
                               IdUsuarioParametro = obj.ID_USUARIO,
                               FechaCreacionParametro = DateTime.Now,

                           },
                           transaction: transaccion);
                }



                /////////////////////////////////
                // si las operaciones relacionadas salieron bien, se realiza un commit
                transaccion.Commit();
            }
            catch (Exception ex)
            {
                // en caso que haya un error en el medio de la funcion
                //lanzamos codigo de error 0 y realizamos un rollback para que los datos
                //no se reflejen en la base de datos
                filasAfectadas = 0;
                transaccion.Rollback();

            }
            finally
            {
                //si el procedimiento salio bien o mal, siempre se debe cerrar la conexion
                conexion.Close();
            }

            // si el resultado de filasafectadas es 1 es porque salio OK
            return filasAfectadas;
        }


        public static string GenerarPasswordSalt(string password)
        {
            //para que no de error si es null, se deja una cadena null
            if (password == null)
            {
                password = "(null)";
            }

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
            //para que no de error si es null, se deja una cadena null
            if (password == null || salt == null)
            {
                password = "(null)";
                salt = "(null)";
            }

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


        public int ActualizarPassword(string usuario, string claveActual)
        {
            /*
               UPDATE Usuarios
               SET PasswordSalt = 'CLAVE SALADA', Password = 'CLAVE'
               WHERE Usuario LIKE 'mperez';
            */

            int filasAfectadas = 0;

            //generamos password salt para guardar en la base
            string passwordSalt = GenerarPasswordSalt(claveActual);

            //generamos Password hash ya encriptada, para que solo el usuario sepa la password
            string passwordHash = GenerarPasswordHash(claveActual, passwordSalt);


            StringBuilder consultaSQL = new StringBuilder();

            consultaSQL.Append("UPDATE Usuarios ");
            consultaSQL.Append("SET PasswordSalt = @passwordSaltParametro, Password = @passwordHashParametro ");
            consultaSQL.Append("WHERE Usuario LIKE @usuarioParametro ;");

            using (var connection = new SqlConnection(cadenaConexion))
            {
                filasAfectadas = connection.Execute(consultaSQL.ToString(),
                   new
                   {
                       passwordSaltParametro = passwordSalt,
                       passwordHashParametro = passwordHash,
                       usuarioParametro = usuario
                   });


            }

            return filasAfectadas;
        }

        public bool VerificarUsuarioExistente(string usuario)
        {
            /*
             
SELECT COUNT(*) FROM USUARIOS
WHERE Usuario LIKE 'bcorrea'

             */

            int contador;

            StringBuilder consultaSQL = new StringBuilder();

            consultaSQL.Append("SELECT COUNT(*) FROM USUARIOS ");
            consultaSQL.Append("WHERE Usuario LIKE @parametroUsuario ");

            using (var connection = new SqlConnection(cadenaConexion))
            {
                contador = connection.ExecuteScalar<int>(consultaSQL.ToString(), new { parametroUsuario = usuario });
            }

            if (contador == 1)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public string ObtenerPasswordSaltPorUsuario(string usuario)
        {
            /*

    SELECT PasswordSalt FROM Usuarios
    WHERE Usuario LIKE 'bcorrea'

                 */

            string passwordSalt = string.Empty;

            StringBuilder consultaSQL = new StringBuilder();

            consultaSQL.Append("SELECT PasswordSalt FROM Usuarios ");
            consultaSQL.Append("WHERE Usuario LIKE @parametroUsuario ");

            using (var connection = new SqlConnection(cadenaConexion))
            {
                passwordSalt = connection.ExecuteScalar<string>(consultaSQL.ToString(), new { parametroUsuario = usuario });
            }

            return passwordSalt;

        }

        public bool VerificarPasswordBlanqueada(string usuario)
        {
            /*
SELECT COUNT(*) FROM Usuarios
WHERE Usuario LIKE 'bcorrea'
AND Password LIKE 'bcorrea(cifrada)'
                         */

            int contador;

            string passwordSalt, passwordCifrada;

            passwordSalt = ObtenerPasswordSaltPorUsuario(usuario);
            passwordCifrada = GenerarPasswordHash(usuario, passwordSalt);

            StringBuilder consultaSQL = new StringBuilder();


            // si la clave(ya cifrada) es igual el nombre de usuario, se deberá actualizar clave
            consultaSQL.Append("SELECT COUNT(*) FROM Usuarios ");
            consultaSQL.Append("WHERE Usuario LIKE @parametroUsuario ");
            consultaSQL.Append("AND Password LIKE @parametroClave ");

            using (var connection = new SqlConnection(cadenaConexion))
            {
                contador = connection.ExecuteScalar<int>(consultaSQL.ToString(), new { parametroUsuario = usuario, parametroClave = passwordCifrada });
            }

            if (contador == 1)
            {
                return true;
            }
            else
            {
                return false;
            }

        }


        public int ValidarClaveActual(string usuario, string clave)
        {
            /*
SELECT COUNT(*) FROM Usuarios
WHERE Usuario LIKE 'bcorrea'
AND Password LIKE 'bcorrea'

                        */

            int contador = 0;

            string passwordSalt, passwordHash;

            passwordSalt = ObtenerPasswordSaltPorUsuario(usuario);
            passwordHash = GenerarPasswordHash(clave, passwordSalt);

            StringBuilder consultaSQL = new StringBuilder();


            // si la clave(ya cifrada) es igual el nombre de usuario, se deberá actualizar clave
            consultaSQL.Append("SELECT COUNT(*) FROM Usuarios ");
            consultaSQL.Append("WHERE Usuario LIKE @parametroUsuario ");
            consultaSQL.Append("AND Password LIKE  @parametroClave ");

            using (var connection = new SqlConnection(cadenaConexion))
            {
                return contador = connection.ExecuteScalar<int>(consultaSQL.ToString(), new { parametroUsuario = usuario, parametroClave = passwordHash });
            }





        }

        public Entidades.Usuarios ObtenerUsuarioPorUsername(string usuario)
        {

            StringBuilder consultaSQL = new StringBuilder();

            consultaSQL.Append("SELECT ");
            consultaSQL.Append("Id, IdRol, Usuario, Nombre, Apellido, Password, PasswordSalt, FechaCreacion, Activo ");
            consultaSQL.Append("FROM Usuarios ");
            consultaSQL.Append("WHERE Usuario = @usuarioParametro ");


            using (var connection = new SqlConnection(cadenaConexion))
            {
                var objUsuario = connection.QuerySingleOrDefault<Entidades.Usuarios>(consultaSQL.ToString(), new { usuarioParametro = usuario });


                return objUsuario;
            }

        }

        public bool ValidarLogin(string usuario, string clave)
        {
            Entidades.Usuarios datosUsuario = ObtenerUsuarioPorUsername(usuario);

            if (datosUsuario == null)
            {
                return false;
            }
            if (datosUsuario.Usuario == null || datosUsuario.Password == null || datosUsuario.PasswordSalt == null || clave == null)
            { return false; }

            string passwordSalt = ObtenerPasswordSaltPorUsuario(usuario);
            string passwordHash = GenerarPasswordHash(clave, passwordSalt);

            clave = passwordHash;



            if (datosUsuario.Usuario == usuario && datosUsuario.Password == clave)
            {
                return true;
            }
            else
            {
                return false;
            }


        }


        public Entidades.Sesion ObtenerUsuarioSesion(string usuario)
        {


            /*
SELECT  
Usuarios.Nombre + ' '+ Usuarios.Apellido AS NOMBRE,
Roles.Descripcion AS ROL, 
Usuarios.Id AS ID, 
Usuarios.Usuario AS USUARIO
FROM Usuarios
INNER JOIN Roles ON 
Usuarios.IdRol = Roles.Id
WHERE Usuarios.Usuario LIKE 'bcorrea'
             
             */

            Entidades.Sesion obj = new Entidades.Sesion();

            StringBuilder consultaSQL = new StringBuilder();

            consultaSQL.Append("SELECT ");
            consultaSQL.Append("Usuarios.Nombre + ' '+ Usuarios.Apellido AS NOMBRE, ");
            consultaSQL.Append("Roles.Descripcion AS ROL, ");
            consultaSQL.Append("Usuarios.Id AS ID,  ");
            consultaSQL.Append("Usuarios.Usuario AS USUARIO ");
            consultaSQL.Append("FROM Usuarios ");
            consultaSQL.Append("INNER JOIN Roles ON ");
            consultaSQL.Append("Usuarios.IdRol = Roles.Id ");
            consultaSQL.Append("WHERE Usuarios.Usuario LIKE @usuarioParametro ");



            using (var connection = new SqlConnection(cadenaConexion))
            {
                obj = connection.QuerySingleOrDefault<Entidades.Sesion>(consultaSQL.ToString(), new { usuarioParametro = usuario });

                return obj;
            }

        }



        public int ResetearClave(int idUsuario)
        {
            /* 
             UPDATE Usuarios
             SET Password = Usuario
             WHERE Id = 33

                */

            Entidades.Usuarios obj = Detalle(idUsuario);

            int filasAfectadas = 0;

            //generamos password salt para guardar en la base
            string passwordSalt = GenerarPasswordSalt(obj.Usuario);

            //generamos Password hash ya encriptada, para que solo el usuario sepa la password
            string passwordHash = GenerarPasswordHash(obj.Usuario, passwordSalt);


            StringBuilder consultaSQL = new StringBuilder();

            consultaSQL.Append("UPDATE Usuarios ");
            consultaSQL.Append("SET PasswordSalt = @passwordSaltParametro, Password = @passwordHashParametro ");
            consultaSQL.Append("WHERE Id = @idParametro ;");

            using (var connection = new SqlConnection(cadenaConexion))
            {
                filasAfectadas = connection.Execute(consultaSQL.ToString(),
                   new
                   {
                       passwordSaltParametro = passwordSalt,
                       passwordHashParametro = passwordHash,
                       idParametro = idUsuario
                   });


            }

            return filasAfectadas;


        }

        public int Editar(Entidades.Usuarios obj)
        {

            /*
UPDATE Usuarios
SET IdRol = 'CLI', 
Nombre = 'VICKY', Apellido = 'JOHNSON',
Activo = 0
WHERE ID = 11

            */


            int filasAfectadas = 0;

            StringBuilder consultaSQL = new StringBuilder();

            consultaSQL.Append("UPDATE Usuarios ");
            consultaSQL.Append("SET IdRol = @idRolParametro,  ");
            consultaSQL.Append("Nombre = @nombreParametro, Apellido = @apellidoParametro, ");
            consultaSQL.Append("Activo = @activoParametro ");
            consultaSQL.Append("WHERE ID = @idParametro ");


            using (var connection = new SqlConnection(cadenaConexion))
            {
                filasAfectadas = connection.Execute(consultaSQL.ToString(),
                   new
                   {
                       idParametro = obj.Id,
                       idRolParametro = obj.IdRol,
                       nombreParametro = obj.Nombre,
                       apellidoParametro = obj.Apellido,
                       activoParametro = obj.Activo
                   });


            }

            return filasAfectadas;
        }

        public int Editar2(Entidades.Join_UsuariosClientes obj)
        {


            /*
UPDATE Usuarios
SET IdRol = 'CLI', 
Nombre = 'VICKY', Apellido = 'JOHNSON',
Activo = 0
WHERE ID = 11

            */

            /*
            int filasAfectadas = 0;

            StringBuilder consultaSQL = new StringBuilder();

            consultaSQL.Append("UPDATE Usuarios ");
            consultaSQL.Append("SET IdRol = @idRolParametro,  ");
            consultaSQL.Append("Nombre = @nombreParametro, Apellido = @apellidoParametro, ");
            consultaSQL.Append("Activo = @activoParametro ");
            consultaSQL.Append("WHERE ID = @idParametro ");


            using (var connection = new SqlConnection(cadenaConexion))
            {
                filasAfectadas = connection.Execute(consultaSQL.ToString(),
                   new
                   {
                       idParametro = obj.Id,
                       idRolParametro = obj.IdRol,
                       nombreParametro = obj.Nombre,
                       apellidoParametro = obj.Apellido,
                       activoParametro = obj.Activo
                   });


            }

            return filasAfectadas;

            */

            return 777;
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
