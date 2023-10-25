using System;
using System.Collections.Generic;
using System.Text;
using mro;
using mro.db;
using mro.BO;
using mro.BL;
using System.Data;

namespace mro.DAL
{
    public class Transactor_DAL : DataWorker, IDisposable
    {
        #region database setup

        private validate validate = validate.getInstance();
        private IDbConnection conn = null;
        private IDbTransaction txn = null;

        public static Transactor_DAL instance(string name)
        {
            return new Transactor_DAL(name);
        }
        public Transactor_DAL(string name) : base(name)
        {
            conn = database.CreateOpenConnection();
        }
        public void begin_transaction()
        {
            txn = conn.BeginTransaction();
        }
        public void commit_transaction()
        {
            txn.Commit();
        }
        public void rollback_transaction()
        {
            txn.Rollback();
        }
        void IDisposable.Dispose()
        {
            conn.Close();
            conn.Dispose();
            if (txn != null) { txn.Dispose(); }
        }
        #endregion

        /// <summary>
        /// Se ecarga de leer el resultado del procedure
        /// </summary>
        /// <param name="reader">Recibe los datos en un reader</param>
        /// <returns>Retorna los datos de las revisiones en el objeto mold_lent_match</returns>
        protected virtual metaDatosCatalogo populateMetadatosTabla(IDataReader reader)
        {
            metaDatosCatalogo m = new metaDatosCatalogo();
            m.table_name      = Convert.ToString((validate.getDefaultIfDBNull(reader["table_name"],     TypeCode.String)));
            m.column_name     = Convert.ToString((validate.getDefaultIfDBNull(reader["column_name"],    TypeCode.String)));
            m.is_nullable     = Convert.ToString((validate.getDefaultIfDBNull(reader["is_nullable"],    TypeCode.String)));
            m.data_type       = Convert.ToString((validate.getDefaultIfDBNull(reader["data_type"],      TypeCode.String)));
            m.max_length      = Convert.ToString((validate.getDefaultIfDBNull(reader["max_length"],     TypeCode.String)));
            m.constraint_Type = Convert.ToString((validate.getDefaultIfDBNull(reader["constraint_type"],TypeCode.String)));
            m.table_name2     = Convert.ToString((validate.getDefaultIfDBNull(reader["table_name2"],    TypeCode.String)));
            m.column_name2    = Convert.ToString((validate.getDefaultIfDBNull(reader["column_name2"],   TypeCode.String)));

            return m;
        }
        
        /// <summary>
        /// Popula un arreglo de strings para consultas autogeneradas
        /// </summary>
        /// <param name="reader">Recibe los datos en un reader</param>
        /// <returns>Retorna los datos en un BO genérico</returns>
        protected virtual metaDatosTransactor populateMetadatosGenerico(IDataReader reader)
        {
            metaDatosTransactor m = new metaDatosTransactor();
            m.columa = new string[reader.FieldCount];
            for (int i = 0 ; i < reader.FieldCount ; i++)
                m.columa[i] = Convert.ToString((validate.getDefaultIfDBNull(reader[i], TypeCode.String)));
            return m;
        }
        
        /// <summary>
        /// Obtiene los metadatos de una tabla para la creación automática de la transacción en Html
        /// </summary>
        /// <param name="datos">Requiere el nombre de la base de datos, el nombre de la tabla y el tipo de columna (Llave primaria o no)</param>
        /// <returns>Retorna una lista con los nombres de las columnas y sus características</returns>
        public List<metaDatosCatalogo> getMetadatosTabla(string tabla)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("getMetadatosTabla", conn, txn))
            {
                cmd.Parameters.Add(database.CreateParameter("@tabla", tabla));

                IDataReader reader = cmd.ExecuteReader();
                List<metaDatosCatalogo> data = new List<metaDatosCatalogo>();

                while (reader.Read())
                    data.Add(populateMetadatosTabla(reader));
                
                reader.Dispose();
                reader.Close();
                return data;
            }
        }

        /// <summary>
        /// Inserta o actualiza el Html de una transacción
        /// </summary>
        /// <param name="datos">Requiere el nombre de la base de datos, el nombre de la transacción y el html</param>
        /// <returns>Retorna la confirmación de que la transacción está completa</returns>
        public bool setTransaccionHtml( string transaccion,
                                        string html,
                                        string descripcionES,
                                        string descripcionEN,
                                        string descripcionPO,
                                        string descripcionGE,
                                        string descripcionCH)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("setTransaccionHtml", conn, txn))
            {
                cmd.Parameters.Add(database.CreateParameter("@transaccion",   transaccion));
                cmd.Parameters.Add(database.CreateParameter("@html",          html));
                cmd.Parameters.Add(database.CreateParameter("@descripcionES", descripcionES));
                cmd.Parameters.Add(database.CreateParameter("@descripcionEN", descripcionEN));
                cmd.Parameters.Add(database.CreateParameter("@descripcionPO", descripcionPO));
                cmd.Parameters.Add(database.CreateParameter("@descripcionGE", descripcionGE));
                cmd.Parameters.Add(database.CreateParameter("@descripcionCH", descripcionCH));

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Inserta la Columna en la tabla t_descriptions
        /// </summary>
        /// <param name="datos">Requiere el nombre de la base de datos, el nombre de la transacción y el html</param>
        /// <returns>Retorna la confirmación de que la transacción está completa</returns>
        public bool setDescripcionColumna(string columna)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("setDescripcionColumna", conn, txn))
            {
                cmd.Parameters.Add(database.CreateParameter("@columna", columna));

                return cmd.ExecuteNonQuery() > 0;
            }
        }
        
        /// <summary>
        /// Ejecuta la consulta de un select generado
        /// </summary>
        /// <param name="datos">Requiere la consulta lista para ser ejecutada</param>
        /// <returns>Retorna un grid</returns>
        public List<metaDatosTransactor> ejecutaSelectGenerado(string sSql)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("ejecutarConsultaGenerada", conn, txn))
            {
                cmd.Parameters.Add(database.CreateParameter("@sql", sSql));
                
                var reader = cmd.ExecuteReader();
                List<metaDatosTransactor> data = new List<metaDatosTransactor>();

                while (reader.Read())
                    data.Add(populateMetadatosGenerico(reader));
                
                reader.Dispose();
                reader.Close();
                return data;
            }
        }

        /// <summary>
        /// Solicita la ejecución para insertar una consulta autogenerada
        /// </summary>
        /// <param name="datos">Requiere la consulta lista para ser ejecutada</param>
        /// <returns>Retorna el número de registros actualizados</returns>
        public int ejecutaConsultaGenerada(string sSql)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("ejecutarConsultaGenerada", conn, txn))
            {
                cmd.Parameters.Add(database.CreateParameter("@sql", sSql));
                return cmd.ExecuteNonQuery();
            }
        }
    }
}
