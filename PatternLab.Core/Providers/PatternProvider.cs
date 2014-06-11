﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using IniParser;
using IniParser.Model;
using PatternLab.Core.Helpers;
using PatternLab.Core.Mustache;
using YamlDotNet.Serialization;

namespace PatternLab.Core.Providers
{
    /// <summary>
    /// The Pattern Lab file and data provider
    /// </summary>
    public class PatternProvider
    {
        private string _cacheBuster;
        private IniData _config;
        private dynamic _data;
        private List<string> _ignoredDirectories;
        private List<string> _ignoredExtensions;
        private IPatternEngine _patternEngine;
        private List<Pattern> _patterns;

        /// <summary>
        /// The file extension of escaped HTML files
        /// </summary>
        public static string FileExtensionEscapedHtml = ".escaped.html";

        /// <summary>
        /// The file extension of HTML files
        /// </summary>
        public static string FileExtensionHtml = ".html";

        /// <summary>
        /// The file extensions of data files
        /// </summary>
        public static string[] FileExtensionsData = { ".json", ".yaml" };

        /// <summary>
        /// The file name of the 'Viewer' page
        /// </summary>
        public static string FileNameViewer = "index.html";

        /// <summary>
        /// The path to the config file
        /// </summary>
        public static string FilePathConfig = "config/config.ini";

        /// <summary>
        /// The path to the styleguide page
        /// </summary>
        public static string FilePathStyleguide = "styleguide/html/styleguide.html";

        /// <summary>
        /// The path to the latest changes file
        /// </summary>
        public static string FilePathLatestChanges = "~/latest-change.txt";

        /// <summary>
        /// The name of the folder containing the annotations file
        /// </summary>
        public static string FolderNameAnnotations = "_annotations";

        /// <summary>
        /// The name of the folder containing the styleguide
        /// </summary>
        public static string FolderNameAssets = "styleguide";

        /// <summary>
        /// The subfolder names contained withing the styleguide folder
        /// </summary>
        public static string[] FolderNamesAssetSubfolder = {"css", "fonts", "html", "images", "js", "vendor"};

        /// <summary>
        /// The name of the folder containing the config file 
        /// </summary>
        public static string FolderNameConfig = "config";

        /// <summary>
        /// The name of the folder containing data files
        /// </summary>
        public static string FolderNameData = "_data";

        /// <summary>
        /// The name of the folder the shared pattern header and footer files
        /// </summary>
        public static string FolderNameMeta = "_meta";

        /// <summary>
        /// The name of the folder containing pattern files
        /// </summary>
        public static string FolderNamePattern = "_patterns";

        /// <summary>
        /// The default name of the folder containing the generated static output
        /// </summary>
        public static string FolderNamePublic = "public";

        /// <summary>
        /// The name of the folder containing snapshots
        /// </summary>
        public static string FolderNameSnapshots = "snapshots";

        /// <summary>
        /// The name of the folder containing core Mustache templates
        /// </summary>
        public static string FolderNameTemplates = "templates";

        /// <summary>
        /// The folder path to public
        /// </summary>
        public string FolderPathPublic
        {
            get
            {
                var directory = Setting("publicDir");
                if (!string.IsNullOrEmpty(directory))
                {
                    // Default to /public if not set
                    directory = FolderNamePublic;
                }

                return string.Format("{0}{1}{2}", HttpRuntime.AppDomainAppPath, directory,
                    Path.DirectorySeparatorChar);
            }
        }

        /// <summary>
        /// The folder path to source
        /// </summary>
        public string FolderPathSource
        {
            get
            {
                var directory = Setting("sourceDir");
                if (!string.IsNullOrEmpty(directory))
                {
                    return string.Format("{0}{1}{2}", HttpRuntime.AppDomainAppPath, directory,
                        Path.DirectorySeparatorChar);
                }

                return HttpRuntime.AppDomainAppPath;
            }
        }

