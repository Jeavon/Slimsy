// Appends a pass/fail table for this run to the GitHub Actions job summary, using the
// "json" reporter's output. Schema confirmed by running a throwaway Playwright test and
// inspecting the real output - Playwright's own docs don't publish the exact shape:
// { suites: [ { title, specs: [ { title, ok, tests: [ { status, results: [ { status, duration } ] } ] } ],
//              suites: [ ...nested... ] } ] }
// A test that failed and then passed on retry has `spec.ok === true` (its final result passed) and
// `test.status === "flaky"`, so `ok` alone would report it as a plain pass.
import fs from "node:fs";

const resultsPath = "results.json";
const summaryPath = process.env.GITHUB_STEP_SUMMARY;

if (!fs.existsSync(resultsPath)) {
  console.log(`No ${resultsPath} found - skipping job summary.`);
  process.exit(0);
}
if (!summaryPath) {
  console.log("GITHUB_STEP_SUMMARY is not set - skipping job summary.");
  process.exit(0);
}

const data = JSON.parse(fs.readFileSync(resultsPath, "utf8"));

const rows = [];
function walk(suite, titlePath) {
  for (const spec of suite.specs ?? []) {
    const lastResults = (spec.tests ?? []).map((t) => t.results?.at(-1)).filter(Boolean);
    const durationMs = lastResults.reduce((sum, r) => sum + (r.duration ?? 0), 0);
    // A conditionally skipped spec (e.g. the per-provider Data Protection specs in the main run) has
    // `ok: true`, so it has to be recognised separately or it would be reported as a pass.
    const skipped = (spec.tests ?? []).length > 0 && (spec.tests ?? []).every((t) => t.status === "skipped");
    const ok = spec.ok !== false;
    const retried = (spec.tests ?? []).some((t) => t.status === "flaky" || (t.results?.length ?? 0) > 1);
    rows.push({
      title: [...titlePath, spec.title].join(" › "),
      ok,
      skipped,
      flaky: ok && !skipped && retried,
      durationSeconds: (durationMs / 1000).toFixed(1),
    });
  }
  for (const child of suite.suites ?? []) {
    walk(child, suite.title ? [...titlePath, suite.title] : titlePath);
  }
}
for (const suite of data.suites ?? []) walk(suite, []);

const skipped = rows.filter((r) => r.skipped).length;
const flaky = rows.filter((r) => r.flaky).length;
const failed = rows.filter((r) => !r.ok).length;
const ran = rows.length - skipped;
const passed = ran - flaky - failed;
const skippedNote = skipped ? `, ${skipped} skipped ⏭️` : "";

const headline =
  failed === 0 && flaky === 0
    ? `**${passed}/${ran} passed** ✅${skippedNote}`
    : `**${passed} passed${flaky ? `, ${flaky} flaky ⚠️` : ""}${failed ? `, ${failed} failed ❌` : ""}${skippedNote}** out of ${ran}`;

const lines = [
  `## Playwright E2E Results${process.env.SUMMARY_TITLE ? ` - ${process.env.SUMMARY_TITLE}` : ""}`,
  "",
  headline,
  "",
  ...(flaky
    ? [
        "> ⚠️ A flaky test failed its first attempt and passed on retry, so the job is green. Its trace and screenshot are in the `playwright-report` artifact.",
        "",
      ]
    : []),
  "| Status | Test | Duration |",
  "| --- | --- | --- |",
  ...rows.map((r) => `| ${!r.ok ? "❌" : r.skipped ? "⏭️ skipped" : r.flaky ? "⚠️ flaky" : "✅"} | ${r.title} | ${r.durationSeconds}s |`),
  "",
];

fs.appendFileSync(summaryPath, lines.join("\n"));
console.log(`Wrote summary for ${rows.length} test(s) to GITHUB_STEP_SUMMARY.`);
