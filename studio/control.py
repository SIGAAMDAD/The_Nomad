"""Strict, deterministic studio task contracts and dependency-aware allocation.

No I/O or model execution in this module. Signed approvals bind the complete
assignment to its reviewed scope. Labels alone do not confer authorization.
"""
import fnmatch
import hashlib
import hmac
import json
import re

SIGNED_FIELDS = (
    "STUDIO_TASK_ID", "TARGET", "TARGET_REPO", "TARGET_BRANCH", "BASE_SHA",
    "DRIVE_BRIEF_URL", "OWNER_APPROVED", "DEPARTMENT", "DEPENDS_ON",
    "OWNED_PATHS", "PROTECTED_PATHS",
)
TASK_RE = re.compile(r"^[A-Za-z0-9][A-Za-z0-9_.-]{2,79}$")
SHA_RE = re.compile(r"^[a-fA-F0-9]{40}$")
HEX_RE = re.compile(r"^[a-fA-F0-9]{64}$")
PATH_RE = re.compile(r"^[A-Za-z0-9_./*+@-]+$")
DRIVE_RE = re.compile(r"^https://(docs[.]google[.]com|drive[.]google[.]com)/")
ACTIVE_LABELS = {
    "ai:reserved", "ai:dispatched", "ai:claimed", "ai:running",
    "ai:checkpointed",
}


class ContractError(ValueError):
    pass


def labels(issue):
    return {x["name"] for x in issue.get("labels", [])}


def fields_from_body(body):
    result = {}
    keys = set(SIGNED_FIELDS) | {"APPROVAL_SIG"}
    for line in (body or "").splitlines():
        key, sep, value = line.partition(":")
        key = key.strip()
        if sep and key in keys:
            if key in result:
                raise ContractError("Duplicate contract field: " + key)
            result[key] = value.strip()
    missing = [key for key in keys if not result.get(key)]
    if missing:
        raise ContractError("Missing fields: " + ", ".join(sorted(missing)))
    return result


def canonical_json(record):
    return json.dumps(
        {key: record[key] for key in SIGNED_FIELDS},
        sort_keys=True, separators=(",", ":"), ensure_ascii=False,
    )


def compute_signature(record, key):
    if not key:
        raise ContractError("Approval key is not configured")
    return hmac.new(
        key.encode("utf-8"), canonical_json(record).encode("utf-8"),
        hashlib.sha256,
    ).hexdigest()


def split_deps(value):
    if value == "-":
        return ()
    deps = tuple(part.strip() for part in value.split(","))
    if not deps or len(set(deps)) != len(deps) or any(not TASK_RE.fullmatch(x) for x in deps):
        raise ContractError("Invalid or duplicate dependencies")
    return deps


def split_paths(value, allow_empty=False):
    if value == "-" and allow_empty:
        return ()
    paths = tuple(part.strip() for part in value.split(";"))
    if not paths or len(set(paths)) != len(paths):
        raise ContractError("Empty or duplicate owned paths")
    for item in paths:
        if (not PATH_RE.fullmatch(item) or item.startswith("/") or
                item.startswith("./") or
                any(segment in (".", "..") for segment in item.split("/"))):
            raise ContractError("Invalid owned/protected path: " + item)
    return paths


def validated(issue, config, secret, require_ready=True):
    if issue.get("pull_request"):
        raise ContractError("PR is not a studio task")
    lab = labels(issue)
    if "studio:approved" not in lab:
        raise ContractError("Not owner-approved")
    if require_ready and ("ai:ready" not in lab or "ai:dispatched" in lab):
        raise ContractError("Not ready for dispatch")
    dept_labels = [x[5:] for x in lab if x.startswith("dept:")]
    if len(dept_labels) != 1:
        raise ContractError("Exactly one department label required")

    record = fields_from_body(issue.get("body"))
    if not TASK_RE.fullmatch(record["STUDIO_TASK_ID"]):
        raise ContractError("Invalid task ID")
    if not (issue.get("title") or "").startswith("[" + record["STUDIO_TASK_ID"] + "]"):
        raise ContractError("Title/task ID mismatch")
    if record["DEPARTMENT"] != dept_labels[0]:
        raise ContractError("Department label mismatch")
    if record["DEPARTMENT"] not in config["departments"]:
        raise ContractError("Unknown department")
    if record["TARGET"] not in config["targets"]:
        raise ContractError("Unknown target")
    target = config["targets"][record["TARGET"]]
    if (record["TARGET_REPO"] != target["repo"] or
            record["TARGET_BRANCH"] != target["branch"]):
        raise ContractError("Repository/branch mismatch")
    if not SHA_RE.fullmatch(record["BASE_SHA"]):
        raise ContractError("Unpinned source baseline")
    if not DRIVE_RE.match(record["DRIVE_BRIEF_URL"]):
        raise ContractError("Invalid Drive URL")
    if record["OWNER_APPROVED"] != "YES":
        raise ContractError("Owner approval absent")
    if not HEX_RE.fullmatch(record["APPROVAL_SIG"]):
        raise ContractError("Invalid approval signature format")
    if not hmac.compare_digest(compute_signature(record, secret),
                               record["APPROVAL_SIG"].lower()):
        raise ContractError("Approval signature mismatch")
    deps = split_deps(record["DEPENDS_ON"])
    if record["STUDIO_TASK_ID"] in deps:
        raise ContractError("Self-dependency")
    allow_empty = record["DEPARTMENT"] in ("design", "narrative", "qa", "integration")
    record["deps"] = deps
    record["owned"] = split_paths(record["OWNED_PATHS"], allow_empty=allow_empty)
    record["protected"] = split_paths(record["PROTECTED_PATHS"], allow_empty=True)
    if set(record["owned"]) & set(record["protected"]):
        raise ContractError("Ownership overlaps protected files")
    return record


