﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Microsoft.HealthVault.Authentication;
using Microsoft.HealthVault.Configuration;
using Microsoft.HealthVault.Exceptions;
using Microsoft.HealthVault.Helpers;
using Microsoft.HealthVault.ItemTypes;
using Microsoft.HealthVault.PlatformInformation;
using Microsoft.HealthVault.Thing;

namespace Microsoft.HealthVault.DesktopWeb.Common
{
    public class ApplicationConfiguration
    {
        #region configuration key constants

        // Url configuration keys
        private const string ConfigKeyHealthServiceUrl = "HealthServiceUrl";
        private const string ConfigKeyRestHealthServiceUrl = "RestHealthServiceUrl";
        private const string ConfigKeyShellUrl = "ShellUrl";

        // Application related configuration keys
        private const string ConfigKeyAppId = "ApplicationId";
        private const string ConfigKeyApplicationCertificateFileName = "ApplicationCertificateFilename";
        private const string ConfigKeyApplicationCertificatePassword = "ApplicationCertificatePassword";
        private const string ConfigKeyCertSubject = "AppCertSubject";
        private const string ConfigKeySignatureCertStoreLocation = "SignatureCertStoreLocation";
        private const string ConfigKeySupportedType = "SupportedTypeVersions";
        private const string ConfigKeyUseLegacyTypeVersionSupport = "UseLegacyTypeVersionSupport";
        private const string ConfigKeyMultiInstanceAware = "MultiInstanceAware";
        private const string ConfigKeyServiceInfoDefaultCacheTtlMilliseconds = "ServiceInfoDefaultCacheTtlMilliseconds";

        // Request/Response related configuration keys
        private const string ConfigKeyDefaultRequestTimeout = "DefaultRequestTimeout";
        private const string ConfigKeyDefaultRequestTimeToLive = "DefaultRequestTimeToLive";
        private const string ConfigKeyRequestRetryOnInternal500Count = "RequestRetryOnInternal500Count";
        private const string ConfigKeyRequestRetryOnInternal500SleepSeconds = "RequestRetryOnInternal500SleepSeconds";
        private const string ConfigKeyRequestCompressionThreshold = "RequestCompressionThreshold";
        private const string ConfigKeyRequestCompressionMethod = "RequestCompressionMethod";
        private const string ConfigKeyResponseCompressionMethods = "ResponseCompressionMethods";
        private const string ConfigKeyDefaultInlineBlobHashBlockSize = "DefaultInlineBlobHashBlockSize";

        // Security related keys
        private const string ConfigKeyHmacAlgorithmName = "HmacAlgorithmName";
        private const string ConfigKeyHashAlgorithmName = "HashAlgorithmName";
        private const string ConfigKeySignatureHashAlgorithmName = "SignatureHashAlgorithmName";
        private const string ConfigKeySignatureAlgorithmName = "SignatureAlgorithmName";
        private const string ConfigSymmetricAlgorithmName = "SymmetricAlgorithmName";

        // Connection related keys
        private const string ConfigKeyConnectionUseHttpKeepAlive = "ConnectionUseHttpKeepAlive";
        private const string ConfigKeyConnectionLeaseTimeout = "ConnectionLeaseTimeout";
        private const string ConfigKeyConnectionMaxIdleTime = "ConnectionMaxIdleTime";

        #endregion

        public Uri DefaultHealthVaultUrl
        {
            get
            {
                if (_healthVaultRootUrl == null)
                {
                    _healthVaultRootUrl = ApplicationConfigurationManager.GetConfigurationUrl(ConfigKeyHealthServiceUrl, true);
                }

                return _healthVaultRootUrl;
            }
        }

        private volatile Uri _healthVaultRootUrl;