        /// <summary>
        /// Denotes a delimited list
        /// </summary>
        public static char IdentifierDelimiter = ',';

        /// <summary>
        /// Denotes a hidden object
        /// </summary>
        public static char IdentifierHidden = '_';

        /// <summary>
        /// Denotes object contains styleModifier
        /// </summary>
        public static char IdentifierModifier = ':';

        /// <summary>
        /// Denotes object contains more than one styleModifier value
        /// </summary>
        public static char IdentifierModifierSeparator = '|';

        /// <summary>
        /// Denotes object contains pattern parameters
        /// </summary>
        public static char IdentifierParameters = '(';

        /// <summary>
        /// Denotes a pattern parameter that is a string
        /// </summary>
        public static char IdentifierParameterString = '"';

        /// <summary>
        /// Denotes a psuedo pattern
        /// </summary>
        public static char IdentifierPsuedo = '~';

        /// <summary>
        /// Denotes a space character in display name parsing
        /// </summary>
        public static char IdentifierSpace = '-';

        /// <summary>
        /// Denotes a pattern has a state
        /// </summary>
        public static char IdentifierState = '@';

        /// <summary>
        /// The reserved keyword for the location of embedded resources
        /// </summary>
        public static string KeywordEmbeddedResources = "EmbeddedResources";

        /// <summary>
        /// The reserved keyword for listItem variables
        /// </summary>
        public static string KeywordListItems = "listItems";

        /// <summary>
        /// The reserved keyword for styleModifiers
        /// </summary>
        public static string KeywordModifier = "styleModifier";

        /// <summary>
        /// The reserved keyword for the 'View all' page partial path
        /// </summary>
        public static string KeywordPartialAll = "all";

        /// <summary>
        /// The reserved keyword for determining the current pattern engine
        /// </summary>
        public static string KeywordPatternEngine = "patternEngine";

        /// <summary>
        /// The reserved keyword for the 'View all' link in the navigation
        /// </summary>
        public static string KeywordViewAll = "View All";

        /// <summary>
        /// Define the list of currently supported listItem variables
        /// </summary>
        public static List<string> ListItemVariables = new List<string>
        {
            "one",
            "two",
            "three",
            "four",
            "five",
            "six",
            "seven",
            "eight",
            "nine",
            "ten",
            "eleven",
            "twelve"
        };

        /// <summary>
        /// The pattern engines supported by Pattern Lab
        /// </summary>
        public List<IPatternEngine> SupportedPatternEngines = new List<IPatternEngine>
        {
            // Register mustache pattern engine
            new MustachePatternEngine(),

            // Register additional pattern engine
            (IPatternEngine) HttpContext.Current.Application[KeywordPatternEngine]
        };

        /// <summary>
        /// Route name for assets contained as embedded resources
        /// </summary>
        public static string RouteNameAsset = "PatternLabAsset";

        /// <summary>
        /// Route name for viewer page
        /// </summary>
        public static string RouteNameDefault = "PatternLabDefault";

        /// <summary>
        /// Route name for snapshots/index.html
        /// </summary>
        public static string RouteNameSnapshots = "PatternLabSnapshots";

        /// <summary>
        /// Route name for styleguide.html
        /// </summary>
        public static string RouteNameStyleguide = "PatternLabStyleguide";

        /// <summary>
        /// Route name for 'view all' HTML pages
        /// </summary>
        public static string RouteNameViewAll = "PatternLabViewAll";

        /// <summary>
        /// Route name for /patterns/pattern.html pages
        /// </summary>
        public static string RouteNameViewSingle = "PatternLabViewSingle";

        /// <summary>
        /// Route name for /patterns/pattern.escaped.html pages
        /// </summary>
        public static string RouteNameViewSingleEncoded = "PatternLabViewSingleEncoded";

        /// <summary>
        /// Route name for /patterns/pattern.{pattern engine extension} pages
        /// </summary>
        public static string RouteNameViewSingleTemplate = "PatternLabViewSingleTemplate";

