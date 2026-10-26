import json
import unittest
from pathlib import Path
import sys

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
import dispatcher

CONFIG = dispatcher.load_config(Path(__file__).resolve().parent / "fixtures" / "v4.json")

def issue(labels=None, body=None):
    return {
        "number": 42,
        "title": "[ITEM-0042] Add a consumable",
        "html_url": "https://github.com/SIGAAMDAD/The_Nomad/issues/42",
        "labels": [{"name": s} for s in (labels or ["studio:approved", "ai:ready", "dept:gameplay"])],
        "body": body or (
            "STUDIO_TASK_ID: ITEM-0042\nTARGET: game\n"
            "TARGET_REPO: SIGAAMDAD/The_Nomad\nTARGET_BRANCH: Nomad3D\n"
            "BASE_SHA: " + "a" * 40 + "\n"
            "DRIVE_BRIEF_URL: https://docs.google.com/document/d/sample/edit\n"
            "OWNER_APPROVED: YES\n"
        )
    }

class DispatchTests(unittest.TestCase):
    def test_target_branches(self):
        self.assertEqual(CONFIG["targets"]["framework"]["branch"], "develop")
        self.assertEqual(CONFIG["targets"]["game"]["branch"], "Nomad3D")

    def test_approved_issue(self):
        dept, task = dispatcher.validate_issue(issue(), CONFIG)
        self.assertEqual(dept, "gameplay")
        self.assertEqual(task["STUDIO_TASK_ID"], "ITEM-0042")

    def test_reject_unapproved(self):
        with self.assertRaises(ValueError):
            dispatcher.validate_issue(issue(labels=["ai:ready", "dept:gameplay"]), CONFIG)

    def test_reject_duplicate_dispatch(self):
        with self.assertRaises(ValueError):
            dispatcher.validate_issue(issue(labels=["studio:approved","ai:ready","ai:dispatched","dept:gameplay"]), CONFIG)

    def test_reject_wrong_branch(self):
        with self.assertRaises(ValueError):
            dispatcher.validate_issue(issue(body=issue()["body"].replace("Nomad3D", "main")), CONFIG)

    def test_reject_duplicate_contract_field(self):
        with self.assertRaises(ValueError):
            dispatcher.validate_issue(issue(body=issue()["body"] + "TARGET: game\n"), CONFIG)

    def test_stage_mode(self):
        self.assertEqual(CONFIG["mode"], "staging")

if __name__ == "__main__":
    unittest.main()