        /// <summary>
        /// Gets the HealthVault Shell URL for
        /// the configured default instance of the HealthVault web-service.
        /// </summary>
        ///
        /// <remarks>
        /// This property corresponds to the "ShellUrl" configuration
        /// value.
        /// </remarks>
        ///
        public virtual Uri DefaultHealthVaultShellUrl
        {
            get
            {
                if (_shellUrl == null)
                {
                    _shellUrl = ApplicationConfigurationManager.GetConfigurationUrl(ConfigKeyShellUrl, true);
                }

                return _shellUrl;
            }
        }

        private volatile Uri _shellUrl;

        /// <summary>
        /// Gets the application's unique identifier.
        /// </summary>
        ///
        /// <remarks>
        /// This property corresponds to the "ApplicationId" configuration
        /// value.
        /// </remarks>
        ///
        public virtual Guid ApplicationId
        {
            get
            {
                if (_appId == Guid.Empty)
                {
                    _appId = ApplicationConfigurationManager.GetConfigurationGuid(ConfigKeyAppId);
                }

                return _appId;
            }
        }

        private Guid _appId;

        /// <summary>
        /// Gets the request timeout in seconds.
        /// </summary>
        ///
        /// <remarks>
        /// This value is used to set the <see cref="HttpWebRequest.Timeout"/> property
        /// when making the request to HealthVault. The timeout is the number of seconds that a
        /// request will wait for a response from HealtVault. If the method response is not
        /// returned within the time-out period the request will throw a <see cref="System.Net.WebException"/>
        /// with the <see cref="System.Net.WebException.Status">Status</see> property set to
        /// <see cref="System.Net.WebExceptionStatus.Timeout"/>.
        /// This property corresponds to the "defaultRequestTimeout" configuration
        /// value. The value defaults to 30 seconds.
        /// </remarks>
        ///
        public virtual int DefaultRequestTimeout
        {
            get
            {
                if (!_configurationRequestTimeoutInitialized)
                {
                    int tempRequestTimeout = ApplicationConfigurationManager.GetConfigurationInt32(ConfigKeyDefaultRequestTimeout,
                        DefaultDefaultRequestTimeout);

                    // Note, -1 signifies an infinite timeout so that is OK.
                    if (tempRequestTimeout < -1)
                    {
                        tempRequestTimeout = DefaultDefaultRequestTimeout;
                    }

                    _configuredRequestTimeout = tempRequestTimeout;
                    _configurationRequestTimeoutInitialized = true;
                }

                return _configuredRequestTimeout;
            }
        }

        private volatile int _configuredRequestTimeout;
        private volatile bool _configurationRequestTimeoutInitialized;

        /// <summary>
        /// The default request time out value.
        /// </summary>
        protected const int DefaultDefaultRequestTimeout = 30;

        /// <summary>
        /// Gets the request time to live in seconds.
        /// </summary>
        ///
        /// <remarks>
        /// This property defines the "msg-ttl" in the HealthVault request header XML. It determines
        /// how long the same XML can be used before HealthVault determines the request invalid.
        /// This property corresponds to the "defaultRequestTimeToLive" configuration
        /// value. The value defaults to 1800 seconds.
        /// </remarks>
        ///
        public virtual int DefaultRequestTimeToLive
        {
            get
            {
                if (!_configuredRequestTimeToLiveInitialized)
                {
                    int tempRequestTimeToLive = ApplicationConfigurationManager.GetConfigurationInt32(ConfigKeyDefaultRequestTimeToLive,
                        DefaultDefaultRequestTimeToLive);

                    if (tempRequestTimeToLive < -1)
                    {
                        tempRequestTimeToLive = DefaultDefaultRequestTimeToLive;
                    }

                    _configuredRequestTimeToLive = tempRequestTimeToLive;
                    _configuredRequestTimeToLiveInitialized = true;
                }

                return _configuredRequestTimeToLive;
            }
        }

        private volatile int _configuredRequestTimeToLive;
        private volatile bool _configuredRequestTimeToLiveInitialized;

        /// <summary>
        /// The default request time to live value.
        /// </summary>
        protected const int DefaultDefaultRequestTimeToLive = 30 * 60;

