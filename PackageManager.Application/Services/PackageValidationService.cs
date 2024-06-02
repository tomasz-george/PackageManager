using Microsoft.Extensions.Logging;
using PackageManager.Application.Interfaces;

namespace PackageManager.Application.Services
{
    /// <summary>
    /// <inheritdoc />
    /// </summary>
    public class PackageValidationService : IPackageValidationService
    {
        private readonly ILogger<PackageValidationService> _logger;

        public PackageValidationService(ILogger<PackageValidationService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        /// <param name="installationFile"></param>
        /// <returns></returns>
        public bool IsInstallationValid(string[] installationFile)
        {
            if (!IsPackageStructureValid(installationFile))
            {
                _logger.LogInformation($"{nameof(IsInstallationValid)}: Package structure validation failed.");
                return false;
            }

            Dictionary<string, string> packagesToInstall = [];
            Dictionary<(string, string), HashSet<(string, string)>> packagesWithDependencies = [];

            try
            {
                var fileIterationIndex = 1;
                ExtractPackages(ref fileIterationIndex, installationFile, packagesToInstall);
                ExtractDependencies(ref fileIterationIndex, installationFile, packagesWithDependencies);
                _logger.LogInformation($"{nameof(IsInstallationValid)}: Extracted packages and package dependencies.");

                HashSet<(string, string)> verifiedPackages = [];
                Dictionary<string, string> versionTracker = [];
                foreach (var (packageName, packageVersion) in packagesToInstall)
                {
                    if (!ArePackageDependenciesValid(packageName, packageVersion, packagesWithDependencies, packagesToInstall, verifiedPackages, versionTracker))
                    {
                        return false;
                    }
                }
            }
            catch (ArgumentException ae)
            {
                _logger.LogInformation($"{nameof(IsInstallationValid)}: Package validation is invalid: {ae.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(IsInstallationValid)}: Package validation failed unexpectedly: {ex.Message}.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        /// <param name="validationResult"></param>
        /// <returns></returns>
        public string ProduceResponseContent(bool validationResult)
        {
            if (validationResult)
            {
                return "PASS";
            }
            else
            {
                return "FAIL";
            }
        }

        private static bool ArePackageDependenciesValid(string package, string version, Dictionary<(string, string), HashSet<(string, string)>> packagesWithDependancies,
            Dictionary<string, string> packagesToInstall, HashSet<(string, string)> verifiedPackages, Dictionary<string, string> versionTracker)
        {
            var currentPackage = (package, version);

            if (verifiedPackages.Contains(currentPackage))
            {
                // Package already processed, skip it
                return true;
            }

            // Check for version conflicts
            if (versionTracker.TryGetValue(package, out var trackedVersion))
            {
                if (trackedVersion != version)
                {
                    return false;
                }
            }
            else
            {
                versionTracker[package] = version;
            }

            verifiedPackages.Add(currentPackage);

            if (!packagesWithDependancies.TryGetValue(currentPackage, out HashSet<(string, string)>? packageWithDependency))
            {
                // No dependancies found, skip it
                return true;
            }

            foreach (var (dependencyName, dependencyVersion) in packageWithDependency)
            {
                // Recursion over each package dependency
                if (!ArePackageDependenciesValid(dependencyName, dependencyVersion, packagesWithDependancies, packagesToInstall, verifiedPackages, versionTracker))
                {
                    return false;
                }
            }

            return true;
        }

        private static void ExtractPackages(ref int index, string[] installationFile, Dictionary<string, string> packagesToInstall)
        {
            var packagesCount = int.Parse(installationFile[0]);

            for (int i = 0; i < packagesCount; i++)
            {
                var packageParts = installationFile[index].Split(',');
                if (packageParts.Length != 2)
                {
                    throw new ArgumentException($"Package format is invalid: {installationFile[i]}");
                }

                if (packagesToInstall.ContainsKey(packageParts[0]))
                {
                    if (packagesToInstall[packageParts[0]] != packageParts[1])
                    {
                        throw new ArgumentException($"Version conflict for package: {packageParts[0]}");
                    }
                }
                else
                {
                    packagesToInstall.Add(packageParts[0], packageParts[1]);
                }

                index++;
            }
        }

        private static void ExtractDependencies(ref int index, string[] installationFile, Dictionary<(string, string), HashSet<(string, string)>> packagesWithDependancies)
        {
            var packagesCount = int.Parse(installationFile[0]);
            if (installationFile.Length <= 2)
            {
                return;
            }

            var dependenciesCount = int.Parse(installationFile[packagesCount + 1]);
            index++;

            for (int i = 0; i < dependenciesCount; i++)
            {
                var dependencyParts = installationFile[index].Split(",");
                if (dependencyParts.Length < 4 || dependencyParts.Length % 2 != 0)
                {
                    throw new ArgumentException($"Dependency format is invalid: {installationFile[index]}");
                }

                var package = (dependencyParts[0], dependencyParts[1]);
                var dependancies = FormatPackageDependencies(dependencyParts.Skip(2).ToArray());

                if (!packagesWithDependancies.ContainsKey(package))
                {
                    packagesWithDependancies.Add(package, dependancies);
                }
                else
                {
                    packagesWithDependancies[package].UnionWith(dependancies);
                }

                index++;
            }
        }

        private static HashSet<(string, string)> FormatPackageDependencies(string[] dependencyParts)
        {
            HashSet<(string, string)> dependency = [];

            for (int i = 0; i < dependencyParts.Length; i += 2)
            {
                dependency.Add((dependencyParts[i], dependencyParts[i + 1]));
            }

            return dependency;
        }

        private static bool IsPackageStructureValid(string[] systemPackagesStructure)
        {
            if (systemPackagesStructure.Length < 2)
            {
                return false;
            }

            if (!int.TryParse(systemPackagesStructure[0], out var packagesCount) || packagesCount == 0)
            {
                return false;
            }

            if (systemPackagesStructure.Length <= packagesCount)
            {
                return false;
            }

            if (systemPackagesStructure.Length == packagesCount + 1)
            {
                return true;
            }

            if (!int.TryParse(systemPackagesStructure[packagesCount + 1], out var depenenciesCount))
            {
                return false;
            }

            int totalItemsCount = packagesCount + depenenciesCount + 2;
            if (totalItemsCount != systemPackagesStructure.Length)
            {
                return false;
            }

            return true;
        }
    }
}
