#!/usr/bin/env python3
"""The Nomad Studio v4: safe, resumable GitHub Issue -> Slack dispatch.

Dependencies: Python standard library only. Does not launch paid AI execution.
Activation requires repository variable STUDIO_ENABLED=true AND config.mode=active.
"""
import argparse
import json
import os
import re
import sys
import urllib.error
import urllib.parse
import urllib.request
from pathlib import Path

FIELDS = ("STUDIO_TASK_ID", "TARGET", "TARGET_REPO", "TARGET_BRANCH",
          "BASE_SHA", "DRIVE_BRIEF_URL", "OWNER_APPROVED")
HEX40 = re.compile(r"^[0-9a-fA-F]{40}$")
TASK_ID = re.compile(r"^[A-Za-z0-9][A-Za-z0-9_.-]{2,79}$")
URL = re.compile(r"^https://(docs\.google\.com|drive\.google\.com)/")


def load_config(path):
    data = json.loads(Path(path).read_text(encoding="utf-8"))
    if data.get("schema_version") != 4:
        raise ValueError("Unsupported studio config schema")
    return data


def extract_contract(body):
    result = {}
    for line in (body or "").splitlines():
        key, sep, value = line.partition(":")
        if sep and key.strip() in FIELDS:
            if key.strip() in result:
                raise ValueError("Duplicate contract field: " + key.strip())
            result[key.strip()] = value.strip()
    missing = [key for key in FIELDS if not result.get(key)]
    if missing:
        raise ValueError("Missing contract fields: " + ", ".join(missing))
    return result


def validate_issue(issue, config):
    labels = {x["name"] for x in issue.get("labels", [])}
    if issue.get("pull_request"):
        raise ValueError("Pull requests are not assignments")
    if not {"studio:approved", "ai:ready"}.issubset(labels):
        raise ValueError("Issue needs studio:approved and ai:ready labels")
    if "ai:dispatched" in labels:
        raise ValueError("Issue was already dispatched")
    dept_labels = [x for x in labels if x.startswith("dept:")]
    if len(dept_labels) != 1:
        raise ValueError("Issue must have exactly one dept: label")
    department = dept_labels[0].split(":", 1)[1]
    if department not in config["departments"]:
        raise ValueError("Unknown department: " + department)

    task = extract_contract(issue.get("body") or "")
    if not TASK_ID.fullmatch(task["STUDIO_TASK_ID"]):
        raise ValueError("Invalid STUDIO_TASK_ID")
    if not (issue.get("title") or "").startswith("[" + task["STUDIO_TASK_ID"] + "]"):
        raise ValueError("Issue title does not match STUDIO_TASK_ID")
    target = config["targets"].get(task["TARGET"])
    if not target:
        raise ValueError("Unknown TARGET")
    if task["TARGET_REPO"] != target["repo"] or task["TARGET_BRANCH"] != target["branch"]:
        raise ValueError("Target repo/branch differs from studio config")
    if not HEX40.fullmatch(task["BASE_SHA"]):
        raise ValueError("Unpinned BASE_SHA")
    if not URL.match(task["DRIVE_BRIEF_URL"]):
        raise ValueError("Missing authorized Drive brief URL")
    if task["OWNER_APPROVED"] != "YES":
        raise ValueError("Owner approval missing")
    return department, task


def priority(issue):
    labels = {x["name"] for x in issue.get("labels", [])}
    level = 0 if "priority:p0" in labels else 1 if "priority:p1" in labels else 2
    return level, issue["number"]


class GitHub:
    def __init__(self, token, repo):
        self.token = token
        self.repo = repo

    def call(self, method, path, payload=None):
        url = "https://api.github.com/repos/" + self.repo + "/" + path
        data = None if payload is None else json.dumps(payload).encode("utf-8")
        headers = {
            "Accept": "application/vnd.github+json",
            "X-GitHub-Api-Version": "2022-11-28",
            "User-Agent": "nomad-studio-v4",
            "Authorization": "Bearer " + self.token,
        }
        request = urllib.request.Request(url, data=data, headers=headers, method=method)
        with urllib.request.urlopen(request, timeout=25) as response:
            raw = response.read()
            return json.loads(raw) if raw else {}

    def get_issue(self, number):
        return self.call("GET", "issues/" + str(int(number)))

    def ready_issues(self):
        # Number of issues is capped per run, but scan multiple pages to avoid starvation.
        records = []
        for page in range(1, 6):
            query = urllib.parse.urlencode({
                "state": "open", "labels": "ai:ready,studio:approved",
                "per_page": "100", "page": str(page)})
            batch = self.call("GET", "issues?" + query)
            records.extend(x for x in batch if not x.get("pull_request"))
            if len(batch) < 100:
                break
        return sorted(records, key=priority)

    def mark_dispatched(self, number, receipt):
        path = "issues/" + str(int(number))
        self.call("POST", path + "/labels", {"labels": ["ai:dispatched"]})
        self.call("POST", path + "/comments", {"body": receipt})
        self.call("DELETE", path + "/labels/ai%3Aready")


