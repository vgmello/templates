// In YourApp.Domain/AssemblyInfo.cs or a similar file
using Operations.ServiceDefaults;
using YourApp.Domain.Entities; // Assuming YourDomainType is in Entities namespace

[assembly: DomainAssembly(typeof(YourDomainType))]
