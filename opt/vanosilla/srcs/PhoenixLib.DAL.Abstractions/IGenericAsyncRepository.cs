﻿// WingsEmu
// 
// Developed by NosWings Team

using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhoenixLib.DAL
{
    /// <summary>
    ///     IGenericAsyncRepository permits to manage the data storage through async operations
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <typeparam name="TObjectId"></typeparam>
    public interface IGenericAsyncRepository<TObject, in TObjectId> where TObject : class
    {
        /// <summary>
        ///     Asynchronously returns all objects that are stored in data storage
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TObject>> GetAllAsync();

        /// <summary>
        ///     Returns object by its id in data storage
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TObject> GetByIdAsync(TObjectId id);

        /// <summary>
        ///     Returns all objects with the given ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<IEnumerable<TObject>> GetByIdsAsync(IEnumerable<TObjectId> ids);


        /// <summary>
        ///     Asynchronously inserts or updates object parameter into data storage
        ///     Parameter's id will be set in this method
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        Task<TObject> SaveAsync(TObject obj);


        /// <summary>
        ///     Asynchronously inserts or updates objects given in parameter into data storage
        ///     Parameter's id will be set in this method
        /// </summary>
        /// <param name="objs"></param>
        /// <returns></returns>
        Task<IEnumerable<TObject>> SaveAsync(IReadOnlyList<TObject> objs);

        /// <summary>
        ///     Asynchronously delete the object from the data storage with the given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteByIdAsync(TObjectId id);

        /// <summary>
        ///     Asynchronously delete all the objects from the data storage with the given id
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task DeleteByIdsAsync(IEnumerable<TObjectId> ids);
    }
}