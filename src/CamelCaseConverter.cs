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

using System.Text;

namespace AASFix;

internal class CamelCaseConverter
{
    internal string ToHumanReadable(string camelCase)
    {
        if (string.IsNullOrEmpty(camelCase)) { return camelCase; }

        var builder = new StringBuilder(camelCase.Length);
        foreach (var c in camelCase)
        {
            if (char.IsUpper(c))
            {
                builder.Append(' ');
            }
            builder.Append(c);
        }

        return builder.ToString();
    }
}