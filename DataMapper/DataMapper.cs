using DataMapper.Models;
using Newtonsoft.Json;
using Servidor.Comun.DataMapper.DataMapper.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Principal;
using System.Text;

namespace DataMapper
{

    /// <summary>
    /// Mapea los datareader a las entidades o inserta en la base de datos.
    /// </summary>
    public sealed class DataMapper<TEntity> : IRepository<TEntity> where TEntity : class
    {

        private static Dictionary<string, bool> _primaryKeys = null;
        private static Dictionary<string, bool> _calculatedKeys = null;
        private static Dictionary<string, string> _CustomAttributes = null;
        String _nombretabla = null;

        private String _identityColumn = null;
        private String _connectionString = null;
        private String _findSQLSentence = null;
        private String _insertStatement = null;
        private String _countSentence = null;

        /// <summary>
        /// Atributo utilizado para evitar problemas con multithreading en el singleton.
        /// </summary>
        private static object syncRoot = new Object();

        #region Crear instancia

        private static volatile DataMapper<TEntity> instancia;

        private DataMapper() {
            string enableAuditDatabase = ConfigurationManager.AppSettings["Enable_Audit"].ToString();
            this._isAuditEnable =  string.IsNullOrEmpty(enableAuditDatabase) ? true : Convert.ToBoolean(enableAuditDatabase);
        }

        /// <summary>
        /// Cadena de Conexión a la BD
        /// </summary>
        public String ConnectionString
        {
            get
            {
                if (_connectionString == null)
                {
                    lock (syncRoot)
                    {
                        if (_connectionString == null)
                        {
                            _connectionString = getConnectionString();

                        }
                    }
                }
                return _connectionString;
            }
        }

        public static DataMapper<TEntity> Instancia
        {
            get
            {
                if (instancia == null)
                {

                    lock (syncRoot)
                    {
                        if (instancia == null)
                        {

                            instancia = new DataMapper<TEntity>();
                            _CustomAttributes = new Dictionary<string, string>();
                            instancia.CamposEspeciales();
                            //FIXME 
                            TEntity entity = (TEntity)Activator.CreateInstance(typeof(TEntity));
                            instancia.buildInsertStatement(entity);

                        }
                    }
                }
                return instancia;
            }
        }

        public bool _isAuditEnable { get; set; }
        public string _sqlConsoleMessage { get; set; }

        #endregion


        #region Metodos


        #region ADO insert statements



