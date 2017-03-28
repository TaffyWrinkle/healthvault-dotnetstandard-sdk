﻿using System;

namespace Microsoft.HealthVault.Client
{
    /// <summary>
    /// Factory for creating client connections to HealthVault.
    /// </summary>
    public interface IHealthVaultConnectionFactory 
    {
        /// <summary>
        /// Sets the client configuration to use.
        /// </summary>
        /// <param name="clientHealthVaultConfiguration">The configuration to use.</param>
        /// <exception cref="InvalidOperationException">Thrown when called after calling <see cref="GetSodaConnection"/>.</exception>
        /// <remarks>This can only be set before calling <see cref="GetSodaConnection"/>. After calling it,
        /// this property cannot be set and no settings on the object can be changed.</remarks>
        void SetConfiguration(ClientHealthVaultConfiguration clientHealthVaultConfiguration);

        /// <summary>
        /// Gets a connection to access HealthVault.
        /// </summary>
        /// <returns>A connection to access HealthVault.</returns>
        /// <exception cref="InvalidOperationException">Thrown when called before calling <see cref="SetConfiguration"/> with required values.</exception>
        /// <remarks>This will not perform any authentication. Authentication happens on the first call to AuthenticateAsync or when the first method is called.</remarks>
        IHealthVaultSodaConnection GetSodaConnection();
    }
}
