"""Studio V5 deterministic tests: approvals, dependencies, ownership, workers."""
import copy
import hashlib
import hmac
import json
import sys
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
import control
import orchestrator

CONFIG = json.loads((Path(__file__).resolve().parents[1] / "config.json").read_text())
SECRET = "test-only-secret-key-not-for-production-0123456789"


def ticket(task="ITEM-0042", dept="gameplay", number=42, depends="-",
           owned="Source/Game/Items/**", labels=None, state="open"):
    record = {
        "STUDIO_TASK_ID": task, "TARGET": "game",
        "TARGET_REPO": "SIGAAMDAD/The_Nomad",
        "TARGET_BRANCH": "Nomad3D", "BASE_SHA": "a" * 40,
        "DRIVE_BRIEF_URL": "https://docs.google.com/document/d/sample/edit",
        "OWNER_APPROVED": "YES", "DEPARTMENT": dept,
        "DEPENDS_ON": depends, "OWNED_PATHS": owned,
        "PROTECTED_PATHS": "Source/NomadFramework/**",
    }
    sig = control.compute_signature(record, SECRET)
    body = "\n".join(key + ": " + record[key] for key in control.SIGNED_FIELDS)
    body += "\nAPPROVAL_SIG: " + sig
    return {
        "number": number, "title": "[" + task + "] implementation",
        "state": state, "body": body, "labels": [
            {"name": name} for name in
            (labels if labels is not None else
             ["studio:approved", "ai:ready", "dept:" + dept, "priority:p1"])
        ],
        "html_url": "https://github.com/SIGAAMDAD/The_Nomad/issues/" + str(number),
    }


class ContractTests(unittest.TestCase):
    def test_configuration_baselines(self):
        self.assertEqual(CONFIG["schema_version"], 5)
        self.assertEqual(CONFIG["mode"], "staging")
        self.assertEqual(CONFIG["targets"]["game"]["branch"], "Nomad3D")
        self.assertEqual(CONFIG["targets"]["framework"]["branch"], "develop")

    def test_signature_is_stable_and_binds_contract(self):
        item = ticket()
        validated = control.validated(item, CONFIG, SECRET)
        self.assertEqual(validated["STUDIO_TASK_ID"], "ITEM-0042")
        tampered = copy.deepcopy(item)
        tampered["body"] = tampered["body"].replace("Nomad3D", "main")
        with self.assertRaises(control.ContractError):
            control.validated(tampered, CONFIG, SECRET)

    def test_reject_tampered_paths(self):
        item = ticket()
        item["body"] = item["body"].replace("Source/Game/Items/**",
                                             "Source/Game/Combat/**")
        with self.assertRaisesRegex(control.ContractError, "signature"):
            control.validated(item, CONFIG, SECRET)

    def test_reject_mismatched_department_label(self):
        item = ticket()
        item["labels"] = [{"name": x} for x in ["studio:approved",
                                                "ai:ready", "dept:engine"]]
        with self.assertRaises(control.ContractError):
            control.validated(item, CONFIG, SECRET)

    def test_reject_duplicate_body_key(self):
        item = ticket()
        item["body"] += "\nTARGET: game"
        with self.assertRaisesRegex(control.ContractError, "Duplicate"):
            control.validated(item, CONFIG, SECRET)

    def test_no_dotdot_or_absolute_owned_paths(self):
        self.assertRaises(control.ContractError, control.split_paths, "../secret")
        self.assertRaises(control.ContractError, control.split_paths, "/root/file")

    def test_dependency_cycle_rejected(self):
        a = ticket("TASK-A", number=1, depends="TASK-B")
        b = ticket("TASK-B", number=2, depends="TASK-A")
        with self.assertRaisesRegex(control.ContractError, "cycle"):
            control.ready_plan([a, b], CONFIG, SECRET)

    def test_missing_dependency_blocks(self):
        a = ticket(depends="TASK-OTHER")
        plan, _ = control.ready_plan([a], CONFIG, SECRET)
        self.assertEqual(plan, [])

    def test_accepted_dependency_unblocks(self):
        parent = ticket("TASK-ONE", number=1, state="closed",
                        labels=["studio:approved", "studio:accepted", "dept:gameplay"])
        child = ticket(depends="TASK-ONE")
        plan, _ = control.ready_plan([child, parent], CONFIG, SECRET)
        self.assertEqual([issue["number"] for issue, _ in plan], [42])

    def test_open_parent_cannot_unblock(self):
        parent = ticket("TASK-ONE", number=1,
                        labels=["studio:approved", "studio:accepted", "dept:gameplay"])
        child = ticket(depends="TASK-ONE")
        plan, _ = control.ready_plan([parent, child], CONFIG, SECRET)
        self.assertFalse(plan)

    def test_worker_capacity_blocks_same_department(self):
        busy = ticket("TASK-BUSY", number=1,
                      labels=["studio:approved", "ai:dispatched", "dept:gameplay"])
        next_one = ticket("TASK-NEXT", number=2, owned="Source/Game/Combat/**")
        plan, _ = control.ready_plan([busy, next_one], CONFIG, SECRET)
        self.assertFalse(plan)

    def test_ownership_conflicts_across_departments(self):
        busy = ticket("TASK-BUSY", dept="engine", number=1,
                      labels=["studio:approved", "ai:running", "dept:engine"])
        next_one = ticket("TASK-NEXT", dept="gameplay", number=2,
                          owned="Source/Game/Items/Consumable.cs")
        plan, _ = control.ready_plan([busy, next_one], CONFIG, SECRET)
        self.assertFalse(plan)

    def test_independent_work_dispatches_in_parallel(self):
        a = ticket("TASK-ENG", dept="engine", number=1,
                   owned="Source/Engine/**")
        b = ticket("TASK-GAME", dept="gameplay", number=2,
                   owned="Source/Game/Items/**")
        plan, _ = control.ready_plan([a, b], CONFIG, SECRET)
        self.assertEqual(len(plan), 2)

    def test_priority_order(self):
        a = ticket("TASK-LOW", number=1,
                   labels=["studio:approved", "ai:ready", "dept:engine", "priority:p2"],
                   dept="engine", owned="Source/Engine/**")
        b = ticket("TASK-HIGH", number=2,
                   labels=["studio:approved", "ai:ready", "dept:gameplay", "priority:p0"])
        plan, _ = control.ready_plan([a, b], CONFIG, SECRET)
        self.assertEqual(plan[0][0]["number"], 2)

    def test_dispatch_requires_signature_key(self):
        with self.assertRaises(control.ContractError):
            control.validated(ticket(), CONFIG, "")