        /// <summary>
        /// Metodo que Verifica cuales campos son calculados, Identity y cual es PK para asi 
        /// excluir los calculados y los identity de las inserciones y guardar la PK en una Variable.
        /// </summary>
        private void CamposEspeciales()
        {
            try
            {
                if (_primaryKeys == null || _calculatedKeys == null || _identityColumn == null)
                {
                    _nombretabla = typeof(TEntity).Name;
                    using (SqlConnection con = new SqlConnection(ConnectionString))
                    {
                        con.Open();
                        _primaryKeys = new Dictionary<string, bool>();
                        _calculatedKeys = new Dictionary<string, bool>();
                        SqlCommand command = new SqlCommand("dbo.pa_Verificacion_Campos_Especiales", con)
                        {
                            CommandType = System.Data.CommandType.StoredProcedure
                        };
                        command.Parameters.AddWithValue("@Table_Name", _nombretabla);
                        SqlDataReader propiedadesReader = command.ExecuteReader();
                        if (propiedadesReader.HasRows)
                        {
                            while (propiedadesReader.Read())
                            {
                                try
                                {

                                    string key = propiedadesReader["NOMBRE_CAMPO"] == DBNull.Value ? "" : propiedadesReader["NOMBRE_CAMPO"].ToString();
                                    string value = propiedadesReader["PROPIEDAD"] == DBNull.Value ? "" : propiedadesReader["PROPIEDAD"].ToString();

                                    switch (value)
                                    {
                                        case "PK":
                                            if (!_primaryKeys.ContainsKey(key))
                                            {
                                                _primaryKeys.Add(key, true);
                                            }
                                            break;

                                        case "IDENTITY":
                                            _identityColumn = key;
                                            break;

                                        case "CALCULATE":
                                            if (!_calculatedKeys.ContainsKey(key))
                                            {
                                                _calculatedKeys.Add(key, true);
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception("NOMBRE_CAMPO Y/O PROPIEDAD no definidas como columnas en el retorno del SP pa_Verificacion_Campos_Especiales.", ex);
                                }
                            }
                        }
                        if (_identityColumn == null) { _identityColumn = ""; }
                        con.Close();
                    }

                }
            }
            catch (SqlException ExSQL)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("DataMapper<").Append(_nombretabla).Append("> : ");
                sb.Append(ExSQL.Message).Append(" - Server : ").Append(ExSQL.Server);
                throw new Exception(sb.ToString());
            }
        }

        /// <summary>
        /// Crea la consulta para realizar la insercion de un registro
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private String buildInsertStatement(TEntity entity)
        {
            lock (syncRoot)
            {

                if (_insertStatement == null)
                {
                    //Build the SQL Statement     
                    Type type = entity.GetType();
                    String tableName = entity.GetType().Name;
                    _insertStatement = "INSERT INTO " + tableName + " ( ";
                    //retrieve object properties
                    List<PropertyInfo> properties = entity.GetType().GetProperties().ToList();

                    for (int i = 0; i < properties.Count; i++)
                    {
                        PropertyInfo property = properties.ElementAt(i);
                        String prop = customAttributeFromProperty(property);
                        if (!String.Equals(prop, property.Name))
                        {
                            if (!IsIdentity(prop) && !_calculatedKeys.ContainsKey(prop))
                            {
                                _insertStatement += (i < properties.Count - 1) ? prop + ", " : prop;
                            }
                        }
                    }
                    _insertStatement += " )" + " VALUES (";
                    //Avoid security issues using parameters
                    for (int i = 0; i < properties.Count; i++)
                    {
                        PropertyInfo property = properties.ElementAt(i);
                        String prop = customAttributeFromProperty(property);
                        if (!IsIdentity(prop) && !_calculatedKeys.ContainsKey(prop))
                        {
                            _insertStatement += (i < properties.Count - 1) ? "@" + prop + ", " : "@" + prop;
                        }
                    }
                    _insertStatement += "); SELECT SCOPE_IDENTITY();";
                }
            }


            return _insertStatement;

        }

        /// <summary>
        /// Crea la consulta y realiza la insercion de un registro
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        private SqlCommand getInsertSqlCommand(TEntity entity, SqlConnection connection)
        {
            lock (syncRoot)
            {
                if (_insertStatement == null)
                {
                    _insertStatement = buildInsertStatement(entity);
                }
            }
            SqlCommand command = new SqlCommand(_insertStatement, connection);
            Type type = entity.GetType();
            List<PropertyInfo> properties = type.GetProperties().ToList();

            for (int i = 0; i < properties.Count; i++)
            {
                PropertyInfo property = properties.ElementAt(i);
                String prop = customAttributeFromProperty(property);
                if (String.Compare(_identityColumn, prop, StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    Object value = property.GetValue(entity, null) ?? DBNull.Value;
                    SqlParameter parameter = new SqlParameter("@" + prop, value);
                    command.Parameters.Add(parameter);

                }
            }
            //command.CommandType = CommandType.StoredProcedure;
            command.CommandType = CommandType.Text;

            return command;
        }
        #endregion

        #region Ejecucion consultas dinamicas

        /// <summary>
        /// Obtiene Tos los campos de una tabla
        /// </summary>
        /// <param name="campoOrdenar">OPCIONAL(String nombre del campo por el cual se desea ordenar el resultado de la consulta.)</param>
        /// <param name="orderDesc">OPCIONAL(Bool true por defecto para seleccionar si se ordena de manera descendente =true o ascendente=false.)</param>
        /// <returns></returns>
        public ICollection<TEntity> GetAll(string campoOrdenar = null, bool orderDesc = true)
        {
            ICollection<TEntity> lEntities = null;
            using (SqlConnection con = new SqlConnection(getConnectionString()))
            {
                if (String.IsNullOrEmpty(campoOrdenar))
                {
                    String pk = getPrimarykeys();
                    campoOrdenar = pk;
                }
                SqlDataReader reader = null;
                Type type = GetType().GenericTypeArguments[0];
                String sqlSentece = "SELECT  * FROM " + type.Name + " ORDER BY " + campoOrdenar;
                sqlSentece += (orderDesc) ? " ASC; " : " DESC;";
                using (SqlCommand command = new SqlCommand(sqlSentece, con))
                {
                    con.Open();
                    reader = command.ExecuteReader();
                    lEntities = mapResultsToEntities(ref reader);
                }
            }
            return lEntities;
        }

        /// <summary>
        /// Persiste el objeto en la BD.
        /// </summary>
        /// <param name="entity"></param>
        public void Create(ref TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(entity.GetType().ToString());
            }

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = getInsertSqlCommand(entity, connection))
                {
                    connection.Open();
                    object IdNumber = command.ExecuteScalar();

                    Type type = entity.GetType();
                    String pk = getPrimarykeys();
                    if (IdNumber != null && !String.IsNullOrEmpty(IdNumber.ToString()))
                    {
                        PropertyInfo property = propertyFromCustomAttribute(pk);
                        TypeConverter converter = TypeDescriptor.GetConverter(property.PropertyType);
                        var result = converter.ConvertFrom(IdNumber.ToString());
                        property.SetValue(entity, result, null);
                    }
                }
            }

        }



