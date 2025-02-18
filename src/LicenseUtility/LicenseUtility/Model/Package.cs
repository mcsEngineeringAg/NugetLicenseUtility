using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace NugetUtility
{
    [XmlRoot(ElementName = "license")]
    public class License
    {
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; } = string.Empty;
        [XmlText]
        public string Text { get; set; } = string.Empty;

        // Hold the license text
        public string LicenseText { get; set; } = string.Empty;
    }

    [XmlRoot(ElementName = "repository")]
    public class Repository
    {
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; } = string.Empty;
        [XmlAttribute(AttributeName = "url")]
        public string Url { get; set; } = string.Empty;
        [XmlAttribute(AttributeName = "commit")]
        public string Commit { get; set; } = string.Empty;
    }

    [XmlRoot(ElementName = "group")]
    public class Group
    {
        [XmlAttribute(AttributeName = "targetFramework")]
        public string TargetFramework { get; set; } = string.Empty;
        [XmlElement(ElementName = "dependency")]
        public List<Dependency> Dependency { get; set; } = new();
    }

    [XmlRoot(ElementName = "dependency")]
    public class Dependency
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; } = string.Empty;
        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; } = string.Empty;
        [XmlAttribute(AttributeName = "exclude")]
        public string Exclude { get; set; } = string.Empty;
    }

    [XmlRoot(ElementName = "dependencies")]
    public class Dependencies
    {
        [XmlElement(ElementName = "group")]
        public List<Group> Group { get; set; } = new();
    }

    [XmlRoot(ElementName = "metadata")]
    public class Metadata
    {
        [XmlElement(ElementName = "id")]
        public string Id { get; set; } = string.Empty;
        [XmlElement(ElementName = "version")]
        public string Version { get; set; } = string.Empty;
        [XmlElement(ElementName = "title")]
        public string Title { get; set; } = string.Empty;
        [XmlElement(ElementName = "authors")]
        public string Authors { get; set; } = string.Empty;
        [XmlElement(ElementName = "owners")]
        public string Owners { get; set; } = string.Empty;
        [XmlElement(ElementName = "requireLicenseAcceptance")]
        public string RequireLicenseAcceptance { get; set; } = string.Empty;
        [XmlElement(ElementName = "license")]
        public License License { get; set; } = new();
        [XmlElement(ElementName = "licenseUrl")]
        public string LicenseUrl { get; set; } = string.Empty;
        [XmlElement(ElementName = "projectUrl")]
        public string ProjectUrl { get; set; } = string.Empty;
        [XmlElement(ElementName = "iconUrl")]
        public string IconUrl { get; set; } = string.Empty;
        [XmlElement(ElementName = "description")]
        public string Description { get; set; } = string.Empty;
        [XmlElement(ElementName = "copyright")]
        public string Copyright { get; set; } = string.Empty;
        [XmlElement(ElementName = "tags")]
        public string Tags { get; set; } = string.Empty;
        [XmlElement(ElementName = "repository")]
        public Repository Repository { get; set; } = new();
        [XmlElement(ElementName = "dependencies")]
        public Dependencies Dependencies { get; set; } = new();
        [XmlAttribute(AttributeName = "minClientVersion")]
        public string MinClientVersion { get; set; } = string.Empty;
    }

    [XmlRoot(ElementName = "package", Namespace = "")]
    public class Package
    {
        [XmlElement(ElementName = "metadata")]
        public Metadata Metadata { get; set; } = new();

        public override string ToString()
        {
            return $"{Metadata.Id} {Metadata.Version}";
        }

        /// <summary>
        /// Check if the package has a license
        /// </summary>
        /// <returns></returns>
        public bool HasLicense()
        {
            if (!string.IsNullOrWhiteSpace(Metadata.LicenseUrl))
                return true;

            return !string.IsNullOrWhiteSpace(Metadata.License.Text);
        }

        /// <summary>
        /// Get the license type.
        /// Empty string if no license is found.
        /// </summary>
        /// <returns></returns>
        public string GetLicenseType()
        {
            return HasLicense() ? Metadata.License.Text : string.Empty;
        }
    }


    // helper class to ignore namespaces when de-serializing
    public class NamespaceIgnorantXmlTextReader : XmlTextReader
    {
        public NamespaceIgnorantXmlTextReader(System.IO.TextReader reader) : base(reader) { }

        public override string NamespaceURI => string.Empty;
    }

    // helper class to omit XML decl at start of document when serializing
    public class XTWFND : XmlTextWriter
    {
        public XTWFND(System.IO.TextWriter w) : base(w) { Formatting = System.Xml.Formatting.Indented; }
        public override void WriteStartDocument() { }
    }



}
