﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Bramf
{
    /// <summary>
    /// Extension methods for the Dna Framework
    /// </summary>
    public static class FrameworkExtensions
    {
        /// <summary>
        /// Configures and injects a default Microsoft configuration
        /// </summary>
        /// <param name="construction">The construction to configure</param>
        /// <param name="configure">The custom configuration action</param>
        /// <returns></returns>
        public static FrameworkConstruction AddDefaultConfiguration(this FrameworkConstruction construction, Action<IConfigurationBuilder> configure = null)
        {
            // Create our configuration sources
            var configurationBuilder = new ConfigurationBuilder()
                // Add environment variables
                .AddEnvironmentVariables();

            // If we are not on a mobile platform...
            if (!construction.Environment.IsMobile)
            {
                // Add file based configuration

                // Set base path for Json files as the startup location of the application
                configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());

                // Add application settings json files
                configurationBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                configurationBuilder.AddJsonFile($"appsettings.{construction.Environment.Configuration}.json", optional: true, reloadOnChange: true);
            }

            // Let custom configuration happen
            configure?.Invoke(configurationBuilder);

            // Inject configuration into services
            var configuration = configurationBuilder.Build();
            construction.Services.AddSingleton<IConfiguration>(configuration);

            // Set the construction Configuration
            construction.UseConfiguration(configuration);

            // Chain the construction
            return construction;
        }

        /// <summary>
        /// Configures and injectes a Microsoft configuration
        /// </summary>
        /// <param name="construction">The construction to configure</param>
        /// <param name="configuration">The configuration</param>
        public static FrameworkConstruction AddConfiguration(this FrameworkConstruction construction, IConfiguration configuration)
        {
            // Add specific configuration
            construction.UseConfiguration(configuration);

            // Add configuration to services
            construction.Services.AddSingleton(configuration);

            // Chain the construction
            return construction;
        }

        /// <summary>
        /// Injects all of the default services used by Dna Framework for a quicker and cleaner setup
        /// </summary>
        /// <param name="construction">The construction</param>
        /// <returns></returns>
        public static FrameworkConstruction AddDefaultServices(this FrameworkConstruction construction)
        {
            // Add exception handler
            construction.AddDefaultExceptionHandler();

            // Add default logger
            construction.AddDefaultLogger();

            // Chain the construction
            return construction;
        }

        /// <summary>
        /// Injects the default logger into the framework construction
        /// </summary>
        /// <param name="construction">The construction</param>
        /// <returns></returns>
        public static FrameworkConstruction AddDefaultLogger(this FrameworkConstruction construction)
        {
            // Add logging as default
            construction.Services.AddLogging(options =>
            {
                // Setup loggers from configuration
                options.AddConfiguration(construction.Configuration.GetSection("Logging"));

                // Add console logger
                options.AddConsole();

                // Add debug logger
                options.AddDebug();
            });

            // Adds a default logger so that we can get a non-generic ILogger
            // that will have the category name of "Bramf"
            construction.Services.AddTransient(provider => provider.GetService<ILoggerFactory>().CreateLogger("Bramf"));

            // Chain the construction
            return construction;
        }

        /// <summary>
        /// Injects the default exception handler into the framework construction
        /// </summary>
        /// <param name="construction">The construction</param>
        /// <returns></returns>
        public static FrameworkConstruction AddDefaultExceptionHandler(this FrameworkConstruction construction)
        {
            // Bind a static instance of the BaseExceptionHandler
            construction.Services.AddSingleton<IExceptionHandler>(new BaseExceptionHandler());

            // Chain the construction
            return construction;
        }
    }
}
