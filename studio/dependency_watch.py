#!/usr/bin/env python3
"""Review-only watcher for the NomadFramework/develop submodule.

It creates/updates an informational GitHub Issue and NEVER moves gitlinks,
merges PRs, or turns dependency updates into approved AI tasks.
"""
import argparse
import json
import os
import sys
import urllib.parse
import urllib.request

GAME = "SIGAAMDAD/The_Nomad"
FRAMEWORK = "SIGAAMDAD/NomadFramework"
BRANCH = "Nomad3D"


def request(token, repo, path, method="GET", payload=None):
    headers = {
        "Accept": "application/vnd.github+json",
        "X-GitHub-Api-Version": "2022-11-28",
        "User-Agent": "nomad-dependency-watch",
    }
    if token:
        headers["Authorization"] = "Bearer " + token
    data = json.dumps(payload).encode("utf-8") if payload is not None else None
    req = urllib.request.Request(
        "https://api.github.com/repos/" + repo + "/" + path,
        headers=headers, method=method, data=data,
    )
    with urllib.request.urlopen(req, timeout=25) as resp:
        return json.loads(resp.read())


def detect(token):
    submodule = request(
        token, GAME, "contents/Source/NomadFramework?ref=" +
        urllib.parse.quote(BRANCH))
    if len(submodule.get("sha", "")) != 40:
        raise RuntimeError("Framework submodule commit unavailable")
    current = request(token, FRAMEWORK, "git/ref/heads/develop")
    head = current["object"]["sha"]
    if len(head) != 40:
        raise RuntimeError("Framework develop commit unavailable")
    return submodule["sha"], head


def existing_issue(token):
    page = 1
    while page <= 4:
        issues = request(token, GAME, "issues?state=open&per_page=100&page=" + str(page))
        for issue in issues:
            if (not issue.get("pull_request") and
                    "<!-- STUDIO_DEPENDENCY_FRAMEWORK -->" in (issue.get("body") or "")):
                return issue
        if len(issues) < 100:
            break
        page += 1
    return None


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--report-only", action="store_true")
    args = parser.parse_args()
    token = os.getenv("GH_TOKEN", "")
    pinned, upstream = detect(token)
    print(json.dumps({"target": "NomadFramework/develop", "game_branch": BRANCH,
                      "pinned_sha": pinned, "upstream_sha": upstream,
                      "update_available": pinned != upstream}, indent=2))
    if args.report_only or not token or pinned == upstream:
        return
    if os.getenv("STUDIO_DEPENDENCY_ISSUES", "false").lower() != "true":
        print("DEPENDENCY_ISSUE_CREATION_DISABLED")
        return
    body = (
        "<!-- STUDIO_DEPENDENCY_FRAMEWORK -->\n"
        "Review the framework submodule pointer in The_Nomad/Nomad3D.\n\n"
        "Pinned gitlink: " + pinned + "\n"
        "Framework develop head: " + upstream + "\n\n"
        "Update is NOT approved automatically. Check source compatibility, "
        "GDExtension/C# builds, ABI and editor behavior, then seek owner approval. "
        "Do not use framework main as the source of truth."
    )
    previous = existing_issue(token)
    if previous:
        if pinned not in (previous.get("body") or "") or upstream not in (previous.get("body") or ""):
            request(token, GAME, "issues/" + str(previous["number"]),
                    method="PATCH", payload={"body": body})
            print("Updated existing review issue #" + str(previous["number"]))
    else:
        created = request(token, GAME, "issues", method="POST", payload={
            "title": "[DEPENDENCY] Review NomadFramework develop gitlink update",
            "body": body,
            "labels": ["dept:engine", "dependency:review"],
        })
        print("Opened dependency review issue #" + str(created["number"]))


if __name__ == "__main__":
    try:
        main()
    except (RuntimeError, OSError, KeyError, ValueError) as error:
        print("DEPENDENCY_WATCH_ERROR: " + str(error), file=sys.stderr)
        sys.exit(2)
