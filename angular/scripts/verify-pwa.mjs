import { existsSync, readFileSync } from 'node:fs';
import { join } from 'node:path';

const root = new URL('..', import.meta.url).pathname.replace(/^\/(.:)/, '$1');
const sourceConfig = JSON.parse(readFileSync(join(root, 'ngsw-config.json'), 'utf8'));
const outputRoot = join(root, 'dist', 'EnglishLearningPlatformApp', 'browser');

function validateSourcePolicy(config) {
  if ('dataGroups' in config) {
    throw new Error('PWA policy violation: dataGroups are prohibited; private and mutable APIs are network-only.');
  }

  for (const group of config.assetGroups ?? []) {
    if (group.resources?.urls?.length) {
      throw new Error(`PWA policy violation: asset group '${group.name}' defines runtime URL caching. Only build-output files are allowed.`);
    }
  }
}

validateSourcePolicy(sourceConfig);

const maliciousFixture = structuredClone(sourceConfig);
maliciousFixture.assetGroups[0].resources.urls = ['/api/**'];
let maliciousFixtureRejected = false;
try {
  validateSourcePolicy(maliciousFixture);
} catch {
  maliciousFixtureRejected = true;
}
if (!maliciousFixtureRejected) throw new Error('PWA verifier self-test failed: an API runtime cache pattern was accepted.');

for (const name of ['index.html', 'manifest.webmanifest', 'ngsw.json', 'ngsw-worker.js']) {
  if (!existsSync(join(outputRoot, name))) throw new Error(`Missing production PWA artifact: ${name}`);
}

const generated = JSON.parse(readFileSync(join(outputRoot, 'ngsw.json'), 'utf8'));
if (generated.dataGroups?.length) throw new Error('PWA policy violation: generated runtime data caching exists.');
for (const group of generated.assetGroups ?? []) {
  if (group.patterns?.length) throw new Error(`Generated asset group '${group.name}' contains runtime URL patterns.`);
}

const manifest = JSON.parse(readFileSync(join(outputRoot, 'manifest.webmanifest'), 'utf8'));
if (manifest.display !== 'standalone' || !manifest.start_url || !manifest.name || !manifest.short_name) {
  throw new Error('Manifest is missing required installability metadata.');
}

for (const size of [192, 512]) {
  const iconPath = join(outputRoot, 'assets', 'icons', `icon-${size}x${size}.png`);
  const png = readFileSync(iconPath);
  const width = png.readUInt32BE(16);
  const height = png.readUInt32BE(20);
  if (width !== size || height !== size) throw new Error(`Invalid ${size}px install icon dimensions: ${width}x${height}.`);
}

const serialized = JSON.stringify(generated).toLowerCase();
for (const forbidden of ['/api/', '/connect/', 'authorization', 'token']) {
  if (serialized.includes(forbidden)) throw new Error(`Generated cache manifest references forbidden pattern '${forbidden}'.`);
}

console.log('PWA cache policy verified: static shell only; no runtime API data groups.');