        /// <summary>
        /// Gets the number of retries the .NET APIs will make when getting an internal
        /// error response (error 500) from HealthVault.
        /// </summary>
        ///
        /// <remarks>
        /// This property corresponds to the "RequestRetryOnInternal500" configuration
        /// value. The value defaults to 2.
        /// </remarks>
        ///
        public virtual int RetryOnInternal500Count
        {
            get
            {
                if (!_retryOnInternal500CountInitialized)
                {
                    _retryOnInternal500Count = ApplicationConfigurationManager.GetConfigurationInt32(ConfigKeyRequestRetryOnInternal500Count,
                        DefaultRetryOnInternal500Count);
                    _retryOnInternal500CountInitialized = true;
                }

                return _retryOnInternal500Count;
            }
        }

        private volatile int _retryOnInternal500Count;
        private volatile bool _retryOnInternal500CountInitialized;

        /// <summary>
        /// The default number of internal retries.
        /// </summary>
        protected const int DefaultRetryOnInternal500Count = 2;

        /// <summary>
        /// Gets the sleep duration in seconds between retries due to HealthVault returning
        /// an internal error (error 500).
        /// </summary>
        ///
        /// <remarks>
        /// This property corresponds to the "RequestRetryOnInternal500SleepSeconds" configuration
        /// value. The value defaults to 1 second.
        /// </remarks>
        ///
        public virtual int RetryOnInternal500SleepSeconds
        {
            get
            {
                if (!_retryOnInternal500SleepSecondsInitialized)
                {
                    _retryOnInternal500SleepSeconds =
                        ApplicationConfigurationManager.GetConfigurationInt32(ConfigKeyRequestRetryOnInternal500SleepSeconds,
                            DefaultRetryOnInternal500SleepSeconds);
                    _retryOnInternal500SleepSecondsInitialized = true;
                }

                return _retryOnInternal500SleepSeconds;
            }
        }

        private volatile int _retryOnInternal500SleepSeconds;
        private volatile bool _retryOnInternal500SleepSecondsInitialized;

        /// <summary>
        /// Default sleep duration in seconds.
        /// </summary>
        protected const int DefaultRetryOnInternal500SleepSeconds = 1;

        /// <summary>
        /// Gets the size in bytes of the block used to hash inlined BLOB data.
        /// </summary>
        ///
        /// <remarks>
        /// This property corresponds to the "defaultInlineBlobHashBlockSize" configuration
        /// value. The value defaults to 2MB.
        /// </remarks>
        ///
        public virtual int InlineBlobHashBlockSize
        {
            get
            {
                if (!_configuredInlineBlobHashBlockSizeInitilialized)
                {
                    int tempBlobHashSize = ApplicationConfigurationManager.GetConfigurationInt32(
                        ConfigKeyDefaultInlineBlobHashBlockSize,
                        BlobHasher.DefaultInlineBlobHashBlockSizeBytes);

                    if (tempBlobHashSize < 1)
                    {
                        tempBlobHashSize =
                            BlobHasher.DefaultInlineBlobHashBlockSizeBytes;
                    }

                    _configuredInlineBlobHashBlockSize = tempBlobHashSize;
                    _configuredInlineBlobHashBlockSizeInitilialized = true;
                }

                return _configuredInlineBlobHashBlockSize;
            }
        }

        private volatile int _configuredInlineBlobHashBlockSize;
        private volatile bool _configuredInlineBlobHashBlockSizeInitilialized;

