// Copyright (c) ABCDEG. All rights reserved.

using Operations.Extensions.Dapper;
using Operations.Extensions.Messaging;
using System;
using System.Data.Common;
using System.Threading.Tasks;
using System.Threading;
using Xunit;
using Microsoft.Extensions.DependencyInjection; // Required for IServiceProvider and GetKeyedService

namespace Operations.Extensions.Tests;

// Test Commands
[DbCommand(sp: "billing.create_cashier_default_ds", nonQuery: true, paramsCase: DbParamsCase.SnakeCase)]
public partial record InsertCashierDefaultDsCommand(Guid CashierId, string Name) : ICommand<int>;

[DbCommand(sp: "billing.create_cashier_keyed_ds", nonQuery: true, paramsCase: DbParamsCase.SnakeCase, dataSourceKey: "TestKey")]
public partial record InsertCashierKeyedDsCommand(Guid CashierId, string Name) : ICommand<int>;

// Original command, now explicitly for testing default resolution if no key is present
[DbCommand(sp: "billing.create_cashier", nonQuery: true, paramsCase: DbParamsCase.SnakeCase)]
public partial record InsertCashierCommand(Guid CashierId, string Name, string? Email) : ICommand<int>;


public partial class DbCommandSourceGeneratorTests
{
    // Manual Mocks
    public class MockDbConnection : DbConnection
    {
        public override string ConnectionString { get; set; } = "MockConnection";
        public override string Database => "MockDatabase";
        public override string DataSource => "MockDataSource";
        public override string ServerVersion => "1.0";
        public override ConnectionState State => ConnectionState.Open;

        public override void ChangeDatabase(string databaseName) { }
        public override void Close() { }
        public override void Open() { }
        protected override DbTransaction BeginDbTransaction(System.Data.IsolationLevel isolationLevel) => null!;
        protected override DbCommand CreateDbCommand() => null!;

        // Track Dapper calls - simplified
        public int ExecuteAsyncCalledCount { get; private set; }

        // Mock Dapper's ExecuteAsync for this test (simplistic)
        public Task<int> ExecuteAsync(CommandDefinition command)
        {
            ExecuteAsyncCalledCount++;
            return Task.FromResult(1); // Simulate 1 row affected
        }
    }

    public class MockDbDataSource : DbDataSource
    {
        private readonly Func<CancellationToken, Task<DbConnection>> _openConnectionAsyncFunc;

        public MockDbDataSource(Func<CancellationToken, Task<DbConnection>> openConnectionAsyncFunc)
        {
            _openConnectionAsyncFunc = openConnectionAsyncFunc;
        }

        protected override DbConnection CreateDbConnection() => _openConnectionAsyncFunc(CancellationToken.None).GetAwaiter().GetResult();
        public override Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
        {
            return _openConnectionAsyncFunc(cancellationToken);
        }
    }

    public class MockServiceProvider : IServiceProvider
    {
        private readonly Func<Type, object?, object?> _getServiceFunc;

        public MockServiceProvider(Func<Type, object?, object?> getServiceFunc)
        {
            _getServiceFunc = getServiceFunc;
        }

        public object? GetService(Type serviceType)
        {
            // This basic mock won't distinguish between GetRequiredService and GetKeyedService directly
            // We will check the key in the test method by inspecting what was requested.
            return _getServiceFunc(serviceType, null);
        }
    }

    // Store what was requested from IServiceProvider
    public Type? LastRequestedServiceType { get; private set; }
    public object? LastRequestedKey { get; private set; } // Will be null for non-keyed, or the actual key for keyed

    // More capable mock for IServiceProvider
    public class AdvancedMockServiceProvider : IServiceProvider, IKeyedServiceProvider
    {
        private readonly Dictionary<Type, object> _services = new();
        private readonly Dictionary<object, Dictionary<Type, object>> _keyedServices = new();

        public Action<Type, object?>? OnGetService; // To track calls

        public void AddService(Type type, object serviceInstance) => _services[type] = serviceInstance;
        public void AddKeyedService(object key, Type type, object serviceInstance)
        {
            if (!_keyedServices.TryGetValue(key, out var servicesForKey))
            {
                servicesForKey = new Dictionary<Type, object>();
                _keyedServices[key] = servicesForKey;
            }
            servicesForKey[type] = serviceInstance;
        }

