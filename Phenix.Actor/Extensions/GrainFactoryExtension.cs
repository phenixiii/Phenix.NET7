using System;
using Phenix.Core.Data;

namespace Orleans
{
    /// <summary>
    /// Êý¾Ý×ÖµäÀ©Õ¹
    /// </summary>
    public static class GrainFactoryExtension
    {
        #region GetGrain

        /// <summary>
        /// <summary>Gets a reference to a grain.</summary>
        /// </summary>
        /// <typeparam name="TGrainInterface">The interface to get.</typeparam>
        /// <param name="grainFactory">IGrainFactory</param>
        /// <param name="primaryKey">The primary key of the grain.</param>
        /// <param name="primaryKeyExtension">The key extension of the grain.</param>
        /// <param name="grainClassNamePrefix">An optional class name prefix used to find the runtime type of the grain.</param>
        /// <returns>A reference to the specified grain.</returns>
        public static TGrainInterface GetGrain<TGrainInterface>(this IClusterClient grainFactory, string primaryKey, string primaryKeyExtension, string grainClassNamePrefix = null)
            where TGrainInterface : IGrainWithStringKey
        {
            if (grainFactory == null)
                throw new ArgumentNullException(nameof(grainFactory));

            return grainFactory.GetGrain<TGrainInterface>(Standards.FormatCompoundKey(primaryKey, primaryKeyExtension), grainClassNamePrefix);
        }

        /// <summary>
        /// A GetGrain overload that returns the runtime type of the grain interface and returns the grain cast to
        /// TGrainInterface.
        /// 
        /// The main use-case is when you want to get a grain whose type is unknown at compile time (e.g. generic type parameters).
        /// </summary>
        /// <typeparam name="TGrainInterface">The output type of the grain</typeparam>
        /// <param name="grainFactory">IGrainFactory</param>
        /// <param name="grainInterfaceType">the runtime type of the grain interface</param>
        /// <param name="primaryKey">The primary key of the grain.</param>
        /// <param name="primaryKeyExtension">The key extension of the grain.</param>
        /// <returns>the requested grain with the given grainID and grainInterfaceType</returns>
        public static TGrainInterface GetGrain<TGrainInterface>(this IClusterClient grainFactory, Type grainInterfaceType, string primaryKey, string primaryKeyExtension)
            where TGrainInterface : IGrain
        {
            if (grainFactory == null)
                throw new ArgumentNullException(nameof(grainFactory));

            return grainFactory.GetGrain<TGrainInterface>(grainInterfaceType, Standards.FormatCompoundKey(primaryKey, primaryKeyExtension));
        }

        /// <summary>
        /// A GetGrain overload that returns the runtime type of the grain interface and returns the grain cast to
        /// <see paramref="TGrainInterface" />. It is the caller's responsibility to ensure <see paramref="TGrainInterface" />
        /// extends IGrain, as there is no compile-time checking for this overload.
        /// 
        /// The main use-case is when you want to get a grain whose type is unknown at compile time.
        /// </summary>
        /// <param name="grainFactory">IGrainFactory</param>
        /// <param name="grainInterfaceType">the runtime type of the grain interface</param>
        /// <param name="primaryKey">The primary key of the grain.</param>
        /// <param name="primaryKeyExtension">The key extension of the grain.</param>
        /// <returns></returns>
        public static IGrain GetGrain(this IClusterClient grainFactory, Type grainInterfaceType, string primaryKey, string primaryKeyExtension)
        {
            if (grainFactory == null)
                throw new ArgumentNullException(nameof(grainFactory));

            return grainFactory.GetGrain(grainInterfaceType, Standards.FormatCompoundKey(primaryKey, primaryKeyExtension));
        }

        #endregion
    }
}