        /// <summary>
        /// Gets the type version identifiers of types supported by this application.
        /// </summary>
        ///
        /// <remarks>
        /// Although most applications don't need this configuration setting, if an application
        /// calls <see cref="HealthRecordAccessor.GetItem(Guid)"/> or makes any query to HealthVault
        /// that doesn't specify the type identifier in the filter, this configuration setting
        /// will tell HealthVault the format of the type to reply with. For example, if a web
        /// application has two servers and makes a call to GetItem for EncounterV1 and the
        /// application authorization is set to the EncounterV1 format then the application will
        /// get EncounterV1 instances back even if the record contains Encounter v2 instances. Now
        /// the application wants to upgrade to Encounter v2 without having application down-time.
        /// In order to do this, one of the application servers must be updated to Encounter v2 while
        /// the other still works with EncounterV1. If we were to rely solely on application
        /// authorization one of the servers would be broken during the upgrade. However, by using
        /// this configuration value to specify what type version the server supports (rather than
        /// the application), then both servers can continue to work while the application is
        /// upgraded.
        /// </remarks>
        ///
        /// <exception cref="InvalidConfigurationException">
        /// If the configuration contains the name of a type that is not registered as a type handler
        /// in <see cref="ItemTypeManager"/>.
        /// </exception>
        ///
        public virtual IList<Guid> SupportedTypeVersions
        {
            get
            {
                if (_supportedTypeVersions == null)
                {
                    _supportedTypeVersions = GetSupportedTypeVersions();
                }

                return _supportedTypeVersions;
            }
        }

        private volatile IList<Guid> _supportedTypeVersions;

        private IList<Guid> GetSupportedTypeVersions()
        {
            Collection<Guid> supportedTypeVersions = new Collection<Guid>();

            string typeVersionsConfig = ApplicationConfigurationManager.GetConfigurationString(ConfigKeySupportedType, String.Empty);
            string[] typeVersions =
                typeVersionsConfig.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string typeVersionClassName in typeVersions)
            {
                if (ItemTypeManager.TypeHandlersByClassName.ContainsKey(typeVersionClassName))
                {
                    supportedTypeVersions.Add(ItemTypeManager.TypeHandlersByClassName[typeVersionClassName].TypeId);
                }
                else
                {
                    throw new InvalidConfigurationException(Resources.InvalidSupportedTypeVersions);
                }
            }

            return supportedTypeVersions;
        }

        /// <summary>
        /// Gets a value indicating whether or not legacy type versioning support should be used.
        /// </summary>
        ///
        /// <remarks>
        /// Type versions support was initially determined by an applications base authorizations
        /// and/or the <see cref="HealthRecordView.TypeVersionFormat"/>. Some of these behaviors
        /// were unexpected which led to changes to automatically put the <see cref="HealthRecordFilter.TypeIds"/>
        /// and <see cref="HealthVaultConfiguration.SupportedTypeVersions"/> into the
        /// <see cref="HealthRecordView.TypeVersionFormat"/> automatically for developers. This 
        /// exhibits the expected behavior for most applications. However, in some rare cases 
        /// applications may need to revert back to the original behavior. When this property
        /// returns true the original behavior will be observed. If false, the new behavior will
        /// be observed. This property defaults to false and can be changed in the web.config file
        /// "UseLegacyTypeVersionSupport" setting.
        /// </remarks>
        ///
        public virtual bool UseLegacyTypeVersionSupport
        {
            get
            {
                if (!_useLegacyTypeVersionSupportInitialized)
                {
                    _useLegacyTypeVersionSupport = ApplicationConfigurationManager.GetConfigurationBoolean(ConfigKeyUseLegacyTypeVersionSupport, false);
                    _useLegacyTypeVersionSupportInitialized = true;
                }
                return _useLegacyTypeVersionSupport;
            }
        }

        private volatile bool _useLegacyTypeVersionSupport;
        private volatile bool _useLegacyTypeVersionSupportInitialized;

