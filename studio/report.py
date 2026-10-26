#!/usr/bin/env python3
"""Read-only production pulse from GitHub Issues.

This is an observable engineering snapshot, not AI-generated progress.
"""
import json
import os
import sys

from orchestrator import Github
from control import labels


def aggregate(issues):
    report = {
        "total_studio_issues": 0, "queued": 0, "dispatched": 0,
        "active": 0, "review": 0, "blocked": 0,
        "needs_attention": 0, "accepted": 0, "closed_unverified": 0,
        "departments": {},
    }
    for issue in issues:
        lab = labels(issue)
        if "studio:approved" not in lab:
            continue
        report["total_studio_issues"] += 1
        dept = next((x[5:] for x in lab if x.startswith("dept:")), "unknown")
        report["departments"][dept] = report["departments"].get(dept, 0) + 1
        if issue.get("state") == "closed":
            name = "accepted" if "studio:accepted" in lab else "closed_unverified"
        elif "ai:needs-attention" in lab:
            name = "needs_attention"
        elif "ai:blocked" in lab:
            name = "blocked"
        elif "ai:review" in lab:
            name = "review"
        elif "ai:claimed" in lab or "ai:running" in lab or "ai:checkpointed" in lab:
            name = "active"
        elif "ai:reserved" in lab or "ai:dispatched" in lab:
            name = "dispatched"
        else:
            name = "queued"
        report[name] += 1
    return report


def main():
    repo = os.getenv("GITHUB_REPOSITORY", "SIGAAMDAD/The_Nomad")
    if repo != "SIGAAMDAD/The_Nomad":
        raise RuntimeError("Unexpected repo")
    token = os.environ.get("GH_TOKEN")
    if not token:
        raise RuntimeError("No GitHub token")
    stats = aggregate(Github(token, repo).list_issues())
    with open("studio-status.json", "w", encoding="utf-8") as handle:
        json.dump(stats, handle, indent=2, sort_keys=True)
        handle.write("\n")
    print("## The Nomad Studio | Observed Issue Status")
    print()
    for name in ("total_studio_issues", "queued", "dispatched", "active",
                 "review", "blocked", "needs_attention", "accepted", "closed_unverified"):
        print("- " + name.replace("_", " ").title() + ": " + str(stats[name]))
    print()
    print("**Accepted** counts require a closed issue with studio:accepted. "
          "This report is not a Godot compilation/merge certificate.")
    print("Department counts: " + json.dumps(stats["departments"], sort_keys=True))


if __name__ == "__main__":
    try:
        main()
    except (OSError, RuntimeError, ValueError) as exc:
        print("REPORT_ERROR: " + str(exc), file=sys.stderr)
        sys.exit(2)
