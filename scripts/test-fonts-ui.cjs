const fs = require("node:fs"),
  path = require("node:path"),
  net = require("node:net");
const assert = require("node:assert/strict");
const { spawn } = require("node:child_process");
const { createHash } = require("node:crypto");
const {
  getPublishDirectory,
} = require("../../AppDock.at365/scripts/publish-paths.cjs");
const root = path.resolve(__dirname, ".."),
  host = path.resolve(root, "../AppDock.at365");
const { copyHost } = require(path.join(host, "scripts/copy-host.cjs"));
const { chromium, _electron: electron } = require(
  path.join(host, "node_modules/playwright"),
);
const { createDefaultSettings } = require(
  path.join(host, "out/main/shared/settings-schema.js"),
);
const profile = path.join(
    root,
    ".artifacts",
    `installed-fonts-ui-${Date.now()}`,
  ),
  probe = path.join(profile, "clock");
const id = "at365.watch",
  checks = [];
const hash = (file) =>
  createHash("sha256").update(fs.readFileSync(file)).digest("hex");
const read = (file) => JSON.parse(fs.readFileSync(file, "utf8"));
const state = () => read(path.join(probe, "clock-state.json"));
async function until(fn, label, timeout = 60000) {
  const end = Date.now() + timeout;
  while (Date.now() < end) {
    try {
      const value = await fn();
      if (value) return value;
    } catch {}
    await new Promise((resolve) => setTimeout(resolve, 80));
  }
  throw Error(label);
}
let app, browser, child, page, hostHash;
async function launch() {
  const env = { ...process.env, APPDOCK_WATCH_TEST_OUTPUT: probe };
  delete env.ELECTRON_RUN_AS_NODE;
  let context;
  if (process.argv[2]) {
    const exe = path.join(profile, "AppDock.at365.exe");
    copyHost(path.resolve(process.argv[2]), exe);
    hostHash = hash(exe);
    assert.equal(hostHash, hash(path.resolve(process.argv[2])));
    const server = net.createServer();
    await new Promise((resolve) => server.listen(0, "127.0.0.1", resolve));
    const port = server.address().port;
    await new Promise((resolve) => server.close(resolve));
    child = spawn(
      exe,
      [`--test-profile=${profile}`, `--remote-debugging-port=${port}`],
      { env, windowsHide: true, stdio: "ignore" },
    );
    browser = await until(async () => {
      try {
        return await chromium.connectOverCDP(`http://127.0.0.1:${port}`, {
          timeout: 1000,
        });
      } catch {}
    }, "portable host connection");
    context = browser.contexts()[0];
  } else {
    app = await electron.launch({
      executablePath: require(path.join(host, "node_modules/electron")),
      args: [host, `--test-profile=${profile}`],
      env,
    });
    context = app.context();
  }
  page = await until(
    () =>
      context
        .pages()
        .find((p) => p.url().startsWith("appdock://host/index.html")),
    "host page",
  );
  await until(
    () =>
      page.evaluate(async (id) => {
        const s = await window.dock.snapshot();
        return (
          s.startupReady &&
          s.extensions.find((a) => a.id === id)?.state === "running"
        );
      }, id),
    "Watch running",
  );
}
async function close() {
  if (app) {
    await app.close();
    app = null;
  } else if (child?.exitCode === null && page) {
    await page.evaluate(() => window.dock.executeCommand("appdock.quit"));
    await until(() => child.exitCode !== null, "portable host exit");
    assert.equal(child.exitCode, 0);
  }
  await browser?.close();
  browser = null;
}
async function text(label, value) {
  const field = page.getByLabel(label, { exact: true });
  await field.fill(value);
  await field.press("Enter");
}
async function selectSettingsPage(pageId) {
  await page.evaluate(async (pageId) => {
    await window.dock.navigate("settings");
    const s = await window.dock.settingsEditing();
    await window.dock.editSettings({
      sessionId: s.sessionId,
      editRevision: s.editRevision,
      operationId: crypto.randomUUID(),
      action: { type: "select-page", pageId },
    });
  }, pageId);
}
(async () => {
  const output = path.join(getPublishDirectory(root), "Applet.Watch.at365");
  const folder = path.join(profile, "extensions/Applet.Watch.at365");
  fs.mkdirSync(folder, { recursive: true });
  for (const name of ["extension.json", "Applet.Watch.at365.exe"]) {
    fs.copyFileSync(path.join(output, name), path.join(folder, name));
    assert.equal(hash(path.join(output, name)), hash(path.join(folder, name)));
  }
  const manifest = read(path.join(folder, "extension.json"));
  assert(!manifest.capabilities.includes("notifications"));
  assert.equal(manifest.notificationTypes?.length ?? 0, 0);
  const config = createDefaultSettings();
  config.host.soundEnabled = false;
  config.host.closeToTray = false;
  config.extensions[id] = {
    enabled: true,
    settings: { fontSize: 160 },
  };
  config.keybindings = [];
  config.keybindingDefaultsInitialized = [id];
  config.gestureDefaultsInitialized = [id];
  config.gestures.bindings = [];
  config.shortcuts = {};
  config.globalShortcutCommands = [];
  fs.writeFileSync(path.join(profile, "settings.json"), JSON.stringify(config));
  await launch();
  const initial = await until(
    () => state().visible && state(),
    "clock visible",
  );
  assert.equal(initial.font, "Segoe UI");
  assert.equal(initial.requestedFont, "");
  assert.equal(initial.styles & 0x080000a0, 0x080000a0);
  assert.equal(initial.showInTaskbar, false);
  assert.equal(initial.showActivated, false);
  await until(() => state().text !== initial.text, "clock ticking");
  checks.push(
    "published native fallback / ticking / click-through / no activation",
  );
  await selectSettingsPage("notifications");
  await page.getByTestId("notification-speech").waitFor();
  assert.equal(await page.getByRole("row").filter({ hasText: "Watch" }).count(), 0);
  assert.equal(await page.getByText("フォント案内", { exact: true }).count(), 0);
  checks.push("notification settings contain neither font guidance nor legacy Watch notifications");
  await selectSettingsPage("applet:at365.watch");
  await page.getByLabel("フォント", { exact: true }).waitFor();
  await page
    .getByRole("button", { name: "フォントを選ぶ", exact: true })
    .waitFor();
  assert.equal(await page.getByRole("button", { name: "フォントの公式情報を開く", exact: true }).count(), 0);
  assert.equal(
    await page.getByLabel("フォント", { exact: true }).inputValue(),
    "",
  );
  await text("フォント", "Segoe UI");
  await until(
    () => state().fontInstalled && state().requestedFont === "Segoe UI",
    "installed font applied",
  );
  await until(
    () =>
      read(path.join(profile, "settings.json")).extensions[id].settings
        .fontFamily === "Segoe UI",
    "font saved",
  );
  const edit = page.getByLabel("フォント", { exact: true });
  await edit.fill("Unsaved Font");
  await edit.press("Escape");
  assert.equal(await edit.inputValue(), "Segoe UI");
  assert.equal(state().requestedFont, "Segoe UI");
  checks.push(
    "font edit saves immediately / reaches native clock / Escape cancellation",
  );
  await text("フォント", "");
  await until(() => state().requestedFont === "" && state().font === "Segoe UI", "clearing font uses fallback");
  await until(() => !read(path.join(profile, "settings.json")).extensions[id].settings.fontFamily, "empty default persisted or omitted");
  assert.equal(await page.getByLabel("フォント", { exact: true }).inputValue(), "");
  checks.push("empty default / clear to Segoe UI / no information button");
  await text("フォント", "Missing Font Fixture");
  await until(
    () => state().requestedFont === "Missing Font Fixture" && state().font === "Segoe UI" && !state().fontInstalled,
    "missing font falls back without changing saved selection",
  );
  await until(
    () => read(path.join(profile, "settings.json")).extensions[id].settings.fontFamily === "Missing Font Fixture",
    "missing font name remains saved",
  );
  const missingFontPid = state().processId;
  await page.evaluate(() => window.dock.restartExtension("at365.watch"));
  await until(
    () => state().processId !== missingFontPid && state().requestedFont === "Missing Font Fixture" && state().font === "Segoe UI",
    "restart retains missing font selection and fallback",
  );
  checks.push("explicit missing font silently falls back and preserves saved family across restart");
  await text("フォント", "Haettenschweiler");
  await until(
    () => state().requestedFont === "Haettenschweiler",
    "preferred font selection",
  );
  const preferred = state();
  assert.equal(
    preferred.font,
    preferred.fontInstalled ? "Haettenschweiler" : "Segoe UI",
  );
  await text("フォント", "Segoe UI");
  await text("時計の文字サイズ", "180");
  await page.getByLabel("時計の位置", { exact: true }).selectOption("bottom");
  await until(
    () =>
      state().top > state().monitor.Top && state().requestedFont === "Segoe UI",
    "bottom placement",
  );
  await page.screenshot({ path: path.join(profile, "font-settings.png") });
  fs.copyFileSync(
    path.join(probe, "clock.png"),
    path.join(profile, "clock-font.png"),
  );
  checks.push("preferred installed font / live size and bottom placement");
  await require("./settings-mcp-check.cjs")({
    host,
    profile,
    page,
    id,
    keys: [
      "visible",
      "alignment",
      "horizontalMargin",
      "verticalMargin",
      "fontSize",
      "fontFamily",
      "opacity",
      "showSeconds",
      "showDate",
    ],
    changes: {
      visible: true,
      alignment: "top",
      horizontalMargin: 22,
      verticalMargin: 12,
      fontSize: 160,
      fontFamily: "Segoe UI",
      opacity: 0.55,
      showSeconds: true,
      showDate: true,
    },
    invalid: [{ opacity: 2 }, { alignment: "middle" }, { fontFamily: 42 }],
    hidden: ["monitor"],
    switches: ["showSeconds", "showDate"],
  });
  await until(
    () => state().opacity === 0.55 && !state().showSeconds && !state().showDate,
    "MCP settings reach native clock",
  );
  checks.push(
    "existing nine MCP public settings / dynamic monitor remains private / validation and persistence",
  );
  await page.evaluate(() =>
    window.dock.executeCommand("at365.watch.settings.visible.off"),
  );
  await until(() => !state().visible, "hide");
  await page.evaluate(() =>
    window.dock.executeCommand("at365.watch.settings.visible.on"),
  );
  await until(() => state().visible, "show");
  const pid = state().processId;
  await page.evaluate(() => window.dock.restartExtension("at365.watch"));
  await until(
    () => state().processId !== pid && state().requestedFont === "Segoe UI",
    "restart restores font",
  );
  checks.push("show/hide / Applet restart preserves font selection");
  await page.evaluate(() => window.dock.toggleExtension("at365.watch", false));
  await until(
    () =>
      page.evaluate(
        async () =>
          (await window.dock.snapshot()).extensions.find(
            (a) => a.id === "at365.watch",
          ).state === "stopped",
      ),
    "disable",
  );
  await close();
  checks.push("disable / isolated host shutdown");
  const result = {
    ok: true,
    profile,
    checks,
    preferredFontInstalled: preferred.fontInstalled,
    hostHash,
  };
  fs.writeFileSync(
    path.join(profile, "result.json"),
    JSON.stringify(result, null, 2),
  );
  console.log(JSON.stringify(result, null, 2));
})()
  .catch((error) => {
    fs.writeFileSync(
      path.join(profile, "result.json"),
      JSON.stringify({ ok: false, checks, error: error.stack }, null, 2),
    );
    console.error(error);
    process.exitCode = 1;
  })
  .finally(async () => {
    await close().catch(() => {});
  });
