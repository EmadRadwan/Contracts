#!/usr/bin/env node
/**
 * Checks every getTranslatedLabel() key in src/ against the locale bundles.
 *
 * Why this exists: getTranslatedLabel(key, default) is a silent fallback. A mistyped key returns
 * the English default, so the screen looks perfectly correct to an English-speaking developer
 * while its Arabic never applies. Two whole reports shipped that way -- "transation-totals" and
 * "acconting.orgGL..." -- and both were found by accident, months later.
 *
 * Two classes of problem:
 *   BAD PATH     the key diverges from the tree before its last segment. Usually a wrong prefix,
 *                so it takes out an entire screen at once. This is what the check gates on.
 *   MISSING LEAF the section resolves but that one label is absent. Common, usually benign
 *                (the default carries it), reported but not gated.
 *
 * Because there is a pre-existing backlog of bad paths, the check ratchets: only keys absent
 * from scripts/i18n-baseline.json fail the build. Fix one, then prune it with --update-baseline
 * so it can never come back.
 *
 * Usage:
 *   node scripts/check-i18n-keys.mjs                  gate on new bad paths (exit 1 on failure)
 *   node scripts/check-i18n-keys.mjs --all            also list every missing leaf
 *   node scripts/check-i18n-keys.mjs --update-baseline  rewrite the baseline from current state
 */
import { readFileSync, writeFileSync, readdirSync, statSync } from 'node:fs';
import { join, relative, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const HERE = dirname(fileURLToPath(import.meta.url));
const SRC = join(HERE, '..', 'src');
const MSGS = join(SRC, 'app', 'common', 'messages');
const BASELINE = join(HERE, 'i18n-baseline.json');

const argv = new Set(process.argv.slice(2));
const SHOW_ALL = argv.has('--all');
const UPDATE = argv.has('--update-baseline');

/** Flatten a message bundle into dotted leaf paths. */
function leaves(node, prefix = '', out = new Map()) {
  if (node && typeof node === 'object' && !Array.isArray(node)) {
    for (const [k, v] of Object.entries(node)) leaves(v, prefix ? `${prefix}.${k}` : k, out);
  } else {
    out.set(prefix, node);
  }
  return out;
}

function loadBundle(name) {
  return leaves(JSON.parse(readFileSync(join(MSGS, name), 'utf8')));
}

const ar = loadBundle('ar.json');
const en = loadBundle('en.json');
const known = new Set([...ar.keys(), ...en.keys()]);

// Every path that exists as an ancestor of a real key, so we can tell a wrong prefix from a
// merely absent label.
const prefixes = new Set();
for (const key of known) {
  const parts = key.split('.');
  for (let i = 1; i < parts.length; i++) prefixes.add(parts.slice(0, i).join('.'));
}

const SKIP_DIRS = new Set(['node_modules', 'dist', 'build', '.git']);
function* walk(dir) {
  for (const entry of readdirSync(dir)) {
    if (SKIP_DIRS.has(entry)) continue;
    const full = join(dir, entry);
    if (statSync(full).isDirectory()) yield* walk(full);
    else if (/\.tsx?$/.test(entry)) yield full;
  }
}

const CALL = /getTranslatedLabel\(\s*(`[^`]*`|"[^"]*"|'[^']*')/g;
const CONST = /(?:const|let)\s+(\w*[lL]ocalizationKey\w*)\s*=\s*["'`]([^"'`]+)["'`]/g;

const badPath = new Map();      // key -> Set("file:line")
const missingLeaf = new Map();  // key -> Set("file:line")
let scanned = 0;

for (const file of walk(SRC)) {
  const src = readFileSync(file, 'utf8');
  if (!src.includes('getTranslatedLabel(')) continue;
  scanned++;

  const consts = new Map();
  for (const m of src.matchAll(CONST)) consts.set(m[1], m[2]);
  const rel = relative(join(HERE, '..'), file);

  for (const m of src.matchAll(CALL)) {
    const raw = m[1];
    const body = raw.slice(1, -1);
    let key;

    if (raw[0] === '`') {
      // Resolve ${localizationKey}; anything else dynamic cannot be checked statically.
      let dynamic = false;
      key = body.replace(/\$\{([^}]*)\}/g, (_, name) => {
        const val = consts.get(name.trim());
        if (val === undefined) { dynamic = true; return ''; }
        return val;
      });
      if (dynamic) continue;
    } else {
      key = body;
    }

    if (!key || key.includes('${') || known.has(key)) continue;

    const line = src.slice(0, m.index).split('\n').length;
    const at = `${rel}:${line}`;
    const parts = key.split('.');
    let deepest = 0;
    for (let i = 1; i < parts.length; i++) {
      if (prefixes.has(parts.slice(0, i).join('.'))) deepest = i;
    }
    const bucket = deepest >= parts.length - 1 ? missingLeaf : badPath;
    if (!bucket.has(key)) bucket.set(key, new Set());
    bucket.get(key).add(at);
  }
}

const currentBad = [...badPath.keys()].sort();

if (UPDATE) {
  writeFileSync(
    BASELINE,
    JSON.stringify(
      {
        _comment:
          'Known bad-path i18n keys, tolerated so the check can ratchet. Fix a key, then rerun ' +
          'with --update-baseline to prune it. Never add by hand.',
        _generated: new Date().toISOString().slice(0, 10),
        keys: currentBad,
      },
      null,
      2
    ) + '\n'
  );
  console.log(`baseline written: ${currentBad.length} known bad-path keys`);
  process.exit(0);
}

let baseline = { keys: [] };
try {
  baseline = JSON.parse(readFileSync(BASELINE, 'utf8'));
} catch {
  console.warn(`no baseline at ${relative(process.cwd(), BASELINE)} — treating every bad path as new`);
}
const allowed = new Set(baseline.keys ?? []);

const introduced = currentBad.filter((k) => !allowed.has(k));
const fixed = [...allowed].filter((k) => !badPath.has(k)).sort();

console.log(`scanned ${scanned} files using getTranslatedLabel`);
console.log(`locale keys: ar=${ar.size} en=${en.size} union=${known.size}`);
console.log(`bad-path keys: ${currentBad.length} (${allowed.size} baselined)`);
console.log(`missing-leaf keys: ${missingLeaf.size}`);

if (SHOW_ALL && missingLeaf.size) {
  console.log('\nMISSING LEAF (section exists, this label does not — not gated):');
  for (const key of [...missingLeaf.keys()].sort()) {
    console.log(`  ${key}`);
    for (const at of missingLeaf.get(key)) console.log(`      ${at}`);
  }
}

if (fixed.length) {
  console.log(`\n${fixed.length} baselined key(s) no longer used — prune with --update-baseline:`);
  for (const k of fixed) console.log(`  ${k}`);
}

if (introduced.length) {
  console.error(`\nFAIL: ${introduced.length} new bad-path i18n key(s).`);
  console.error('The key does not match the locale tree, so this label will silently render its');
  console.error('English default in every language. Fix the key, or add the section to');
  console.error('src/app/common/messages/ar.json and en.json.\n');
  for (const key of introduced) {
    console.error(`  ${key}`);
    for (const at of badPath.get(key)) console.error(`      ${at}`);
  }
  process.exit(1);
}

console.log('\nOK — no new bad-path i18n keys.');
