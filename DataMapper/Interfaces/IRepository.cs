﻿using DataMapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Servidor.Comun.DataMapper.DataMapper.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Consulta el numero de entidades presentes en la BD
        /// </summary>
        /// <returns>Numero de Entidades en la BD</returns>
        Int64 Count();

        void Create(ref TEntity entity);

        /// <summary>
        /// Realiza la eliminacion de una entidad en la persistencia. (Utilizando su Llave Primaria y
        /// su valor en el Atributo)
        /// </summary>
        /// <param name="entity">Entidad a eliminar de la persistencia.</param>
        /// <returns></returns>
        bool Delete(TEntity entity);

        /// <summary>
        /// Ejecuta un procedimiento almacenado de crecion de un registro
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="procedureName"></param>
        /// <param name="sqlParam"></param>
        TEntityObj ExecuteCreateSP<TEntityObj>(TEntityObj entity, string procedureName, params SqlParameter[] sqlParam);

        TEntityObj ExecuteCreateSP<TEntityObj>(TEntityObj entity, string procedureName, SqlParameterCollection sqlParamsCollection);

        /// <summary>
        /// Ejecuta un sp que devuelve un boolean.
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="sqlParam"></param>
        /// <returns></returns>
        object ExecuteGeneralSP(string procedureName, params SqlParameter[] sqlParam);

        object ExecuteGeneralSP(string procedureName, SqlParameterCollection sqlParamsCollection);

        /// <summary>
        /// Ejecuta un procedimiento almacenado de actualizacion o eliminacion
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="sqlParam"></param>
        /// <returns></returns>
        int ExecuteNonQuerySP(string procedureName, params SqlParameter[] sqlParam);

        int ExecuteNonQuerySP(string procedureName, SqlParameterCollection sqlParamsCollection);

        object ExecuteProcedure(string procedureName, ExecuteType executeType, params SqlParameter[] sqlParam);

        object ExecuteProcedure(string procedureName, ExecuteType executeType, SqlParameterCollection sqlParamsCollection);

        /// <summary>
        /// Ejecuta un procedimiento almacenado de seleccion de registros
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="sqlParam"></param>
        /// <returns></returns>
        ICollection<TEntity> ExecuteSelectSP(string procedureName, params SqlParameter[] sqlParam);

        ICollection<TEntity> ExecuteSelectSP(string procedureName, SqlParameterCollection sqlParamsCollection);

        /// <summary>
        /// Encuentra una o varias entidades que cumplan con el valor pasado por valor al metodo.
        /// </summary>
        /// <param name="value">Valor por el cual van a ser buscadas las entidades en la persistencia</param>
        /// <param name="Attribute">Nombre del atributo en la entidad EX: "CIU_IdCiudad"</param>
        /// <returns>
        /// Una colección de objetos (entidaddes) las cuales cumplen con la condición en le valor del
        /// attributo, si no existe ninguna entidad que cumpla con la condición se retorna null.
        /// </returns>
        ICollection<TEntity> findByAttribute(String value, String Attribute, bool exacto, string campoOrdenar = null, bool orderDesc = true);

        /// <summary>
        /// Retorna todas las entidades en un IEnumerable a partir del query enviado
        /// </summary>
        /// <returns></returns>
        ICollection<TEntity> GetAll(string campoOrdenar = null, bool orderDesc = true);

        /// <summary>
        /// Actualiza la entidad que es pasada al metodo
        /// </summary>
        /// <param name="entity">Entidad con los valores en los atributos cambiados</param>
        /// <returns>
        /// <code>
        /// true
        /// </code>
        /// . Si la actualización se realizo de manera exitosa.
        /// <code>
        /// false
        /// </code>
        /// . si no se realizo la actualización en la persistencia.
        /// </returns>
        bool Update(TEntity entity);
    }
}