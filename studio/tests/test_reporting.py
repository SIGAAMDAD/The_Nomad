"""Additional tests for review-only CI reporting, no GitHub API required."""
import sys
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
import ci_triage
import report


def issue(state, names):
    return {"state": state, "labels": [{"name": x} for x in names]}


class StatusTests(unittest.TestCase):
    def test_report_counts_only_signed_queue_label_group(self):
        rows = [
            issue("open", ["studio:approved", "dept:gameplay", "ai:ready"]),
            issue("open", ["studio:approved", "dept:engine", "ai:needs-attention"]),
            issue("closed", ["studio:approved", "studio:accepted", "dept:qa"]),
            issue("open", ["not-a-studio-task"]),
        ]
        out = report.aggregate(rows)
        self.assertEqual(out["total_studio_issues"], 3)
        self.assertEqual(out["queued"], 1)
        self.assertEqual(out["needs_attention"], 1)
        self.assertEqual(out["accepted"], 1)

    def test_triage_filters_success(self):
        event = {"workflow_run": {"name": "Studio V5 / Contracts and Config",
                                   "head_sha": "a" * 40, "conclusion": "success"}}
        self.assertIsNone(ci_triage.fingerprint(event))

    def test_triage_is_deduplicated_by_run_attempt(self):
        event = {"workflow_run": {"name": "Studio V5 / Contracts and Config",
                                   "head_sha": "a" * 40, "run_attempt": 1,
                                   "conclusion": "failure"}}
        first = ci_triage.fingerprint(event)
        self.assertEqual(len(first), 24)
        event["workflow_run"]["run_attempt"] = 2
        self.assertNotEqual(first, ci_triage.fingerprint(event))

    def test_triage_filters_unknown_workflow(self):
        event = {"workflow_run": {"name": "Random external",
                                   "head_sha": "a" * 40, "conclusion": "failure"}}
        self.assertIsNone(ci_triage.fingerprint(event))


if __name__ == "__main__":
    unittest.main()