        public object? GetService(Type serviceType)
        {
            OnGetService?.Invoke(serviceType, null);
            _services.TryGetValue(serviceType, out var service);
            return service;
        }

        public object? GetKeyedService(Type serviceType, object? serviceKey)
        {
            OnGetService?.Invoke(serviceType, serviceKey);
            if (serviceKey != null && _keyedServices.TryGetValue(serviceKey, out var servicesForKey))
            {
                servicesForKey.TryGetValue(serviceType, out var service);
                return service;
            }
            return null;
        }

        public object GetRequiredKeyedService(Type serviceType, object? serviceKey)
        {
            var service = GetKeyedService(serviceType, serviceKey);
            if (service == null)
                throw new InvalidOperationException($"No service for type {serviceType} with key {serviceKey} found.");
            return service;
        }
    }


    [Fact]
    public async Task HandleAsync_WithInsertCashierCommand_ResolvesDefaultDataSource()
    {
        // Arrange
        var mockConnection = new MockDbConnection();
        var mockDataSource = new MockDbDataSource(_ => Task.FromResult<DbConnection>(mockConnection));

        var serviceProvider = new AdvancedMockServiceProvider();
        serviceProvider.AddService(typeof(DbDataSource), mockDataSource);
        serviceProvider.OnGetService = (type, key) =>
        {
            LastRequestedServiceType = type;
            LastRequestedKey = key;
        };

        var command = new InsertCashierCommand(Guid.NewGuid(), "Test User", "test@example.com");

        // Act
        // Assumes the generated handler is in the same namespace and accessible.
        // The generator creates a static class like InsertCashierCommandHandler.
        await InsertCashierCommandHandler.HandleAsync(command, serviceProvider, CancellationToken.None);

        // Assert
        Assert.Equal(typeof(DbDataSource), LastRequestedServiceType);
        Assert.Null(LastRequestedKey); // Default DbDataSource should be requested
        Assert.True(mockConnection.ExecuteAsyncCalledCount > 0); // Verify Dapper call
    }

    [Fact]
    public async Task HandleAsync_WithInsertCashierDefaultDsCommand_ResolvesDefaultDataSource()
    {
        // Arrange
        var mockConnection = new MockDbConnection();
        var mockDataSource = new MockDbDataSource(_ => Task.FromResult<DbConnection>(mockConnection));

        var serviceProvider = new AdvancedMockServiceProvider();
        serviceProvider.AddService(typeof(DbDataSource), mockDataSource);
        serviceProvider.OnGetService = (type, key) =>
        {
            LastRequestedServiceType = type;
            LastRequestedKey = key;
        };

        var command = new InsertCashierDefaultDsCommand(Guid.NewGuid(), "Test Default DS User");

        // Act
        await InsertCashierDefaultDsCommandHandler.HandleAsync(command, serviceProvider, CancellationToken.None);

        // Assert
        Assert.Equal(typeof(DbDataSource), LastRequestedServiceType);
        Assert.Null(LastRequestedKey);
        Assert.True(mockConnection.ExecuteAsyncCalledCount > 0);
    }

    [Fact]
    public async Task HandleAsync_WithInsertCashierKeyedDsCommand_ResolvesKeyedDataSource()
    {
        // Arrange
        var mockConnection = new MockDbConnection();
        var mockDataSource = new MockDbDataSource(_ => Task.FromResult<DbConnection>(mockConnection));
        var testKey = "TestKey";

        var serviceProvider = new AdvancedMockServiceProvider();
        // Setup for GetKeyedService. The generator should call this.
        serviceProvider.AddKeyedService(testKey, typeof(DbDataSource), mockDataSource);
        serviceProvider.OnGetService = (type, key) => // This will be called by the GetKeyedService extension
        {
            LastRequestedServiceType = type;
            LastRequestedKey = key;
        };

        var command = new InsertCashierKeyedDsCommand(Guid.NewGuid(), "Test Keyed DS User");

        // Act
        await InsertCashierKeyedDsCommandHandler.HandleAsync(command, serviceProvider, CancellationToken.None);

        // Assert
        Assert.Equal(typeof(DbDataSource), LastRequestedServiceType);
        Assert.Equal(testKey, LastRequestedKey); // Keyed DbDataSource should be requested
        Assert.True(mockConnection.ExecuteAsyncCalledCount > 0);
    }
}
