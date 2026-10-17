const fs = require('fs');
const path = require('path');

const sourceDir = path.resolve(__dirname, '..', '..', 'ASSETS-Repo', 'assetsetuff', 'tents');
const outputDir = path.resolve(__dirname, '..', 'repaired-assets');
const inputNames = [
  'medieval_market_tent_02.glb',
  'medieval_market_tent_prop_02.glb'
];

function readGlb(filePath) {
  const buffer = fs.readFileSync(filePath);
  if (buffer.readUInt32LE(0) !== 0x46546c67) {
    throw new Error(`Not a GLB file: ${filePath}`);
  }

  const chunks = [];
  let offset = 12;
  while (offset < buffer.length) {
    const length = buffer.readUInt32LE(offset);
    const type = buffer.readUInt32LE(offset + 4);
    chunks.push({ type, data: buffer.subarray(offset + 8, offset + 8 + length) });
    offset += 8 + length;
  }

  const jsonChunk = chunks.find(chunk => chunk.type === 0x4e4f534a);
  if (!jsonChunk) throw new Error(`JSON chunk missing: ${filePath}`);
  return { chunks, json: JSON.parse(jsonChunk.data.toString('utf8').replace(/\0+$/, '').trim()) };
}

function repairJson(json) {
  let changedMaterials = 0;
  let removedUvAttributes = 0;

  for (const material of json.materials || []) {
    const textureInfos = [];
    if (material.pbrMetallicRoughness) {
      textureInfos.push(...Object.values(material.pbrMetallicRoughness));
    }
    textureInfos.push(material.normalTexture, material.occlusionTexture, material.emissiveTexture);
    for (const info of textureInfos) {
      if (info && typeof info === 'object' && info.texCoord > 1) {
        info.texCoord = 0;
        changedMaterials++;
      }
    }
  }

  for (const mesh of json.meshes || []) {
    for (const primitive of mesh.primitives || []) {
      for (const key of ['TEXCOORD_2', 'TEXCOORD_3']) {
        if (Object.prototype.hasOwnProperty.call(primitive.attributes || {}, key)) {
          delete primitive.attributes[key];
          removedUvAttributes++;
        }
      }
    }
  }

  return { changedMaterials, removedUvAttributes };
}

function writeGlb(input, json) {
  const jsonText = JSON.stringify(json);
  const jsonBytes = Buffer.from(jsonText + ' '.repeat((4 - (jsonText.length % 4)) % 4), 'utf8');
  const chunks = input.chunks.map(chunk => chunk.type === 0x4e4f534a ? { type: chunk.type, data: jsonBytes } : chunk);
  const totalLength = 12 + chunks.reduce((sum, chunk) => sum + 8 + chunk.data.length, 0);
  const output = Buffer.alloc(totalLength);
  output.writeUInt32LE(0x46546c67, 0);
  output.writeUInt32LE(2, 4);
  output.writeUInt32LE(totalLength, 8);

  let offset = 12;
  for (const chunk of chunks) {
    output.writeUInt32LE(chunk.data.length, offset);
    output.writeUInt32LE(chunk.type, offset + 4);
    chunk.data.copy(output, offset + 8);
    offset += 8 + chunk.data.length;
  }
  return output;
}

fs.mkdirSync(outputDir, { recursive: true });
for (const name of inputNames) {
  const inputPath = path.join(sourceDir, name);
  const outputPath = path.join(outputDir, name.replace(/\.glb$/i, '_unity_repaired.glb'));
  const parsed = readGlb(inputPath);
  const changes = repairJson(parsed.json);
  fs.writeFileSync(outputPath, writeGlb(parsed, parsed.json));
  console.log(`${name}: remapped ${changes.changedMaterials} texture UV references and removed ${changes.removedUvAttributes} extra UV attributes`);
  console.log(`  -> ${outputPath}`);
}