        /// <summary>
        /// Gets the value which indicates whether the application is able to handle connecting to multiple
        /// instances of the HealthVault web-service.
        /// </summary>
        ///
        /// <remarks>
        /// This setting defaults to <b>true</b> and can be set in an application
        /// configuration file, using the "MultiInstanceAware" setting key.
        /// <p>
        /// Applications in HealthVault can be configured to support more than one instance of the HealthVault web-service.
        /// In such a case, and when the MultiInstanceAware configuration is set to <b>true</b>, all redirects generated
        /// through the HealthVault .NET API will have a flag set indicating that the application is able to deal with
        /// HealthVault accounts that may reside in other HealthVault instances.  In such a case, HealthVault Shell can
        /// redirect back with an account associated with any one of the instances of the HealthVault web-service which
        /// the application has chosen to support.  The application may then need to be able to handle connecting to the
        /// appropriate instance of the HealthVault web-service for each account.
        /// </p>
        /// <p>
        /// For more information see the <a href="http://go.microsoft.com/?linkid=9830913">Global HealthVault Architecture</a> article.
        /// </p>
        /// </remarks>
        public virtual bool MultiInstanceAware
        {
            get
            {
                if (!_multiInstanceAwareInitialized)
                {
                    _multiInstanceAware = ApplicationConfigurationManager.GetConfigurationBoolean(ConfigKeyMultiInstanceAware, true);
                    _multiInstanceAwareInitialized = true;
                }

                return _multiInstanceAware;
            }
        }

        private volatile bool _multiInstanceAware;
        private volatile bool _multiInstanceAwareInitialized;

        /// <summary>
        /// Gets the amount of time, in milliseconds, that the application's connection can
        /// remain idle before the HealthVault framework closes the connection.
        /// </summary>
        ///
        /// <remarks>
        /// This default value is 110 seconds of inactivity.
        /// <p>
        /// This setting only applies when using HTTP Persistent Connections
        /// <see cref="HealthVaultConfiguration.ConnectionUseHttpKeepAlive"/>.  
        /// </p>
        /// <p>
        /// Setting this property to -1 indicates the connection should never
        /// time out.
        /// </p>
        /// <p>
        /// This property corresponds to the "ConnectionMaxIdleTime" configuration value.
        /// </p>
        /// </remarks>
        public virtual int ConnectionMaxIdleTime
        {
            get
            {
                if (!_connectionMaxIdleTimeInitialized)
                {
                    _connectionMaxIdleTime = ApplicationConfigurationManager.GetConfigurationInt32(ConfigKeyConnectionMaxIdleTime, 110 * 1000);

                    if (_connectionMaxIdleTime < -1)
                    {
                        _connectionMaxIdleTime = -1;
                    }

                    _connectionMaxIdleTimeInitialized = true;
                }

                return _connectionMaxIdleTime;
            }
        }

        private volatile int _connectionMaxIdleTime;
        private volatile bool _connectionMaxIdleTimeInitialized;

        /// <summary>
        /// Gets the amount of time, in milliseconds, that the application's connection can
        /// remain open before the HealthVault framework closes the connection.
        /// </summary>
        ///
        /// <remarks>
        /// The default value is 5 minutes.
        /// <p>
        /// This setting only applies when using HTTP Persistent Connections
        /// <see cref="HealthVaultConfiguration.ConnectionUseHttpKeepAlive"/>.  
        /// </p>
        /// <p>
        /// Using this property ensures that active connections do not remain open
        /// indefinitely, even if actively used. This property is intended
        /// when connections should be dropped and reestablished periodically, such
        /// as load balancing scenarios.
        /// </p>
        /// <p>
        /// Setting the property to -1 indicates connections should stay open idefinitely.
        /// </p>
        /// <p>
        /// This property corresponds to the "ConnectionLeaseTimeout" configuration value.
        /// </p>
        /// </remarks>
        public virtual int ConnectionLeaseTimeout
        {
            get
            {
                if (!_connectionLeaseTimeoutInitialized)
                {
                    _connectionLeaseTimeout = ApplicationConfigurationManager.GetConfigurationInt32(ConfigKeyConnectionLeaseTimeout, 5 * 60 * 1000);

                    if (_connectionLeaseTimeout < -1)
                    {
                        _connectionLeaseTimeout = -1;
                    }

                    _connectionLeaseTimeoutInitialized = true;
                }

                return _connectionLeaseTimeout;
            }
        }

