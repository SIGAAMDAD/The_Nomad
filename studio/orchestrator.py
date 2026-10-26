#!/usr/bin/env python3
"""GitHub-backed studio event loop.

Run by an Actions workflow with concurrency=1. Persist state on GitHub; no
in-process state is assumed to survive an execution. Does not invoke AI APIs.
"""
import argparse
import datetime as dt
import hashlib
import json
import os
import re
import sys
import urllib.error
import urllib.parse
import urllib.request
from pathlib import Path

from control import (ACTIVE_LABELS, ContractError, labels, ready_plan,
                     validated)

OUTBOX_RE = re.compile(r"<!-- STUDIO_OUTBOX (\{[^\n]*\}) -->")
EVENT_RE = re.compile(r"<!-- STUDIO_EVENT (\{[^\n]*\}) -->")
ACTIVE_STATUSES = ("ai:reserved", "ai:dispatched", "ai:claimed",
                   "ai:running", "ai:checkpointed", "ai:review", "ai:blocked",
                   "ai:needs-attention")
EVENT_LABELS = {
    "CLAIMED": "ai:claimed",
    "RUNNING": "ai:running",
    "CHECKPOINT": "ai:checkpointed",
    "SUBMITTED": "ai:review",
    "BLOCKED": "ai:blocked",
}
UTC = dt.timezone.utc


def now_utc():
    return dt.datetime.now(UTC)


def parse_time(value):
    return dt.datetime.fromisoformat(value.replace("Z", "+00:00"))


def github_url(repo, path):
    return "https://api.github.com/repos/" + repo + "/" + path


class Github:
    def __init__(self, token, repo):
        self.token = token
        self.repo = repo

    def call(self, method, path, payload=None, repo=None):
        url = github_url(repo or self.repo, path)
        data = json.dumps(payload).encode("utf-8") if payload is not None else None
        req = urllib.request.Request(
            url, data=data, method=method, headers={
                "Accept": "application/vnd.github+json",
                "X-GitHub-Api-Version": "2022-11-28",
                "Authorization": "Bearer " + self.token,
                "User-Agent": "nomad-studio-v5",
                "Content-Type": "application/json",
            },
        )
        with urllib.request.urlopen(req, timeout=25) as response:
            data = response.read()
            return json.loads(data) if data else {}

    def list_issues(self):
        result = []
        for page in range(1, 11):
            query = urllib.parse.urlencode({
                "state": "all", "per_page": 100, "page": page,
            })
            batch = self.call("GET", "issues?" + query)
            result.extend(row for row in batch if not row.get("pull_request"))
            if len(batch) < 100:
                return result
        raise RuntimeError("Issue pagination safety limit hit; refuse partial plan")

    def comments(self, number):
        out = []
        for page in range(1, 4):
            batch = self.call(
                "GET", "issues/" + str(number) + "/comments?per_page=100&page=" + str(page))
            out.extend(batch)
            if len(batch) < 100:
                return out
        raise RuntimeError("Comment pagination safety limit hit")

    def add_label(self, number, label):
        self.call("POST", "issues/" + str(number) + "/labels", {"labels": [label]})

    def remove_label(self, number, label):
        try:
            self.call("DELETE", "issues/" + str(number) +
                      "/labels/" + urllib.parse.quote(label, safe=""))
        except urllib.error.HTTPError as error:
            if error.code != 404:
                raise

    def comment(self, number, body):
        return self.call("POST", "issues/" + str(number) + "/comments",
                         {"body": body})

    def transition(self, issue, state):
        number = issue["number"]
        if state not in ACTIVE_STATUSES:
            raise ValueError("Invalid state")
        self.add_label(number, state)
        for previous in ACTIVE_STATUSES:
            if previous != state and previous in labels(issue):
                self.remove_label(number, previous)

    def source_head(self, rec):
        escaped_branch = urllib.parse.quote(rec["TARGET_BRANCH"], safe="/")
        result = self.call("GET", "git/ref/heads/" + escaped_branch,
                           repo=rec["TARGET_REPO"])
        return result["object"]["sha"]


