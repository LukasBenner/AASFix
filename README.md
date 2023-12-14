# AASFix

A tool to fix namespace issues in AASX files.

This tool becomes obsolete when [Bug #666](https://github.com/admin-shell-io/aasx-package-explorer/issues/666) is fixed and AASX Package Explorer produces standard compliant files.

## Description

This tool can fix the namespace problems of certain versions of AASX Package Explorer (e.g. v2023-11-17), documented in [Bug #666](https://github.com/admin-shell-io/aasx-package-explorer/issues/666).

After applying the fix (`--fix`), the AASX file should be standard compliant and work with libraries such as the [Basyx Python SDK](https://github.com/eclipse-basyx/basyx-python-sdk).  After un-applying the fix (`--unfix`), the AASX file should be compatible with AASX Package Explorer again. 

Usage: `AASFix <input_file> <output_file> --fix|--unfix`
