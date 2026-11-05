"""Fail CI on vulnerable dependencies or incomplete NuGet audit results."""
import json
import sys
from pathlib import Path

report = json.loads(Path(sys.argv[1]).read_text(encoding="utf-8-sig"))
issues = list(report.get("problems", []))
for project in report.get("projects", []):
    for framework in project.get("frameworks", []):
        for group in ("topLevelPackages", "transitivePackages"):
            for package in framework.get(group, []):
                if package.get("vulnerabilities"):
                    issues.append({"package": package["id"], "vulnerabilities": package["vulnerabilities"]})
if not report.get("projects"):
    issues.append({"error": "No projects were audited"})
if issues:
    print(json.dumps(issues, ensure_ascii=False, indent=2))
    raise SystemExit(1)
print("PASS: NuGet audit completed without reported vulnerable packages.")
