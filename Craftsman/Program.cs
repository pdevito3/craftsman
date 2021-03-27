namespace Craftsman
{
    using CommandLine;
    using Craftsman.Commands;
    using Craftsman.CraftsmanOptions;
    using Craftsman.Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using Autofac;
    using System.Reflection;
    using Craftsman.Helpers;

    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyTypes(typeof(ProcessCommand).GetTypeInfo().Assembly);
            builder.Register(_ => new FileSystem()).As<IFileSystem>().SingleInstance();
            var container = builder.Build();

            //var configuration = container.Resolve<IConfiguration>();

            var processCommand = container.Resolve<ProcessCommand>();

            processCommand.Run(args);
        }        
    }
}
