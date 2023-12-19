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
using System.Xml.Linq;
using System.Xml;
using System.Xml.XPath;

namespace AASFix;

/// <summary>
/// Class for fixing AASX Package Explorer external references to model references.
/// See issue 681 at https://github.com/admin-shell-io/aasx-package-explorer/issues/681
/// </summary>
internal class ExternalReferenceFixer
{
    private readonly PackagePartCollection _parts;
    private readonly XmlNamespaceManager _nsMgr;

    internal ExternalReferenceFixer(Package package)
    {
        _parts = package.GetParts();
        _nsMgr = new XmlNamespaceManager(new NameTable());
        _nsMgr.AddNamespace("aas", "https://admin-shell.io/aas/3/0");
    }

    internal void Fix()
    {
        foreach (var part in _parts)
        {
            if (part.ContentType == "text/xml")
            {
                using var stream = part.GetStream(FileMode.Open, FileAccess.ReadWrite);

                var xml = XDocument.Load(stream);
                var submodelIDs = FindAllSubmodelIDs(xml);
                var externalReferences = FindAllExternalReferences(xml);
                var fixableReferences = FindLikelyLocalReferences(externalReferences, submodelIDs);
                ApplyFix(fixableReferences);

                stream.Seek(0, SeekOrigin.Begin);
                xml.Save(stream);
            }
        }
    }

    private void ApplyFix(List<XElement> fixableReferences)
    {
        foreach (var reference in fixableReferences)
        {
            reference.XPathSelectElement("aas:type", _nsMgr).Value = "ModelReference";
        }
    }

    private List<XElement> FindLikelyLocalReferences(IEnumerable<XElement> externalReferences, List<string> submodelIDs)
    {
        var result = new List<XElement>();
        foreach (var reference in externalReferences)
        {
            var key = reference.XPathSelectElement("aas:keys/aas:key/aas:value", _nsMgr).Value;
            if (submodelIDs.Contains(key))
            {
                Console.WriteLine("Found an external reference that refers to a submodel with the same key as a local submodel.");
                Console.WriteLine($"Fixing the reference to a model reference. Key: {key}");
                result.Add(reference);
            }
        }

        return result;
    }

    private IEnumerable<XElement> FindAllExternalReferences(XDocument xml)
    {
        var references =
            xml.XPathSelectElements(
                "/aas:environment/aas:assetAdministrationShells/aas:assetAdministrationShell/aas:submodels/aas:reference[aas:type/text()=\"ExternalReference\"]",
                _nsMgr);
        return references;
    }

    private List<string> FindAllSubmodelIDs(XDocument xml)
    {
        var submodels = xml.XPathSelectElements("/aas:environment/aas:submodels/aas:submodel/aas:id", _nsMgr);
        var submodelIDs = new List<string>();
        foreach (var submodel in submodels)
        {
            submodelIDs.Add(submodel.Value);
        }

        return submodelIDs;
    }
}