def post_slack(webhook, message):
    request = urllib.request.Request(
        webhook,
        data=json.dumps({"text": message}).encode("utf-8"),
        headers={"Content-Type": "application/json; charset=utf-8"},
        method="POST")
    with urllib.request.urlopen(request, timeout=25) as response:
        if response.status < 200 or response.status >= 300:
            raise RuntimeError("Slack rejected event")


def dispatch_one(api, issue, config, webhook, dry_run=False):
    department, contract = validate_issue(issue, config)
    task_id = contract["STUDIO_TASK_ID"]
    issue_url = issue["html_url"]
    marker = "STUDIO_WORK dept=" + department
    message = (
        marker + " task=" + task_id + " target=" + contract["TARGET"] +
        " issue=" + issue_url + " base=" + contract["BASE_SHA"] +
        " brief=" + contract["DRIVE_BRIEF_URL"] +
        "\nRead the approved issue and current assignment. Verify permissions and "
        "ownership. Persist changes in a branch/PR or record BLOCKED. "
        "Never treat this notification as merge approval."
    )
    print(("DRY_RUN " if dry_run else "DISPATCH ") + task_id + " " + issue_url)
    if dry_run:
        return
    if not webhook:
        raise RuntimeError("STUDIO_SLACK_WEBHOOK not configured; issue remains ready")
    post_slack(webhook, message)
    # If writing this receipt fails after Slack accepts the message, the worker must
    # de-duplicate by issue URL and task ID. Do not silently report success.
    receipt = (
        "<!-- studio-v4-dispatch:" + task_id + " -->\n" +
        "Studio v4 dispatched to dept:" + department + ". "
        "Execution and compilation are NOT verified."
    )
    api.mark_dispatched(issue["number"], receipt)


def run(args):
    config = load_config(args.config)
    dry_run = args.dry_run
    if not dry_run and (config.get("mode") != "active" or
                        os.environ.get("STUDIO_ENABLED", "").lower() != "true"):
        print("STUDIO_DISABLED: enable config mode and repository variable STUDIO_ENABLED")
        return 0
    token = os.environ.get("GH_TOKEN", "")
    if not token:
        raise RuntimeError("GH_TOKEN missing")
    repo = os.environ.get("GITHUB_REPOSITORY") or config["dispatcher_repo"]
    if repo != config["dispatcher_repo"]:
        raise RuntimeError("Wrong dispatcher repository")
    api = GitHub(token, repo)
    issues = [api.get_issue(args.issue)] if args.issue else api.ready_issues()
    sent = 0
    for issue in issues:
        if sent >= min(int(config.get("max_per_run", 10)), 30):
            break
        try:
            dispatch_one(api, issue, config, os.environ.get("STUDIO_SLACK_WEBHOOK", ""), dry_run)
            sent += 1
        except ValueError as exc:
            print("SKIP issue #" + str(issue.get("number")) + ": " + str(exc))
        except (RuntimeError, urllib.error.URLError, urllib.error.HTTPError) as exc:
            print("BLOCKED issue #" + str(issue.get("number")) + ": " + str(exc), file=sys.stderr)
            return 2
    print("processed=" + str(sent))
    return 0


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--config", default="studio/config.json")
    group = parser.add_mutually_exclusive_group(required=True)
    group.add_argument("--issue", type=int)
    group.add_argument("--scan", action="store_true")
    parser.add_argument("--dry-run", action="store_true")
    args = parser.parse_args()
    try:
        return run(args)
    except (ValueError, RuntimeError, urllib.error.URLError, urllib.error.HTTPError) as exc:
        print("STUDIO_ERROR: " + str(exc), file=sys.stderr)
        return 2


if __name__ == "__main__":
    sys.exit(main())
