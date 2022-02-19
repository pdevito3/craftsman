namespace Craftsman.Models;

using System;
using System.Net;
using System.Net.Sockets;
using Enums;
using Exceptions;
using Helpers;

public class DockerConfig
{
    public string ProjectName { get; set; }

    private string _dbName;
    public string DbName
    {
        get => _dbName ?? $"dev_{ProjectName.ToLower()}"; 
        set => _dbName = value;
    }

    private string _dbUser;
    public string DbUser
    {
        // Enforce SA for SqlServer
        get => Enum.Parse<DbProvider>(Provider) == DbProvider.SqlServer ? "SA" : _dbUser ?? "postgres";
        set => _dbUser = value;
    }

    private string _dbPassword;
    public string DbPassword
    {
        get => _dbPassword ?? (Enum.Parse<DbProvider>(Provider) == DbProvider.SqlServer
            ? "#localDockerPassword#"
            : "postgres");
        set => _dbPassword = value;
    }
    public string AuthServerPort { get; set; }
    
    private int? _dbPort = Utilities.GetFreePort();
    public int? DbPort
    {
        get => _dbPort;
        set => _dbPort = value ?? _dbPort;
    }
    
    private int? _apiPort = Utilities.GetFreePort();
    public int? ApiPort
    {
        get => _apiPort;
        set => _apiPort = value ?? _apiPort;
    }
    
    private string _dbHostName;
    public string DbHostName
    {
        get => _dbHostName ?? $"{ProjectName.ToLower()}-db"; 
        set => _dbHostName = value;
    }
    
    public string DbConnectionStringCompose
    {
        get {
            var dbProvider = Enum.Parse<DbProvider>(Provider);
            var dbConnectionString = dbProvider == DbProvider.SqlServer
                ? $"Data Source={DbHostName},{1433};Integrated Security=False;Database={DbName};User ID={DbUser};Password={DbPassword}"
                : $"Host={DbHostName};Port={5432};Database={DbName};Username={DbUser};Password={DbPassword}";
            return dbConnectionString;
        }
    }
    
    public string DbConnectionString
    {
        get {
            var dbProvider = Enum.Parse<DbProvider>(Provider);
            var dbConnectionString = dbProvider == DbProvider.SqlServer
                ? $"Data Source=localhost,{DbPort};Integrated Security=False;Database={DbName};User ID={DbUser};Password={DbPassword}"
                : $"Host=localhost;Port={DbPort};Database={DbName};Username={DbUser};Password={DbPassword}";
            return dbConnectionString;
        }
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
}