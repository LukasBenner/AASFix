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

namespace AASFix;

public class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 3)
        {
            Console.WriteLine($"Too few arguments. Only {args.Length} arguments given.");
            PrintUsage(); return;
        }

        if (!new[] { "--fix", "--unfix" }.Contains(args[2]))
        {
            Console.WriteLine($"Unknown operation: {args[2]}");
            PrintUsage(); return;
        }

        if (File.Exists(args[1]))
        {
            Console.WriteLine("Output file already exists. This program will not overwrite existing files.");
            return;
        }

        new Program().Run(args[0], args[1], args[2] == "--unfix");
    }

    readonly List<Fix> _fixes = new()
    {
        new Fix
        {
            From = "http://www.admin-shell.io/aasx/relationships/aasx-origin",
            To = "http://admin-shell.io/aasx/relationships/aasx-origin"
        },
        new Fix
        {
            From = "http://www.admin-shell.io/aasx/relationships/aas-spec",
            To = "http://admin-shell.io/aasx/relationships/aas-spec"
        },
    };

    private void Run(string inputFileName, string outputFileName, bool unfix)
    {
        if (unfix) SwapFixes(_fixes);

        // Package does not have a SaveAs() method. It can be modified in place only.
        // So, let's work on the destination file immediately.
        File.Copy(inputFileName, outputFileName);
        using var package = Package.Open(outputFileName, FileMode.Open, FileAccess.ReadWrite);

        new RelationshipFixer().Fix(package, _fixes);
        new RelationshipDefinitionFixer().Fix(package, _fixes);
            
        // DisplayContentsForDebuggingPurposes(package);
        package.Flush();
    }

    private void SwapFixes(List<Fix> fixes)
    {
        foreach (var fix in fixes)
        {
            (fix.From, fix.To) = (fix.To, fix.From);
        }
    }

    private void DisplayContentsForDebuggingPurposes(Package package)
    {
        foreach (var part in package.GetParts())
        {
            Console.WriteLine($"URI of part: {part.Uri}");

            using var partStream = part.GetStream(FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(partStream);
            Console.WriteLine($"Content of the part:\n {reader.ReadToEnd()}");
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("AASFix");
        Console.WriteLine("Fixes or unfixes the namespace issue (bug #666) in an AASX file.");
        Console.WriteLine("You can convert a file back and forth.");
        Console.WriteLine();
        Console.WriteLine("Usage: AASFix <input file name> <output file name> [--fix|--unfix]");
        Console.WriteLine("       <input file name> : source file for reading");
        Console.WriteLine("       <output file name>: destination file for writing");
        Console.WriteLine("       --fix             : correct namespace issues, i.e. repair a file according the standard.");
        Console.WriteLine("                           This will make the output file usable e.g. with the Python basyx library.");
        Console.WriteLine("                           The output file may not work in unpatched versions of AASX package explorer.");
        Console.WriteLine("       --unfix           : reverse the operation, i.e. break the file according the standard.");
        Console.WriteLine("                           This will make the output file usable with unpatched versions of AASX package explorer.");
        Console.WriteLine("                           The output file may not work e.g. with the Python basyx library.");
    }
}