class RuntimeTests(unittest.TestCase):
    def test_lease_stable(self):
        issue = ticket()
        rec = control.validated(issue, CONFIG, SECRET)
        self.assertEqual(orchestrator.lease_id(issue, rec),
                         orchestrator.lease_id(issue, rec))
        self.assertEqual(len(orchestrator.lease_id(issue, rec)), 24)

    def test_worker_events_ignore_other_accounts(self):
        issue = ticket()
        rec = control.validated(issue, CONFIG, SECRET)
        lease = orchestrator.lease_id(issue, rec)
        event = {"type": "CLAIMED", "task_id": rec["STUDIO_TASK_ID"],
                 "lease_id": lease}
        comments = [{
            "user": {"login": "untrusted-user"}, "created_at": "2026-10-26T16:00:00Z",
            "id": 1, "body": "<!-- STUDIO_EVENT " + json.dumps(event) + " -->",
        }]
        self.assertIsNone(orchestrator.worker_event(comments, rec["STUDIO_TASK_ID"],
                                                   lease, {"SIGAAMDAD"}))
        comments[0]["user"]["login"] = "SIGAAMDAD"
        self.assertEqual(orchestrator.worker_event(comments, rec["STUDIO_TASK_ID"],
                                                  lease, {"SIGAAMDAD"})[1]["type"],
                         "CLAIMED")

    def test_outbox_deduplication_marker(self):
        comment = {
            "body": '<!-- STUDIO_OUTBOX {"task_id":"ITEM-0042","lease_id":"abc","status":"RESERVED"} -->'
        }
        self.assertEqual(len(orchestrator.outbox([comment], "ITEM-0042")), 1)
        self.assertEqual(len(orchestrator.outbox([comment], "OTHER")), 0)


if __name__ == "__main__":
    unittest.main()
