using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Configuration;
using Servidor.Comun.DataMapper.DataMapper.Interfaces;
using System.ComponentModel;
using Servidor.Comun.DataMapper;

namespace DataMapper
{
    /// <summary>
    /// Mapea los datareader a las entidades o inserta en la base de datos.
    /// </summary>
    public sealed class DataMapper<TEntity> : IRepository<TEntity> where TEntity : class
    {

        private static String _primaryKey = null;
        private String _connectionString = null;
        private String _findSQLSentence = null;
        private String _insertStatement = null;
        private String _identityColumn = null;
        private String _countSentence = null;

        /// <summary>
        /// Atributo utilizado para evitar problemas con multithreading en el singleton.
        /// </summary>
        private static object syncRoot = new Object();

        #region Crear instancia

        private static volatile DataMapper<TEntity> instancia;

        private DataMapper() { }



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
                        }
                    }
                }
                return instancia;
            }
        }
        #endregion


        #region Metodos


        #region ADO insert statements
        /// <summary>
        /// Crea la consulta para realizar la insercion de un registro
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private String buildInsertStatement(TEntity entity)
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
                    if (!isIdentity(prop))
                    {
                        _insertStatement += (i < properties.Count - 1) ? prop + ", " : prop;
                    }
                }
                _insertStatement += " )" + " VALUES (";
                //Avoid security issues using parameters
                for (int i = 0; i < properties.Count; i++)
                {
                    PropertyInfo property = properties.ElementAt(i);
                    String prop = customAttributeFromProperty(property);
                    if (!isIdentity(prop))
                    {
                        _insertStatement += (i < properties.Count - 1) ? "@" + prop + ", " : "@" + prop;
                    }
                }
                _insertStatement += "); SELECT SCOPE_IDENTITY();";
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
            if (_insertStatement == null)
            {
                _insertStatement = buildInsertStatement(entity);
            }
            SqlCommand command = new SqlCommand(_insertStatement, connection);
            Type type = entity.GetType();
            List<PropertyInfo> properties = type.GetProperties().ToList();

            for (int i = 0; i < properties.Count; i++)
            {
                PropertyInfo property = properties.ElementAt(i);
                String prop = customAttributeFromProperty(property);
                if (!isIdentity(prop))
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
        /// Obtiene todos los registros de una tabla
        /// </summary>
        /// <returns></returns>
        public ICollection<TEntity> GetAll()
        {
            ICollection<TEntity> lEntities = null;
            using (SqlConnection con = new SqlConnection(getConnectionString()))
            {
                SqlDataReader reader = null;
                Type type = this.GetType().GenericTypeArguments[0];
                String sqlSentece = "SELECT  * FROM " + type.Name;
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
                    Object IdNumber = command.ExecuteScalar();

                    Type type = entity.GetType();
                    String pk = getPrimarykeys(entity);
                    PropertyInfo property = propertyFromCustomAttribute(pk);
                    TypeConverter converter = TypeDescriptor.GetConverter(property.PropertyType);
                    var result = converter.ConvertFrom(IdNumber.ToString());
                    property.SetValue(entity, result, null);
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
            String pkey = getPrimarykeys(entity);

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
            String pkey = getPrimarykeys(entity);

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
        /// <returns>La entidad que contiene el valor indicado, de otra manera null</returns>
        public ICollection<TEntity> findByAttribute(string value, string attribute, bool exacto = true)
        {
            ICollection<TEntity> lEntities = null;
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                SqlDataReader reader = null;
                Type type = this.GetType().GenericTypeArguments[0];
                PropertyInfo property = type.GetProperty(attribute);

                String sqlSentece = buildFindSqlSentence(property, type, value, exacto);
                using (SqlCommand command = new SqlCommand(sqlSentece, con))
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
            }
            return lEntities;
        }


        /// <summary>
        /// Consulta la cantidad de entidades en la BD
        /// </summary>
        /// <returns>Numero de entidades con persistencia en BD</returns>
        public long Count()
        {
            Type type = this.GetType().GenericTypeArguments[0];
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
                String field = customAttributeFromProperty(property);
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
        public TEntityObject ExecuteCreateSP<TEntityObject>(TEntityObject entity,string procedureName, params SqlParameter[] sqlParam)
        {
            //TEntityObject entity = (TEntityObject)Activator.CreateInstance(typeof(TEntityObject)); ;
            using (SqlConnection connection = new SqlConnection(getConnectionString()))
            {
                connection.Open();
                //Set Object Id
                Type type = entity.GetType();
                String pk = getPrimarykeys(entity);
                PropertyInfo property = propertyFromCustomAttribute(pk);
                TypeConverter converter = TypeDescriptor.GetConverter(property.PropertyType);
                string Ident = ExecuteProcedure(procedureName, ExecuteType.ExecuteScalar, connection, sqlParam)?.ToString();
                var result = converter.ConvertFrom(Ident);
                property.SetValue(entity, result, null);
                return entity;
            }
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
        /// Ejecuta un procedimiento almacenado de consulta, actualizacion, creacion o eliminacion
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="executeType"></param>
        /// <param name="conection"></param>
        /// <param name="sqlParams"></param>
        /// <returns></returns>
        private object ExecuteProcedure(string procedureName, ExecuteType executeType, SqlConnection conection, params SqlParameter[] sqlParams)
        {
            object returnObject = null;
            using (var cmd = conection.CreateCommand())
            {
                cmd.CommandText = procedureName;
                cmd.CommandType = CommandType.StoredProcedure;
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

            return returnObject;
        }
        #endregion


        #region FuncionesGenerales

        /// <summary>
        /// Obtiene la llave primaria de una tabla
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private String getPrimarykeys<TEntityObj>(TEntityObj entity)
        {
            if (_primaryKey != null || !String.IsNullOrEmpty(_primaryKey))
            {
                return _primaryKey;
            }
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand("paObtenerLlavePrimaria", con);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@NombreTabla", entity.GetType().Name);

                SqlParameter outParameter = new SqlParameter();
                outParameter.ParameterName = "@PrimaryKey";
                outParameter.SqlDbType = SqlDbType.NVarChar;
                outParameter.Direction = ParameterDirection.Output;
                outParameter.Value = "";
                outParameter.Size = 50;
                command.Parameters.Add(outParameter);

                con.Open();
                command.ExecuteNonQuery();
                _primaryKey = outParameter.Value.ToString();
            }

            return _primaryKey;
        }

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
        private bool isIdentity(string columnName)
        {
            if (_identityColumn == null)
            {
                int result = 0;
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("PaIsIdentity", con);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@COLUMN_NAME", columnName);
                    //FIXME Save identity columns on memory like singleton handles instancies
                    con.Open();
                    result = (int)command.ExecuteScalar();
                    _identityColumn = (result == 0) ? null : columnName;
                }
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
            var properties = typeof(TEntity).GetProperties()
                      .Where(p => p.IsDefined(typeof(ColumnAttribute), false))
                      .Select(p => new
                      {
                          PropertyName = p.Name,
                          p.GetCustomAttributes(typeof(ColumnAttribute),
                                  false).Cast<ColumnAttribute>().Single().Name
                      });
            String columnMapping = properties.FirstOrDefault(a => a.Name == customAttribute).PropertyName;
            PropertyInfo attr = typeof(TEntity).GetProperty(columnMapping);
            return attr;
        }

        /// <summary>
        /// Retorna el alias de la propiedad filtrada
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private String customAttributeFromProperty(PropertyInfo property)
        {
            var customAttri = property.GetCustomAttributes(false);
            var columnMapping = customAttri.FirstOrDefault(a => a.GetType() == typeof(ColumnAttribute));
            ColumnAttribute map = columnMapping as ColumnAttribute;
            return map.Name;
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
            SqlParameter[] parameters = sqlParameterCollectionToSqlParameterArray(sqlParamsCollection);
            return ExecuteSelectSP(procedureName, parameters);
        }

        public TEntityObject ExecuteCreateSP<TEntityObject>(TEntityObject entity, string procedureName, SqlParameterCollection sqlParamsCollection)
        {
            SqlParameter[] parameters = sqlParameterCollectionToSqlParameterArray(sqlParamsCollection);
            return ExecuteCreateSP<TEntityObject>(entity,procedureName, parameters);
        }

        public int ExecuteNonQuerySP(string procedureName, SqlParameterCollection sqlParamsCollection)
        {
            SqlParameter[] parameters = sqlParameterCollectionToSqlParameterArray(sqlParamsCollection);
            return ExecuteNonQuerySP(procedureName, parameters);
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