def outbox(comments, task_id):
    events = []
    for comment in comments:
        match = OUTBOX_RE.search(comment.get("body") or "")
        if not match:
            continue
        try:
            value = json.loads(match.group(1))
        except (ValueError, TypeError):
            continue
        if value.get("task_id") == task_id:
            events.append((comment, value))
    return events


def lease_id(issue, record):
    return hashlib.sha256(
        (str(issue["number"]) + ":" + record["APPROVAL_SIG"]).encode("utf-8")
    ).hexdigest()[:24]


def post_slack(webhook, text):
    if not webhook.startswith("https://hooks.slack.com/"):
        raise RuntimeError("Expected an authorized Slack incoming webhook")
    request = urllib.request.Request(
        webhook, data=json.dumps({"text": text}).encode("utf-8"),
        headers={"Content-Type": "application/json"}, method="POST")
    with urllib.request.urlopen(request, timeout=25) as response:
        if response.status != 200 or response.read().strip() != b"ok":
            raise RuntimeError("Slack webhook did not acknowledge delivery")


def dispatch(api, issue, rec, webhook, dry_run):
    number = issue["number"]
    task_id = rec["STUDIO_TASK_ID"]
    if api.source_head(rec).lower() != rec["BASE_SHA"].lower():
        raise ContractError("Target source head changed: refresh approved baseline")
    previous = outbox(api.comments(number), task_id)
    if previous:
        raise ContractError("Prior outbox exists; reconcile before redispatch")

    lease = lease_id(issue, rec)
    data = {
        "version": 1, "task_id": task_id, "lease_id": lease,
        "status": "RESERVED", "created_at": now_utc().isoformat(),
    }
    print(("DRY_RUN" if dry_run else "RESERVE") + " " + task_id +
          " issue=" + str(number))
    if dry_run:
        return
    if not webhook:
        raise RuntimeError("STUDIO_SLACK_WEBHOOK is not configured")
    api.add_label(number, "ai:reserved")
    api.comment(number, "<!-- STUDIO_OUTBOX " +
                json.dumps(data, separators=(",", ":")) + " -->\n" +
                "Dispatch reserved; waiting for worker delivery.")
    text = (
        "STUDIO_WORK version=5 task=" + task_id + " lease=" + lease +
        " department=" + rec["DEPARTMENT"] + " issue=" + issue["html_url"] +
        " target=" + rec["TARGET_REPO"] + " branch=" + rec["TARGET_BRANCH"] +
        " baseline=" + rec["BASE_SHA"] +
        " brief=" + rec["DRIVE_BRIEF_URL"] +
        "\nAcknowledge using a verified GitHub worker event. Do not claim "
        "code compiled or merge approved without actual evidence. "
        "Deduplicate by task + lease. Never run from a different baseline."
    )
    try:
        post_slack(webhook, text)
    except Exception:
        api.add_label(number, "ai:needs-attention")
        api.remove_label(number, "ai:reserved")
        raise
    api.comment(number, "<!-- STUDIO_OUTBOX " +
                json.dumps({**data, "status": "DELIVERED"},
                           separators=(",", ":")) + " -->\n" +
                "Notification delivered; worker acknowledgment pending.")
    api.add_label(number, "ai:dispatched")
    api.remove_label(number, "ai:ready")
    api.remove_label(number, "ai:reserved")


def worker_event(comments, task_id, lease, allowed_logins):
    valid = []
    for comment in comments:
        sender = (comment.get("user") or {}).get("login", "")
        if sender not in allowed_logins:
            continue
        match = EVENT_RE.search(comment.get("body") or "")
        if not match:
            continue
        try:
            event = json.loads(match.group(1))
        except (TypeError, ValueError):
            continue
        if (event.get("task_id") != task_id or
                event.get("lease_id") != lease or
                event.get("type") not in EVENT_LABELS):
            continue
        valid.append((comment, event))
    valid.sort(key=lambda pair: (pair[0].get("created_at", ""), pair[0].get("id", 0)))
    return valid[-1] if valid else None


