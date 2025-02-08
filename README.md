# Durable Task Framework Learning Project

A sample implementation to learn and explore the [Azure Durable Task Framework (DTFx)](https://github.com/Azure/durabletask) - an open-source framework for writing long-running persistent workflows in C# using async/await patterns.

## About DTFx

The Durable Task Framework allows developers to write long-running workflows (orchestrations) using simple async/await coding constructs. It's used extensively within Microsoft for:
- Reliable orchestration of long-running operations
- Provisioning and management workflows
- Monitoring and maintenance tasks

This project serves as a learning ground for understanding DTFx concepts and implementation patterns.

## Configuration

Both Worker and Client projects use appsettings.json for configuration:

## Getting Started

1. Clone this repository
2. Update storage account settings in both Worker and Client appsettings.json
3. Start the Worker project first to initialize the Task Hub
4. Use the Client project to start sample orchestrations

## Prerequisites

- .NET 8.0 SDK
- Azure Storage Account
- Visual Studio 2022 or VS Code or Cursor

## Key Dependencies

- Microsoft.Azure.DurableTask.Core (3.0.0)
- Microsoft.Azure.DurableTask.AzureStorage (2.0.1)
- Microsoft.Extensions.Hosting (9.0.1)
- Azure.Storage.Blobs (12.23.0)

## Logging

The solution implements a custom logging system that:
- Supports multiple log levels (Verbose, Info, Error)
- Includes contextual information (timestamps, source)
- Configurable via appsettings.json
- Integrates with Microsoft.Extensions.Logging

## Learning Resources

- [Official DTFx Documentation](https://github.com/Azure/durabletask/wiki)
- [DTFx Blog Series](https://abhikmitra.github.io/blog/durable-task/)
- [Sample Implementations](https://github.com/kaushiksk/durabletask-samples)
- [Azure Durable Functions Documentation](https://docs.microsoft.com/azure/azure-functions/durable/) (related concepts)

## Contributing

Feel free to:
- Submit issues for bugs or suggestions
- Create pull requests with improvements
- Use this as a reference for learning DTFx
- Share your learning experiences

## License
MIT License
