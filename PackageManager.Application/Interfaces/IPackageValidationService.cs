namespace PackageManager.Application.Interfaces
{
    /// <summary>
    /// Determines the validity of installing a set of software packages given their dependencies.
    /// </summary>
    public interface IPackageValidationService
    {
        /// <summary>
        /// Validates if provided packages structure is valid
        /// </summary>
        /// <param name="systemPackagesStructure">The collection of packages and its dependencies</param>
        /// <returns>true if installation will succeed, false if it won't</returns>
        bool IsInstallationValid(string[] systemPackagesStructure);

        /// <summary>
        /// Generates the validation result content 
        /// </summary>
        /// <param name="validationResult">Flag that corresponds to validation outcome</param>
        /// <returns>Content returned inside result file</returns>
        string ProduceResponseContent(bool validationResult);
    }
}