def reconcile(api, issues, config, secret, dry_run, now=None):
    now = now or now_utc()
    allowed = set(config.get("worker_logins", []))
    lease_minutes = int(config.get("lease_minutes", 120))
    actions = []
    for issue in issues:
        if issue.get("state") != "open" or not (ACTIVE_LABELS & labels(issue)):
            continue
        try:
            rec = validated(issue, config, secret, require_ready=False)
        except ContractError:
            continue
        comments = api.comments(issue["number"])
        receipts = outbox(comments, rec["STUDIO_TASK_ID"])
        if not receipts:
            continue
        lease = lease_id(issue, rec)
        event = worker_event(comments, rec["STUDIO_TASK_ID"], lease, allowed)
        if event:
            state = EVENT_LABELS[event[1]["type"]]
            if state not in labels(issue):
                actions.append((issue["number"], state))
                if not dry_run:
                    api.transition(issue, state)
            continue
        oldest = min(parse_time(x[1]["created_at"]) for x in receipts
                     if "created_at" in x[1])
        if (now - oldest).total_seconds() >= lease_minutes * 60:
            actions.append((issue["number"], "ai:needs-attention"))
            if not dry_run:
                api.transition(issue, "ai:needs-attention")
                api.comment(issue["number"], "Lease expired without an authorized "
                            "worker acknowledgment. Manual reconciliation required; "
                            "no automatic duplicate execution.")
    return actions


def run(args):
    config = json.loads(Path(args.config).read_text(encoding="utf-8"))
    if config.get("schema_version") != 5:
        raise RuntimeError("Expected Studio V5 configuration")
    if not args.dry_run and (config.get("mode") != "active" or
                             os.getenv("STUDIO_ENABLED", "").lower() != "true"):
        print("STUDIO_DISABLED: dispatcher in staging or repository kill switch off")
        return
    token = os.getenv("GH_TOKEN")
    if not token:
        raise RuntimeError("GH_TOKEN is not configured")
    secret = os.getenv("STUDIO_APPROVAL_KEY")
    if not secret:
        raise RuntimeError("STUDIO_APPROVAL_KEY required for signed task dispatch")
    repo = os.getenv("GITHUB_REPOSITORY") or config["dispatcher_repo"]
    if repo != config["dispatcher_repo"]:
        raise RuntimeError("Unexpected repository")
    api = Github(token, repo)
    issues = api.list_issues()
    if args.mode == "reconcile":
        print("reconciliation=" + json.dumps(reconcile(
            api, issues, config, secret, args.dry_run)))
        return
    ready, errors = ready_plan(issues, config, secret)
    for number, error in sorted(errors.items(), key=lambda x: str(x[0])):
        print("INVALID issue=" + str(number) + " reason=" + error)
    if args.issue:
        ready = [pair for pair in ready if pair[0]["number"] == args.issue]
    for issue, rec in ready:
        try:
            dispatch(api, issue, rec,
                     os.getenv("STUDIO_SLACK_WEBHOOK", ""), args.dry_run)
        except (ContractError, RuntimeError, urllib.error.URLError) as error:
            print("BLOCKED issue=" + str(issue["number"]) + " reason=" + str(error))
            # An unverified dispatch remains blocked; no ambiguous retries.
            if not args.dry_run:
                api.add_label(issue["number"], "ai:needs-attention")
    print("selected=" + str(len(ready)))


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--config", default="studio/config.json")
    parser.add_argument("--mode", choices=("dispatch", "reconcile"),
                        default="dispatch")
    parser.add_argument("--issue", type=int)
    parser.add_argument("--dry-run", action="store_true")
    args = parser.parse_args()
    try:
        run(args)
    except (RuntimeError, ValueError, urllib.error.URLError) as exc:
        print("STUDIO_ERROR: " + str(exc), file=sys.stderr)
        sys.exit(2)


if __name__ == "__main__":
    main()
