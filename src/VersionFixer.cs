// Copyright 2023 Lukas Benner, Thomas Weller
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.IO.Packaging;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace AASFix
{
    internal class VersionFixer
    {
        private readonly PackagePartCollection _parts;
        private readonly XmlNamespaceManager _nsMgr = new(new NameTable());
        private readonly XNamespace _version3 = "https://admin-shell.io/aas/3/0";
        private readonly string _oldVersionNodesQuery;
        private readonly List<string> _oldVersions = new()
        {
            "https://admin-shell.io/aas/1/0",
            "http://www.admin-shell.io/aas/1/0",
            "https://www.admin-shell.io/aas/1/0",
            "https://admin-shell.io/aas/2/0",
            "http://www.admin-shell.io/aas/2/0",
            "https://www.admin-shell.io/aas/2/0"
        };

        internal VersionFixer(Package package)
        {
            _parts = package.GetParts();
            _nsMgr.AddNamespace("aas3", "https://admin-shell.io/aas/3/0");
            _oldVersionNodesQuery = BuildQueryForOldVersions(_nsMgr, _oldVersions);
        }

        internal void Fix()
        {
            var xmlFiles = FindXmlFiles();
            foreach (var xmlFile in xmlFiles)
            {
                var xml = LoadXml(xmlFile);
                var incorrectNodes = FindIncorrectNodes(xml);
                FixNamespace(incorrectNodes);
                SaveXml(xmlFile, xml);
            }
        }

        private IEnumerable<PackagePart> FindXmlFiles()
        {
            return _parts.Where(p => p.ContentType == "text/xml");
        }

        private IEnumerable<XElement> FindIncorrectNodes(XDocument xml)
        {
            return xml.XPathSelectElements(_oldVersionNodesQuery, _nsMgr);
        }

        private void FixNamespace(IEnumerable<XElement> incorrectNodes)
        {
            if (incorrectNodes.Any())
            {
                Console.WriteLine("Converting XML to version 3.");
            }

            foreach (var node in incorrectNodes)
            {
                node.Name = _version3 + node.Name.LocalName;
            }
        }

        private string BuildQueryForOldVersions(XmlNamespaceManager nsMgr, List<string> oldVersions)
        {
            var queries = new List<string>();
            var i= 0;
            foreach (var version in oldVersions)
            {
                i++;
                var prefix = $"old{i}";
                queries.Add($"//{prefix}:*");
                nsMgr.AddNamespace(prefix, version);
            }
            return string.Join(" | ", queries);
        }

        private void SaveXml(PackagePart part, XDocument xml)
        {
            using var stream = part.GetStream(FileMode.Open, FileAccess.ReadWrite);
            stream.Seek(0, SeekOrigin.Begin);
            stream.SetLength(0);
            xml.Save(stream);
        }

        private XDocument LoadXml(PackagePart part)
        {
            using var stream = part.GetStream(FileMode.Open, FileAccess.ReadWrite);
            var xml = XDocument.Load(stream);
            return xml;
        }
    }
}
