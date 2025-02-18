namespace NugetUtility
{
    public class PackageNameAndVersion
    {
        public string Name = string.Empty;
        public string Version = string.Empty;

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{Name}_{Version}";
        }
    }
}