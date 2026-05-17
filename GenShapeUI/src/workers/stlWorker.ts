// Dedicated Web Worker for Offloading Heavy 3D Mesh Parsing Mathematics

self.onmessage = function (e) {
  const buffer = e.data as ArrayBuffer;

  // 1. Decode STL Binary Buffer into raw float array
  const viewer = new DataView(buffer);
  
  // Check if ASCII STL (starts with 'solid')
  const decoder = new TextDecoder('utf-8');
  let isAscii = false;
  if (buffer.byteLength > 5) {
      const headerStr = decoder.decode(new Uint8Array(buffer, 0, 5));
      if (headerStr === 'solid') isAscii = true;
  }

  let rawVerticesList: Float32Array;

  if (isAscii) {
    const text = decoder.decode(new Uint8Array(buffer));
    const lines = text.split('\n');
    const vList: number[] = [];
    for (let i = 0; i < lines.length; i++) {
      const line = lines[i].trim();
      if (line.startsWith('vertex')) {
        const parts = line.split(/\s+/);
        vList.push(parseFloat(parts[1]), parseFloat(parts[2]), parseFloat(parts[3]));
      }
    }
    rawVerticesList = new Float32Array(vList);
  } else {
    // Binary STL (Fastest)
    const numFaces = viewer.getUint32(80, true);
    rawVerticesList = new Float32Array(numFaces * 9); // 3 vertices * 3 coords per face
    
    let offset = 84;
    for (let face = 0; face < numFaces; face++) {
      if (offset + 50 > buffer.byteLength) break;
      
      offset += 12; // skip normal vector
      
      // Vertex 1
      rawVerticesList[face * 9] = viewer.getFloat32(offset, true);
      rawVerticesList[face * 9 + 1] = viewer.getFloat32(offset + 4, true);
      rawVerticesList[face * 9 + 2] = viewer.getFloat32(offset + 8, true);
      offset += 12;
      
      // Vertex 2
      rawVerticesList[face * 9 + 3] = viewer.getFloat32(offset, true);
      rawVerticesList[face * 9 + 4] = viewer.getFloat32(offset + 4, true);
      rawVerticesList[face * 9 + 5] = viewer.getFloat32(offset + 8, true);
      offset += 12;
      
      // Vertex 3
      rawVerticesList[face * 9 + 6] = viewer.getFloat32(offset, true);
      rawVerticesList[face * 9 + 7] = viewer.getFloat32(offset + 4, true);
      rawVerticesList[face * 9 + 8] = viewer.getFloat32(offset + 8, true);
      offset += 12;
      
      offset += 2; // skip attribute spacer
    }
  }

  // 2. Perform Dictionary Vertex Merging for Index Generation (Enables Normal Smoothing)
  const precisionPoints = 4;
  const map: { [key: string]: number } = {};
  const uniqueVertices: number[] = [];
  const indices: number[] = [];
  
  for (let i = 0; i < rawVerticesList.length; i += 3) {
    const x = rawVerticesList[i];
    const y = rawVerticesList[i + 1];
    const z = rawVerticesList[i + 2];
    
    const key = `${x.toFixed(precisionPoints)}_${y.toFixed(precisionPoints)}_${z.toFixed(precisionPoints)}`;
    
    if (map[key] !== undefined) {
      indices.push(map[key]);
    } else {
      const idx = uniqueVertices.length / 3;
      map[key] = idx;
      uniqueVertices.push(x, y, z);
      indices.push(idx);
    }
  }

  const mergedVertices = new Float32Array(uniqueVertices);
  const mergedIndices = new Uint32Array(indices);

  // 3. Post the compiled structured data back to the main UI thread via Transferable Objects
  (postMessage as any)(
    { mergedVertices, mergedIndices },
    [mergedVertices.buffer, mergedIndices.buffer] // Transfer ownership for zero-copy high speed!
  );
};
