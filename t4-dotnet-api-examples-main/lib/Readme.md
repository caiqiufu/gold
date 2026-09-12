# API DLLs

This folder contains compiled **CTS API DLLs** provided for **convenience only**.

> ⚠️ **Do not reference these DLLs directly.**  
> Always use our official **NuGet packages** instead.

---

### Use the CTS NuGet Packages

All CTS API libraries are published as NuGet packages under the **CTS.Futures** namespace.

Example packages include:
- `CTS.T4API`
- `CTS.T4Data`
- `CTS.T4Connections`
- `CTS.T4Definitions`
- `CTS.T4Messages`
- `CTS.T4TraceListener`

---

### Example Usage

**.csproj**
```xml
<PackageReference Include="CTS.T4API" Version="4.7.72.356" />
<PackageReference Include="CTS.T4Data" Version="4.7.72.356" />
