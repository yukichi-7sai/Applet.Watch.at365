const path = require("node:path"),
  assert = require("node:assert/strict");
const { spawn, execFileSync } = require("node:child_process");
const {
  getPublishDirectory,
} = require("../../AppDock.at365/scripts/publish-paths.cjs");
const root = path.resolve(__dirname, ".."),
  host = path.resolve(root, "../AppDock.at365");
const { JsonLinePeer } = require(path.join(host, "out/main/main/core/rpc.js"));
const settings = { visible: false },
  calls = [];
const exe = path.join(
  getPublishDirectory(root),
  "Applet.Watch.at365/Applet.Watch.at365.exe",
);
const child = spawn(exe, [], {
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
  assert.equal(calls.filter((c) => c.method === "host.notifications.publish").length, 0);
  assert(panel.facts.some((f) => f.label === "フォント" && f.value === "Segoe UI"));
  settings.fontFamily = "Missing Font Fixture";
  await peer.request("settings.changed", settings);
  assert.equal(
    calls.filter((c) => c.method === "host.notifications.publish").length,
    1,
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
    1,
  );
  settings.fontFamily = "";
  await peer.request("settings.changed", settings);
  assert(
    calls.some(
      (c) =>
        c.method === "host.notifications.end" && c.input.status === "resolved",
    ),
  );
  await assert.rejects(peer.request("command.execute", { id: "at365.watch.font-information" }));
  assert.equal(calls.filter((c) => c.method === "host.browser.open").length, 0);
  assert(calls.filter((c) => c.method === "host.notifications.publish").every((c) => !c.input.actions?.length));
  const setCalls = () =>
    calls.filter((c) => c.method === "host.settings.set").length;
  const before = setCalls();
  for (const action of ["cancel", "choose"]) {
    const command = peer.request("command.execute", {
      id: "at365.watch.choose-font",
    });
    await new Promise((resolve) => setTimeout(resolve, 100));
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
  child.stdin.end();
  assert.equal(await exited, 0, diagnostics);
  console.log(
    "PASS empty default / missing font fallback / notice deduplication and resolution / removed information command / real font dialog cancel and OK / settings reflection / clean stop",
  );
})()
  .catch((error) => {
    console.error(error);
    process.exitCode = 1;
  })
  .finally(() => {
    child.stdin.end();
  });