        /// <summary>
        /// The name of the 'View all' page view
        /// </summary>
        public static string ViewNameViewAllPage = "ViewAll";

        /// <summary>
        /// The name of the 'Snapshots' page view
        /// </summary>
        public static string ViewNameSnapshots = "Snapshot";

        /// <summary>
        /// The name of the 'Viewer' page view
        /// </summary>
        public static string ViewNameViewerPage = "Index";

        /// <summary>
        /// The name of the 'View single' page view
        /// </summary>
        public static string ViewNameViewSingle = "ViewSingle";

        /// <summary>
        /// Determines whether cache busting is enable or disabled
        /// </summary>
        /// <param name="noCache">Set the cacheBuster value to 0</param>
        /// <returns>The cache buster value to be appended to asset URLs</returns>
        public string CacheBuster(bool? noCache)
        {
            // Return cached value if set
            if (!string.IsNullOrEmpty(_cacheBuster)) return _cacheBuster;

            bool enabled;
            // Check the config file to see if it's enabled
            if (!Boolean.TryParse(Setting("cacheBusterOn"), out enabled))
            {
                enabled = false;
            }

            if (noCache.HasValue && noCache.Value)
            {
                enabled = false;
            }

            // Return the current time as unix timestamp if enabled, or 0 if disabled
            var timestamp = enabled
                ? Math.Floor((DateTime.UtcNow - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds)
                : 0;

            _cacheBuster = timestamp.ToString(CultureInfo.InvariantCulture);
            return _cacheBuster;
        }

        /// <summary>
        /// Clears the pattern provider's cached objects
        /// </summary>
        public void Clear()
        {
            _cacheBuster = null;
            _config = null;
            _data = null;
            _ignoredDirectories = null;
            _ignoredExtensions = null;
            _patterns = null;
            _patternEngine = null;
        }

        /// <summary>
        /// Reads the configuration settings from disk
        /// </summary>
        /// <returns>The configuration settings for Pattern Lab</returns>
        public IniData Config()
        {
            // Return cached value if set
            if (_config != null) return _config;

            // Configure the INI parser to handler the comments in the Pattern Lab config file
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.AllowKeysWithoutSection = true;
            parser.Parser.Configuration.SkipInvalidLines = true;

            var webroot = HttpRuntime.AppDomainAppPath;
            var path = Path.Combine(webroot, FilePathConfig);
            if (!File.Exists(path))
            {
                // If  the config doesn't exist create a new version
                var virtualPath = string.Format("~/{0}", FilePathConfig);
                var defaultConfig = new EmbeddedResource(string.Format("{0}.default", virtualPath));
                var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                var patternEngine = SupportedPatternEngines.Last(e => e != null).Name().ToLowerInvariant();

                Builder.CreateFile(virtualPath,
                    defaultConfig.ReadAllText().Replace("$version$", version).Replace("$patternEngine$", patternEngine),
                    null,
                    new DirectoryInfo(webroot));
            }

            // Read the contents of the config file into a read-only stream
            using (
                var stream = new FileStream(path, FileMode.Open,
                    FileAccess.Read, FileShare.ReadWrite))
            {
                _config = parser.ReadData(new StreamReader(stream));
            }

            return _config;
        }

        /// <summary>
        /// Generates a data collection for the files in the data folder
        /// </summary>
        /// <returns>The data collection for Pattern Lab</returns>
        public dynamic Data()
        {
            // Return cached value if set
            if (_data != null) return _data;

            var host = Dns.GetHostEntry(Dns.GetHostName());

            // Get local IP address
            var ipAddresses = host.AddressList;
            var ipAddress = ipAddresses[ipAddresses.Length - 1].ToString();

            // Identify hidden ish controls from config
            var ishSettings = Setting("ishControlsHide")
                .Split(new[] {IdentifierDelimiter}, StringSplitOptions.RemoveEmptyEntries);
            var hiddenIshControls = ishSettings.ToDictionary(s => s.Trim(), s => true);

            // Hide the 'Page follow' ish control if disabled in config
            if (Setting("pageFollowNav").Equals("false", StringComparison.InvariantCultureIgnoreCase))
            {
                hiddenIshControls.Add("tools-follow", true);
            }
            else
            {
                // TODO: #24 Implement page follow from PHP version. Currently always hidden. Delete this else statement once implemented
                hiddenIshControls.Add("tools-follow", true);
            }

            // Hide the 'Auto-reload' ish control if disabled in config
            if (Setting("autoReloadNav").Equals("false", StringComparison.InvariantCultureIgnoreCase))
            {
                hiddenIshControls.Add("tools-reload", true);
            }
            else
            {
                // TODO: #23 Implement page auto-reload from PHP version. Currently always hidden. Delete this else statement once implemented
                hiddenIshControls.Add("tools-reload", true);
            }

            // Hide the 'Snapshots' ish control if no snapshots have been created
            var snapshotsFolderPath = Path.Combine(FolderPathSource, FolderNameSnapshots);
            if (!Directory.Exists(snapshotsFolderPath))
            {
                hiddenIshControls.Add("tools-snapshot", true);
            }

            var patternLinks = new Dictionary<string, dynamic>();
            var patternPaths = new Dictionary<string, dynamic>();
            var viewAllPaths = new Dictionary<string, dynamic>();
            var patternTypes = new List<dynamic>();

            // Use all patterns that aren't hidden
            var patterns =
                Patterns()
                    .Where(p => !p.Hidden)
                    .ToList();

            if (patterns.Any())
            {
                // Get a list of distinct types
                var types = patterns.Select(p => p.Type).Distinct().ToList();
                foreach (var patternType in types)
                {
                    var type = patternType;
                    var typeName = type.StripOrdinals();
                    var typeDisplayName = typeName.ToDisplayCase();

                    // Create JSON object to hold information about the current type
                    var typeDetails =
                        new
                        {
                            patternTypeLC = typeName,
                            patternTypeUC = typeDisplayName,
                            patternTypeItems = new List<dynamic>(),
                            patternItems = new List<dynamic>()
                        };

                    // Get patterns that match the current type (e.g. Atoms)
                    var typedPatterns =
                        patterns.Where(p => p.Type.Equals(type, StringComparison.InvariantCultureIgnoreCase)).ToList();

                    // Get the sub-types from the patterns that match the current type (e.g. Global, under Atoms)
                    var subTypes =
                        typedPatterns.Select(p => p.SubType).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();

                    var typedPatternPaths = new Dictionary<string, dynamic>();
                    var subTypePaths = new Dictionary<string, dynamic>();

                    if (subTypes.Any())
                    {
                        foreach (var patternSubType in subTypes)
                        {
                            var subType = patternSubType;
                            var subTypeName = subType.StripOrdinals();
                            var subTypeDisplayName = subTypeName.ToDisplayCase();
                            var subTypePath = string.Format("{0}-{1}", type, subType);

                            // Create JSON object to hold information about the current sub-type
                            var subTypeDetails = new
                            {
                                patternSubtypeLC = subTypeName,
                                patternSubtypeUC = subTypeDisplayName,
                                patternSubtypeItems = new List<dynamic>()
                            };

                            // Find all patterns that match the current type, and sub-type
                            var subTypedPatterns =
                                patterns.Where(
                                    p =>
                                        p.Type.Equals(type, StringComparison.InvariantCultureIgnoreCase) &&
                                        p.SubType.Equals(subType, StringComparison.InvariantCultureIgnoreCase)).ToList();

                            foreach (var pattern in subTypedPatterns)
                            {
                                // Create JSON object to hold information about the pattern and add to sub-type JSON
                                subTypeDetails.patternSubtypeItems.Add(
                                    new
                                    {
                                        patternPath = pattern.HtmlUrl,
                                        patternState = GetState(pattern),
                                        patternPartial = pattern.Partial,
                                        patternName = pattern.Name.StripOrdinals().ToDisplayCase()
                                    });
                            }

                            // Add a 'View all' JSON object for use in the navigation
                            subTypeDetails.patternSubtypeItems.Add(
                                new
                                {
                                    patternPath = string.Format("{0}/{1}", subTypePath, FileNameViewer),
                                    patternPartial =
                                        string.Format("{0}-{1}-{2}", ViewNameViewAllPage.ToLowerInvariant(), typeName,
                                            subTypeName),
                                    patternName = KeywordViewAll
                                });

                            // Add sub-type JSON object to the type JSON object
                            typeDetails.patternTypeItems.Add(subTypeDetails);

                            if (!subTypePaths.ContainsKey(subTypeName))
                            {
                                // Handle duplicate sub-type names
                                subTypePaths.Add(subTypeName, subTypePath);
                            }
                        }
                    }

                    foreach (var pattern in typedPatterns)
                    {
                        var patternName = pattern.Name.StripOrdinals();

                        if (!patternLinks.ContainsKey(pattern.Partial))
                        {
                            // Build list of link variables - http://patternlab.io/docs/data-link-variable.html
                            patternLinks.Add(pattern.Partial,
                                string.Format("../../{0}/{1}", FolderNamePattern.TrimStart(IdentifierHidden),
                                    pattern.HtmlUrl));
                        }

                        if (!typedPatternPaths.ContainsKey(patternName))
                        {
                            // Build list of pattern paths for footer
                            typedPatternPaths.Add(patternName, pattern.PathDash);
                        }

                        if (!subTypes.Any())
                        {
                            // Create JSON object for data required by footer
                            typeDetails.patternItems.Add(
                                new
                                {
                                    patternPath = pattern.HtmlUrl,
                                    patternState = GetState(pattern),
                                    patternPartial = pattern.Partial,
                                    patternName = pattern.Name.StripOrdinals().ToDisplayCase()
                                });
                        }
                    }

                    if (subTypes.Any())
                    {
                        // Add a 'View all' JSON object for use in the navigation
                        typeDetails.patternItems.Add(
                            new
                            {
                                patternPath = string.Format("{0}/{1}", type, FileNameViewer),
                                patternPartial =
                                    string.Format("{0}-{1}-{2}", ViewNameViewAllPage.ToLowerInvariant(), typeName,
                                        KeywordPartialAll),
                                patternName = KeywordViewAll
                            });
                    }

                    patternPaths.Add(typeName, typedPatternPaths);
                    if (subTypePaths.Any())
                    {
                        subTypePaths.Add(KeywordPartialAll, type);

                        viewAllPaths.Add(typeName, subTypePaths);
                    }
                    patternTypes.Add(typeDetails);
                }
            }

            // Get the media queries used by the patterns
            var mediaQueries = GetMediaQueries(FolderPathSource, IgnoredDirectories());

            var serializer = new JavaScriptSerializer();

            var annotationsFolderPath = Path.Combine(FolderPathSource, FolderNameAnnotations);

            // Create /_annotations if missing
            Builder.CreateDirectory(string.Concat(annotationsFolderPath, Path.DirectorySeparatorChar));

            var dataFolderPath = Path.Combine(FolderPathSource, FolderNameData);

            // Create /_data if missing
            Builder.CreateDirectory(string.Concat(dataFolderPath, Path.DirectorySeparatorChar));

            var dataFolder = new DirectoryInfo(dataFolderPath);

            // Find any data files in the data folder and create the data collection
            var dataFiles =
                FileExtensionsData.SelectMany(
                    e => dataFolder.GetFiles(string.Concat("*", e), SearchOption.AllDirectories));

            // Get data collection from files
            _data = GetData(dataFiles);

            // Pass config settings and collections of pattern data to a new data collection
            _data.patternEngineName = Setting(KeywordPatternEngine).ToDisplayCase();
            _data.ishminimum = Setting("ishMinimum");
            _data.ishmaximum = Setting("ishMaximum");
            _data.qrcodegeneratoron = Setting("qrCodeGeneratorOn");
            _data.ipaddress = ipAddress;
            _data.xiphostname = Setting("xipHostname");
            _data.autoreloadnav = Setting("autoReloadNav");
            _data.autoreloadport = Setting("autoReloadPort");
            _data.pagefollownav = Setting("pageFollowNav");
            _data.pagefollowport = Setting("pageFollowPort");
            _data.ishControlsHide = hiddenIshControls;
            _data.link = patternLinks;
            _data.patternpaths = serializer.Serialize(patternPaths);
            _data.viewallpaths = serializer.Serialize(viewAllPaths);
            _data.mqs = mediaQueries;
            _data.patternTypes = patternTypes;

            // Return the combined data collection
            return _data;
        }

        /// <summary>
        /// The list of directories ignored by Pattern Lab
        /// </summary>
        /// <returns>A list of directory names</returns>
        public List<string> IgnoredDirectories()
        {
            if (_ignoredDirectories != null) return _ignoredDirectories;

            // Read directory names from config
            _ignoredDirectories =
                Setting("id").Split(new[] {IdentifierDelimiter}, StringSplitOptions.RemoveEmptyEntries).ToList();

            // Add some that are required to be ignored by the .NET version of Pattern Lab
            _ignoredDirectories.AddRange(new[] {FolderNameMeta, FolderPathPublic});

            return _ignoredDirectories;
        }

        /// <summary>
        /// The list of file extensions ignored by Pattern Lab
        /// </summary>
        /// <returns>A list of file extensions</returns>
        public List<string> IgnoredExtensions()
        {
            if (_ignoredExtensions != null) return _ignoredExtensions;

            // Read file extensions from config
            _ignoredExtensions =
                Setting("ie").Split(new[] {IdentifierDelimiter}, StringSplitOptions.RemoveEmptyEntries).ToList();

            // Add some that are required to be ignored by the .NET version of Pattern Lab (string.empty handles README files)
            _ignoredExtensions.AddRange(new[] {string.Empty});

            return _ignoredExtensions;
        }

        /// <summary>
        /// The currently enabled pattern engine for handling templates
        /// </summary>
        /// <returns>A pattern engine</returns>
        public IPatternEngine PatternEngine()
        {
            if (_patternEngine != null) return _patternEngine;

            _patternEngine = SupportedPatternEngines.FirstOrDefault(
                e =>
                    e != null &&
                    e.Name().Equals(Setting(KeywordPatternEngine), StringComparison.InvariantCultureIgnoreCase)) ??
                             SupportedPatternEngines.Last(e => e != null);

            return _patternEngine;
        }

        /// <summary>
        /// The list of patterns available to Pattern Lab
        /// </summary>
        /// <returns>A list of patterns</returns>
        public List<Pattern> Patterns()
        {
            if (_patterns != null) return _patterns;

            var metaFolderPath = Path.Combine(FolderPathSource, FolderNameMeta);

            // Create /_meta if missing
            Builder.CreateDirectory(string.Concat(metaFolderPath, Path.DirectorySeparatorChar));

            var folder = new DirectoryInfo(metaFolderPath);
            var extension = PatternEngine().Extension();

            // Find pattern header and footer
            var views = folder.GetFiles(string.Concat("*", extension),  SearchOption.AllDirectories).ToList();

            // Create a new pattern for the header and footer
            _patterns = views.Select(v => new Pattern(PatternEngine(), v.FullName)).ToList();

            var patternFolderPath = Path.Combine(FolderPathSource, FolderNamePattern);

            // Create /_patterns if missing
            Builder.CreateDirectory(string.Concat(patternFolderPath, Path.DirectorySeparatorChar));

            folder = new DirectoryInfo(patternFolderPath);

            // Find all template files in /_patterns 
            views = folder.GetFiles(string.Concat("*", extension),
                SearchOption.AllDirectories)
                .Where(v => v.Directory != null && v.Directory.FullName != folder.FullName).ToList();

            // Create a new pattern in the list for each file
            _patterns.AddRange(views.Select(v => new Pattern(PatternEngine(), v.FullName)).ToList());

            // Find any patterns that contain pseudo patterns
            var parentPatterns = _patterns.Where(p => p.PseudoPatterns.Any()).ToList();
            foreach (var pattern in parentPatterns)
            {
                var filePath = pattern.FilePath;

                // Create a new pattern in the list for each pseudo pattern 
                _patterns.AddRange(pattern.PseudoPatterns.Select(p => new Pattern(PatternEngine(), filePath, p)));
            }

            // Order the patterns by their dash delimited path
            _patterns = _patterns.OrderBy(p => p.PathDash).ToList();

            return _patterns;
        }

        /// <summary>
        /// Reads a setting from the config collection
        /// </summary>
        /// <param name="name">The name of the setting</param>
        /// <returns>The value of the setting</returns>
        public string Setting(string name)
        {
            var value = Config().Global[name];

            if (!string.IsNullOrEmpty(value))
            {
                // Replace any encoded quotation marks
                value = value.Replace("\"", string.Empty);
            }

            return value;
        }

        /// <summary>
        /// Find a pattern based on it's url, slash delimited path, or partial path
        /// </summary>
        /// <param name="searchTerm">The search term</param>
        /// <returns>The pattern</returns>
        public static Pattern FindPattern(string searchTerm)
        {
            // Remove pattern parameters
            searchTerm = searchTerm.StripPatternParameters();

            // Find a pattern based on it's url, slash delimited path, or partial path - http://patternlab.io/docs/pattern-including.html (see 'examples')
            var provider = Controllers.PatternLabController.Provider ?? new PatternProvider();
            return provider.Patterns()
                .FirstOrDefault(
                    p =>
                        p.ViewUrl.Equals(searchTerm, StringComparison.InvariantCultureIgnoreCase) ||
                        p.PathSlash.Equals(searchTerm, StringComparison.InvariantCultureIgnoreCase) ||
                        p.Partial.Equals(searchTerm, StringComparison.InvariantCultureIgnoreCase)) ??
                   provider.Patterns()
                       .FirstOrDefault(
                           p =>
                               p.Partial.StartsWith(searchTerm, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Creates a dynamic object from a collection of data files
        /// </summary>
        /// <param name="dataFiles">The list of data files</param>
        /// <returns>The dynamic data collection</returns>
        public static dynamic GetData(IEnumerable<FileInfo> dataFiles)
        {
            IDictionary<string, object> result = new DynamicDictionary();
            var jsonSerializer = new JavaScriptSerializer();
            var yamlSerializer = new Deserializer();

            foreach (var dataFile in dataFiles)
            {
                dynamic dictionary;
                var text = File.ReadAllText(dataFile.FullName);

                try
                {
                    if (dataFile.Extension.Equals(FileExtensionsData[0], StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Parse .json files
                        dictionary =
                            jsonSerializer.Deserialize<IDictionary<string, object>>(text)
                                .ToDynamic();
                    }
                    else
                    {
                        // Parse .yaml files
                        dictionary =
                            yamlSerializer.Deserialize<DynamicDictionary>(new StringReader(text));
                    }
                }
                catch
                {
                    // Skip files with errors
                    dictionary = null;
                }

                if (dictionary == null) continue;

                foreach (KeyValuePair<string, object> keyValuePair in dictionary)
                {
                    result[keyValuePair.Key] = keyValuePair.Value;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the media queries used by all CSS files in the directory
        /// </summary>
        /// <path>The path of the directory</path>
        /// <ignoredDirectories>The directory names to ignore</ignoredDirectories>
        /// <returns>A list of PX or EM values for use in the navigation</returns>
        public static List<string> GetMediaQueries(string path, List<string> ignoredDirectories)
        {
            var mediaQueries = new List<string>();

            // Find all .css files in application
            foreach (
                var filePath in Directory.GetFiles(path, "*.css", SearchOption.AllDirectories).ToList())
            {
                var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
                if (!string.IsNullOrEmpty(directory))
                {
                    // Remove application root from string
                    directory = directory.Replace(path, string.Empty);
                }

                // Skip files in ignored directories
                if (!ignoredDirectories.Where(directory.StartsWith).Any())
                {
                    var css = File.ReadAllText(filePath);
                    var queries = mediaQueries;

                    // Parse the contents and find any media queries used
                    mediaQueries.AddRange(
                        Regex.Matches(css, @"(min|max)-width:([ ]+)?(([0-9]{1,5})(\.[0-9]{1,20}|)(px|em))")
                            .Cast<Match>()
                            .Select(match => match.Groups[3].Value)
                            .Where(mediaQuery => !queries.Contains(mediaQuery)));
                }
            }

            // Sort the media queries by numeric value
            mediaQueries =
                mediaQueries.OrderBy(
                    m =>
                        double.Parse(m.Substring(0, m.LastIndexOfAny("0123456789".ToCharArray()) + 1),
                            CultureInfo.InvariantCulture))
                    .ToList();

            return mediaQueries;
        }

        /// <summary>
        /// Get the state of a pattern - http://patternlab.io/docs/pattern-states.html 
        /// </summary>
        /// <param name="pattern">The pattern</param>
        /// <param name="state">The currently found state</param>
        /// <returns>The current state of the pattern, and its referenced child pattern</returns>
        public static string GetState(Pattern pattern, string state = null)
        {
            var provider = Controllers.PatternLabController.Provider ?? new PatternProvider();

            // Read states from config. Priority is determined by the order
            var states = provider.Setting("patternStates")
                .Split(new[] {IdentifierDelimiter}, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (state == null)
            {
                // If this method hasn't been called already use the current patterns state
                state = pattern.State;
            }

            if (!string.IsNullOrEmpty(pattern.State))
            {
                var currentIndex = states.IndexOf(state);
                var newIndex = states.IndexOf(pattern.State);

                if (((newIndex < currentIndex || currentIndex < 0) && newIndex < states.Count - 1))
                {
                    // If the priority of the found state is lower that the current state and isn't the last configured state change the state to the lower value
                    state = pattern.State;
                }
            }

            // Find the lowest priority state of the pattern's referenced child patterns
            foreach (var childPattern in pattern.Lineages.Select(partial => provider.Patterns().FirstOrDefault(
                p => p.Partial.Equals(partial, StringComparison.InvariantCultureIgnoreCase)))
                .Where(childPattern => childPattern != null))
            {
                if (state == null)
                {
                    // Set to empty string to denote that this is a nested call
                    state = string.Empty;
                }

                state = GetState(childPattern, state);
            }

            if (string.IsNullOrEmpty(state))
            {
                // Reset value to null if empty for use with code-viewer.js
                state = null;
            }

            return state;
        }

        /// <summary>
        /// Merges two dynamic objects
        /// </summary>
        /// <param name="original">The original dynamic object</param>
        /// <param name="additional">The additional dynamic object</param>
        /// <returns>The merged dynamic object</returns>
        public static dynamic MergeData(dynamic original, dynamic additional)
        {
            IDictionary<string, object> result = new DynamicDictionary();

            // Loop through the original object and replicate the properties
            foreach (KeyValuePair<string, object> keyValuePair in original)
            {
                result[keyValuePair.Key] = keyValuePair.Value;
            }

            // Loop through the additional object and append its proprties
            foreach (KeyValuePair<string, object> keyValuePair in additional)
            {
                result[keyValuePair.Key] = keyValuePair.Value;
            }

            return result;
        }
    }
}