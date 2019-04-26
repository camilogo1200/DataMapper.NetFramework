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
        private static Dictionary<string, bool> _calculatedKeys = null;
        private static Dictionary<string, string> _CustomAttributes = null;
        private static Dictionary<string, bool> _primaryKeys = null;

        /// <summary>
        /// Atributo utilizado para evitar problemas con multithreading en el singleton.
        /// </summary>
        private static object syncRoot = new object();

        private const string _separator = "-----------------------------------------------------";
        private string _connectionString = null;
        private string _countSentence = null;
        private string _findSQLSentence = null;
        private string _identityColumn = null;
        private string _insertStatement = null;
        private string _nombretabla = null;
        private const string _atSign = "@";
        private const string _addDbLogSp = "[database].[pa_AddDatabaseLog]";
        private const string _updateDbLogSP = "[database].[pa_UpdateExecutionLog]";
        private const string _ZeroSt = "0";
        private const string _ValueSt = "@Value";
        #region Crear instancia

        private static volatile DataMapper<TEntity> instancia;

        private DataMapper()
        {
            string enableAuditDatabase = ConfigurationManager.AppSettings["Enable_Audit"];
            _isAuditEnable = string.IsNullOrEmpty(enableAuditDatabase) ? true : Convert.ToBoolean(enableAuditDatabase);
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
                            instancia.BuildInsertStatement(entity);
                        }
                    }
                }
                return instancia;
            }
        }

        public bool _isAuditEnable { get; set; }

        public string _sqlConsoleMessage { get; set; }

        /// <summary>
        /// Cadena de Conexión a la BD
        /// </summary>
        public string ConnectionString
        {
            get
            {
                if (_connectionString == null)
                {
                    lock (syncRoot)
                    {
                        if (_connectionString == null)
                        {
                            _connectionString = GetConnectionString();
                        }
                    }
                }
                return _connectionString;
            }
        }

        #endregion Crear instancia              

        #region Metodos

        #region ADO insert statements

        /// <summary>
        /// Crea la consulta para realizar la insercion de un registro
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private string BuildInsertStatement(TEntity entity)
        {
            lock (syncRoot)
            {
                if (_insertStatement == null)
                {
                    //Build the SQL Statement
                    //Type type = entity.GetType();
                    string tableName = entity.GetType().Name;
                    _insertStatement = $"INSERT INTO {tableName} ( ";
                    //retrieve object properties
                    List<PropertyInfo> properties = entity.GetType().GetProperties().ToList();
                    var builder = new StringBuilder();
                    builder.Append(_insertStatement);

                    for (int i = 0; i < properties.Count; i++)
                    {
                        PropertyInfo property = properties.ElementAt(i);
                        string prop = CustomAttributeFromProperty(property);
                        if (!string.Equals(prop, property.Name))
                        {
                            if (!IsIdentity(prop) && !_calculatedKeys.ContainsKey(prop))
                            {
                                builder.Append((i < properties.Count - 1) ? prop + ", " : prop);
                            }
                        }
                    }

                    builder.Append(" ) VALUES (");
                    //Avoid security issues using parameters
                    for (int i = 0; i < properties.Count; i++)
                    {
                        PropertyInfo property = properties.ElementAt(i);
                        string prop = CustomAttributeFromProperty(property);
                        if (!IsIdentity(prop) && !_calculatedKeys.ContainsKey(prop))
                        {
                            builder.Append((i < properties.Count - 1) ? _atSign + prop + ", " : _atSign + prop);
                        }
                    }
                    builder.Append("); SELECT SCOPE_IDENTITY();");
                    _insertStatement = builder.ToString();
                }
            }

            return _insertStatement;
        }

        /// <summary>
        /// Metodo que Verifica cuales campos son calculados, Identity y cual es PK para asi excluir
        /// los calculados y los identity de las inserciones y guardar la PK en una Variable.
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
                        try
                        {
                            con.Open();
                            _primaryKeys = new Dictionary<string, bool>();
                            _calculatedKeys = new Dictionary<string, bool>();
                            using (SqlCommand command = new SqlCommand("dbo.pa_Verificacion_Campos_Especiales", con))
                            {
                                command.CommandType = System.Data.CommandType.StoredProcedure;

                                command.Parameters.AddWithValue("@Table_Name", _nombretabla);
                                SqlDataReader propiedadesReader = command.ExecuteReader();
                                if (propiedadesReader.HasRows)
                                {
                                    while (propiedadesReader.Read())
                                    {
                                        try
                                        {
                                            string key = propiedadesReader["NOMBRE_CAMPO"] == DBNull.Value ? string.Empty : propiedadesReader["NOMBRE_CAMPO"].ToString();
                                            string value = propiedadesReader["PROPIEDAD"] == DBNull.Value ? string.Empty : propiedadesReader["PROPIEDAD"].ToString();

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
                                            throw new Exception("NOMBRE_CAMPO Y/O PROPIEDAD no definidas como columnas en el retorno del SP pa_Verificacion_Campos_Especiales. [" + con.Database + "] ", ex);
                                        }
                                    }
                                }
                            }
                            if (_identityColumn == null) { _identityColumn = string.Empty; }
                            con.Close();
                        }
                        catch (Exception ex)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine("---------- Error en permisos ----------");
                            sb.AppendLine(ex.Message + "[" + con.Database + "] ");
                            sb.AppendLine(ex.ToString());
                            throw new Exception(sb.ToString(), ex);
                        }
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
        /// Crea la consulta y realiza la insercion de un registro
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        private SqlCommand GetInsertSqlCommand(TEntity entity, SqlConnection connection)
        {
            lock (syncRoot)
            {
                if (_insertStatement == null)
                {
                    _insertStatement = BuildInsertStatement(entity);
                }
            }
            SqlCommand command = new SqlCommand(_insertStatement, connection);
            Type type = entity.GetType();
            List<PropertyInfo> properties = type.GetProperties().ToList();

            for (int i = 0; i < properties.Count; i++)
            {
                PropertyInfo property = properties.ElementAt(i);
                string prop = CustomAttributeFromProperty(property);
                if (string.Compare(_identityColumn, prop, StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    Object value = property.GetValue(entity, null) ?? DBNull.Value;
                    SqlParameter parameter = new SqlParameter(_atSign + prop, value);
                    command.Parameters.Add(parameter);
                }
            }
            command.CommandType = CommandType.Text;

            return command;
        }

        #endregion ADO insert statements

        #region Ejecucion consultas dinamicas

        /// <summary>
        /// Consulta la cantidad de entidades en la BD
        /// </summary>
        /// <returns>Numero de entidades con persistencia en BD</returns>
        public long Count()
        {
            Type type = GetType().GenericTypeArguments[0];
            object rowsST;
            if (_countSentence == null)
            {
                _countSentence = $"SELECT COUNT(*) FROM {type.Name};";
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
            return (rowsST == null) ? 0 : long.Parse(rowsST.ToString());
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
                using (SqlCommand command = GetInsertSqlCommand(entity, connection))
                {
                    connection.Open();
                    object IdNumber = command.ExecuteScalar();

                    Type type = entity.GetType();
                    string pk = GetPrimarykeys();
                    if (IdNumber != null && !string.IsNullOrEmpty(IdNumber.ToString()))
                    {
                        PropertyInfo property = PropertyFromCustomAttribute(pk);
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
            string pkey = GetPrimarykeys();

            if (string.IsNullOrEmpty(pkey))
            {
                throw new ArgumentNullException(pkey);
            }

            PropertyInfo prop = PropertyFromCustomAttribute(pkey);
            int rowAfected = 0;
            string tableName = entity.GetType().Name;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = $"DELETE FROM {tableName} WHERE {pkey} = @{pkey}";
                    command.Parameters.AddWithValue(_atSign + pkey, prop.GetValue(entity, null));
                    rowAfected = command.ExecuteNonQuery();
                }
            }
            return (rowAfected > 0);
        }

        /// <summary>
        /// Encuentra una entidad a partir de un valor y el nombre del atributo en la entidad.
        /// </summary>
        /// <param name="value">Valor del campo en la BD</param>
        /// <param name="attribute">Nombre de la propiedad (Columna) BD</param>
        /// <param name="exacto">indica si el valor a consultar es exacto</param>
        /// <param name="campoOrdenar">
        /// OPCIONAL(string nombre del campo por el cual se desea ordenar el resultado de la consulta.)
        /// </param>
        /// <param name="orderDesc">
        /// OPCIONAL(Bool true por defecto para seleccionar si se ordena de manera descendente =true
        /// o ascendente=false.)
        /// </param>
        /// <returns>La entidad que contiene el valor indicado, de otra manera null</returns>
        public ICollection<TEntity> findByAttribute(string value, string attribute, bool exacto = true, string campoOrdenar = null, bool orderDesc = true)
        {
            ICollection<TEntity> lEntities = null;
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                SqlDataReader reader = null;
                Type type = GetType().GenericTypeArguments[0];
                PropertyInfo property = type.GetProperty(attribute);

                string sqlSentence = null;
                sqlSentence = BuildFindSqlSentence(property, type, value, exacto);
                if (campoOrdenar != null)
                {
                    sqlSentence += " ORDER BY " + campoOrdenar;
                    sqlSentence += (orderDesc) ? " ASC; " : " DESC;";
                }
                else
                {
                    if (string.IsNullOrEmpty(campoOrdenar))
                    {
                        string pkey = GetPrimarykeys();
                        campoOrdenar = pkey;
                        sqlSentence += " ORDER BY " + campoOrdenar;
                        sqlSentence += (orderDesc) ? " ASC; " : " DESC;";
                    }
                }
                Console.WriteLine(sqlSentence);
                using (SqlCommand command = new SqlCommand(sqlSentence, con))

                {
                    Type propertyType = property.PropertyType;
                    if (!exacto && propertyType.Equals(Type.GetType(DataTypes._STRING)))
                    {
                        command.Parameters.AddWithValue(_ValueSt, "%" + value + "%");
                    }
                    else
                    {
                        command.Parameters.AddWithValue(_ValueSt, value);
                    }

                    command.CommandType = CommandType.Text;
                    con.Open();
                    reader = command.ExecuteReader(CommandBehavior.CloseConnection);

                    if (reader.HasRows)
                    {
                        lEntities = MapResultsToEntities(ref reader);
                    }
                }
                con.Close();
            }

            return lEntities;
        }

        /// <summary>
        /// Obtiene Tos los campos de una tabla
        /// </summary>
        /// <param name="campoOrdenar">
        /// OPCIONAL(string nombre del campo por el cual se desea ordenar el resultado de la consulta.)
        /// </param>
        /// <param name="orderDesc">
        /// OPCIONAL(Bool true por defecto para seleccionar si se ordena de manera descendente =true
        /// o ascendente=false.)
        /// </param>
        /// <returns></returns>
        public ICollection<TEntity> GetAll(string campoOrdenar = null, bool orderDesc = true)
        {
            ICollection<TEntity> lEntities = null;
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                if (string.IsNullOrEmpty(campoOrdenar))
                {
                    string pk = GetPrimarykeys();
                    campoOrdenar = pk;
                }
                SqlDataReader reader = null;
                Type type = GetType().GenericTypeArguments[0];
                string sqlSentece = "SELECT  * FROM " + type.Name + " ORDER BY " + campoOrdenar;
                sqlSentece += (orderDesc) ? " ASC; " : " DESC;";
                using (SqlCommand command = new SqlCommand(sqlSentece, con))
                {
                    con.Open();
                    reader = command.ExecuteReader();
                    lEntities = MapResultsToEntities(ref reader);
                }
            }
            return lEntities;
        }

        /// <summary>
        /// Actualiza un registro en la base de datos dependiendo de la entidad de entrada y llave primaia.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool Update(TEntity entity)
        {
            int rowAfected = 0;
            var properties = typeof(TEntity).GetProperties().ToList();
            string pkey = GetPrimarykeys();

            if (string.IsNullOrEmpty(pkey))
            {
                throw new ArgumentNullException(pkey);
            }

            properties = properties.Where(p => p.GetValue(entity) != null).ToList();

            SqlParameter[] sqlParams = new SqlParameter[properties.Count];
            SqlParameter param = null;
            // get identity key
            PropertyInfo prop = PropertyFromCustomAttribute(pkey);
            // delete identity key
            properties.Remove(prop);

            string sqlStatement = "UPDATE " + entity.GetType().Name + " SET ";

            // set sqlparameters and query
            for (int i = 0; i < properties.Count; i++)
            {
                PropertyInfo property = properties.ElementAt(i);
                string field = CustomAttributeFromProperty(property);
                sqlStatement += field + " = " + ((i < properties.Count - 1) ? _atSign + field + ", " : _atSign + field);
                param = new SqlParameter(_atSign + field, property.GetValue(entity) ?? DBNull.Value);
                sqlParams[i] = param;
            }

            string fieldWhere = CustomAttributeFromProperty(prop);
            sqlStatement += $" WHERE {fieldWhere} = @{fieldWhere}";
            param = new SqlParameter(_atSign + fieldWhere, prop.GetValue(entity, null));
            sqlParams[properties.Count] = param;

            rowAfected = ExecuteUpdate(sqlStatement, sqlParams);

            return (rowAfected > 0);
        }

        /// <summary>
        /// </summary>
        /// <param name="property"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private string BuildFindSqlSentence(PropertyInfo property, Type type, string value = null, bool exacto = true)
        {
            if (_findSQLSentence == null)
            {
                string field = null;
                field = CustomAttributeFromProperty(property);
                _findSQLSentence = $"SELECT * FROM {type.Name} WHERE {field}";
                Type propertyType = property.PropertyType;
                if (!exacto && propertyType.Equals(Type.GetType(DataTypes._STRING)))
                {
                    _findSQLSentence += " like @value";
                }
                else
                {
                    _findSQLSentence += _ValueSt;
                }
            }
            return _findSQLSentence;
        }

        /// <summary>
        /// Ejecuta el query de actualizacion
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <param name="sqlParams"></param>
        /// <returns></returns>
        private int ExecuteUpdate(string sqlStatement, SqlParameter[] sqlParams)
        {
            int rowAfected = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand(sqlStatement, connection);
                {
                    connection.Open();
                    if (sqlParams != null && sqlParams.Length > 0)
                    {
                        command.Parameters.AddRange(sqlParams);
                    }

                    rowAfected = command.ExecuteNonQuery();
                    command.Dispose();
                }
            }
            return rowAfected;
        }

        #endregion Ejecucion consultas dinamicas

        #region EjecucionSP

        /// <summary>
        /// Ejecuta un procedimiento almacenado de creacion de un registro
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="procedureName"></param>
        /// <param name="sqlParam"></param>
        public TEntityObject ExecuteCreateSP<TEntityObject>(TEntityObject entity, string procedureName, params SqlParameter[] sqlParam)
        {
            string pk = GetPrimarykeys();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                //Set Object Id
                Type type = entity.GetType();

                PropertyInfo property = PropertyFromCustomAttribute(pk);
                TypeConverter converter = TypeDescriptor.GetConverter(property.PropertyType);
                string valorIn = property.GetValue(entity)?.ToString();
                string Ident = ExecuteProcedure(procedureName, ExecuteType.ExecuteScalar, connection, sqlParam)?.ToString();
                if (!string.IsNullOrEmpty(Ident) && (string.IsNullOrEmpty(valorIn) || valorIn == _ZeroSt))
                {
                    var result = converter.ConvertFrom(Ident);
                    property.SetValue(entity, result, null);
                }
                return entity;
            }
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

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                returnValue = ExecuteProcedure(procedureName, ExecuteType.ExecuteScalar, connection, sqlParam);
            }

            return returnValue;
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

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                returnValue = (int)ExecuteProcedure(procedureName, ExecuteType.ExecuteNonQuery, connection, sqlParam);
            }

            return returnValue;
        }

        /// <summary>
        /// Metodo publico que ejecuta un procedimiento almacenado de consulta, actualizacion,
        /// creacion o eliminacion
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="executeType"></param>
        /// <param name="sqlParams"></param>
        /// <returns></returns>
        public object ExecuteProcedure(string procedureName, ExecuteType executeType, params SqlParameter[] sqlParam)
        {
            object result = null;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                result = ExecuteProcedure(procedureName, executeType, connection, sqlParam);
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

                        SqlParameter parameter = null;
                        SqlParameter parameter2 = null;
                        //Micro Optimization in foreach
                        int qtyParameters = cmd.Parameters.Count;
                        int qtyParameters2 = sqlParams.Count();
                        string st1 = null;
                        string st2 = null;
                        for (int i = 0; i < qtyParameters; i++)
                        {
                            parameter = cmd.Parameters[i];

                            parameter.IsNullable = true;

                            for (int j = 0; j < qtyParameters2; j++)
                            {
                                parameter2 = sqlParams[i];

                                //micro opt
                                st1 = parameter.ParameterName.Replace(_atSign, string.Empty);
                                st2 = parameter2.ParameterName.Replace(_atSign, string.Empty);

                                if ((string.Compare(st1, st2, true)) == 0)  //Ignoring cases
                                {
                                    parameter.Value = parameter2.Value ?? DBNull.Value;
                                }
                            }
                        }

                        //foreach (SqlParameter parm in cmd.Parameters)
                        //{
                        //    parm.IsNullable = true;
                        //    foreach (SqlParameter parmeter in sqlParams)
                        //    {
                        //        if (parm.ParameterName.Replace(_atSign, "").ToUpper() == parmeter.ParameterName.Replace(_atSign, "").ToUpper())
                        //        {
                        //            parm.Value = parmeter.Value ?? DBNull.Value;
                        //        }
                        //    }
                        //}
                    }

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
                    EndAuditing(ExecutionId, returnObject, _sqlConsoleMessage, true);
                }
            }
            catch (Exception Ex)
            {
                EndAuditing(ExecutionId, Ex.Message + " [" + conection.Database + "] ", _sqlConsoleMessage, false);
                throw new Exception(Ex.Message + " [" + conection.Database + "] ");
            }

            return returnObject;
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado de seleccion de registros
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="sqlParam"></param>
        /// <returns></returns>
        public ICollection<TEntity> ExecuteSelectSP(string procedureName, params SqlParameter[] sqlParam)
        {
            ICollection<TEntity> lEntities = null;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                SqlDataReader reader = (SqlDataReader)ExecuteProcedure(procedureName, ExecuteType.ExecuteReader, connection, sqlParam);

                if (reader.HasRows)
                {
                    lEntities = MapResultsToEntities(ref reader);
                }
                reader.Close();
            }

            return lEntities;
        }

        private void EndAuditing(long executionId, object returnObject, string sqlConsoleMessage, bool isSuccessful)
        {
            string jsonParameters = null;
            //jsonParameters = JsonConvert.SerializeObject(returnObject);

            if (jsonParameters == null)
            {
                jsonParameters = "No existe retorno en ejecucion del procedimiento. (NULL)";
            }

            using (SqlConnection connection = new SqlConnection(GetAuditConnectionString()))
            {
                using (SqlCommand command = new SqlCommand
                {
                    Connection = connection,
                    CommandText = _updateDbLogSP,
                    CommandType = CommandType.StoredProcedure
                })
                {
                    command.Parameters.AddWithValue("@ExecutionId", executionId);
                    command.Parameters.AddWithValue("@ReturnObject", jsonParameters);
                    command.Parameters.AddWithValue("@SqlConsoleMessages", sqlConsoleMessage);
                    command.Parameters.AddWithValue("@ExecutionEndTime", DateTime.Now);
                    command.Parameters.AddWithValue("@IsSuccessful", isSuccessful);

                    connection.Open();
                    int i = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        private string GetAuditConnectionString()
        {
            string connectionString = null;

            connectionString = ConfigurationManager.AppSettings["ConnectionString.Auditoria"];

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("Connection string Auditoria no encontrada Key (ConnectionString.Auditoria) No encontrada.");
            }
            return connectionString;
        }

        private string GetPrimarykeys()
        {
            if (_primaryKeys.Count == 0)
            {
                throw new Exception("La Tabla { " + _nombretabla + " } no contiene llaves primarias.");
            }
            return _primaryKeys.Keys.ElementAt(0);
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
                using (SqlCommand command = new SqlCommand
                {
                    Connection = connection,
                    CommandText = _addDbLogSp,
                    CommandType = CommandType.StoredProcedure
                })
                {
                    List<Parameter> lParameters = new List<Parameter>();
                    int qty = sqlParams.Count();
                    if (sqlParams != null && qty > 0)
                    {
                        SqlParameter param = null;
                        for (int i = 0; i < qty; i++)
                        {
                            param = sqlParams[i];

                            Parameter p = new Parameter
                            {
                                Name = param.ParameterName,
                                Type = param.DbType.ToString(),
                                Value = param.Value?.ToString()
                            };

                            lParameters.Add(p);
                        }
                    }
                    string userID = WindowsIdentity.GetCurrent().Name;
                    string jsonParameters = null;
                    //Get connectionInfo
                    string dbName = conection.Database;
                    string workstationId = conection.WorkstationId;
                    string datasource = conection.DataSource;
                    if (lParameters != null && lParameters.Count() > 0)
                    {
                        jsonParameters = JsonConvert.SerializeObject(lParameters);
                    }
                    else
                    {
                        jsonParameters = procedureName + " - No contiene parametros de entrada.";
                    }

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
            }
            return executionId;
        }

        #endregion EjecucionSP

        #region FuncionesGenerales

        /// <summary>
        /// Crea los parametros a enviar, recibidos mediante la entidad
        /// </summary>
        /// <param name="entityValues"></param>
        /// <returns></returns>
        public SqlParameter[] CreateParametersFromEntity(TEntity entityValues)
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
                param.ParameterName = CustomAttributeFromProperty(property);
                param.Value = property.GetValue(entityValues);
                sqlParams[i] = param;
            }

            return sqlParams;
        }

        public TEntityObject ExecuteCreateSP<TEntityObject>(TEntityObject entity, string procedureName, SqlParameterCollection sqlParamsCollection)
        {
            SqlParameter[] parameters = SqlParameterCollectionToSqlParameterArray(sqlParamsCollection);
            return ExecuteCreateSP<TEntityObject>(entity, procedureName, parameters);
        }

        public object ExecuteGeneralSP(string procedureName, SqlParameterCollection sqlParamsCollection)
        {
            SqlParameter[] parameters = SqlParameterCollectionToSqlParameterArray(sqlParamsCollection);
            return ExecuteGeneralSP(procedureName, parameters);
        }

        public int ExecuteNonQuerySP(string procedureName, SqlParameterCollection sqlParamsCollection)
        {
            SqlParameter[] parameters = SqlParameterCollectionToSqlParameterArray(sqlParamsCollection);
            return ExecuteNonQuerySP(procedureName, parameters);
        }

        public object ExecuteProcedure(string procedureName, ExecuteType executeType, SqlParameterCollection sqlParamsCollection)
        {
            SqlParameter[] parameters = SqlParameterCollectionToSqlParameterArray(sqlParamsCollection);
            return ExecuteProcedure(procedureName, executeType, parameters);
        }

        public ICollection<TEntity> ExecuteSelectSP(string procedureName, SqlParameterCollection sqlParamsCollection = null)
        {
            SqlParameter[] parameters = null;
            if (sqlParamsCollection != null)
            {
                parameters = SqlParameterCollectionToSqlParameterArray(sqlParamsCollection);
            }
            return ExecuteSelectSP(procedureName, parameters);
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
        /// Retorna el alias de la propiedad filtrada
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private string CustomAttributeFromProperty(PropertyInfo property)
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
        /// Obtiene la cadena de conexion parametrizada
        /// </summary>
        /// <returns></returns>
        private string GetConnectionString()
        {
            string projectStage = ConfigurationManager.AppSettings["PROJECT_STAGE"];
            if (string.IsNullOrEmpty(projectStage))
            {
                throw new Exception("App Settings Key = [PROJECT_STAGE], No encontrada en web.config.");
            }
            string connectionString = null;
            switch (projectStage.ToUpper())
            {
                case "DESARROLLO":
                    connectionString = ConfigurationManager.AppSettings["ConnectionString.Desarrollo"];
                    break;

                case "PRUEBAS":
                    connectionString = ConfigurationManager.AppSettings["ConnectionString.Pruebas"];
                    break;

                case "PRODUCCION":
                    connectionString = ConfigurationManager.AppSettings["ConnectionString.Produccion"];
                    break;
                default:
                    connectionString = ConfigurationManager.AppSettings["ConnectionString.Desarrollo"];
                    break;
            }
            if (connectionString == null)
            {
                throw new ArgumentNullException("Connection string no encontrada (PROJECT_STAGE) No encontrado" + projectStage);
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
            if (_identityColumn == string.Empty)
            {
                return false;
            }
            return columnName.Equals(_identityColumn, StringComparison.OrdinalIgnoreCase);
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
                string attributeName = reader.GetName(i);
                PropertyInfo attr = PropertyFromCustomAttribute(attributeName);
                if (attr != null)
                {
                    Type dataType = reader.GetFieldType(i);
                    bool isNull = reader.IsDBNull(i);
                    if (isNull) { attr.SetValue(entity, null); }
                    else
                    {
                        switch (dataType.ToString())
                        {
                            case DataTypes._INT64:
                                attr.SetValue(entity, reader.GetInt64(i));
                                break;

                            case DataTypes._INT32:
                                attr.SetValue(entity, reader.GetInt32(i));
                                break;

                            case DataTypes._INT16:

                                attr.SetValue(entity, reader.GetInt16(i));
                                break;

                            case DataTypes._STRING:
                                attr.SetValue(entity, reader.GetString(i));
                                break;

                            case DataTypes._DATETIME:
                                attr.SetValue(entity, reader.GetDateTime(i));
                                break;

                            case DataTypes._DECIMAL:
                                attr.SetValue(entity, reader.GetDecimal(i));
                                break;

                            case DataTypes._DOUBLE:
                                attr.SetValue(entity, reader.GetDouble(i));
                                break;

                            case DataTypes._BYTE:
                                attr.SetValue(entity, reader.GetByte(i));
                                break;

                            case DataTypes._FLOAT:
                                attr.SetValue(entity, reader.GetFloat(i));
                                break;

                            case DataTypes._BOOL:
                                attr.SetValue(entity, reader.GetBoolean(i));
                                break;

                            case DataTypes._GUID:
                                attr.SetValue(entity, reader.GetGuid(i));
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            return entity;
        }

        /// <summary>
        /// Mapea el datareader a la entidad y devuelve una coleccion
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private ICollection<TEntity> MapResultsToEntities(ref SqlDataReader reader)
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
        /// Retorna la propiedad correspondiente al alias(ColumnAttribute) filtrado
        /// </summary>
        /// <param name="customAttribute"></param>
        /// <returns></returns>
        private PropertyInfo PropertyFromCustomAttribute(string customAttribute)
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
            string columnMapping = properties.FirstOrDefault(a => a.Name == customAttribute)?.PropertyName;
            if (!string.IsNullOrEmpty(columnMapping))
            {
                attr = typeof(TEntity).GetProperty(columnMapping);
            }
            return attr;
        }

        #endregion FuncionesGenerales

        #region Utils

        private static SqlParameter[] SqlParameterCollectionToSqlParameterArray(SqlParameterCollection sqlParamsCollection)
        {
            SqlParameter[] parameters = new SqlParameter[sqlParamsCollection.Count];

            int count = sqlParamsCollection.Count;

            for (int i = 0; i < count; i++)
            {
                parameters[i] = sqlParamsCollection[i];
            }

            return parameters;
        }

        #endregion Utils

        #endregion Metodos
    }
}