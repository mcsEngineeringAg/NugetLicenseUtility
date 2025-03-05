# LicenseUtility

## Description

The LicenseUtility is a dotnet tool which can read the NuGet package information from a [CycloneDX](https://github.com/CycloneDX) bom.xml file and get the package information from the NuGet page including the license information and the license text. The package information can be saved in different file formats.

## What the tool can not do

The LicenseUtility is not able to generate the bom.xml from a given Visual studio solution or project file. This has to be done with the CycloneDX dotnet tool:

```
dotnet tool install --global CycloneDX --version 2.5.1
dotnet CycloneDX src/MyProject.sln -o .
```

## Usage Example
```
nuget-license-utility --bom=bom.xml --config=NugetUtility_config.json
```

## Command line arguments

The LicenseUtility has the following command line arguments:

| Argument | Description | Usage | Optional |
|----------|-------------|-------|----------|
| bom | Path to the bom XML file | --bom="C:\temp\small_bom.xml" | Yes. If missing, it assumes there is a bom.xml in the working directory |
| output-folder | The folder where to write the file with package information | --output-folder="C:\temp" | Yes. If missing, the output is written to the working directory  |
| config | A configuration file in JSON format | --config="C:\temp\config.json" | Yes. If missing, a standard settings object is created, without any license filtering |

## The configuration file

The configuration is a JSON file. Its main purpose is to filter, exclude or map licenses.

### General properties:

| Name | Default | Description |
|------|---------|-------------|
| ProxySystemAuth | false | Use proxy in Http client |
| ProxyURL | string.Empty | The proxy url |
| HttpClientTimeout | 10 | The Http client timeout in seconds |
| IgnoreSslCertificateErrors | false | Ignore certificate errors |

### Output file types

The `OutputFileType` entry can have the following string values (casing is important)
- JSON
- MARKDOWN
- EXCEL

### Allowed and Forbidden license types

There are two entries `AllowedLicenseTypes` and `ForbiddenLicenseTypes`. These are lists of strings, containing the licenses which are allowed or not allowed. For example with `"AllowedLicenseTypes": [ "MIT" ]` the license validation will fail if there is any NuGet in the project, whose license is not of type MIT. On the other hand `"ForbiddenLicenseTypes": [ "BSD-3-Clause" ]` means that all licenses are allowed except the BSD-3-Clause license. Please note that these two entries can only be used exclusively, i.e. if `AllowedLicenseTypes` contains values then `ForbiddenLicenseTypes` must be empty or vice versa. By default both lists are empty.

### License mapping

The `LicenseMappings` entry is a list of mapping objects for example

```
"LicenseMappings": [
		{
			"Identifier": "",
			"ReadLocalLicense": false,
			"LicenseFile": "",
			"OriginalLicenseUrl": "http://go.microsoft.com/fwlink/?LinkId=329770",
			"MappedLicenseUrl": "https://github.com/dotnet/core/blob/main/LICENSE.TXT",
			"LicenseType": "MIT"
		},
		{
			"Identifier": "mcs.NetCoreUtils",
			"ReadLocalLicense": true,
			"LicenseFile": "C:\\temp\\mcsLicense.txt",
			"OriginalLicenseUrl": "",
			"MappedLicenseUrl": "",
			"LicenseType": "MCS"
		}
	]
```

| Entry | Description and usage |
|-------|-----------------------|
| Identifier | A string which must exactly match the NuGet package name if it is not empty. |
| ReadLocalLicense | A Boolean indicating if the license is to be read from a local file. If true, the identifier and LicenseFile path must not be empty |
| LicenseFile | A local license file. Must not be empty if ReadLocalLicense is true |
| OriginalLicenseUrl | A string which matches the license url from the NuGet info but does not lead to a valid license file or page |
| MappedLicenseUrl | Used in combination with OriginalLicenseUrl. Represents a string where the license should be fetched instead |
| LicenseType | The type of license, e.g. Apache-2.0 |

The mapping object allows the user to specify some rules depending on values of its properties. Please note that the `LicenseType` should not be empty, otherwise this field may be empty in the output file. The following rules for mapping can be applied:

- If the `Identifier` is not empty, the mapping is applied only for packages whose name exactly matches the `Identifier`. For this strict mapping, the user can choose to read the license from a local file or provide a `MappedLicenseUrl` to read the text from there. 
- If the `Identifier` is empty, the mapping is applied for all packages whose license url matches the `OriginalLicenseUrl`. In this case, the `MappedLicenseUrl` is used to fetch the license text. For this generic kind of mapping a local file can not be used.
- For some NuGet packages the license type may be unknown although the license url is given. For such cases the user can provide the `MappedLicenseUrl` which must match the NuGet's license url and the `LicenseType` which is then used for the output file.

### Excluding packages

The `ExcludedPackages` list entry provides an option to exclude NuGet packages completely. For example
```
"ExcludedPackages": ["System.*", "Microsoft.OData.Edm"]
```
will filter out the package whose name exactly matches Microsoft.OData.Edm and all packages which contain the string `System.`. The `*` operates as kind of wildcard: If a string in the list contains `*`, the star characters are removed and package names are tested if they contain this string. If no star is present, the string must match the package name.


### Example

Here is a complete example configuration:

```
{
	"ProxyURL": "",
	"ProxySystemAuth": false,
	"IgnoreSslCertificateErrors": false,
	"HttpClientTimeout": 10,
	"AllowedLicenseTypes": [],
	"ForbiddenLicenseTypes": [],
	"OutputFileType": "EXCEL",
	"LicenseMappings": [
		{
			"Identifier": "",
			"ReadLocalLicense": false,
			"LicenseFile": "",
			"OriginalLicenseUrl": "http://go.microsoft.com/fwlink/?LinkId=329770",
			"MappedLicenseUrl": "https://github.com/dotnet/core/blob/main/LICENSE.TXT",
			"LicenseType": "MIT"
		},
		{
			"Identifier": "mcs.NetCoreUtils",
			"ReadLocalLicense": true,
			"LicenseFile": "C:\\temp\\mcsLicense.txt",
			"OriginalLicenseUrl": "",
			"MappedLicenseUrl": "",
			"LicenseType": "MCS"
		}
	],
	"ExcludedPackages": ["System.*", "Microsoft.OData.Edm"]
}
```

## Validation and check sequence

The validations and checks are done in the following order:

1. The bom XML must exist, otherwise the program exits with error.
2. Check if either `AllowedLicenseTypes` or `ForbiddenLicenseTypes` is empty. If both are not empty, the program exits with error.
3. If there are any packages to be excluded, these packages are skipped.
4. If a license mapping for a specific package is found, the license is taken from the mapping for this package.
5. If the license url of the current package matches any `OriginalLicenseUrl` in the mapping list, the first match is taken and the `MappedLicenseUrl` is used.
6. If either `AllowedLicenseTypes` or `ForbiddenLicenseTypes` is not empty, the license validation is done. If any license is not allowed, the program exits with error.




