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

internal class EmptySemanticIdFixer
{
	private readonly PackagePartCollection _parts;
	private readonly XmlNamespaceManager _nsMgr;

	internal EmptySemanticIdFixer(Package package)
	{
		_parts = package.GetParts();
		_nsMgr = new XmlNamespaceManager(new NameTable());
		_nsMgr.AddNamespace("aas", "https://admin-shell.io/aas/3/0");
	}

	internal void Fix()
	{
		foreach (var part in _parts)
		{
			if (!ContentType.IsXml(part)) continue;

			using var stream = part.GetStream(FileMode.Open, FileAccess.ReadWrite);

			var xml = XDocument.Load(stream);
			var emptySemanticIds = FindAllEmptySemanticIds(xml);
			ApplyFix(emptySemanticIds);

			stream.Seek(0, SeekOrigin.Begin);
			stream.SetLength(0);
			xml.Save(stream);
		}
	}

	private void ApplyFix(List<XElement> emptySemanticIds)
	{
		foreach (XElement emptySemanticId in emptySemanticIds)
		{
			emptySemanticId.Remove();
		}
	}

	private List<XElement> FindAllEmptySemanticIds(XDocument xml)
	{
		var semanticIds = xml.XPathSelectElements("//aas:semanticId[aas:keys[not(aas:key)]]", _nsMgr);
		return semanticIds.ToList();
	}
}