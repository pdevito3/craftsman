namespace Craftsman.Models;

using System;
using System.Net;
using System.Net.Sockets;
using Enums;
using Exceptions;

public class DockerConfig
{
    public string ProjectName { get; set; }

    private string _dbName;
    public string DbName
    {
        get => _dbName ?? $"dev_{ProjectName.ToLower()}"; 
        set => _dbName = value;
    }
    public string DbUser { get; set; }
    public string DbPassword { get; set; }
    public string AuthServerPort { get; set; }
    
    private int? _dbPort;
    public int DbPort
    {
        get => _dbPort ?? GetFreePort(); 
        set => _dbPort = value;
    }
    
    private int? _apiPort;
    public int ApiPort
    {
        get => _apiPort ?? GetFreePort(); 
        set => _apiPort = value;
    }
    
    private string _dbHostName;
    public string DbHostName
    {
        get => _dbHostName ?? $"{ProjectName.ToLower()}-db"; 
        set => _dbHostName = value;
    }
    
    private string _apiServiceName;
    public string ApiServiceName
    {
        get => _apiServiceName ?? $"{ProjectName.ToLower()}-api"; 
        set => _apiServiceName = value;
    }

    private string _volumeName;
    public string VolumeName
    {
        get => _volumeName ?? $"{ProjectName.ToLower()}-data"; 
        set => _volumeName = value;
    }

    public DbProvider DbProviderEnum = DbProvider.SqlServer;
    public string Provider
    {
        get => Enum.GetName(typeof(DbProvider), DbProviderEnum);
        set
        {
            if (!Enum.TryParse<DbProvider>(value, true, out var parsed))
            {
                throw new InvalidDbProviderException(value);
            }
            DbProviderEnum = parsed;
        }
    }
    
    private static int GetFreePort()
    {
        // From https://stackoverflow.com/a/150974/4190785
        var tcpListener = new TcpListener(IPAddress.Loopback, 0);
        tcpListener.Start();
        var port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
        tcpListener.Stop();
        return port;
    }
}