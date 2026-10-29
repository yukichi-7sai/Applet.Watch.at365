const path = require("node:path"),
  fs = require("node:fs"),
  assert = require("node:assert/strict");
const { spawn, execFileSync } = require("node:child_process");
const {
  getPublishDirectory,
} = require("../../AppDock.at365/scripts/publish-paths.cjs");
const root = path.resolve(__dirname, ".."),
  host = path.resolve(root, "../AppDock.at365");
const { JsonLinePeer } = require(path.join(host, "out/main/main/core/rpc.js"));
const settings = {},
  calls = [];
const exe = process.argv[2] ? path.resolve(process.argv[2]) : path.join(
  getPublishDirectory(root),
  "Applet.Watch.at365/Applet.Watch.at365.exe",
);
const probe = path.join(root, ".artifacts", `independent-protocol-${Date.now()}`);
fs.mkdirSync(probe, { recursive: true });
const state = (name) => JSON.parse(fs.readFileSync(path.join(probe, `${name}-state.json`), "utf8"));
async function until(check, label) {
  const deadline = Date.now() + 15000;
  while (Date.now() < deadline) {
    try { if (check()) return; } catch {}
    await new Promise((resolve) => setTimeout(resolve, 50));
  }
  throw Error(label);
}
const child = spawn(exe, [], {
  env: { ...process.env, APPDOCK_WATCH_TEST_OUTPUT: probe },
  windowsHide: true,
  stdio: ["pipe", "pipe", "pipe"],
});
let diagnostics = "",
  panel;
child.stderr.on("data", (data) => (diagnostics += data));
const exited = new Promise((resolve, reject) => {
  child.on("error", reject);
  child.on("exit", resolve);
});
const peer = new JsonLinePeer(
  child.stdout,
  child.stdin,
  async (method, input) => {
    calls.push({ method, input });
    if (method === "host.ui.panel") panel = input;
    if (method === "host.notifications.publish")
      return { status: "accepted", notificationId: "font-notice" };
    if (method === "host.settings.set") settings[input.key] = input.value;
    return null;
  },
);
(async () => {
  await peer.request("activate", { id: "at365.watch", settings });
  assert.equal(state("clock").visible, false);
  assert.equal(state("date").visible, false);
  assert.equal(state("clock").timerRunning, false);
  assert.equal(state("date").timerRunning, false);
  assert.notEqual(state("clock").windowHandle, state("date").windowHandle);
  assert.deepEqual(calls.filter((c) => c.method === "host.settings.options").map((c) => c.input.key).sort(), ["dateMonitor", "monitor"]);
  Object.assign(settings, { showDate: true, dateFontSize: 80, dateFormat: "yyyy/MM/dd", dateAlignment: "bottom-right", dateVerticalMargin: 30 });
  await peer.request("settings.changed", settings);
  await until(() => state("date").visible && !state("clock").visible, "date-only display");
  assert.equal(state("date").fontSize, 80);
  assert.match(state("date").text, /^\d{4}\/\d{2}\/\d{2}$/);
  assert.equal(state("date").alignment, "bottom-right");
  Object.assign(settings, { visible: true, fontSize: 120, secondsFontSize: 60 });
  await peer.request("settings.changed", settings);
  await until(() => state("clock").visible && state("date").visible, "both displays");
  assert.equal(state("clock").fontSize, 120);
  assert.equal(state("clock").secondsFontSize, 60);
  settings.secondsFontSize = 90;
  await peer.request("settings.changed", settings);
  assert.equal(state("clock").fontSize, 120);
  assert.equal(state("clock").secondsFontSize, 90);
  assert.equal(state("date").fontSize, 80);
  for (const anchor of ["top", "top-center", "top-right", "bottom", "bottom-center", "bottom-right"]) {
    settings.alignment = anchor;
    settings.dateAlignment = anchor;
    await peer.request("settings.changed", settings);
    await until(() => [state("clock"), state("date")].every((s) => {
      const expected = anchor.endsWith("-right") ? s.windowWidth - s.horizontalMargin - s.contentWidth
        : anchor.endsWith("-center") ? (s.windowWidth - s.contentWidth) / 2 : s.horizontalMargin;
      return s.alignment === anchor && Math.abs(s.contentLeft - expected) < 2;
    }), `native content anchor ${anchor}`);
  }
  settings.showDate = false;
  await peer.request("settings.changed", settings);
  assert.equal(state("clock").visible, true);
  assert.equal(state("date").visible, false);
  settings.visible = false;
  await peer.request("settings.changed", settings);
  assert.equal(calls.filter((c) => c.method === "host.notifications.publish").length, 0);
  assert(panel.facts.some((f) => f.label === "フォント" && f.value === "Segoe UI"));
  settings.fontFamily = "Missing Font Fixture";
  await peer.request("settings.changed", settings);
  assert.equal(
    calls.filter((c) => c.method === "host.notifications.publish").length,
    0,
  );
  assert(
    panel.facts.some(
      (f) => f.label === "フォント" && f.value.includes("Segoe UI"),
    ),
  );
  settings.opacity = 0.5;
  await peer.request("settings.changed", settings);
  assert.equal(
    calls.filter((c) => c.method === "host.notifications.publish").length,
    0,
  );
  settings.fontFamily = "";
  await peer.request("settings.changed", settings);
  assert(panel.facts.some((f) => f.label === "フォント" && f.value === "Segoe UI"));
  await assert.rejects(peer.request("command.execute", { id: "at365.watch.font-information" }));
  assert.equal(calls.filter((c) => c.method === "host.browser.open").length, 0);
  const setCalls = () =>
    calls.filter((c) => c.method === "host.settings.set").length;
  const before = setCalls();
  for (const action of ["cancel", "choose"]) {
    const command = peer.request("command.execute", {
      id: "at365.watch.choose-font",
    });
    execFileSync(
      "pwsh.exe",
      [
        "-NoProfile",
        "-File",
        path.join(__dirname, "test-font-dialog.ps1"),
        "-WatchProcessId",
        String(child.pid),
        "-Action",
        action,
      ],
      { windowsHide: true, stdio: "inherit" },
    );
    await command;
    if (action === "cancel") assert.equal(setCalls(), before);
    else {
      assert.equal(settings.fontFamily, "Arial");
      assert.equal(setCalls(), before + 1);
    }
  }
  await peer.request("settings.changed", settings);
  assert(
    panel.facts.some((f) => f.label === "フォント" && f.value === "Arial"),
    JSON.stringify(panel.facts),
  );
  await peer.request("deactivate");
  assert.equal(calls.filter((c) => c.method.startsWith("host.notifications.")).length, 0);
  child.stdin.end();
  assert.equal(await exited, 0, diagnostics);
  fs.writeFileSync(path.join(probe, "result.json"), JSON.stringify({ ok: true, exe, checks: ["defaults off", "independent visibility", "independent sizes", "date format and six native anchors", "font fallback and dialog", "clean shutdown"] }, null, 2));
  console.log(
    "PASS empty default / missing font fallback without notification calls / removed information command / real font dialog cancel and OK / settings reflection / clean stop",
  );
})()
  .catch((error) => {
    console.error(error);
    process.exitCode = 1;
  })
  .finally(() => {
    child.stdin.end();
  });
