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

namespace AASFix
{
    internal class RelationshipFixer
    {
        internal void Fix(Package package, List<Fix> fixes)
        {
            foreach (var fix in fixes)
            {
                // Directly modifying a relationship is not supported,
                // so we need to delete the old one and create a new one with the modified properties.
                // We can't do that in one loop, because that would modify the collection we're iterating over.
                var brokenRelationships = FindBrokenRelationships(package, fix);
                RecreateRelationship(brokenRelationships, fix);
            }
        }

        private void RecreateRelationship(List<PackageRelationship> relationShipsToFix, Fix fix)
        {
            foreach (var relationship in relationShipsToFix)
            {
                Console.WriteLine($"Found incorrect type {fix.From}");
                Console.WriteLine($"Fixing to correct type {fix.To}");

                relationship.Package.DeleteRelationship(relationship.Id);
                relationship.Package.CreateRelationship(relationship.TargetUri, relationship.TargetMode, fix.To, relationship.Id);
            }
        }

        private List<PackageRelationship> FindBrokenRelationships(Package package, Fix fix)
        {
            var relationShipsToFix = new List<PackageRelationship>();
            foreach (var relationship in package.GetRelationships())
            {
                if (relationship.RelationshipType != fix.From) continue;
                relationShipsToFix.Add(relationship);
            }

            return relationShipsToFix;
        }
    }
}
