﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// MIT License
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the ""Software""), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Diagnostics;
using System.Reflection;
using Grace.DependencyInjection;
using Microsoft.HealthVault.Connection;
using Microsoft.HealthVault.Extensions;
using UIKit;

namespace Microsoft.HealthVault.Client
{
    internal static class ClientIoc
    {
        internal static void EnsureTypesRegistered()
        {
            // CoreClientIoc will call RegisterPlatformTypes.  Do all our registering there.
            CoreClientIoc.EnsureTypesRegistered(RegisterPlatformTypes);
        }

        private static void RegisterPlatformTypes(DependencyInjectionContainer container)
        {
            IosBrowserAuthBroker authBroker = container.Locate<IosBrowserAuthBroker>();
            container.Configure(c => c.ExportInstance(authBroker).As<IosBrowserAuthBroker>());
            container.Configure(c => c.ExportInstance(authBroker).As<IBrowserAuthBroker>());

            container.RegisterSingleton<ISecretStore, IosSecretStore>();
            container.RegisterSingleton<IMessageHandlerFactory, IosMessageHandlerFactory>();

            RegisterTelemetryInformation(container);
        }

        /// <summary>
        /// Register Telemetry information for SDK
        /// </summary>
        /// <param name="container">IOC container</param>
        private static void RegisterTelemetryInformation(DependencyInjectionContainer container)
        {
            var sdkTelemetryInformation = new SdkTelemetryInformation
            {
                Category = HealthVaultConstants.SdkTelemetryInformationCategories.IosClient,
                FileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion,
                OsInformation = $"iOS {UIDevice.CurrentDevice.SystemVersion}"
            };

            container.Configure(c => c.ExportInstance(sdkTelemetryInformation).As<SdkTelemetryInformation>());
        }
    }
}