        /// <summary>
        /// Elimina un registro de acuerdo a la entidad y llave primaria
        /// </summary>
        /// <param name="entity"></param>
        public bool Delete(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(entity.GetType().ToString());
            }
            var properties = typeof(TEntity).GetProperties().ToList();
            String pkey = getPrimarykeys();

            if (String.IsNullOrEmpty(pkey))
            {
                throw new ArgumentNullException(pkey);
            }

            PropertyInfo prop = propertyFromCustomAttribute(pkey);
            int rowAfected = 0;
            String tableName = entity.GetType().Name;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = "DELETE FROM " + tableName + " WHERE " + pkey + " = @" + pkey;
                    command.Parameters.AddWithValue("@" + pkey, prop.GetValue(entity, null));
                    rowAfected = command.ExecuteNonQuery();
                }
            }
            return (rowAfected > 0) ? true : false;
        }


        /// <summary>
        /// Actualiza un registro en la base de datos dependiendo 
        /// de la entidad de entrada y llave primaia.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool Update(TEntity entity)
        {
            int rowAfected = 0;
            var properties = typeof(TEntity).GetProperties().ToList();
            String pkey = getPrimarykeys();

            if (String.IsNullOrEmpty(pkey))
            {
                throw new ArgumentNullException(pkey);
            }

            properties = properties.Where(p => p.GetValue(entity) != null).ToList();

            SqlParameter[] sqlParams = new SqlParameter[properties.Count];
            SqlParameter param = null;
            // get identity key
            PropertyInfo prop = propertyFromCustomAttribute(pkey);
            // delete identity key
            properties.Remove(prop);

            String sqlStatement = "UPDATE " + entity.GetType().Name + " SET ";

            // set sqlparameters and query
            for (int i = 0; i < properties.Count; i++)
            {
                PropertyInfo property = properties.ElementAt(i);
                String field = customAttributeFromProperty(property);
                sqlStatement += field + " = " + ((i < properties.Count - 1) ? "@" + field + ", " : "@" + field);
                param = new SqlParameter("@" + field, property.GetValue(entity) ?? DBNull.Value);
                sqlParams[i] = param;
            }

            String fieldWhere = customAttributeFromProperty(prop);
            sqlStatement += " WHERE " + fieldWhere + " = @" + fieldWhere;
            param = new SqlParameter("@" + fieldWhere, prop.GetValue(entity, null));
            sqlParams[properties.Count] = param;

            rowAfected = executeUpdate(sqlStatement, sqlParams);

            return (rowAfected > 0);
        }

        /// <summary>
        /// Ejecuta el query de actualizacion
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <param name="sqlParams"></param>
        /// <returns></returns>
        private int executeUpdate(String sqlStatement, SqlParameter[] sqlParams)
        {

            int rowAfected = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand(sqlStatement, connection);
                {
                    connection.Open();
                    if (sqlParams != null && sqlParams.Length > 0)
                        command.Parameters.AddRange(sqlParams);
                    rowAfected = command.ExecuteNonQuery();
                    command.Dispose();
                }
            }
            return rowAfected;
        }



        /// <summary>
        /// Encuentra una entidad a partir de un valor y el nombre del atributo en la entidad.
        /// </summary>
        /// <param name="value">Valor del campo en la BD</param>
        /// <param name="attribute">Nombre de la propiedad (Columna) BD</param>
        /// <param name="exacto">indica si el valor a consultar es exacto</param>
        /// <param name="campoOrdenar">OPCIONAL(String nombre del campo por el cual se desea ordenar el resultado de la consulta.)</param>
        /// <param name="orderDesc">OPCIONAL(Bool true por defecto para seleccionar si se ordena de manera descendente =true o ascendente=false.)</param>
        /// <returns>La entidad que contiene el valor indicado, de otra manera null</returns>
        public ICollection<TEntity> findByAttribute(string value, string attribute, bool exacto = true, string campoOrdenar = null, bool orderDesc = true)
        {
            ICollection<TEntity> lEntities = null;
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                SqlDataReader reader = null;
                Type type = GetType().GenericTypeArguments[0];
                PropertyInfo property = type.GetProperty(attribute);

                String sqlSentence = null;
                sqlSentence = buildFindSqlSentence(property, type, value, exacto);
                if (campoOrdenar != null)
                {
                    sqlSentence += " ORDER BY " + campoOrdenar;
                    sqlSentence += (orderDesc) ? " ASC; " : " DESC;";
                }
                else
                {
                    if (String.IsNullOrEmpty(campoOrdenar))
                    {
                        String pkey = getPrimarykeys();
                        campoOrdenar = pkey;
                        sqlSentence += " ORDER BY " + campoOrdenar;
                        sqlSentence += (orderDesc) ? " ASC; " : " DESC;";
                    }
                }
                Console.WriteLine(sqlSentence);
                using (SqlCommand command = new SqlCommand(sqlSentence, con))

                {
                    Type propertyType = property.PropertyType;
                    if (!exacto && propertyType.Equals(Type.GetType("System.String")))
                    {
                        command.Parameters.AddWithValue("@Value", "%" + value + "%");
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@Value", value);
                    }

                    command.CommandType = CommandType.Text;
                    con.Open();
                    reader = command.ExecuteReader(CommandBehavior.CloseConnection);

                    if (reader.HasRows)
                    {
                        lEntities = mapResultsToEntities(ref reader);
                    }
                }
                con.Close();
            }

            return lEntities;
        }


        /// <summary>
        /// Consulta la cantidad de entidades en la BD
        /// </summary>
        /// <returns>Numero de entidades con persistencia en BD</returns>
        public long Count()
        {
            Type type = GetType().GenericTypeArguments[0];
            Object rowsST;
            if (_countSentence == null)
            {
                _countSentence = "SELECT COUNT(*) FROM " + type.Name + ";";
            }

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(_countSentence, con))
                {
                    command.CommandType = CommandType.Text;
                    con.Open();
                    rowsST = command.ExecuteScalar();
                }
            }
            return (rowsST == null) ? 0 : Int64.Parse(rowsST.ToString());
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private string buildFindSqlSentence(PropertyInfo property, Type type, String value = null, bool exacto = true)
        {

            if (_findSQLSentence == null)
            {
                String field = null;
                field = customAttributeFromProperty(property);
                _findSQLSentence = "SELECT * FROM " + type.Name + " WHERE " + field;
                Type propertyType = property.PropertyType;
                if (!exacto && propertyType.Equals(Type.GetType("System.String")))
                {
                    _findSQLSentence += " like @value";
                }
                else
                {
                    _findSQLSentence += " = @Value";

                }
            }
            return _findSQLSentence;

        }
        #endregion
        #region EjecucionSP

        /// <summary>
        /// Ejecuta un procedimiento almacenado de seleccion de registros
        /// </summary>
        /// <param name="procedureName"></param>        
        /// <param name="sqlParam"></param>
        /// <returns></returns>
        public ICollection<TEntity> ExecuteSelectSP(string procedureName, params SqlParameter[] sqlParam)
        {
            ICollection<TEntity> lEntities = null;
            String connectionSt = getConnectionString();
            using (SqlConnection connection = new SqlConnection(connectionSt))
            {
                connection.Open();
                SqlDataReader reader = (SqlDataReader)ExecuteProcedure(procedureName, ExecuteType.ExecuteReader, connection, sqlParam);

                if (reader.HasRows)
                {
                    lEntities = mapResultsToEntities(ref reader);
                }
                reader.Close();
            }

            return lEntities;
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado de creacion de un registro
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="procedureName"></param>
        /// <param name="sqlParam"></param>
        public TEntityObject ExecuteCreateSP<TEntityObject>(TEntityObject entity, string procedureName, params SqlParameter[] sqlParam)
        {

            String pk = getPrimarykeys();

            using (SqlConnection connection = new SqlConnection(getConnectionString()))
            {
                connection.Open();
                //Set Object Id
                Type type = entity.GetType();

                PropertyInfo property = propertyFromCustomAttribute(pk);
                TypeConverter converter = TypeDescriptor.GetConverter(property.PropertyType);
                string valorIn = property.GetValue(entity)?.ToString();
                string Ident = ExecuteProcedure(procedureName, ExecuteType.ExecuteScalar, connection, sqlParam)?.ToString();
                if (!string.IsNullOrEmpty(Ident) && (string.IsNullOrEmpty(valorIn) || valorIn == "0"))
                {
                    var result = converter.ConvertFrom(Ident);
                    property.SetValue(entity, result, null);
                }
                return entity;
            }
        }

        private string getPrimarykeys()
        {
            if (_primaryKeys.Count == 0)
            {
                throw new Exception("La Tabla { " + _nombretabla + " } no contiene llaves primarias.");
            }
            return _primaryKeys.Keys.ElementAt(0);
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado de actualizacion o eliminacion
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="sqlParam"></param>
        /// <returns></returns>
        public int ExecuteNonQuerySP(string procedureName, params SqlParameter[] sqlParam)
        {
            int returnValue;

            using (SqlConnection connection = new SqlConnection(getConnectionString()))
            {
                connection.Open();
                returnValue = (int)ExecuteProcedure(procedureName, ExecuteType.ExecuteNonQuery, connection, sqlParam);
            }

            return returnValue;
        }

        /// <summary>
        /// Metodo publico que ejecuta un procedimiento almacenado de consulta, actualizacion, creacion o eliminacion
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="executeType"></param>
        /// <param name="sqlParams"></param>
        /// <returns></returns>
        public object ExecuteProcedure(string procedureName, ExecuteType executeType, params SqlParameter[] sqlParams)
        {
            object result = null;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                result = ExecuteProcedure(procedureName, executeType, connection, sqlParams);
            }
            return result;
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado de consulta, actualizacion, creacion o eliminacion
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="executeType"></param>
        /// <param name="conection"></param>
        /// <param name="sqlParams"></param>
        /// <returns></returns>
        public object ExecuteProcedure(string procedureName, ExecuteType executeType, SqlConnection conection, params SqlParameter[] sqlParams)
        {
           
            object returnObject = null;
            long ExecutionId = 0;
            string SQLConsoleMessages;

            try
            {
                using (var cmd = conection.CreateCommand())
                {


                    cmd.CommandText = procedureName;
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (sqlParams != null)
                    {
                        // get parameters procedure
                        SqlCommandBuilder.DeriveParameters(cmd);
                        foreach (SqlParameter parm in cmd.Parameters)
                        {
                            parm.IsNullable = true;

                            foreach (SqlParameter parmeter in sqlParams)
                            {
                                if (parm.ParameterName.Replace("@", "").ToUpper() == parmeter.ParameterName.Replace("@", "").ToUpper())
                                {
                                    parm.Value = parmeter.Value ?? DBNull.Value;
                                }
                            }
                        }
                    }

                    conection.InfoMessage += new SqlInfoMessageEventHandler(OnSQLMessageInfo);

                    if (_isAuditEnable)
                    {
                        ExecutionId = StartAuditing(procedureName, conection, sqlParams);
                    }

                    switch (executeType)
                    {
                        case ExecuteType.ExecuteReader:
                            returnObject = cmd.ExecuteReader();
                            break;
                        case ExecuteType.ExecuteNonQuery:
                            returnObject = cmd.ExecuteNonQuery();
                            break;
                        case ExecuteType.ExecuteScalar:
                            returnObject = cmd.ExecuteScalar();
                            break;
                        default:
                            break;
                    }
                }
                if (_isAuditEnable)
                {
                    EndAuditing(ExecutionId, returnObject, _sqlConsoleMessage,true);
                }
            }
            catch (Exception Ex) {

                conection.InfoMessage += new SqlInfoMessageEventHandler(OnSQLMessageInfo);
                EndAuditing(ExecutionId, Ex.Message, _sqlConsoleMessage,false);
            }

            return returnObject;
        }

        private void EndAuditing(long executionId, object returnObject, string sqlConsoleMessage, bool isSuccessful)
        {
            string jsonParameters = JsonConvert.SerializeObject(returnObject);

            using (SqlConnection connection = new SqlConnection(GetAuditConnectionString()))
            {
                SqlCommand command = new SqlCommand
                {
                    Connection = connection,
                    CommandText = "[database].[pa_UpdateExecutionLog]",
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@ExecutionId", executionId);
                command.Parameters.AddWithValue("@ReturnObject", returnObject);
                command.Parameters.AddWithValue("@SqlConsoleMessages", sqlConsoleMessage);
                command.Parameters.AddWithValue("@ExecutionEndTime", DateTime.Now);
                command.Parameters.AddWithValue("@IsSuccessful", isSuccessful);

                connection.Open();
                SqlDataReader rdr = command.ExecuteReader();
                connection.Close();
            }
        }

        private void OnSQLMessageInfo(object sender, SqlInfoMessageEventArgs e)
        {
            if (_isAuditEnable)
            {
                _sqlConsoleMessage = e.Message;
                _sqlConsoleMessage += e.Errors;
            }
        }

        private long StartAuditing(string procedureName, SqlConnection conection, SqlParameter[] sqlParams)
        {
            long executionId = -1;
            using (SqlConnection connection = new SqlConnection(GetAuditConnectionString()))
            {
               
                SqlCommand command = new SqlCommand
                {
                    Connection = connection,
                    CommandText = "[database].[pa_AddDatabaseLog]",
                    CommandType = CommandType.StoredProcedure
                };
                
                List<Parameter> lParameters = new List<Parameter>();

                foreach (SqlParameter param in sqlParams)
                {
                    Parameter p = new Parameter
                    {
                        Name = param.ParameterName,
                        Type = param.DbType.ToString(),
                        Value = param.Value?.ToString()
                    };

                    lParameters.Add(p);
                }
                string userID = WindowsIdentity.GetCurrent().Name;
                //Get connectionInfo
                string dbName = conection.Database;
                string workstationId = conection.WorkstationId;
                string datasource = conection.DataSource;
                string jsonParameters = JsonConvert.SerializeObject(lParameters);

                command.Parameters.AddWithValue("@Datasource", datasource);
                command.Parameters.AddWithValue("@DatabaseName", dbName);
                command.Parameters.AddWithValue("@Username", userID);
                command.Parameters.AddWithValue("@StoreProcedureName", procedureName);
                command.Parameters.AddWithValue("@workstationName", workstationId);
                command.Parameters.AddWithValue("@Parameters", jsonParameters);
                command.Parameters.AddWithValue("@ExecutionStartTime", DateTime.Now);

                connection.Open();
                SqlDataReader rdr = command.ExecuteReader();

                if (rdr.Read())
                {
                    executionId = Convert.ToInt64(rdr["ExecutionId"].ToString());

                }
                connection.Close();

            }
            return executionId;
        }

        private string GetAuditConnectionString()
        {
            string connectionString = null;

            connectionString = ConfigurationManager.AppSettings["ConnectionString.Auditoria"].ToString();


            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("Connection String Auditoria no encontrada Key (ConnectionString.Auditoria) No encontrada.");
            }
            return connectionString;
        }

        /// <summary>
        /// Ejecuta un procedimiento que retorna un Boolean
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="sqlParam"></param>
        /// <returns></returns>
        public object ExecuteGeneralSP(string procedureName, params SqlParameter[] sqlParam)
        {
            object returnValue;

            using (SqlConnection connection = new SqlConnection(getConnectionString()))
            {
                connection.Open();
                returnValue = ExecuteProcedure(procedureName, ExecuteType.ExecuteScalar, connection, sqlParam);
            }

            return returnValue;
        }
        #endregion


        #region FuncionesGenerales


        /// <summary>
        /// Obtiene la cadena de conexion parametrizada
        /// </summary>
        /// <returns></returns>
        private string getConnectionString()
        {
            string ProjectStage = ConfigurationManager.AppSettings["PROJECT_STAGE"].ToString();
            string connectionString = null;
            switch (ProjectStage.ToUpper())
            {
                case "DESARROLLO":
                    connectionString = ConfigurationManager.AppSettings["ConnectionString.Desarrollo"].ToString();
                    break;
                case "PRUEBAS":
                    connectionString = ConfigurationManager.AppSettings["ConnectionString.Pruebas"].ToString();
                    break;
                case "PRODUCCION":
                    connectionString = ConfigurationManager.AppSettings["ConnectionString.Produccion"].ToString();
                    break;
            }
            if (connectionString == null)
            {
                throw new ArgumentNullException("Connection String no encontrada (PROJECT_STAGE) No encontrado" + ProjectStage);
            }
            return connectionString;
        }





        /// <summary>
        /// Verifica si el campo es Identity en base de datos
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        private bool IsIdentity(string columnName)
        {
            if (_identityColumn == "")
            {
                return false;
            }
            return columnName.Equals(_identityColumn, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Mapea el datareader a la entidad y devuelve una coleccion
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private ICollection<TEntity> mapResultsToEntities(ref SqlDataReader reader)
        {
            ICollection<TEntity> lEntities = new Collection<TEntity>();
            while (reader.Read())
            {
                TEntity entity = null;
                //TODO find a constructor that has zero parameters on TEntity then, use the Activator.CreateInstance method. 
                //Otherwise, you use the Factory<T>.CreateNew() method
                //var constructors = typeof(TEntity).GetConstructors();
                entity = (TEntity)Activator.CreateInstance(typeof(TEntity));
                entity = MapColumnsToEntity(ref entity, ref reader);
                lEntities.Add(entity);
            }
            return lEntities;
        }

        /// <summary>
        /// Mapea el datareader a la entidad
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        private TEntity MapColumnsToEntity(ref TEntity entity, ref SqlDataReader reader)
        {
            int columns = reader.FieldCount;
            for (int i = 0; i < columns; i++)
            {

                String attributeName = reader.GetName(i);
                PropertyInfo attr = propertyFromCustomAttribute(attributeName);
                if (attr != null)
                {
                    Type dataType = reader.GetFieldType(i);
                    bool isNull = reader.IsDBNull(i);
                    if (isNull) { attr.SetValue(entity, null); }
                    else
                    {
                        switch (dataType.ToString())
                        {
                            case "System.Int64":
                                attr.SetValue(entity, reader.GetInt64(i));
                                break;
                            case "System.Int32":
                                attr.SetValue(entity, reader.GetInt32(i));
                                break;
                            case "System.Int16":

                                attr.SetValue(entity, reader.GetInt16(i));
                                break;
                            case "System.String":
                                attr.SetValue(entity, reader.GetString(i));
                                break;
                            case "System.DateTime":
                                attr.SetValue(entity, reader.GetDateTime(i));
                                break;
                            case "System.Decimal":
                                attr.SetValue(entity, reader.GetDecimal(i));
                                break;
                            case "System.Double":
                                attr.SetValue(entity, reader.GetDouble(i));
                                break;
                            case "System.Byte":
                                attr.SetValue(entity, reader.GetByte(i));
                                break;
                            case "System.Single":
                                attr.SetValue(entity, reader.GetFloat(i));
                                break;
                            case "System.Boolean":
                                attr.SetValue(entity, reader.GetBoolean(i));
                                break;
                            case "System.Guid":
                                attr.SetValue(entity, reader.GetGuid(i));
                                break;
                        }
                    }
                }
            }
            return entity;
        }

        /// <summary>
        /// Obtiene el nombre de la propiedad
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public string GetPropertyName<T>(Expression<Func<T>> expression)
        {
            MemberExpression body = (MemberExpression)expression.Body;
            return body.Member.Name;
        }

        /// <summary>
        /// Retorna la propiedad correspondiente al alias(ColumnAttribute) filtrado
        /// </summary>
        /// <param name="customAttribute"></param>
        /// <returns></returns>
        private PropertyInfo propertyFromCustomAttribute(String customAttribute)
        {
            PropertyInfo attr = null;
            var properties = typeof(TEntity).GetProperties()
                      .Where(p => p.IsDefined(typeof(ColumnAttribute), false))
                      .Select(p => new
                      {
                          PropertyName = p.Name,
                          p.GetCustomAttributes(typeof(ColumnAttribute),
                                  false).Cast<ColumnAttribute>().Single().Name
                      });
            String columnMapping = properties.FirstOrDefault(a => a.Name == customAttribute)?.PropertyName;
            if (!String.IsNullOrEmpty(columnMapping))
            {
                attr = typeof(TEntity).GetProperty(columnMapping);
            }
            return attr;
        }

        /// <summary>
        /// Retorna el alias de la propiedad filtrada
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private String customAttributeFromProperty(PropertyInfo property)
        {
            string propiedad = property.ToString();

            if (!_CustomAttributes.ContainsKey(propiedad))
            {
                var customAttri = property.GetCustomAttributes(false);
                var columnMapping = customAttri.FirstOrDefault(a => a.GetType() == typeof(ColumnAttribute));
                ColumnAttribute map = columnMapping as ColumnAttribute;
                if (map == null)
                {
                    return property.Name;
                }
                _CustomAttributes.Add(propiedad, map.Name);
            }
            return _CustomAttributes[propiedad];
        }

        /// <summary>
        /// Crea los parametros a enviar, recibidos mediante la entidad
        /// </summary>
        /// <param name="entityValues"></param>
        /// <returns></returns>
        public SqlParameter[] createParametersFromEntity(TEntity entityValues)
        {
            SqlParameter[] sqlParams = null;
            SqlParameter param = null;
            PropertyInfo property = null;
            var properties = typeof(TEntity).GetProperties().ToList();

            //// delete properties null values
            //properties = properties.Where(i => i.GetValue(entityValues) != null).ToList();

            sqlParams = new SqlParameter[properties.Count];

            for (int i = 0; i < properties.Count; i++)
            {
                param = new SqlParameter();
                property = properties.ElementAt(i);
                param.ParameterName = customAttributeFromProperty(property);
                param.Value = property.GetValue(entityValues);
                sqlParams[i] = param;
            }

            return sqlParams;
        }


        public ICollection<TEntity> ExecuteSelectSP(string procedureName, SqlParameterCollection sqlParamsCollection = null)
        {

            SqlParameter[] parameters = null;
            if (sqlParamsCollection != null)
            {
                parameters = sqlParameterCollectionToSqlParameterArray(sqlParamsCollection);
            }
            return ExecuteSelectSP(procedureName, parameters);
        }

        public TEntityObject ExecuteCreateSP<TEntityObject>(TEntityObject entity, string procedureName, SqlParameterCollection sqlParamsCollection)
        {
            SqlParameter[] parameters = sqlParameterCollectionToSqlParameterArray(sqlParamsCollection);
            return ExecuteCreateSP<TEntityObject>(entity, procedureName, parameters);
        }

        public int ExecuteNonQuerySP(string procedureName, SqlParameterCollection sqlParamsCollection)
        {
            SqlParameter[] parameters = sqlParameterCollectionToSqlParameterArray(sqlParamsCollection);
            return ExecuteNonQuerySP(procedureName, parameters);
        }

        public object ExecuteGeneralSP(string procedureName, SqlParameterCollection sqlParamsCollection)
        {
            SqlParameter[] parameters = sqlParameterCollectionToSqlParameterArray(sqlParamsCollection);
            return ExecuteGeneralSP(procedureName, parameters);
        }
        public object ExecuteProcedure(string procedureName, ExecuteType executeType, SqlParameterCollection sqlParamsCollection)
        {
            SqlParameter[] parameters = sqlParameterCollectionToSqlParameterArray(sqlParamsCollection);
            return ExecuteProcedure(procedureName, executeType, parameters);
        }

        #endregion

        #region Utils
        private static SqlParameter[] sqlParameterCollectionToSqlParameterArray(SqlParameterCollection sqlParamsCollection)
        {
            SqlParameter[] parameters = new SqlParameter[sqlParamsCollection.Count];

            for (int i = 0; i < sqlParamsCollection.Count; i++)
            {
                SqlParameter p = sqlParamsCollection[i];
                parameters[i] = p;
            }

            return parameters;
        }
        #endregion

        #endregion

    }

}


