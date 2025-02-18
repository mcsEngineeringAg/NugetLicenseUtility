using NugetUtility.Configuration;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("NugetUtilityTests")]
namespace NugetUtility.Validators;

internal class LicenseValidator
{
    internal LicenseValidator(LicenseUtilitySettings settings)
    {
        mSettings = settings;
    }

    /// <summary>
    /// Return true if validation is successful.
    /// </summary>
    /// <param name="packages"></param>
    /// <returns></returns>
    internal bool ValidateLicense(Dictionary<string, Package> packages)
    {
        return CheckAllowedLicenses(packages) && CheckForbiddenLicenses(packages);
    }

    /// <summary>
    /// Returns true if all packages have a license type that is in the allowed licenses list.
    /// </summary>
    /// <param name="packages"></param>
    /// <returns></returns>
    private bool CheckAllowedLicenses(Dictionary<string, Package> packages)
    {
        if (mSettings.AllowedLicenseTypes.Count == 0)
            return true;

        int forbiddenLicenseCount = 0;
        foreach (var (id, package) in packages)
        {
            var type = package.GetLicenseType();
            if (!string.IsNullOrEmpty(type) && !mSettings.AllowedLicenseTypes.Contains(type))
            {
                Console.WriteLine($"ERROR: The license type {type} of package {id} is not in allowed licenses!");
                forbiddenLicenseCount++;
            }
        }

        return forbiddenLicenseCount == 0;
    }

    /// <summary>
    /// Returns true if all packages have a license type that is not in the forbidden licenses list.
    /// </summary>
    /// <param name="packages"></param>
    /// <returns></returns>
    private bool CheckForbiddenLicenses(Dictionary<string, Package> packages)
    {
        if (mSettings.ForbiddenLicenseTypes.Count == 0)
            return true;

        int forbiddenLicenseCount = 0;
        foreach (var (id, package) in packages)
        {
            var type = package.GetLicenseType();
            if (!string.IsNullOrEmpty(type) && mSettings.ForbiddenLicenseTypes.Contains(type))
            {
                Console.WriteLine($"ERROR: The license type {type} of package {id} is in forbidden licenses!");
                forbiddenLicenseCount++;
            }
        }

        return forbiddenLicenseCount == 0;
    }


    private readonly LicenseUtilitySettings mSettings;
}