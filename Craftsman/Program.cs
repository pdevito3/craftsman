namespace Craftsman
{
    using Craftsman.Commands;
    using System.IO.Abstractions;
    using Autofac;
    using System.Reflection;

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
