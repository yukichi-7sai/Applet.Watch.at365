const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const { spawn } = require("node:child_process");
const { createHash } = require("node:crypto");
const root = path.resolve(__dirname, "..");
const host = path.resolve(root, "../AppDock.at365");
const { JsonLinePeer } = require(path.join(host, "out/main/main/core/rpc.js"));
const { getPublishDirectory } = require(path.join(host, "scripts/publish-paths.cjs"));
const exe = process.argv[2] ? path.resolve(process.argv[2]) : path.join(
  getPublishDirectory(root), "Applet.Watch.at365/Applet.Watch.at365.exe",
);
const output = path.join(root, ".artifacts", `startup-protocol-${Date.now()}`);
fs.mkdirSync(output, { recursive: true });
const checks = [];
async function until(check, label) {
  const deadline = Date.now() + 10000;
  while (Date.now() < deadline) {
    if (check()) return;
    await new Promise((resolve) => setTimeout(resolve, 20));
  }
  throw Error(label);
}
async function run(mode) {
  const probe = path.join(output, mode);
  fs.mkdirSync(probe);
  const child = spawn(exe, [], {
    env: { ...process.env, APPDOCK_WATCH_TEST_OUTPUT: probe },
    windowsHide: true, stdio: ["pipe", "pipe", "pipe"],
  });
  const exited = new Promise((resolve, reject) => {
    child.once("error", reject); child.once("exit", resolve);
  });
  let stderr = "", release;
  const gate = new Promise((resolve) => { release = resolve; });
  const logs = [];
  child.stderr.on("data", (data) => { stderr += data; });
  const peer = new JsonLinePeer(child.stdout, child.stdin, async (method, input) => {
    if (method === "host.log") {
      assert.equal(input.level, "debug");
      logs.push(input.message);
      if (mode === "log-rejected") throw Error("Fixture log rejection");
      if (mode === "log-unanswered") await gate;
    }
    if (method === "host.settings.options" && input.key === "monitor") {
      if (mode === "host-wait") await gate;
      if (mode === "activation-failed") throw Error("Fixture monitor failure");
    }
    return null;
  });
  const stages = () => logs.map((message) => message.match(/ stage=(\S+)/)?.[1]);
  try {
    const settings = { visible: mode === "visible", showDate: mode === "visible", fontSize: 100, dateFontSize: 40, fontFamily: "Missing Private Font Fixture" };
    const activation = peer.request("activate", { id: "at365.watch", settings })
      .then((result) => ({ result }), (error) => ({ error }));
    if (mode === "host-wait") {
      await until(() => stages().includes("monitor.options"), "stage must reach the host before the monitored operation completes");
      assert.equal(stages().at(-1), "monitor.options");
      assert(!stages().includes("activate.ready"));
      release();
    }
    const result = await activation;
    if (mode === "activation-failed") {
      assert.match(result.error?.message ?? "", /Fixture monitor failure/);
      assert.match(logs.at(-1), /stage=activate.failed .*previousStage=monitor.options /);
      assert(!stages().includes("activate.ready"));
    } else {
      if (result.error) throw result.error;
      assert(Array.isArray(result.result.commands));
      if (mode.startsWith("log-")) assert.equal(logs.length, 1, "failed or stalled logging must not retry or block activation");
      else {
        assert.equal(stages()[0], "activate.received");
        assert.equal(stages().at(-1), "activate.ready");
        for (const stage of ["ui.dispatch", "clock.create", "date.create", "clock.fonts", "date.fonts", "clock.layout", "date.layout", "monitor.options", "dateMonitor.options", "state.publish"])
          assert(stages().includes(stage), stage);
        const visibilityStage = mode === "visible" ? "show" : "hide";
        assert(stages().includes(`clock.${visibilityStage}`));
        assert(stages().includes(`date.${visibilityStage}`));
        const elapsed = logs.map((message) => Number(message.match(/ elapsedMs=([\d.]+)/)[1]));
        assert(elapsed.every((value, index) => value >= 0 && (!index || value >= elapsed[index - 1])));
        assert.equal(new Set(logs.map((message) => message.match(/attempt=(\w+)/)[1])).size, 1);
      }
      for (const name of ["clock", "date"]) {
        const state = JSON.parse(fs.readFileSync(path.join(probe, `${name}-state.json`), "utf8"));
        assert.equal(state.visible, mode === "visible");
      }
      const count = logs.length;
      await peer.request("settings.changed", { ...settings, opacity: 0.6 });
      assert.equal(logs.length, count, "settings changes must not emit startup diagnostics");
    }
    assert(logs.every((message) => !message.includes("Missing Private Font Fixture")));
    await peer.request("deactivate");
    child.stdin.end();
    assert.equal(await exited, 0, stderr);
    checks.push(mode);
    console.log(`PASS ${mode}`);
  } finally {
    release();
    fs.writeFileSync(path.join(probe, "startup-log.json"), JSON.stringify(logs, null, 2));
    fs.writeFileSync(path.join(probe, "stderr.log"), stderr);
    peer.close();
    if (child.exitCode === null) { child.kill(); await exited; }
  }
}
(async () => {
  for (const mode of ["hidden", "visible", "host-wait", "activation-failed", "log-rejected", "log-unanswered"])
    await run(mode);
  fs.writeFileSync(path.join(output, "result.json"), JSON.stringify({ ok: true, exe, sha256: createHash("sha256").update(fs.readFileSync(exe)).digest("hex"), checks }, null, 2));
  console.log(output);
})().catch((error) => {
  fs.writeFileSync(path.join(output, "result.json"), JSON.stringify({ ok: false, checks, error: error.stack }, null, 2));
  console.error(error); process.exitCode = 1;
});