def _fixed_prefix(path):
    parts = path.split("/")
    prefix = []
    for part in parts:
        if any(x in part for x in "*?["):
            break
        prefix.append(part)
    return "/".join(prefix).rstrip("/")


def may_conflict(paths_a, paths_b):
    """Conservative path-lock intersection, including directory globs."""
    for a in paths_a:
        for b in paths_b:
            if a == b or fnmatch.fnmatchcase(a, b) or fnmatch.fnmatchcase(b, a):
                return True
            x, y = _fixed_prefix(a), _fixed_prefix(b)
            if x and y and (x == y or x.startswith(y + "/") or y.startswith(x + "/")):
                return True
    return False


def _dependency_cycle(records):
    visiting = set()
    visited = set()

    def visit(key):
        if key in visiting:
            raise ContractError("Dependency cycle detected at " + key)
        if key in visited:
            return
        visiting.add(key)
        for dep in records[key]["deps"]:
            if dep in records:
                visit(dep)
        visiting.remove(key)
        visited.add(key)

    for task in records:
        visit(task)


def ready_plan(issues, config, secret):
    """Return a bounded list of eligible (issue, validated-contract) pairs.

    A closed issue with studio:accepted is a completed prerequisite. A submitted
    PR alone cannot unblock anything. Active leases block same-domain workers
    and conflicting source paths.
    """
    records, task_issues, errors = {}, {}, {}
    for issue in issues:
        if issue.get("pull_request"):
            continue
        try:
            rec = validated(issue, config, secret, require_ready=False)
        except ContractError as error:
            if "ai:ready" in labels(issue):
                errors[issue["number"]] = str(error)
            continue
        task = rec["STUDIO_TASK_ID"]
        if task in records:
            errors[issue["number"]] = "Duplicate task ID"
            errors[task_issues[task]["number"]] = "Duplicate task ID"
            continue
        records[task] = rec
        task_issues[task] = issue

    _dependency_cycle(records)
    active = []
    capacity = {dept: 0 for dept in config["departments"]}
    for task, record in records.items():
        issue = task_issues[task]
        if issue.get("state") != "open":
            continue
        if ACTIVE_LABELS & labels(issue):
            active.append(record)
            capacity[record["DEPARTMENT"]] += 1

    candidates = sorted(
        ((task_issues[key], record) for key, record in records.items()
         if "ai:ready" in labels(task_issues[key])),
        key=lambda x: (
            0 if "priority:p0" in labels(x[0]) else
            1 if "priority:p1" in labels(x[0]) else 2,
            x[0]["number"],
        ),
    )
    selected = []
    bound = max(0, min(int(config.get("max_per_run", 10)), 30))
    for issue, record in candidates:
        if len(selected) >= bound:
            break
        if issue["number"] in errors or issue.get("state") != "open":
            continue
        lab = labels(issue)
        if ACTIVE_LABELS & lab or "ai:needs-attention" in lab or "ai:blocked" in lab:
            continue
        if any(
            dep not in records or
            "studio:accepted" not in labels(task_issues[dep]) or
            task_issues[dep].get("state") != "closed"
            for dep in record["deps"]
        ):
            continue
        limit = int(config.get("worker_capacity", {}).get(record["DEPARTMENT"], 1))
        if capacity[record["DEPARTMENT"]] >= limit:
            continue
        if any(other["TARGET_REPO"] == record["TARGET_REPO"] and
               may_conflict(other["owned"], record["owned"]) and
               other["owned"] and record["owned"] for other in active):
            continue
        selected.append((issue, record))
        active.append(record)
        capacity[record["DEPARTMENT"]] += 1
    return selected, errors