        private volatile int _connectionLeaseTimeout;
        private volatile bool _connectionLeaseTimeoutInitialized;

        /// <summary>
        /// Gets a value that indicates whether the application uses Http 1.1 persistent
        /// connections to HealthVault.
        /// </summary>
        ///
        /// <remarks>
        /// True to use persistent connections; otherwise false. The default is true.
        /// <p>
        /// This property corresponds to the "ConnectionUseHttpKeepAlive" configuration value.
        /// </p>
        /// </remarks>
        public virtual bool ConnectionUseHttpKeepAlive
        {
            get
            {
                if (!_connectionUseHttpKeepAliveInitialized)
                {
                    _connectionUseHttpKeepAlive = ApplicationConfigurationManager.GetConfigurationBoolean(ConfigKeyConnectionUseHttpKeepAlive, true);
                    _connectionUseHttpKeepAliveInitialized = true;
                }

                return _connectionUseHttpKeepAlive;
            }
        }

        private volatile bool _connectionUseHttpKeepAlive;
        private volatile bool _connectionUseHttpKeepAliveInitialized;

        /// <summary>
        /// Gets the value which specifies the period of time before the <see cref="P:ServiceInfo.Current"/> built-in cache is considered expired.
        /// </summary>
        ///
        /// <remarks>
        /// <p>
        /// Default value is <b>24 hours</b>.  This property corresponds to the "ServiceInfoDefaultCacheTtlMilliseconds" configuration value.
        /// </p>
        /// <p>
        /// The next request for the object after the cache is expired will result in a call to the HealthVault web-service
        /// to obtain an up-to-date copy of the service information.
        /// </p>
        /// <p>
        /// An application can override the entire caching and service info retrieval behavior
        /// by passing its own implementation of <see cref="IServiceInfoProvider"/> to
        /// <see cref="ServiceInfo.SetSingletonProvider(IServiceInfoProvider)"/>.  In such
        /// a case this configuration is no longer applicable.
        /// </p>
        /// </remarks>
        public TimeSpan ServiceInfoDefaultCacheTtl
        {
            get
            {
                // _serviceInfoDefaultCacheTtl cannot have volatile semantics because
                // it's a struct type (it can't be guaranteed to be assigned atomically).
                // So we will use a full lock here instead.
                // (an alternative is to use Thread.MemoryBarrier after the value write
                // but before the init flag write, and another barrier
                // for the read.

                lock (_serviceInfoDefaultCacheTtlInitLock)
                {
                    if (!_serviceInfoDefaultCacheTtlInitialized)
                    {
                        _serviceInfoDefaultCacheTtl =
                            ApplicationConfigurationManager.GetConfigurationTimeSpanMilliseconds(
                                ConfigKeyServiceInfoDefaultCacheTtlMilliseconds,
                                TimeSpan.FromDays(1));

                        _serviceInfoDefaultCacheTtlInitialized = true;
                    }

                    return _serviceInfoDefaultCacheTtl;
                }
            }
        }

        private TimeSpan _serviceInfoDefaultCacheTtl;

        /// <summary>
        /// Gets the root URL for a default instance of the
        /// Rest HealthVault service
        /// </summary>
        ///
        /// <remarks>
        /// This property corresponds to the "RestHealthServiceUrl" configuration.
        /// </remarks>
        ///
        public virtual Uri RestHealthVaultUrl
        {
            get
            {
                if (_restHealthVaultRootUrl == null)
                {
                    _restHealthVaultRootUrl = ApplicationConfigurationManager.GetConfigurationUrl(ConfigKeyRestHealthServiceUrl, true /* appendSlash */);
                }

                return _restHealthVaultRootUrl;
            }
        }

        private volatile Uri _restHealthVaultRootUrl;

