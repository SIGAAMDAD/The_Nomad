#!/usr/bin/env python3
"""Turn a failed trusted GitHub CI run into a review-only regression Issue.

Requires a workflow_run event. Does NOT approve or dispatch implementation.
"""
import hashlib
import json
import os
import sys
import urllib.parse
import urllib.request

ALLOWED = (
    "Studio V5 / Contracts and Config",
    "Studio / Dependabot Review",
    "Studio v4 / Contract Tests",
)


def fingerprint(event):
    run = event.get("workflow_run") or {}
    if run.get("conclusion") != "failure" or run.get("name") not in ALLOWED:
        return None
    sha = run.get("head_sha") or ""
    if len(sha) != 40:
        return None
    data = "|".join((run.get("name", ""), sha,
                     str(run.get("run_attempt", 1))))
    return hashlib.sha256(data.encode("utf-8")).hexdigest()[:24]


def call(token, repo, method, path, payload=None):
    request = urllib.request.Request(
        "https://api.github.com/repos/" + repo + "/" + path,
        method=method,
        data=json.dumps(payload).encode("utf-8") if payload is not None else None,
        headers={
            "Accept": "application/vnd.github+json",
            "Authorization": "Bearer " + token,
            "X-GitHub-Api-Version": "2022-11-28",
            "Content-Type": "application/json",
            "User-Agent": "nomad-studio-triage",
        },
    )
    with urllib.request.urlopen(request, timeout=25) as response:
        return json.loads(response.read())


def main():
    if os.getenv("STUDIO_TRIAGE_ENABLED", "").lower() != "true":
        print("TRIAGE_DISABLED")
        return
    event_path = os.getenv("GITHUB_EVENT_PATH")
    token = os.getenv("GH_TOKEN", "")
    repo = os.getenv("GITHUB_REPOSITORY", "")
    if not event_path or not token or repo != "SIGAAMDAD/The_Nomad":
        raise RuntimeError("Missing trusted GitHub Actions environment")
    with open(event_path, encoding="utf-8") as handle:
        event = json.load(handle)
    key = fingerprint(event)
    if not key:
        print("Ignored unrelated or successful workflow run")
        return
    run = event["workflow_run"]
    marker = "<!-- STUDIO_CI_FAILURE:" + key + " -->"
    for page in range(1, 6):
        issues = call(token, repo, "GET", "issues?state=all&per_page=100&page=" + str(page))
        if any(marker in (item.get("body") or "") for item in issues if not item.get("pull_request")):
            print("Regression already recorded: " + key)
            return
        if len(issues) < 100:
            break
    body = (
        marker + "\nAutomated CI failure requiring independent triage.\n\n"
        "Workflow: " + run["name"] + "\nCommit: " + run["head_sha"] +
        "\nRun: " + run.get("html_url", "") +
        "\n\nNo source correction, retry, or model execution was "
        "automatically authorized. Determine the first bad commit and "
        "reproduction before planning a patch."
    )
    issue = call(token, repo, "POST", "issues", {
        "title": "[CI-TRIAGE] " + run["name"] + " failure " + key[:8],
        "body": body,
        "labels": ["dept:qa", "priority:p0", "studio:needs-review"],
    })
    print("Created review-only regression issue " + issue["html_url"])


if __name__ == "__main__":
    try:
        main()
    except (RuntimeError, OSError, ValueError, KeyError) as error:
        print("TRIAGE_ERROR: " + str(error), file=sys.stderr)
        sys.exit(2)
