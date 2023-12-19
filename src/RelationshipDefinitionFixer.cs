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

namespace AASFix
{
    internal class RelationshipDefinitionFixer
    {
        internal void Fix(Package package, List<Fix> fixes)
        {
            // Now that the relationships are fixed,
            // we need to tell the relationship definition that the type has changed.
            // We can't modify that file directly (can we?), so let's load it as an XML document
            // and modify the XML directly.
            foreach (var part in package.GetParts())
            {
                if (!part.Uri.ToString().EndsWith(".rels")) continue;

                Console.WriteLine($"Processing relationship definition {part.Uri}");
                var definition = LoadXml(part);
                FixDefinitionInXml(definition, fixes);
                SaveXml(part, definition);
            }
        }

        private XDocument LoadXml(PackagePart part)
        {
            using var stream = part.GetStream(FileMode.Open, FileAccess.Read);
            return XDocument.Load(stream);
        }

        private void FixDefinitionInXml(XDocument xml, List<Fix> fixes)
        {
            foreach (var fix in fixes)
            {
                var mistakes = xml.Root?.Elements()
                    .Where(e => e.Attribute("Type")?.Value == fix.From);

                foreach (var mistake in mistakes)
                {
                    Console.WriteLine($"Found incorrect definition of type {fix.From}");
                    Console.WriteLine($"Fixing to correct definition of type {fix.To}");
                    mistake.Attribute("Type").Value = fix.To;
                }
            }
        }

        private void SaveXml(PackagePart part, XDocument xml)
        {
            using var stream = part.GetStream(FileMode.Open, FileAccess.Write);
            stream.SetLength(0);
            xml.Save(stream);
        }
    }
}