        private bool _serviceInfoDefaultCacheTtlInitialized;
        private readonly object _serviceInfoDefaultCacheTtlInitLock = new object();

        public virtual string ApplicationCertificatePassword
        {
            get
            {
                if (_applicationCertificatePassword == null)
                {
                    _applicationCertificatePassword = ApplicationConfigurationManager.GetConfigurationString(ConfigKeyApplicationCertificatePassword);
                }

                return _applicationCertificatePassword;
            }
        }

        private volatile string _applicationCertificatePassword;

        public string ApplicationCertificateFileName
        {
            get
            {
                if (_applicationCertificateFileName == null)
                {
                    _applicationCertificateFileName = ApplicationConfigurationManager.GetConfigurationString(ConfigKeyApplicationCertificateFileName);
                }

                return _applicationCertificateFileName;
            }
        }

        private volatile string _applicationCertificateFileName;

        public virtual StoreLocation SignatureCertStoreLocation
        {
            get
            {
                string signatureCertStoreLocation = ApplicationConfigurationManager.GetConfigurationString(ConfigKeySignatureCertStoreLocation,
                    DefaultSignatureCertStoreLocation);

                StoreLocation result = StoreLocation.LocalMachine;
                try
                {
                    result = (StoreLocation)Enum.Parse(
                        typeof(StoreLocation),
                        signatureCertStoreLocation,
                        true);
                }
                catch (Exception)
                {
                    if (String.IsNullOrEmpty(signatureCertStoreLocation))
                    {
                        throw new InvalidConfigurationException(Resources.SignatureCertStoreLocationMissing);
                    }
                }

                return result;
            }
        }

        private const string DefaultSignatureCertStoreLocation = "LocalMachine";

        public virtual string CertSubject
        {
            get
            {
                if (_certSubject == null)
                {
                    _certSubject = ApplicationConfigurationManager.GetConfigurationString(ConfigKeyCertSubject);
                }

                return _certSubject;
            }
        }
        private volatile string _certSubject;

        public ICryptoConfiguration CryptoConfiguration
        {
            get
            {
                if (_cryptoConfiguration == null)
                {
                    PopulateCrytoConfiguration();
                }

                return _cryptoConfiguration;
            }

            internal set
            {
                _cryptoConfiguration = value;
            }
        }

        public bool IsMultiRecordApp { get; set; }

        private volatile ICryptoConfiguration _cryptoConfiguration;

        private void PopulateCrytoConfiguration()
        {
            var hmacAlgorithmName = ApplicationConfigurationManager.GetConfigurationString(ConfigKeyHmacAlgorithmName,
                BaseCryptoConfiguration.DefaultHmacAlgorithmName);

            var hashAlgorithmName = ApplicationConfigurationManager.GetConfigurationString(ConfigKeyHashAlgorithmName,
               BaseCryptoConfiguration.DefaultHashAlgorithmName);

            var signatureHahAlgorithmName = ApplicationConfigurationManager.GetConfigurationString(ConfigKeySignatureHashAlgorithmName,
               BaseCryptoConfiguration.DefaultSignatureHashAlgorithmName);

            var signatureAlgorithmName = ApplicationConfigurationManager.GetConfigurationString(ConfigKeySignatureAlgorithmName,
               BaseCryptoConfiguration.DefaultSignatureAlgorithmName);

            var symmetricAlgorithmName = ApplicationConfigurationManager.GetConfigurationString(ConfigSymmetricAlgorithmName,
               BaseCryptoConfiguration.DefaultSymmetricAlgorithmName);

            _cryptoConfiguration = new AppCryptoConfiguration()
            {
                HmacAlgorithmName = hmacAlgorithmName,
                HashAlgorithmName = hashAlgorithmName,
                SignatureHashAlgorithmName = signatureHahAlgorithmName,
                SignatureAlgorithmName = signatureAlgorithmName,
                SymmetricAlgorithmName = symmetricAlgorithmName
            };
        }
    }
}